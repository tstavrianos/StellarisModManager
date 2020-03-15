using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using System.IO;
using Ionic.Zip;
using Stellaris.Data.Parsers;
using Path2 = System.IO.Path;
using Stellaris.Data.Parsers.pck;

namespace Stellaris.Data
{

    public sealed class Mod
    {
        private static readonly string[] AllowedExtensions = { ".gfx", ".gui", ".txt", ".asset" };
        internal static readonly Logger Log;

        private static string TrimQuotes(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 3 || value[0] != '"' ||
                value[value.Length - 1] != '"') return value;
            return value.Substring(1, value.Length - 2);
        }
        static Mod()
        {
#if DEBUG
            Log = new LoggerConfiguration()//
                .MinimumLevel.Debug()//
                .Enrich.WithExceptionDetails()//
                .Enrich.FromLogContext()//
                .WriteTo.File("Mod.log")//
                .CreateLogger();//
#endif
        }

        private readonly ParseNode _tree;

        public string Id { get; }

        public string Key => $"mod/{this.Id}";

        public string Name { get; }

        public List<ModFile> Files { get; } = new List<ModFile>();

        public List<string> Tags { get; } = new List<string>();
        public List<string> Dependencies { get; } = new List<string>();

        public string Archive { get; private set; }

        public string Path { get; private set; }

        public string Picture { get; }
        public bool Valid { get; private set; }

        public string RemoteFileId { get; }
        public SupportedVersion SupportedVersion { get; }
        public string Version { get; }

        private readonly string _replacePath;

        private static string Strip(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            if (value.Length <= 2) return value;
            if (value[0] == '"' && value[value.Length - 1] == '"')
                return value.Substring(1, value.Length - 2);
            return value;
        }

        public Mod(string file)
        {
            this.Id = Path2.GetFileName(file);
            this.Valid = true;

            var text = File.ReadAllText(file);
            var lexer = new Tokenizer(text);
            var parser = new Parser(lexer);
            this._tree = parser.ParseReductions(true);
            this.Tags = new List<string>();
            this.Dependencies = new List<string>();
            if (this._tree.Symbol == "assignmentList")
            {
                foreach (var child in this._tree.Children)
                {
                    if (child.Symbol == "assignment" && child.Children.Count == 3 && child.Children[1].Symbol == "OPERATOR" && child.Children[1].Value == "=" && child.Children[0].Symbol == "SYMBOL")
                    {
                        switch (child.Children[0].Value)
                        {
                            case "name" when child.Children[2].Symbol == "STRING":
                                this.Name = TrimQuotes(child.Children[2].Value);
                                break;
                            case "picture" when child.Children[2].Symbol == "STRING":
                                this.Picture = TrimQuotes(child.Children[2].Value);
                                break;
                            case "supported_version" when child.Children[2].Symbol == "STRING":
                                this.SupportedVersion = new SupportedVersion(TrimQuotes(child.Children[2].Value));
                                break;
                            case "path" when child.Children[2].Symbol == "STRING":
                                this.Path = TrimQuotes(child.Children[2].Value);
                                break;
                            case "remote_file_id" when child.Children[2].Symbol == "STRING":
                                this.RemoteFileId = TrimQuotes(child.Children[2].Value);
                                break;
                            case "version" when child.Children[2].Symbol == "STRING":
                                this.Version = TrimQuotes(child.Children[2].Value);
                                break;
                            case "archive" when child.Children[2].Symbol == "STRING":
                                this.Archive = TrimQuotes(child.Children[2].Value);
                                break;
                            case "replace_path" when child.Children[2].Symbol == "STRING":
                                this._replacePath = TrimQuotes(child.Children[2].Value);
                                break;
                            case "tags" when child.Children[2].Symbol == "array":
                                this.Tags.AddRange(child.Children[2].Children.Select(x => TrimQuotes(x.Value)));
                                break;
                            case "dependencies" when child.Children[2].Symbol == "array":
                                this.Dependencies.AddRange(child.Children[2].Children.Select(x => TrimQuotes(x.Value)));
                                break;
                            default:
                                this.Valid = false;
                                Log?.Error($"{file}: unknown symbol found: {child.Children[0].Value}");
                                break;
                        }
                    }
                    else
                    {
                        if(child.Symbol != "assignment")
                            Log?.Error("{file}: node is not an assignment");
                        if(child.Children.Count == 3 && child.Children[1].Symbol == "OPERATOR" && child.Children[1].Value == "=" && child.Children[0].Symbol == "SYMBOL")
                            Log?.Error("{file}: node is not an assignment with three sub-nodes");
                        this.Valid = false;
                        break;
                    }

                    if (!this.Valid) break;
                }
            }
            else
            {
                this.Valid = false;
                Log?.Error($"{file} does not begin with an assignment list");
            }

            switch (this.SupportedVersion)
            {
                case null when !string.IsNullOrWhiteSpace(this.Version) && this.Version.Contains(".*"):
                    this.SupportedVersion = new SupportedVersion(this.Version);
                    break;
                case null:
                    this.SupportedVersion = new SupportedVersion("1.0.0");
                    break;
            }
            if (!string.IsNullOrWhiteSpace(this.Path) || !string.IsNullOrWhiteSpace(this.Archive)) return;
            this.Valid = false;
            Log?.Debug($"{this.Id} - Path and Archive are blank");
        }

