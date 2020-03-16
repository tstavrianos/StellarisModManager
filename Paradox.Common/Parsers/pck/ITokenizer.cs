using System.Collections.Generic;

namespace Paradox.Common.Parsers.pck
{
    public interface ITokenizer : IEnumerable<Token>
    {
        void Restart(IEnumerable<char> input);

    }
}
