using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Serilog;
using Serilog.Core;
using Stellaris.Data.Json;
using Stellaris.Data.Parser;

namespace Stellaris.Data
{
    public sealed class Mod
    {
        internal static readonly Logger Logger;

        static Mod()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("parser.log")
                .CreateLogger();
        }

        public Mod(IReadOnlyDictionary<string, IParsedEntry> parsed)
        {
            foreach (var (name, entry) in parsed)
            {
                switch (entry)
                {
                    case ParsedStringEntry stringEntry when string.Equals(name, "name", StringComparison.OrdinalIgnoreCase):
                        this.Name = stringEntry.Value;
                        break;
                    case ParsedStringEntry stringEntry when string.Equals(name, "picture", StringComparison.OrdinalIgnoreCase):
                        this.Picture = stringEntry.Value;
                        break;
                    case ParsedStringEntry stringEntry when string.Equals(name, "supported_version", StringComparison.OrdinalIgnoreCase):
                        this.SupportedVersion = stringEntry.Value;
                        break;
                    case ParsedStringEntry stringEntry when string.Equals(name, "path", StringComparison.OrdinalIgnoreCase):
                        this.Path = stringEntry.Value;
                        break;
                    case ParsedStringEntry stringEntry when string.Equals(name, "archive", StringComparison.OrdinalIgnoreCase):
                        this.Archive = stringEntry.Value;
                        break;
                    case ParsedStringEntry stringEntry when string.Equals(name, "remote_file_id", StringComparison.OrdinalIgnoreCase):

                        if (!long.TryParse(stringEntry.Value, out var id))
                        {
                            this.Valid = false;
                        }
                        
                        this.RemoteFileId = id;
                        break;
                    case ParsedStringEntry stringEntry when string.Equals(name, "version", StringComparison.OrdinalIgnoreCase):
                        this.Version = stringEntry.Value;
                        break;
                    case ParsedListEntry listEntry when string.Equals(name, "tags", StringComparison.OrdinalIgnoreCase):
                        this.Tags = listEntry.Values;
                        break;
                    case ParsedListEntry listEntry when string.Equals(name, "dependencies", StringComparison.OrdinalIgnoreCase):
                        this.Dependencies = listEntry.Values;
                        break;
                    default:
                        Logger.Information($"Name = {name}, String: {entry is ParsedStringEntry}, List: {entry is ParsedListEntry}");
                        break;
                }
            }
        }
        
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Picture { get; private set; }
        public string SupportedVersion { get; private set; }
        public string Path { get; private set; }
        public string Archive { get; private set; }
        public long RemoteFileId { get; private set; }
        public string Version { get; private set; }
        public IList<string> Tags { get; private set; }
        public IList<string> Dependencies { get; private set; }
        public string Key => $"mod/{this.Id}";
        public bool Valid { get; private set; }
        public string File { get; private set; }
        public IEnumerable<ModFile> Files => this._files;
        private List<ModFile> _files;
        private bool _validated;

        public void Validate(string file)
        {
            this.File = file;
            this.Id = System.IO.Path.GetFileName(file);
            this.Valid = true;
            this._files = new List<ModFile>();
            this._validated = false;            
            if (this._validated) return;
            this._validated = true;
            this._files.Clear();
            if (string.IsNullOrWhiteSpace(this.Path) && string.IsNullOrWhiteSpace(this.Archive)) this.Valid = false;
            var basePath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(this.File));
            var mPath = System.IO.Path.Combine(basePath, this.Archive ?? this.Path);
            if (System.IO.Path.GetExtension(mPath) == ".zip" && !System.IO.File.Exists(mPath))
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(mPath))
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
            }
            else if (System.IO.Path.GetExtension(mPath) != ".zip" && !Directory.Exists(mPath))
            {
                this.Valid = false;
            }

            if (!this.Valid) return;
            if (System.IO.Path.GetExtension(mPath) == ".zip")
            {
                using (var zipFile = new ZipFile(mPath))
                {
                    foreach (var relativePath in zipFile.OfType<ZipEntry>().Where(item =>
                        string.Compare(item.Name, "descriptor.mod", StringComparison.OrdinalIgnoreCase) != 0).Select(item => item.Name.Replace('/', '\\')).Where(relativePath => relativePath.Contains('\\')))
                    {
                        this._files.Add(new ModFile(relativePath, this));
                    }
                }
            }
            else
            {
                mPath += System.IO.Path.DirectorySeparatorChar;
                var paths = Directory.EnumerateFiles(mPath, "*.*", SearchOption.AllDirectories);
                foreach (var relativePath in (from path in paths where string.Compare(System.IO.Path.GetFileName(path), "descriptor.mod", StringComparison.OrdinalIgnoreCase) != 0 select Uri.UnescapeDataString(new Uri(mPath).MakeRelativeUri(new Uri(path)).OriginalString)).Select(refPath => refPath.Replace('/', '\\')).Where(relativePath => relativePath.Contains('\\')))
                {
                    this._files.Add(new ModFile(relativePath, this));
                }
            }
        }

        public bool Matches(ModsRegistryEntry modsRegistryEntry)
        {
            if (!string.IsNullOrEmpty(modsRegistryEntry.GameRegistryId))
                return modsRegistryEntry.GameRegistryId.Equals(this.Key, StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(modsRegistryEntry.SteamId) && this.RemoteFileId > 0)
            {
                return modsRegistryEntry.SteamId.Equals($"{this.RemoteFileId}", StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(modsRegistryEntry.DisplayName, this.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}