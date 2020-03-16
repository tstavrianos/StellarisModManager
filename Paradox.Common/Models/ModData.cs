using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using Ionic.Zip;
using Paradox.Common.Json;
using Paradox.Common.Parsers;
using Paradox.Common.Parsers.pck;
using ReactiveUI;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Path2 = System.IO.Path;

namespace Paradox.Common.Models
{

    public sealed class ModData: ReactiveObject
    {
        //private static readonly string[] AllowedExtensions = { ".gfx", ".gui", ".txt", ".asset" };
        internal static readonly Logger Log;

        private static string TrimQuotes(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 3 || value[0] != '"' ||
                value[^1] != '"') return value;
            return value.Substring(1, value.Length - 2);
        }
        static ModData()
        {
#if DEBUG
            Log = new LoggerConfiguration()//
                .MinimumLevel.Debug()//
                .Enrich.WithExceptionDetails()//
                .Enrich.FromLogContext()//
                .WriteTo.File("ModData.log")//
                .CreateLogger();//
#endif
        }

        //private readonly ParseNode _tree;
        private string _id;
        private string _name;
        private string _archive;
        private string _path;
        private string _picture;
        private bool _valid;
        private string _remoteFileId;
        private string _version;
        private bool _isChecked;
        private bool _outdated;
        private int _originalSpot;

        public bool IsChecked
        {
            get => this._isChecked;
            set => this.RaiseAndSetIfChanged(ref this._isChecked, value);
        }

        public bool Outdated
        {
            get => this._outdated;
            set => this.RaiseAndSetIfChanged(ref this._outdated, value);
        }

        public int OriginalSpot
        {
            get => this._originalSpot;
            set => this.RaiseAndSetIfChanged(ref this._originalSpot, value);
        }

        public ModsRegistryEntry Entry { get; }

        public string Id
        {
            get => this._id;
            private set => this.RaiseAndSetIfChanged(ref this._id, value);
        }

        public string Key => $"mod/{this.Id}";

        public string Name
        {
            get => this._name;
            set => this.RaiseAndSetIfChanged(ref this._name, value);
        }

        //public List<ModDataFile> Files { get; } = new List<ModDataFile>();

        public List<string> Tags { get; }
        public List<string> Dependencies { get; }

        public string Archive
        {
            get => this._archive;
            private set => this.RaiseAndSetIfChanged(ref this._archive, value);
        }

        public string Path
        {
            get => this._path;
            private set => this.RaiseAndSetIfChanged(ref this._path, value);
        }

        public string Picture
        {
            get => this._picture;
            set => this.RaiseAndSetIfChanged(ref this._picture, value);
        }

        public bool Valid
        {
            get => this._valid;
            private set => this.RaiseAndSetIfChanged(ref this._valid, value);
        }

        public string RemoteFileId
        {
            get => this._remoteFileId;
            set => this.RaiseAndSetIfChanged(ref this._remoteFileId, value);
        }

        public SupportedVersion SupportedVersion { get; set; }

        public string Version
        {
            get => this._version;
            set => this.RaiseAndSetIfChanged(ref this._version, value);
        }

        private string _replacePath;

        public string ReplacePath
        {
            get => this._replacePath;
            set => this.RaiseAndSetIfChanged(ref this._replacePath, value);
        }

        private static string Strip(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            if (value.Length <= 2) return value;
            if (value[0] == '"' && value[^1] == '"')
                return value.Substring(1, value.Length - 2);
            return value;
        }

        private static ParseNode Parse(string file)
        {
            var text = File.ReadAllText(file);
            var lexer = new Tokenizer(text);
            var parser = new Parser(lexer);
            return parser.ParseReductions(true);
        }

