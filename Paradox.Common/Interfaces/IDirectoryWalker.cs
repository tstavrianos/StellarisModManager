using System.Collections.Generic;

namespace Paradox.Common.Interfaces
{
    public interface IDirectoryWalker {
        IEnumerable<string> FindFilesInDirectoryTree(string root, string includeFileMask,
            IEnumerable<string> excludedFileNames = null);
    }

}