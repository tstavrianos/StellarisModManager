using System;
using System.Collections.Generic;
using System.Linq;

namespace StellarisModManager.Core.Parsers.pck
{
    public class Lalr1TableParser : Lalr1Parser
    {
        private readonly int[][][] _parseTable;

        private Token _token;

        private Token _errorToken;

        private readonly string[] _symbols;

        private readonly int[] _nodeFlags;

        private readonly int[] _substitutions;

        private ITokenizer _tokenizer;

        private IEnumerator<Token> _tokenEnum;

        private LRNodeType _nodeType;

        private readonly Stack<int> _stack;

        private int[] _ruleDef;

        private readonly int _eosId;

        private readonly int _errorId;

        private readonly ParseAttribute[][] _attributeSets;

        protected Lalr1TableParser(int[][][] parseTable, string[] symbols, int[] nodeFlags, int[] substitutions, ParseAttribute[][] attributeSets, ITokenizer tokenizer)
        {
            this._parseTable = parseTable;
            this._symbols = symbols;
            this._nodeFlags = nodeFlags;
            this._substitutions = substitutions;
            this._attributeSets = attributeSets;

            this._eosId = Array.IndexOf(symbols, "#EOS");
            this._errorId = Array.IndexOf(symbols, "#ERROR");
            if (0 > this._eosId || 0 > this._errorId)
                throw new ArgumentException("Error in symbol table", nameof(symbols));

            this._stack = new Stack<int>();
            this._tokenizer = tokenizer;
            if (null != tokenizer)
                this._tokenEnum = tokenizer.GetEnumerator();
            this._nodeType = LRNodeType.Initial;

        }

        protected override ParseAttribute[] GetAttributeSet(int symbolId)
        {
            if (0 < symbolId || this._attributeSets.Length <= symbolId)
                return null;
            return this._attributeSets[symbolId];
        }
        public override object GetAttribute(string name, object @default = null)
        {
            var s = this.SymbolId;
            if (null == this._attributeSets || -1 >= s) return @default;
            var attrs = this._attributeSets[s];
            if (null == attrs) return @default;
            foreach (var attr in attrs.Where(attr => attr.Name == name))
            {
                return attr.Value;
            }
            return @default;
        }

        protected override int Line
        {
            get
            {
                switch (this._nodeType)
                {

                    case LRNodeType.Error:
                        return this._errorToken.Line;
                    default:
                        return this._token.Line;
                }
            }
        }

        protected override int Column
        {
            get
            {
                switch (this._nodeType)
                {

                    case LRNodeType.Error:
                        return this._errorToken.Column;
                    default:
                        return this._token.Column;
                }
            }
        }

        protected override long Position
        {
            get
            {
                switch (this._nodeType)
                {

                    case LRNodeType.Error:
                        return this._errorToken.Position;
                    default:
                        return this._token.Position;
                }

            }
        }

        protected override LRNodeType NodeType => this._nodeType;

        protected override int SymbolId
        {
            get
            {
                switch (this._nodeType)
                {
                    case LRNodeType.Shift:
                        return this._token.SymbolId;
                    case LRNodeType.Reduce:
                        return this._ruleDef[0];
                    case LRNodeType.Error:
                        return this._errorToken.SymbolId;
                }
                return -1;
            }
        }

        protected override string Symbol => (-1 < this.SymbolId) ? this._symbols[this.SymbolId] : null;

        protected override string Value
        {
            get
            {
                switch (this._nodeType)
                {

                    case LRNodeType.Error:
                        return this._errorToken.Value;
                    case LRNodeType.Shift:
                        return this._token.Value;
                }

                return null;
            }
        }

        protected override string[] RuleDefinition
        {
            get
            {
                if (LRNodeType.Reduce != this._nodeType) return null;
                var result = new string[this._ruleDef.Length];
                for (var i = 0; i < this._ruleDef.Length; i++)
                    result[i] = this._symbols[this._ruleDef[i]];
                return result;
            }
        }
        public override int[] RuleDefinitionIds => LRNodeType.Reduce == this._nodeType ? this._ruleDef : null;

        protected override bool IsHidden => this._IsHidden(this.SymbolId);
        protected override bool IsCollapsed => this._IsCollapsed(this.SymbolId);
        protected override string Substitute => (-1 < this.SubstituteId) ? this._symbols[this.SubstituteId] : null;

        protected override int SubstituteId
        {
            get
            {
                var s = this.SymbolId;
                return 0 > s ? s : this._substitutions[s];
            }
        }

