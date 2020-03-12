//https://github.com/Vanlightly/DslParser

using System.Collections.Generic;

namespace Stellaris.Data.Parsers.Tokenizer
{
    public interface ITokenizer
    {
        IReadOnlyList<Token> Tokenize(string queryDsl);
    }
}