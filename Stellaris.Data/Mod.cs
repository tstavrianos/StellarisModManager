using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Antlr4.Runtime;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Stellaris.Data.Antlr;
using Stellaris.Data.Parser;

namespace Stellaris.Data
{
    public sealed class Mod
    {
        private static readonly Logger Log;

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

        private MapEntry _mapEntry;
        
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
            if(string.IsNullOrWhiteSpace(value)) return string.Empty;
            if (value.Length <= 2) return value;
            if (value[0] == '"' && value[value.Length - 1] == '"')
                return value.Substring(1, value.Length - 2);
            return value;
        }

        public Mod(string file)
        {
            this.Id = System.IO.Path.GetFileName(file);
            this.Valid = true;
            
            var text = System.IO.File.ReadAllText(file);
            try
            {
                var lexer = new ParadoxLexer(new AntlrInputStream(text));
                var commonTokenStream = new CommonTokenStream(lexer);
                var parser = new ParadoxParser(commonTokenStream);
                this._mapEntry = parser.config().ToConfigBlock();
            }
            catch (Exception ex)
            {
                Log?.Error(ex, "Mod");
                this.Valid = false;
            }

            if (!this.Valid) return;
            this.Tags = new List<string>();
            this.Dependencies = new List<string>();
            foreach (var (key, members) in this._mapEntry)
            {
                switch (key)
                {
                    case "name":
                        if (members.Count > 0)
                        {
                            if (members.First() is StringEntry s) this.Name = s.Value;
                        }
                        break;
                    case "archive":
                        if (members.Count > 0)
                        {
                            if (members.First() is StringEntry s) this.Archive = s.Value;
                        }
                        break;
                    case "path":
                        if (members.Count > 0)
                        {
                            if (members.First() is StringEntry s) this.Path = s.Value;
                        }
                        break;
                    case "picture":
                        if (members.Count > 0)
                        {
                            if (members.First() is StringEntry s) this.Picture = s.Value;
                        }
                        break;
                    case "remote_file_id":
                        if (members.Count > 0)
                        {
                            if (members.First() is StringEntry s) this.RemoteFileId = s.Value;
                        }
                        break;
                    case "supported_version":
                        if (members.Count > 0)
                        {
                            if (members.First() is StringEntry s) this.SupportedVersion = new SupportedVersion(s.Value);
                        }
                        break;
                    case "version":
                        if (members.Count > 0)
                        {
                            if (members.First() is StringEntry s) this.Version = s.Value;
                        }
                        break;
                    case "replace_path":
                        if (members.Count > 0)
                        {
                            if (members.First() is StringEntry s) this._replacePath = s.Value;
                        }
                        break;
                    case "tags":
                        foreach (var entry in members)
                        {
                            if(entry is StringEntry s) this.Tags.Add(s.Value);
                        }
                        break;
                    case "dependencies":
                        foreach (var entry in members)
                        {
                            if(entry is StringEntry s) this.Dependencies.Add(s.Value);
                        }
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
            var mPath = System.IO.Path.Combine(basePath, this.Archive ?? this.Path);

            if (System.IO.Path.GetExtension(mPath) == ".zip" && !System.IO.File.Exists(mPath))
            {
                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(mPath))
                    && System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(mPath), "descriptor.mod")))
                {
                    mPath = System.IO.Path.GetDirectoryName(mPath);
                    this.Archive = null;
                    this.Path = mPath;
                }
            }

            if (System.IO.Path.GetExtension(mPath) == ".zip" && !System.IO.File.Exists(mPath))
            {
                this.Valid = false;
                Log?.Debug($"{this.Id} - Archive does not exist");
            }
            else if (System.IO.Path.GetExtension(mPath) != ".zip" && !System.IO.Directory.Exists(mPath))
            {
                this.Valid = false;
                Log?.Debug($"{this.Id} - Path does not exist");
            }
            
            if (!this.Valid) return;

            if (System.IO.Path.GetExtension(mPath) == ".zip")
            {
                var zipInfo = new System.IO.FileInfo(mPath);
                var workshopNumber = zipInfo.Directory.Name;
                
                var tempFolder = System.IO.Path.Combine(basePath, "extracted_mods");
                if (!System.IO.Directory.Exists(tempFolder))
                    System.IO.Directory.CreateDirectory(tempFolder);
                tempFolder = System.IO.Path.Combine(tempFolder, workshopNumber);
                if (!System.IO.Directory.Exists(tempFolder))
                {
                    System.IO.Directory.CreateDirectory(tempFolder);
                    if (!tempFolder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                        tempFolder += System.IO.Path.DirectorySeparatorChar;

                    using (var archive = ZipFile.OpenRead(mPath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            // Gets the full path to ensure that relative segments are removed.
                            var destinationPath =
                                System.IO.Path.GetFullPath(System.IO.Path.Combine(tempFolder, entry.FullName));

                            // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                            // are case-insensitive.
                            if (destinationPath.StartsWith(tempFolder, StringComparison.Ordinal))
                                entry.ExtractToFile(destinationPath);
                        }
                    }
                }
                else
                {
                    if (!tempFolder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                        tempFolder += System.IO.Path.DirectorySeparatorChar;
                }

                mPath = tempFolder;
            }
            else
            {
                mPath += System.IO.Path.DirectorySeparatorChar;
            }

            var paths = System.IO.Directory.EnumerateFiles(mPath, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (var modFile in from path in paths where string.Compare(System.IO.Path.GetFileName(path), "descriptor.mod",
                StringComparison.OrdinalIgnoreCase) != 0 let refPath = Uri.UnescapeDataString(new Uri(mPath).MakeRelativeUri(new Uri(path)).OriginalString) select new ModFile(refPath, this, path))
            {
                this.Files.Add(modFile);
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