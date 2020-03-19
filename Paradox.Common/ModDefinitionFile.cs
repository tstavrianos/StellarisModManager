using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Path2 = System.IO.Path;
using Ionic.Zip;

namespace Paradox.Common
{
    /// <summary>
    /// Accessor for Stellaris Mod Definition files
    /// </summary>
    public sealed class ModDefinitionFile
    {
        private static readonly string[] FileExtensions = { ".gfx", ".gui", ".txt" };
        private readonly string _stellarisDataPath;
        private readonly CwNode _node;

        /// <summary>
        /// Get the full path to the mod definition file
        /// </summary>
        public string ModDefinitionFilePath { get; }

        /// <summary>
        /// Get the Mods name
        /// </summary>
        public string Name => this._node.GetKeyValue("name");

        /// <summary>
        /// Get the path to the mod archive for steam workshop mods.  This may be <c>null</c> if the mod is an extracted folder (e.g. created using the launcher) in which case you should use <see cref="Path"/>
        /// </summary>
        public string Archive => this._node.GetKeyValue("archive");

        /// <summary>
        /// Get the path to the mod directory for local mods.  This may be <c>null</c> if the mod is an archive (e.g. steam workshop mod) in which case you should use <see cref="Archive"/>
        /// </summary>
        public string Path => this._node.GetKeyValue("path") != null ? System.IO.Path.Combine(this._stellarisDataPath, this._node.GetKeyValue("path")) : null;

        public IList<string> Tags => this._node.GetNode("tags")?.Values;

        public IList<string> Dependencies => this._node.GetNode("dependencies")?.Values;

        public string Picture => this._node.GetKeyValue("picture");

        public string RemoteFileId => this._node.GetKeyValue("remote_file_id");

        public string Version => this._node.GetKeyValue("version");

        public string SupportedVersion => this._node.GetKeyValue("supported_version");

        public string ReplacePath => this._node.GetKeyValue("replace_path");

        public string Key => $"mod/{System.IO.Path.GetFileName(this.ModDefinitionFilePath)}";

        public IEnumerable<ModDataFile> ModifiedFiles { get; }

        internal ModDefinitionFile(string modDefinitionFilePath, string stellarisDataPath, CwNode node)
        {
            this.ModDefinitionFilePath = modDefinitionFilePath;
            this._stellarisDataPath = stellarisDataPath;
            this._node = node;
            this.ModifiedFiles = this.LoadFiles(stellarisDataPath);
        }

        private IEnumerable<ModDataFile> LoadFiles(string basePath)
        {
            var ret = new List<ModDataFile>();
            var mPath = Path2.Combine(basePath, this.Archive ?? this.Path);

            if (Path2.GetExtension(mPath) == ".zip")
            {
                if (!File.Exists(mPath)) return ret;

                using var zipFile = ZipFile.Read(new FileInfo(mPath).FullName);
                foreach (var item in zipFile.Entries)
                {
                    if (item.IsDirectory) continue;
                    var name = item.FileName;
                    name = name.Replace('\\', '/');
                    if (name.Length > 0 && name[0] == '/') name = name.Substring(1);
                    if (string.Compare(name, "descriptor.mod", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        continue;
                    }

                    if (!FileExtensions.Contains(System.IO.Path.GetExtension(name), StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    ret.Add(new ModDataFile(name, item.InputStream));
                }
            }
            else
            {
                mPath += Path2.DirectorySeparatorChar;
                if (!Directory.Exists(mPath)) return ret;
                var paths = Directory.EnumerateFiles(mPath, "*.*", SearchOption.AllDirectories);
                foreach (var path in paths)
                {
                    if (string.Compare(Path2.GetFileName(path), "descriptor.mod", StringComparison.OrdinalIgnoreCase) ==
                        0) continue;
                    var refPath = Uri.UnescapeDataString(new Uri(mPath).MakeRelativeUri(new Uri(path)).OriginalString);
                    var name = refPath.Replace('\\', '/');
                    if (!FileExtensions.Contains(System.IO.Path.GetExtension(name), StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    using (var stream = File.OpenRead(path))
                    {
                        ret.Add(new ModDataFile(name, stream));
                    }
                }
            }

            return ret;
        }
    }
}