using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Newtonsoft.Json;
using Serilog;

namespace Paradox.Common
{
    /// <summary>
    /// Helper methods for working with mods and getting the mod files into a state where they can be used by the rest of the parser system whether they are in folders or zipped. 
    /// </summary>
    public static class ModDirectoryHelper {
        /// <summary>
        /// Load details of all mods that have been registered with Stellaris.  All mods listed in the game launcher have entries in Stellaris user data directory that can be parsed.
        /// </summary>
        /// <param name="stellarisUserDirectory">The path to the Stellaris User Data Directory.  Usually [Documents Folder]/Paradox Interactive/Stellaris</param>
        /// <returns>A list of <see cref="ModDefinitionFile"/>s, one for each mod descriptor in the users data directory</returns>
        public static IEnumerable<ModDefinitionFile> LoadModDefinitions(string stellarisUserDirectory, bool continueOnError = false, ILogger logger = null) {
            var directoryInfo = new DirectoryInfo(Path.Combine(stellarisUserDirectory, "mod"));
            var modfiles = directoryInfo.GetFiles("*.mod");
            var cwParserHelper = new CwParserHelper(logger);
            var modFiles = cwParserHelper.ParseParadoxFiles(modfiles.Select(x => x.FullName), continueOnError);
            return modFiles.Select(x => new ModDefinitionFile(x.Key, stellarisUserDirectory, x.Value)).ToList();
        }

        /// <summary>
        /// Convert the <see cref="ModDefinitionFile"/>s into a single file of all mods that can be configured for load.
        /// </summary>
        /// <param name="modInfoFilePath">Full path to write the mod info file to.</param>
        /// <param name="modFiles">The <see cref="ModDefinitionFile"/>s to write out.</param>
        public static void WriteModInfoFile(string modInfoFilePath, IEnumerable<ModDefinitionFile> modFiles) {
            var list = modFiles.Select(x => new ModInfo
            {
                Name = string.Join("_", x.Name.Split(Path.GetInvalidFileNameChars())),
                NameRaw = x.Name,
                ArchiveFilePath = x.Archive,
                ModDirectoryPath = x.Path
            }).ToList();
            list.Sort(Extensions.Create<ModInfo>(x => x.Name));
            var serializer = new JsonSerializer {Formatting = Formatting.Indented};
            using (JsonWriter writer = new JsonTextWriter(new StreamWriter(modInfoFilePath)))
            {
                serializer.Serialize(writer, list);
            }
        }

        /// <summary>
        /// Reads a file of <see cref="ModInfo"/>s from disk. 
        /// </summary>
        /// <param name="modInfoFilePath">Full path to read the mod info file from.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ModInfo"/>s</returns>
        public static List<ModInfo> ReadModInfoFile(string modInfoFilePath) {
            var serializer = new JsonSerializer();
            using (var file = File.OpenText(modInfoFilePath))
            {
                return (List<ModInfo>)serializer.Deserialize(file, typeof(List<ModInfo>));
            }
        }

        /// <summary>
        /// Create <see cref="StellarisDirectoryHelper"/>s for configured mods.  The <see cref="ModInfo"/>s will be filtered to only ones that have set <see cref="ModInfo#Include"/>.
        /// Mods that are unpacked directories will be directly referenced.
        /// Mods that are archived (e.g. Steam Workshop) will be unpacked into the a directory in the temp area.  (<see cref="Path#GetTempPath"/>)
        /// </summary>
        /// <remarks>
        /// <para>This is the thing that requires Ionic.Zip</para>
        /// <para>
        /// Recommend <see cref="forceOverride"/> to be true for the first time you run in a "set" to pick up any changes from mods that have been updated, otherwise while it is <c>false</c> mod
        /// updates will be ignored unless your temp directory is cleared regularly.
        /// Once you have got the latest versions in a session then it is worth setting <c>false</c> for the speed improvement of not unpacking everything.
        /// </para>
        /// </remarks>
        /// <param name="mods">The <see cref="ModInfo"/>s to process</param>
        /// <param name="forceOverride">When extracting a mod from zip, whether to override it if the temp folder already exists.  Defaults to <c>false</c></param>
        /// <returns><see cref="StellarisDirectoryHelper"/>s for the included mods</returns>
        public static IList<StellarisDirectoryHelper> CreateDirectoryHelpers(IEnumerable<ModInfo> mods, bool forceOverride = false, ILogger logger = null) {
            return mods.Where(x => x.Include).Select(x => CreateDirectoryHelper(x, forceOverride, logger)).ToList();
        }

