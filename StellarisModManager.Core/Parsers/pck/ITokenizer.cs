using System.Collections.Generic;

namespace StellarisModManager.Core.Parsers.pck
{
    public interface ITokenizer : IEnumerable<Token>
    {
        void Restart(IEnumerable<char> input);

    }
}
