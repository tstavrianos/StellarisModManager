namespace Paradox.Common
{
    /// <summary>
    /// Mod config class to allow for a file based config of what mods will be loaded and their names / groupings. 
    /// </summary>
    public sealed class ModInfo {
        /// <summary>
        /// The Mods name, that may have been sanitised to remove characters that cannot appear in directory paths.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The mods complete name as it appears in Stellaris.
        /// </summary>
        public string NameRaw { get; set; }
        /// <summary>
        /// A user specified "Grouping" for mods that can be used in the UI / later processing.
        /// For example, the Zenith of Fallen Empires mod has numerous sub mods that I wanted to display all in one go on my tech tree with one checkbox.
        /// If no group is specified, the system will internally use the mods name, so it will be a single-mod group.
        /// </summary>
        public string ModGroup { get; set; }
        /// <summary>
        /// The path to the zip archive if the mod is zipped.  e.g. A Steam Workshop mod.
        /// <c>null</c> in the case of an unpacked directory mod.
        /// </summary>
        public string ArchiveFilePath { get; set; }
        /// <summary>
        /// The path to the mods root directory if it is an unpacked directory.   e.g. A mod you have created using the Stellaris launcher.
        /// <c>null</c> in the vase of an archived mod.
        /// </summary>
        public string ModDirectoryPath { get; set; }
        /// <summary>
        /// User specified flag for if the mod should be processed.  Defaults to false.
        /// </summary>
        public bool Include { get; set; }
    }
}