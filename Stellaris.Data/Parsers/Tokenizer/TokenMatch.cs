//https://github.com/Vanlightly/DslParser

namespace Stellaris.Data.Parsers.Tokenizer
{
    public sealed class TokenMatch
    {
        public bool IsMatch { get; set; }
        public TokenType TokenType { get; set; }
        public StringSegment Value { get; set; }
        public StringSegment RemainingText { get; set; }
    }
}