using System.Collections.Generic;
using System.IO;
using System.Linq;
using Paradox.Common.Interfaces;
using Serilog;

namespace Paradox.Common
{
     /// <summary>
    /// Helper for finding files in nested directory structures.
    /// </summary>
    public sealed class DirectoryWalker : IDirectoryWalker
    {
        /// <summary>
        /// Walks a directory tree and updates the list with  all the files found matching the specified mask.
        /// </summary>
        /// <remarks>
        ///Taken from one of the .Net tutorials in walking file structures.
        /// </remarks>
        /// <param name="root">The path to the directory to start searching</param>
        /// <param name="includeFileMask">A file mask to look for, e.g. *.txt</param>
        /// <param name="excludedFileNames">File names (not mask) to exclude</param>
        /// <returns>All files found anywhere in the directory tree of the <c>root</c> that match the mask and are not on the exclude list</returns>
        public static IEnumerable<FileInfo> FindFilesInDirectoryTree(string root, string includeFileMask, IEnumerable<string> excludedFileNames, ILogger logger = null) {
            excludedFileNames = excludedFileNames.NullToEmpty();
            var result = new List<FileInfo>();
            FindFilesInDirectoryTree(new DirectoryInfo(root), result, includeFileMask, excludedFileNames, logger);
            return result;
        }
        
        public static IEnumerable<FileInfo> FindFilesInDirectoryTree(string root, string includeFileMask, ILogger logger = null)
        {
            return FindFilesInDirectoryTree(root, includeFileMask, null, logger);
        }

        private static void FindFilesInDirectoryTree(DirectoryInfo root, ICollection<FileInfo> fileInfos, string fileMask, IEnumerable<string> excludedFileNames, ILogger logger)
        {
            FileInfo[] files = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles(fileMask);
            }
            catch (DirectoryNotFoundException e)
            {
                logger?.Error(e.Message);
            }

            if (files == null) return;
            foreach (var info in files.Where(fileInfo => excludedFileNames.All(x => x != fileInfo.Name))) {
                fileInfos.Add(info);
            }
            // Now find all the subdirectories under this directory.
            var subDirs = root.GetDirectories();

            foreach (var dirInfo in subDirs)
            {
                // Resursive call for each subdirectory.
                // ReSharper disable once PossibleMultipleEnumeration
                FindFilesInDirectoryTree(dirInfo, fileInfos, fileMask, excludedFileNames, logger);
            }
        }

        IEnumerable<string> IDirectoryWalker.FindFilesInDirectoryTree(string root, string includeFileMask, IEnumerable<string> excludedFileNames, ILogger logger) {
            return FindFilesInDirectoryTree(root, includeFileMask, excludedFileNames, logger).Select(x => x.FullName).ToList();
        }
    }
}