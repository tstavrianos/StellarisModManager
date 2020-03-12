using System;
using System.Collections.Generic;

namespace Stellaris.Data.Parsers.Tokenizer
{
    public class Tokenizer : ITokenizer
    {
        private string source;
        private int start = 0;
        private int current = 0;
        private int line = 1;
        private readonly List<Token> tokens = new List<Token>();

        public IEnumerable<Token> Tokenize(string queryDsl)
        {
            this.source = queryDsl;
            this.start = 0;
            this.current = 0;
            this.line = 1;

            while (!IsAtEnd())
            {
                // We are at the beginning of the next lexeme.
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.SequenceTerminator, ""));

            return this.tokens;
        }

        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case '{': AddToken(TokenType.LeftBracket); break;
                case '}': AddToken(TokenType.RightBracket); break;
                case '=': AddToken(TokenType.Specifier); break;
                case '<':
                    if (!Match('=')) Match('>');
                    AddToken(TokenType.Specifier); break;
                case '>':
                    Match('=');
                    AddToken(TokenType.Specifier); break;
                case '\n':
                    line++;
                    break;
                case '\t':
                case ' ':
                case '\r':
                case '\f':
                case '\v':
                    break;
                default:
                    if (AllowIntegerStart(c))
                    {
                        Number(c == '-');
                    }
                    else if (AllowedStringStart(c))
                    {
                        String(c == '"');
                    }
                    else
                    {
                        //error
                    }
                    break;
            }
        }

        private void Number(bool force)
        {
            while (IsDigit(Peek())) Advance();
            
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                this.Advance();
                while (IsDigit(Peek())) Advance();
                AddToken(TokenType.Real);
            }
            else if (AllowedUnquotedString(this.Peek()))
            {
                String(false);
            } else if (this.Peek() == '%')
            {
                AddToken(TokenType.Percent);
            }
            else
            {
                AddToken(TokenType.Integer);
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
                        //error
                    }

                    this.Advance();
                }
                if (this.Peek() != '"')
                {
                    //error
                }

                // The closing ".
                Advance();
            }
            else
            {
                while (AllowedUnquotedString(this.Peek()) && !this.IsAtEnd())
                {
                    this.Advance();
                }
            }
            AddToken(TokenType.String);
        }

        private static bool AllowIntegerStart(char c)
        {
            return c == '-' || IsDigit(c);
        }
        
        private static bool AllowedStringStart(char c)
        {
            return IsDigit(c) || IsAlpha(c) || c == '"';
        }

        private static bool AllowedUnquotedString(char c)
        {
            return AllowedStringStart(c) || c == ':' || c == '@' || c == '.' || c == '%' || c =='-';
        } 
        
        private bool Match(char expected)
        {
            if (IsAtEnd())
            {
                return false;
            }

            if (source[current] != expected)
            {
                return false;
            }

            current++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd())
            {
                return '\0';
            }

            return source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length)
            {
                return '\0';
            }

            return source[current + 1];
        }
        
        private static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z')
                   ;
        }

        private static bool IsAlphaNumeric(char c) => 
            IsAlpha(c) || IsDigit(c);

        private static bool IsDigit(char c) => 
            c >= '0' && c <= '9';

        private bool IsAtEnd() => 
            current >= source.Length;

        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        private void AddToken(TokenType type)
        {
            tokens.Add(new Token(type, new StringSegment(this.source, this.start, this.current - this.start), line));
        }
    }
}