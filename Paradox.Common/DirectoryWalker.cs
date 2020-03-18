using System.Collections.Generic;
using System.IO;
using System.Linq;
using Paradox.Common.Interfaces;
using Splat;

namespace Paradox.Common
{
     /// <summary>
    /// Helper for finding files in nested directory structures.
    /// </summary>
    public sealed class DirectoryWalker : IDirectoryWalker, IEnableLogger
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
        private IEnumerable<FileInfo> FindFilesInDirectoryTree(string root, string includeFileMask, IEnumerable<string> excludedFileNames) {
            excludedFileNames = excludedFileNames.NullToEmpty();
            var result = new List<FileInfo>();
            this.FindFilesInDirectoryTree(new DirectoryInfo(root), result, includeFileMask, excludedFileNames);
            return result;
        }
        
        public IEnumerable<FileInfo> FindFilesInDirectoryTree(string root, string includeFileMask)
        {
            return this.FindFilesInDirectoryTree(root, includeFileMask, null);
        }

        private void FindFilesInDirectoryTree(DirectoryInfo root, ICollection<FileInfo> fileInfos, string fileMask, IEnumerable<string> excludedFileNames)
        {
            FileInfo[] files = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles(fileMask);
            }
            catch (DirectoryNotFoundException e)
            {
                this.Log().Error(e.Message);
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
                this.FindFilesInDirectoryTree(dirInfo, fileInfos, fileMask, excludedFileNames);
            }
        }

        IEnumerable<string> IDirectoryWalker.FindFilesInDirectoryTree(string root, string includeFileMask, IEnumerable<string> excludedFileNames) {
            return this.FindFilesInDirectoryTree(root, includeFileMask, excludedFileNames).Select(x => x.FullName).ToList();
        }
    }
}