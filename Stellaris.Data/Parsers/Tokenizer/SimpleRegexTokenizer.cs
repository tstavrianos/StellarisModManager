using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Stellaris.Data.Parsers.Tokenizer
{
    public sealed class SimpleRegexTokenizer : ITokenizer
    {
        private static readonly Regex InvalidRegex;
        private static readonly List<TokenDefinition> TokenDefinitions;

        static SimpleRegexTokenizer()
        {
            InvalidRegex = new Regex("(^\\S+\\s)|^\\S+", RegexOptions.Compiled);
            TokenDefinitions = new List<TokenDefinition>
            {
                new TokenDefinition(TokenType.String, "^\"[^\"]*\""),
                new TokenDefinition(TokenType.Comment, "^#.*?\\n"),
                new TokenDefinition(TokenType.String, "^[A-Za-z][A-Za-z_0-9.%-]*"),
                new TokenDefinition(TokenType.LeftBracket, "^{"),
                new TokenDefinition(TokenType.RightBracket, "^}"),
                new TokenDefinition(TokenType.Specifier, "^\\<\\>"),
                new TokenDefinition(TokenType.Specifier, "^\\<\\="),
                new TokenDefinition(TokenType.Specifier, "^\\>\\="),
                new TokenDefinition(TokenType.Specifier, "^\\="),
                new TokenDefinition(TokenType.Specifier, "^\\<"),
                new TokenDefinition(TokenType.Specifier, "^\\>"),
                new TokenDefinition(TokenType.Date, "^[0-9]+\\.[0-9]+\\.[0-9]+"),
                new TokenDefinition(TokenType.Real, "^-?[0-9]+\\.[0-9]+"),
                new TokenDefinition(TokenType.Percent, "^-?[0-9]+%"),
                new TokenDefinition(TokenType.Integer, "^-?[0-9]+")
            };
        }
        private readonly List<TokenDefinition> _tokenDefinitions;

        public SimpleRegexTokenizer()
        {
            this._tokenDefinitions = TokenDefinitions;
        }
            
        public IEnumerable<Token> Tokenize(string lqlText)
        {
            var tokens = new List<Token>();

            StringSegment remainingText = lqlText;
            while (!remainingText.IsNullOrWhiteSpace())
            {
                var match = this.FindMatch(remainingText);
                if (match.IsMatch)
                {
                    tokens.Add(new Token(match.TokenType, match.Value));
                    remainingText = match.RemainingText;
                }
                else
                {
                    if (char.IsWhiteSpace(remainingText[0]))
                    {
                        remainingText = remainingText.Subsegment(1);
                    }
                    else
                    {
                        var invalidTokenMatch = CreateInvalidTokenMatch(remainingText);
                        tokens.Add(new Token(invalidTokenMatch.TokenType, invalidTokenMatch.Value));
                        remainingText = invalidTokenMatch.RemainingText;
                    }
                }
            }

            tokens.Add(new Token(TokenType.SequenceTerminator, StringSegment.Empty));

            return tokens;
        }

        private TokenMatch FindMatch(StringSegment lqlText)
        {
            foreach (var tokenDefinition in this._tokenDefinitions)
            {
                var match = tokenDefinition.Match(lqlText);
                if (match.IsMatch)
                    return match;
            }

            return new TokenMatch {  IsMatch = false };
        }

        private static TokenMatch CreateInvalidTokenMatch(StringSegment lqlText)
        {
            var match = InvalidRegex.Match(lqlText.ToString());
            if (match.Success)
            {
                return new TokenMatch
                {
                    IsMatch = true,
                    RemainingText = lqlText.Subsegment(match.Length),
                    TokenType = TokenType.Invalid,
                    Value = match.Value.Trim()
                };
            }

            throw new ParserException("Failed to generate invalid token");
        }
    }
}