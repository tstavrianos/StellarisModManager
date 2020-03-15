using System.Collections.Generic;

namespace Stellaris.Data.Parsers.pck
{
    public interface ITokenizer : IEnumerable<Token>
    {
        void Restart(IEnumerable<char> input);

    }
}
