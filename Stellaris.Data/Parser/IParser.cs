using System.Collections.Generic;

namespace Stellaris.Data.Parser
{
    public interface IParser
    {
        IReadOnlyDictionary<string, IParsedEntry> Parse(string file);
    }
}