        private static StellarisDirectoryHelper CreateDirectoryHelper(ModInfo modInfo, bool forceOverride = false, ILogger logger = null) {
            var path = modInfo.ArchiveFilePath ?? modInfo.ModDirectoryPath;
            var modName = modInfo.Name;
            var modGroup = modInfo.ModGroup ?? modName;
            if (!IsArchiveFile(path) && !IsSteamWorkshopModDirectory(path)) {
                return new StellarisDirectoryHelper(path, modGroup);
            }

            FileInfo zipInfo;
            string workshopNumber;
            if (IsArchiveFile(path)) {
                zipInfo = new FileInfo(path);
                workshopNumber = zipInfo.Directory.Name;
            }
            else {
                var directoryInfo = new DirectoryInfo(path);
                var zipFiles = directoryInfo.GetFiles("*.zip");
                if (zipFiles.Length > 1) {
                    throw new Exception("Path " + path + " was determined to be a steam workshop file, but contained multiple zip files " + zipFiles.Select(x => x.Name));
                }

                zipInfo = zipFiles.First();
                workshopNumber = directoryInfo.Name;
            }
           
            var tempFolder = Path.Combine(Path.GetTempPath(), workshopNumber, modName);

            if (Directory.Exists(tempFolder)) {
                if (forceOverride) {
                    Directory.Delete(tempFolder, true);
                    Directory.CreateDirectory(tempFolder);
                    try {
                        ZipFile.Read(zipInfo.FullName).ExtractAll(tempFolder, ExtractExistingFileAction.OverwriteSilently);
                    }
                    catch (Exception e) {
                        throw new Exception("Unable to process: " + zipInfo.FullName, e);
                    }
                }
                else {
                    logger?.Debug("Directory {dir} exists, skipping zip extraction as overwrite is off", tempFolder);
                }
            }
            else {
                Directory.CreateDirectory(tempFolder);
                try {
                    ZipFile.Read(zipInfo.FullName).ExtractAll(tempFolder, ExtractExistingFileAction.OverwriteSilently);
                }
                catch (Exception e) {
                    throw new Exception("Unable to process: " + zipInfo.FullName, e);
                }
            }
            
            return new StellarisDirectoryHelper(tempFolder, modGroup);
        }
        
        /// <summary>
        /// Given a directory, determine if it is a steam workshop mod directory or an extracted mod directory.
        /// </summary>
        /// <param name="path">The directory path</param>
        /// <returns><c>true</c> if this directory is a steam workshop mod directory</returns>
        /// <remarks>
        /// This is hardly a perfect check, looking to see if it contains a single file with a .zip extension.
        /// </remarks>
        public static bool IsSteamWorkshopModDirectory(string path) {
            if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory)) {
                return false;
            }
            var directoryInfo = new DirectoryInfo(path);
            var zipFiles = directoryInfo.GetFiles("*.zip");
            if (!zipFiles.Any()) {
                return false;
            }
            var files = directoryInfo.GetFiles();
            return zipFiles.Length == files.Length;
        }
        
        /// <summary>
        /// Determine if the given path is to a zip archive.
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns><c>true</c> if the path is to a zip file</returns>
        public static bool IsArchiveFile(string path) {
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory)) {
                return false;
            }

            var fileInfo = new FileInfo(path);
            return fileInfo.Extension.ToLowerInvariant() == ".zip";
        }
        
    }
}