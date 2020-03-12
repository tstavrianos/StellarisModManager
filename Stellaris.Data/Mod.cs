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
using Stellaris.Data.Parsers.Models;
using Stellaris.Data.Parsers.Tokenizer;
using Array = Stellaris.Data.Parsers.Models.Array;
using String = Stellaris.Data.Parsers.Models.String;

namespace Stellaris.Data
{

    public sealed class Mod
    {
        private static readonly string[] AllowedExtensions = { ".gfx", ".gui", ".txt", ".asset" };
        internal static readonly Logger Log;

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

        private readonly Config _config;

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
            try
            {
                var lexer = new SimpleRegexTokenizer();
                var tokens = lexer.Tokenize(text);
                var parser = new Parser();
                this._config = parser.Parse(tokens);
            }
            catch (Exception ex)
            {
                Log?.Error(ex, "Mod");
                this.Valid = false;
            }

            if (!this.Valid) return;
            this.Tags = new List<string>();
            this.Dependencies = new List<string>();
            foreach (var a in this._config)
            {
                var key = (a.Field as String)?.Value;
                var value = a.Value;
                switch (key)
                {
                    case "name" when value is String s:
                        this.Name = s.Value;
                        break;
                    case "archive" when value is String s:
                        this.Archive = s.Value;
                        break;
                    case "path" when value is String s:
                        this.Path = s.Value;
                        break;
                    case "picture" when value is String s:
                        this.Picture = s.Value;
                        break;
                    case "remote_file_id" when value is String s:
                        this.RemoteFileId = s.Value;
                        break;
                    case "supported_version" when value is String s:
                        this.SupportedVersion = new SupportedVersion(s.Value);
                        break;
                    case "version" when value is String s:
                        this.Version = s.Value;
                        break;
                    case "replace_path" when value is String s:
                        this._replacePath = s.Value;
                        break;
                    case "tags" when value is Array s:
                        foreach (var entry in s)
                        {
                            if(entry is String s1) this.Tags.Add(s1.Value);
                        }
                        break;
                    case "dependencies" when value is Array s:
                        foreach (var entry in s)
                        {
                            if(entry is String s1) this.Dependencies.Add(s1.Value);
                        }
                        break;
                    default:
                        Log.Debug($"{a}, {key}");
                        break;
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

                    /*using (var archive = ZipFile.OpenRead(mPath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            // Gets the full path to ensure that relative segments are removed.
                            var destinationPath =
                                Path2.GetFullPath(Path2.Combine(tempFolder, entry.FullName));

                            if (!System.IO.Directory.Exists(Path2.GetDirectoryName(destinationPath)))
                            {
                                Directory.CreateDirectory(Path2.GetDirectoryName(destinationPath));
                            }
                            // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                            // are case-insensitive.
                            if (destinationPath.StartsWith(tempFolder, StringComparison.Ordinal))
                                entry.ExtractToFile(destinationPath);
                        }
                    }*/
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