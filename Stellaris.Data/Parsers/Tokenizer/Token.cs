//https://github.com/Vanlightly/DslParser

namespace Stellaris.Data.Parsers.Tokenizer
{
    public readonly struct Token
    {
        /*public Token(TokenType tokenType)
        {
            this.TokenType = tokenType;
            this.Value = new ArraySegment<char>(string.Empty.ToCharArray());
        }*/

        public Token(TokenType tokenType, StringSegment value, int line = 0)
        {
            this.TokenType = tokenType;
            this.Value = value;
            this.Line = line;
        }

        public TokenType TokenType { get; }
        public StringSegment Value { get; }
        
        public int Line { get; }

        public Token Clone()
        {
            return new Token(this.TokenType, this.Value, this.Line);
        }

        public override string ToString()
        {
            return $"{this.TokenType}, {this.Value}";
        }
    }
}