        public void LoadFiles(string basePath)
        {
            var mPath = Path2.Combine(basePath, this.Archive ?? this.Path);

            if (Path2.GetExtension(mPath) == ".zip" && !File.Exists(mPath))
            {
                if (Directory.Exists(Path2.GetDirectoryName(mPath))
                    && File.Exists(Path2.Combine(Path2.GetDirectoryName(mPath), "descriptor.mod")))
                {
                    mPath = Path2.GetDirectoryName(mPath);
                    this.Archive = null;
                    this.Path = mPath;
                }
            }

            if (Path2.GetExtension(mPath) == ".zip" && !File.Exists(mPath))
            {
                this.Valid = false;
                Log?.Debug($"{this.Id} - Archive does not exist");
            }
            else if (Path2.GetExtension(mPath) != ".zip" && !Directory.Exists(mPath))
            {
                this.Valid = false;
                Log?.Debug($"{this.Id} - Path does not exist");
            }

            if (!this.Valid) return;

            if (Path2.GetExtension(mPath) == ".zip")
            {
                var zipInfo = new FileInfo(mPath);
                var workshopNumber = zipInfo.Directory.Name;

                var tempFolder = Path2.Combine(basePath, "extracted_mods");
                if (!Directory.Exists(tempFolder))
                    Directory.CreateDirectory(tempFolder);
                tempFolder = Path2.Combine(tempFolder, workshopNumber);
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                    if (!tempFolder.EndsWith(Path2.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                        tempFolder += Path2.DirectorySeparatorChar;

                    ZipFile.Read(zipInfo.FullName).ExtractAll(tempFolder, ExtractExistingFileAction.OverwriteSilently);
                }
                else
                {
                    if (!tempFolder.EndsWith(Path2.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                        tempFolder += Path2.DirectorySeparatorChar;
                }

                mPath = tempFolder;
            }
            else
            {
                mPath += Path2.DirectorySeparatorChar;
            }

            var paths = Directory.EnumerateFiles(mPath, "*.*", SearchOption.AllDirectories);

            foreach (var path in Directory.EnumerateFiles(mPath, "*.*", SearchOption.AllDirectories))
            {
                if (path.Equals(Path2.Combine(mPath, Path2.GetFileName(path)), StringComparison.OrdinalIgnoreCase)) continue;
                if (!AllowedExtensions.Contains(System.IO.Path.GetExtension(path).ToLower())) continue;

                var refPath = Uri.UnescapeDataString(new Uri(mPath).MakeRelativeUri(new Uri(path)).OriginalString);
                this.Files.Add(new ModFile(refPath, this, path));
            }

            var scFiles = this.Files.Where(x => !x.Valid).ToArray();
            if (scFiles.Length <= 0) return;
            this.Valid = false;
            Log?.Debug($"{this.Id} - Files with errors");
            foreach (var x in scFiles)
            {
                Log?.Debug($"{this.Id} - {x.Path}");
            }
        }
    }
}