//https://github.com/Vanlightly/DslParser

using System.Text.RegularExpressions;

namespace Stellaris.Data.Parsers.Tokenizer
{
    public sealed class TokenDefinition
    {
        private readonly Regex _regex;
        private readonly TokenType _returnsToken;
        
        public TokenDefinition(TokenType returnsToken, string regexPattern)
        {
            this._regex = new Regex(regexPattern, RegexOptions.IgnoreCase|RegexOptions.Compiled);
            this._returnsToken = returnsToken;
        }

        public TokenMatch Match(StringSegment inputString)
        {
            var match = this._regex.Match(inputString.ToString());
            if (!match.Success) return new TokenMatch {IsMatch = false};
            var remainingText = StringSegment.Empty;
            if (match.Length != inputString.Length)
                remainingText = inputString.Subsegment(match.Length);

            return new TokenMatch
            {
                IsMatch = true,
                RemainingText = remainingText,
                TokenType = this._returnsToken,
                Value = match.Value
            };
        }
    }
}