        protected override bool Read()
        {
            switch (this._nodeType)
            {
                case LRNodeType.Initial:
                {
                    this._stack.Push(0); // push initial state
                    if (!this._tokenEnum.MoveNext())
                        throw new Exception("Error in tokenizer implementation: Expecting #EOS token");
                    break;
                }
                case LRNodeType.Accept:
                    this._nodeType = LRNodeType.EndDocument;
                    this._stack.Clear();
                    return true;
                case LRNodeType.EndDocument:
                    return false;
                case LRNodeType.Error:
                    this._nodeType = LRNodeType.EndDocument;
                    this._stack.Clear();
                    return true;
            }
            if (LRNodeType.Error != this._nodeType)
            {
                if (!this.ShowHidden)
                {
                    while (this._IsHidden(this._tokenEnum.Current.SymbolId))
                        this._tokenEnum.MoveNext();
                }
                else if (this._IsHidden(this._tokenEnum.Current.SymbolId))
                {
                    this._token = this._tokenEnum.Current;
                    this._nodeType = LRNodeType.Shift;
                    this._tokenEnum.MoveNext();
                    return true;
                }
            }
            if (0 < this._stack.Count)
            {
                var entry = this._parseTable[this._stack.Peek()];
                if (this._errorId == this._tokenEnum.Current.SymbolId)
                {
                    this._Panic();
                    return true;
                }
                var trns = entry[this._tokenEnum.Current.SymbolId];
                if (null == trns)
                {
                    this._Panic();
                    return true;
                }
                if (1 == trns.Length) // shift or accept
                {
                    if (-1 != trns[0]) // shift
                    {
                        this._nodeType = LRNodeType.Shift;
                        this._token = this._tokenEnum.Current;
                        this._tokenEnum.MoveNext();
                        this._stack.Push(trns[0]);
                        return true;
                    }
                    else
                    { // accept 
                      //throw if _tok is not $ (end)
                        if (this._eosId != this._tokenEnum.Current.SymbolId)
                        {
                            this._Panic();
                            return true;
                        }

                        this._nodeType = LRNodeType.Accept;
                        this._stack.Clear();
                        return true;
                    }
                }
                else // reduce
                {
                    this._ruleDef = new int[trns.Length - 1];
                    for (var i = 1; i < trns.Length; i++)
                        this._ruleDef[i - 1] = trns[i];
                    for (var i = 2; i < trns.Length; ++i)
                        this._stack.Pop();

                    // There is a new number at the top of the stack. 
                    // This number is our temporary state. Get the symbol 
                    // from the left-hand side of the rule #. Treat it as 
                    // the next input token in the GOTO table (and place 
                    // the matching state at the top of the set stack).
                    var e = this._parseTable[this._stack.Peek()];
                    if (null == e)
                    {
                        this._Panic();
                        return true;
                    }
                    this._stack.Push(this._parseTable[this._stack.Peek()][trns[1]][0]);
                    this._nodeType = LRNodeType.Reduce;
                    return true;
                }
            }
            else
            {
                // if we already encountered an error
                // return false in this case, since the
                // stack is empty there's nothing to do
                var cont = LRNodeType.Error != this._nodeType;
                this._Panic();
                return cont;
            }

        }

        protected override void Close()
        {
            if (null != this._tokenEnum)
            {
                this._tokenEnum.Dispose();
                this._tokenEnum = null;
            }
            this._nodeType = LRNodeType.EndDocument;
            this._stack.Clear();
        }
        public override void Restart()
        {
            if (null == this._tokenEnum) throw new ObjectDisposedException(this.GetType().Name);
            this._tokenEnum.Reset();
            this._stack.Clear();
            this._nodeType = LRNodeType.Initial;
        }
        public override void Restart(ITokenizer tokenizer)
        {
            this.Close();
            this._tokenizer = null;
            if (null == tokenizer) return;
            this._tokenizer = tokenizer;
            this._tokenEnum = tokenizer.GetEnumerator();
            this._nodeType = LRNodeType.Initial;
        }
        public override void Restart(IEnumerable<char> input)
        {
            this.Close();
            this._tokenizer.Restart(input);
            this._tokenEnum = this._tokenizer.GetEnumerator();
            this._nodeType = LRNodeType.Initial;
        }

        private void _Panic()
        {
            // This is primitive. Should see if the Dragon Book has something better
            this._nodeType = LRNodeType.Error;
            int[] e;
            this._errorToken.Symbol = "#ERROR";
            this._errorToken.SymbolId = this._errorId;
            this._errorToken.Value = this._tokenEnum.Current.Value;
            this._errorToken.Line = this._tokenEnum.Current.Line;
            this._errorToken.Column = this._tokenEnum.Current.Column;
            this._errorToken.Position = this._tokenEnum.Current.Position;
            var s = this._tokenEnum.Current.SymbolId;
            if (0 == this._stack.Count)
                return;
            var state = this._stack.Peek();
            var d = this._parseTable[state];
            if (this._errorId != s && null != (e = d[s]) && this._eosId != s)
            {
                this._errorToken.Value += this._tokenEnum.Current.Value;
                while (this._tokenEnum.MoveNext() && this._eosId != (s = this._tokenEnum.Current.SymbolId))
                    if (null != (e = d[s]))
                        this._errorToken.Value += this._tokenEnum.Current.Value;
                    else
                        break;
            }
            else
            {
                //_errorToken.Value += _tokenEnum.Current.Value;
                this._tokenEnum.MoveNext();
            }
        }

        private bool _IsHidden(int symbolId)
        {
            if (0 > symbolId)
                return false;
            return 2 == (this._nodeFlags[symbolId] & 2);
        }

        private bool _IsCollapsed(int symbolId)
        {
            if (0 > symbolId)
                return false;
            return 1 == (this._nodeFlags[symbolId] & 1);
        }
    }
}
