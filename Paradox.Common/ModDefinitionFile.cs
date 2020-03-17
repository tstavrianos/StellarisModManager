using System.Collections.Generic;

namespace Paradox.Common
{
    /// <summary>
    /// Accessor for Stellaris Mod Definition files
    /// </summary>
    public sealed class ModDefinitionFile {
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

        internal ModDefinitionFile(string modDefinitionFilePath, string stellarisDataPath, CwNode node) {
            this.ModDefinitionFilePath = modDefinitionFilePath;
            this._stellarisDataPath = stellarisDataPath;
            this._node = node;
        }
    }
}