        private void ParseChildNode(ParseNode child)
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
                        this.ReplacePath = TrimQuotes(child.Children[2].Value);
                        break;
                    case "tags" when child.Children[2].Symbol == "array":
                        this.Tags.AddRange(child.Children[2].Children.Select(x => TrimQuotes(x.Value)));
                        break;
                    case "dependencies" when child.Children[2].Symbol == "array":
                        this.Dependencies.AddRange(child.Children[2].Children.Select(x => TrimQuotes(x.Value)));
                        break;
                    default:
                        this.Valid = false;
                        Log?.Error($"{this.Id}: unknown symbol found: {child.Children[0].Value}");
                        break;
                }
            }
            else
            {
                if(child.Symbol != "assignment")
                    Log?.Error($"{this.Id}: node is not an assignment");
                if(child.Children.Count == 3 && child.Children[1].Symbol == "OPERATOR" && child.Children[1].Value == "=" && child.Children[0].Symbol == "SYMBOL")
                    Log?.Error($"{this.Id}: node is not an assignment with three sub-nodes");
                this.Valid = false;
            }
        }

        public ModData(string file, ModsRegistryEntry entry = null)
        {
            this.Id = Path2.GetFileName(file);
            this.Valid = true;
            this.Tags = new List<string>();
            this.Dependencies = new List<string>();
            this.Entry = entry;
            if(!string.IsNullOrWhiteSpace(entry?.DisplayName))
                this.Name = entry.DisplayName;
            if(!string.IsNullOrWhiteSpace(entry?.SteamId))
                this.RemoteFileId = entry.SteamId;
            if(entry?.Tags != null)
                this.Tags.AddRange(entry.Tags);
            if(!string.IsNullOrWhiteSpace(entry?.RequiredVersion))
                this.SupportedVersion = new SupportedVersion(entry.RequiredVersion);
            if(!string.IsNullOrWhiteSpace(entry?.ArchivePath))
                this.Archive = entry.ArchivePath;
            if(!string.IsNullOrWhiteSpace(entry?.DirPath))
                this.Path = entry.DirPath;

            var tree = Parse(file);

            if (tree.Symbol == "assignmentList")
            {
                foreach (var child in tree.Children)
                {
                    this.ParseChildNode(child);
                    if (!this.Valid) break;
                }
            }
            else
            {
                this.Valid = false;
                Log?.Error($"{this.Id} does not begin with an assignment list");
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

            if (string.IsNullOrWhiteSpace(this.Path) && string.IsNullOrWhiteSpace(this.Archive))
            {
                this.Valid = false;
                Log?.Debug($"{this.Id} - Path and Archive are blank");
            }

            if (this.Entry != null) return;
            this.Entry = new ModsRegistryEntry
            {
                Id = Guid.NewGuid().ToString(),
                Source = !string.IsNullOrWhiteSpace(this.Path)
                         && (this.Path.StartsWith(
                                 Path2.GetDirectoryName(file),
                                 StringComparison.OrdinalIgnoreCase)
                             || this.Path.StartsWith("mod/", StringComparison.OrdinalIgnoreCase))
                    ? SourceType.Local
                    : SourceType.Steam,
                Status = this.Valid ? StatusType.ReadyToPlay : StatusType.InvalidMod,
                SteamId = this.RemoteFileId,
                DisplayName = this.Name,
                GameRegistryId = this.Key,
                RequiredVersion = this.SupportedVersion.ToString(),
                Tags = new List<string>(this.Tags)
            };
            if (!string.IsNullOrWhiteSpace(this.Archive))
            {
                this.Entry.ArchivePath = this.Archive;
            }

            if (!string.IsNullOrWhiteSpace(this.Path)) this.Entry.DirPath = this.Path;
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

            /*if (!this.Valid) return;

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

            foreach (var path in Directory.EnumerateFiles(mPath, "*.*", SearchOption.AllDirectories))
            {
                if (path.Equals(Path2.Combine(mPath, Path2.GetFileName(path)), StringComparison.OrdinalIgnoreCase)) continue;
                if (!AllowedExtensions.Contains(System.IO.Path.GetExtension(path).ToLower())) continue;

                var refPath = Uri.UnescapeDataString(new Uri(mPath).MakeRelativeUri(new Uri(path)).OriginalString);
                this.Files.Add(new ModDataFile(refPath, this, path));
            }

            var scFiles = this.Files.Where(x => !x.Valid).ToArray();
            if (scFiles.Length <= 0) return;
            this.Valid = false;
            Log?.Debug($"{this.Id} - Files with errors");
            foreach (var x in scFiles)
            {
                Log?.Debug($"{this.Id} - {x.Path}");
            }*/
        }
    }
}