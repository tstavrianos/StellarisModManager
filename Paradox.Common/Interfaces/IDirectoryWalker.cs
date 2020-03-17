using System.Collections.Generic;
using Serilog;

namespace Paradox.Common.Interfaces
{
    public interface IDirectoryWalker {
        IEnumerable<string> FindFilesInDirectoryTree(string root, string includeFileMask,
            IEnumerable<string> excludedFileNames = null, ILogger logger = null);
    }

}