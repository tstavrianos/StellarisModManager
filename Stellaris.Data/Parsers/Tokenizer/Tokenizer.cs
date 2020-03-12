using System.Collections.Generic;

namespace Stellaris.Data.Parsers.Tokenizer
{
    public class Tokenizer : ITokenizer
    {
        private StringSegment _source;
        private int _start;
        private int _current;
        private int _line;
        private int _column;
        private readonly List<Token> _tokens = new List<Token>();

        public IReadOnlyList<Token> Tokenize(string queryDsl)
        {
            this._source = queryDsl;
            this._start = 0;
            this._current = 0;
            this._line = 1;
            this._column = 1;
            this._tokens.Clear();

            while (!this.IsAtEnd())
            {
                this._start = this._current;
                this.ScanToken();
            }

            this._tokens.Add(new Token(TokenType.SequenceTerminator, ""));

            return this._tokens;
        }

        private void ScanToken()
        {
            var c = this.Advance();

            switch (c)
            {
                case '#':
                    while (this.Peek() != '\n' && !this.IsAtEnd()) this.Advance();
                    break;
                case '{':
                    this.AddToken(TokenType.LeftBracket); break;
                case '}':
                    this.AddToken(TokenType.RightBracket); break;
                case '=':
                    this.AddToken(TokenType.Specifier); break;
                case '<':
                    if (!this.Match('=')) this.Match('>');
                    this.AddToken(TokenType.Specifier); break;
                case '>':
                    this.Match('=');
                    this.AddToken(TokenType.Specifier); break;
                case '\n':
                    this._line++;
                    this._column = 1;
                    break;
                default:
                    if (char.IsWhiteSpace(c))
                    {
                        while (char.IsWhiteSpace(this.Peek())) this.Advance();
                    }
                    else if (c == '@' && this.Peek() == '\\' && this.PeekNext() == '[')
                    {
                        while (this.Peek() != ']' && !this.IsAtEnd()) this.Advance();
                        if(this.IsAtEnd()) 
                            throw new ParserException($"Line: {this._line}, Unterminated metaprogramming token");
                        this.Advance();
                        this.AddToken(TokenType.String);
                    }
                    else if (AllowIntegerStart(c))
                    {
                        this.Number(c == '-');
                    }
                    else if (AllowedStringStart(c))
                    {
                        this.String(c == '"');
                    }
                    else
                    {
                        throw new ParserException($"{this._line}:{this._column} Unexpected token: {c}, {(int)c}");
                    }
                    break;
            }
        }

        private void Number(bool force)
        {
            while (char.IsDigit(this.Peek())) this.Advance();
            
            if (this.Peek() == '.' && char.IsDigit(this.PeekNext()))
            {
                this.Advance();
                while (char.IsDigit(this.Peek())) this.Advance();
                this.AddToken(TokenType.Real);
            }
            else if (AllowedUnquotedString(this.Peek()))
            {
                this.String(false);
            } else if (this.Peek() == '%')
            {
                this.AddToken(TokenType.Percent);
            }
            else
            {
                this.AddToken(TokenType.Integer);
            }
        }

        private void String(bool quoted)
        {
            if (quoted)
            {
                while (!this.IsAtEnd())
                {
                    if (this.Peek() == '\\' && this.PeekNext() == '"')
                    {
                        this.Advance();
                        this.Advance();
                    }

                    if (this.Peek() == '"') break;
                    if (this.Peek() == '\n')
                    {
                        throw new ParserException($"{this._line}:{this._column} Line ending in string");
                    }

                    this.Advance();
                }
                if (this.Peek() != '"')
                {
                    throw new ParserException($"{this._line}:{this._column} No closing quote for string");
                }

                // The closing ".
                this.Advance();
            }
            else
            {
                while (AllowedUnquotedString(this.Peek()) && !this.IsAtEnd())
                {
                    this.Advance();
                }
            }

            this.AddToken(TokenType.String);
        }

        private static bool AllowIntegerStart(char c)
        {
            return c == '-' || char.IsDigit(c) || c == '+';
        }
        
        private static bool AllowedStringStart(char c)
        {
            return char.IsDigit(c) || char.IsLetter(c) || c == '"' || c == '@' || c == '$' || c == '?';
        }

        private static bool AllowedUnquotedString(char c)
        {
            return AllowedStringStart(c) || c == ':' || c == '@' || c == '.' || c == '%' || c =='-' || c == '_' || c == '\'' || c == '$' || c == '|';
        } 
        
        private bool Match(char expected)
        {
            if (this.IsAtEnd())
            {
                return false;
            }

            if (this._source[this._current] != expected)
            {
                return false;
            }

            this._current++;
            return true;
        }

        private char Peek()
        {
            return this.IsAtEnd() ? '\0' : this._source[this._current];
        }

        private char PeekNext()
        {
            return this._current + 1 >= this._source.Length ? '\0' : this._source[this._current + 1];
        }

        private bool IsAtEnd() => 
            this._current >= this._source.Length;

        private char Advance()
        {
            this._current++;
            this._column++;
            return this._source[this._current - 1];
        }

        private void AddToken(TokenType type)
        {
            this._tokens.Add(new Token(type, this._source.Subsegment(this._start, this._current - this._start), this._line));
        }
    }
}