//https://github.com/Vanlightly/DslParser

namespace Stellaris.Data.Parsers.Tokenizer
{
    public enum TokenType
    {
        Comment,
        String,
        LeftBracket,
        RightBracket,
        Specifier,
        Integer,
        Percent,
        Real,
        Date,
        SequenceTerminator,
        Invalid
    }
}