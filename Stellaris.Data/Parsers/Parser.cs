//https://github.com/Vanlightly/DslParser

using System;
using System.Collections.Generic;
using System.Linq;
using Stellaris.Data.Parsers.Models;
using Stellaris.Data.Parsers.Tokenizer;
using Array = Stellaris.Data.Parsers.Models.Array;
using Assignment = Stellaris.Data.Parsers.Models.Assignment;
using Config = Stellaris.Data.Parsers.Models.Config;
using IField = Stellaris.Data.Parsers.Models.IField;
using IValue = Stellaris.Data.Parsers.Models.IValue;
using String = Stellaris.Data.Parsers.Models.String;

namespace Stellaris.Data.Parsers
{
    public sealed class Parser
    {
        private Stack<Token> _tokenSequence;
        private Token _lookaheadFirst;
        private Token _lookaheadSecond;
        private Config _config;

        public Config Parse(IReadOnlyList<Token> tokens)
        {
            if (tokens.Count == 0 || tokens[0].TokenType == TokenType.SequenceTerminator) return new Config(new List<Assignment>());
            this.LoadSequenceStack(tokens);
            this.PrepareLookaheads();

            this.Config();

            this.DiscardToken(TokenType.SequenceTerminator);

            return this._config;
        }

        private void Config()
        {
            var list = new List<Assignment>();
            while (this._lookaheadFirst.TokenType != TokenType.SequenceTerminator)
            {
                list.Add(this.Assignment());
            }

            this._config = new Config(list);
        }

        private Assignment Assignment()
        {
            if (this._lookaheadSecond.TokenType == TokenType.Specifier)
            {
                IField field;
                if (this._lookaheadFirst.TokenType == TokenType.String)
                {
                    var token = this.ReadToken(TokenType.String);
                    field = new String (token.Value.Value);
                }
                else
                {
                    var token = this.ReadToken(TokenType.Integer);
                    field = new Integer (long.Parse(token.Value.Value));
                }

                var op = this.ReadToken(TokenType.Specifier);
                Operator actualOperator;
                switch (op.Value.Value)
                {
                    case "=": actualOperator = Operator.Equal; break;
                    case "<>": actualOperator = Operator.NotEqual; break;
                    case ">": actualOperator = Operator.Greater; break;
                    case "<": actualOperator = Operator.Less; break;
                    case ">=": actualOperator = Operator.GreaterEqual; break;
                    case "<=": actualOperator = Operator.LessEqual; break;
                    default: throw new ParserException("");
                }
                var value = this.Value();
                return new Assignment (field, actualOperator, value);
            }
            else
            {
                var value = this.Value();
                return new Assignment (null, Operator.None, value);
            }
        }

        private IValue Value()
        {
            switch (this._lookaheadFirst.TokenType)
            {
                case TokenType.Integer:
                    return new Integer(long.Parse(this.ReadToken().Value));
                case TokenType.Percent:
                    return new Percent(int.Parse(this.ReadToken().Value.Value.TrimEnd('%')));
                case TokenType.Real:
                    return new Real(double.Parse(this.ReadToken().Value));
                case TokenType.Date:
                    return new Date(DateTime.ParseExact(this.ReadToken().Value, "yyyy.MM.dd", null));
                case TokenType.String:
                    return new String(this.ReadToken().Value.Value);
            }

            this.DiscardToken(TokenType.LeftBracket);
            if (this._lookaheadSecond.TokenType == TokenType.Specifier)
            {
                var list = new List<Assignment>();
                while (this._lookaheadFirst.TokenType != TokenType.RightBracket)
                {
                    list.Add(this.Assignment());
                }
                this.DiscardToken(TokenType.RightBracket);
                return new Map(list);
            }
            else
            {
                var list = new List<IValue>();
                while (this._lookaheadFirst.TokenType != TokenType.RightBracket)
                {
                    list.Add(this.Value());
                }
                this.DiscardToken(TokenType.RightBracket);
                return new Array(list);
            }
        }
        
        private void LoadSequenceStack(IReadOnlyList<Token> tokens)
        {
            this._tokenSequence = new Stack<Token>();
            for(var i = tokens.Count - 1; i >= 0; i--)
            {
                this._tokenSequence.Push(tokens[i]);
            }
        }

        private void PrepareLookaheads()
        {
            this._lookaheadFirst = this._tokenSequence.Pop();
            this._lookaheadSecond = this._tokenSequence.Pop();
        }
        
        private Token ReadToken(TokenType tokenType)
        {
            if (this._lookaheadFirst.TokenType != tokenType)
                throw new ParserException(
                    $"Expected {tokenType.ToString().ToUpper()} but found: {this._lookaheadFirst.Value}");
            var ret = this._lookaheadFirst.Clone();
            this.DiscardToken();
            return ret;
        }
        
        private Token ReadToken()
        {
            var ret = this._lookaheadFirst.Clone();
            this.DiscardToken();
            return ret;
        }

        private void DiscardToken()
        {
            this._lookaheadFirst = this._lookaheadSecond.Clone();

            this._lookaheadSecond = this._tokenSequence.Any() ? this._tokenSequence.Pop() : new Token(TokenType.SequenceTerminator, StringSegment.Empty);
        }

        private void DiscardToken(TokenType tokenType)
        {
            if (this._lookaheadFirst.TokenType != tokenType)
                throw new ParserException(
                    $"Expected {tokenType.ToString().ToUpper()} but found: {this._lookaheadFirst.Value}");

            this.DiscardToken();
        }
    }
}