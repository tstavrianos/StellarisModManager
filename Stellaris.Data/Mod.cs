using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using Serilog.Core;
using Stellaris.Data.Parser;

namespace Stellaris.Data
{
    public class Mod
    {
        public string Name { get; }
        public IEnumerable<string> Tags { get; }
        public IEnumerable<string> Dependencies { get; }
        public string Picture { get; }
        public string SupportedVersion { get; }
        public string Version { get; }
        public string Path { get; }
        public string Archive { get; }
        public long RemoteFileId { get; }
        public string Key => $"mod/{this.Id}";
        public string Id { get; }        
        public bool Valid { get; }

        internal static readonly Logger Logger;
        
        static Mod()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("parser.log").CreateLogger();
        }
        
        public Mod(IReadOnlyDictionary<string, IParsedEntry> table, string file)
        {
            this.Valid = true;
            this.Id = System.IO.Path.GetFileName(file);
            foreach (var (name, entry) in table)
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

            if (string.IsNullOrWhiteSpace(this.Path) && string.IsNullOrWhiteSpace(this.Archive)) this.Valid = false;
            var basePath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(file));
            var mPath = System.IO.Path.Combine(basePath, this.Archive ?? this.Path);
            if (System.IO.Path.GetExtension(mPath) == ".zip" && !File.Exists(mPath))
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(mPath))
                    && File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(mPath), "descriptor.mod")))
                {
                    mPath = System.IO.Path.GetDirectoryName(mPath);
                    this.Archive = null;
                    this.Path = mPath;
                }
            }
            
            if (System.IO.Path.GetExtension(mPath) == ".zip" && !File.Exists(mPath))
            {
                this.Valid = false;
            }
            else if (System.IO.Path.GetExtension(mPath) != ".zip" && !Directory.Exists(mPath))
            {
                this.Valid = false;
            }
        }
    }
}