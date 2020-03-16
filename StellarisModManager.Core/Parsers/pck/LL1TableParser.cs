using System;
using System.Collections.Generic;
using System.Linq;

namespace StellarisModManager.Core.Parsers.pck
{
    /// <summary>
	/// An LL(1) parser implemented as a pull-style parser.
	/// </summary>
	/// <remarks>This interface is similar in use to <see cref="System.Xml.XmlReader"/></remarks>
	public class LL1TableParser : LL1Parser
    {
        private readonly int[][][] _parseTable;

        private ITokenizer _tokenizer;

        private IEnumerator<Token> _tokenEnum;

        private Token _errorToken;

        private readonly Stack<int> _stack;

        private readonly int[] _nodeFlags; // for hidden, collapsed

        private readonly string[] _symbolTable;

        private readonly int[] _substitutions;

        private readonly ParseAttribute[][] _attributeSets;

        private readonly int[] _initCfg;

        private readonly int _eosSymbolId;

        private readonly int _errorSymbolId;
        protected override bool IsHidden => this._IsHidden(this.SymbolId);
        protected override bool IsCollapsed => this._IsCollapsed(this.SymbolId);
        protected override string Substitute => (0 > this.SubstituteId) ? null : this._symbolTable[this.SubstituteId];

        protected override int SubstituteId
        {
            get
            {
                var s = this.SymbolId;
                if (0 > s)
                    return -1;
                return this._substitutions[s];
            }
        }

        protected override ParseAttribute[] GetAttributeSet(int symbolId)
        {
            if (0 > symbolId || this._attributeSets.Length <= symbolId)
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
        /// <summary>
        /// Indicates the <see cref="LLNodeType"/> at the current position.
        /// </summary>
        protected override LLNodeType NodeType
        {
            get
            {
                if (null != this._errorToken.Symbol)
                    return LLNodeType.Error;
                if (this._stack.Count > 0)
                {
                    var s = this._stack.Peek();
                    if (0 > s)
                        return LLNodeType.EndNonTerminal;
                    return s == this._tokenEnum.Current.SymbolId ? LLNodeType.Terminal : LLNodeType.NonTerminal;
                }
                try
                {
                    if (this._eosSymbolId == this._tokenEnum.Current.SymbolId)
                        return LLNodeType.EndDocument;
                }
                catch
                {
                    // ignored
                }

                return LLNodeType.Initial;
            }
        }
        public override void Restart(ITokenizer tokenizer)
        {
            this.Close();
            this._tokenizer = null;
            if (null == tokenizer) return;
            this._tokenizer = tokenizer;
            this._tokenEnum = tokenizer.GetEnumerator();
        }
        public override void Restart(IEnumerable<char> input)
        {
            this.Close();
            this._tokenizer.Restart(input);
            this._tokenEnum = this._tokenizer.GetEnumerator();
        }
        public override void Restart()
        {
            if (null == this._tokenEnum) throw new ObjectDisposedException(this.GetType().Name);
            this._tokenEnum.Reset();
            this._errorToken.Symbol = null;
            this._stack.Clear();
        }
        /// <summary>
        /// Indicates the current symbol
        /// </summary>
        protected override string Symbol
        {
            get
            {
                var id = this.SymbolId;
                return -1 != id ? this._symbolTable[id] : null;
            }
        }

        protected override int SymbolId
        {
            get
            {
                if (null != this._errorToken.Symbol)
                    return this._errorToken.SymbolId;
                if (this._stack.Count <= 0) return -1;
                var s = this._stack.Peek();
                if (0 > s)
                    return ~s;
                return s;

            }
        }
        /// <summary>
        /// Indicates the current line
        /// </summary>
        protected override int Line => (null == this._errorToken.Symbol) ? this._tokenEnum.Current.Line : this._errorToken.Line;
        /// <summary>
        /// Indicates the current column
        /// </summary>
        protected override int Column => (null == this._errorToken.Symbol) ? this._tokenEnum.Current.Column : this._errorToken.Column;
        /// <summary>
        /// Indicates the current position
        /// </summary>
        protected override long Position => (null == this._errorToken.Symbol) ? this._tokenEnum.Current.Position : this._errorToken.Position;
        /// <summary>
        /// Indicates the current value
        /// </summary>
        protected override string Value
        {
            get
            {
                switch (this.NodeType)
                {
                    case LLNodeType.Error:
                        return this._errorToken.Value;
                    case LLNodeType.Terminal:
                        return this._tokenEnum.Current.Value;
                }
                return null;
            }
        }
        /// <summary>
        /// Constructs a new instance of the parser
        /// </summary>
        /// <param name="parseTable">The parse table to use</param>
        /// <param name="tokenizer">The tokenizer to use </param>
        /// <param name="startSymbol">The start symbol</param>
        public LL1TableParser(
            int[][][] parseTable,
            int[] initCfg,
            string[] symbolTable,
            int[] nodeFlags,
            int[] substitutions,
            ParseAttribute[][] attributeSets,
            ITokenizer tokenizer)
        {
            this._parseTable = parseTable;
            this._initCfg = initCfg;
            this._symbolTable = symbolTable;
            this._nodeFlags = nodeFlags;
            this._substitutions = substitutions;
            this._attributeSets = attributeSets;
            this._stack = new Stack<int>();
            this._errorToken.Symbol = null;
            this._eosSymbolId = Array.IndexOf(this._symbolTable, "#EOS");
            this._errorSymbolId = Array.IndexOf(this._symbolTable, "#ERROR");
            // we do actually handle this error since it's rough to track otherwise.
            if (0 > this._eosSymbolId || 0 > this._errorSymbolId)
                throw new ArgumentException("The symbol table is invalid.", nameof(symbolTable));
            this.Restart(tokenizer);
        }

        /// <summary>
        /// Reads and parses the next node from the document
        /// </summary>
        /// <returns>True if there is more to read, otherwise false.</returns>
        protected override bool Read()
        {
            var result = this._ReadImpl();
            // this is a big part of the "magic" behind clean parse trees
            // all it does is skip "collapsed" and "hidden" nodes in the parse tree
            // meaning any symbol with a "collapsed" or "hidden" attribute
            while ((!this.ShowCollapsed && result && this._IsCollapsed(this.SymbolId)) ||
                (!this.ShowHidden && result && this._IsHidden(this.SymbolId)))
                result = this._ReadImpl();
            return result;
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

        private bool _ReadImpl()
        {
            var n = this.NodeType;
            switch (n)
            {
                case LLNodeType.Error when this._eosSymbolId == this._tokenEnum.Current.SymbolId:
                    this._errorToken.Symbol = null;
                    this._stack.Clear();
                    return true;
                case LLNodeType.Initial:
                {
                    this._stack.Push(this._initCfg[0]);
                    if (this._tokenEnum.MoveNext() && this._IsHidden(this._tokenEnum.Current.SymbolId))
                        this._stack.Push(this._tokenEnum.Current.SymbolId);


                    return true;
                }
            }

            this._errorToken.Symbol = null; // clear the error status
            if (0 < this._stack.Count)
            {
                var sid = this._stack.Peek();
                if (0 > sid)
                {
                    this._stack.Pop();
                    return true;
                }
                if (sid == this._tokenEnum.Current.SymbolId) // terminal
                {
                    this._stack.Pop();
                    // lex the next token
                    if (this._tokenEnum.MoveNext() && this._IsHidden(this._tokenEnum.Current.SymbolId))
                        this._stack.Push(this._tokenEnum.Current.SymbolId);
                    return true;
                }
                // non-terminal
                var ntc = this._initCfg[1];
                if (0 > sid || ntc <= sid)
                {
                    this._Panic();
                    return true;
                }
                var row = this._parseTable[sid];
                if (null == row)
                {
                    this._Panic();
                    return true;
                }
                var tid = this._tokenEnum.Current.SymbolId - ntc;
                if (0 > tid || row.Length <= tid || null == row[tid])
                {
                    this._Panic();
                    return true;
                }
                this._stack.Pop();
                var rule = row[tid];
                // store the end non-terminal marker for later
                this._stack.Push(~sid);

                // push the rule's derivation onto the stack in reverse order
                for (var i = rule.Length - 1; 0 <= i; --i)
                    this._stack.Push(rule[i]);

                return true;

            }
            // last symbol must be the end of the input stream or there's a problem
            if (this._eosSymbolId == this._tokenEnum.Current.SymbolId) return false;
            this._Panic();
            return true;
        }

        /// <summary>
        /// Does panic-mode error recovery
        /// </summary>
        private void _Panic()
        {

            // fill the error token
            this._errorToken.Symbol = "#ERROR"; // turn on error reporting
            this._errorToken.SymbolId = this._errorSymbolId;
            this._errorToken.Value = "";
            this._errorToken.Column = this._tokenEnum.Current.Column;
            this._errorToken.Line = this._tokenEnum.Current.Line;
            this._errorToken.Position = this._tokenEnum.Current.Position;
            var s = this._stack.Peek();
            int[][] row;
            if (-1 < s && s < this._initCfg[1] && null != (row = this._parseTable[s]))
            {
                this._errorToken.Value += this._tokenEnum.Current.Value;
                while (-1 < (s = this._tokenEnum.Current.SymbolId - this._initCfg[1]) &&
                    s < row.Length &&
                    null == row[s] &&
                    this._eosSymbolId != this._tokenEnum.Current.SymbolId &&
                    this._tokenEnum.MoveNext())
                {
                    s = this._tokenEnum.Current.SymbolId - this._initCfg[1];
                    if (0 > s || row.Length <= s || null == row[s])
                        this._errorToken.Value += this._tokenEnum.Current.Value;
                }
                if (this._stack.Contains(this._tokenEnum.Current.SymbolId))
                {
                    // TODO: not even 100% sure this works, but it passed the tests so far
                    this._errorToken.Value += this._tokenEnum.Current.Value;
                }
            }
            else
            {
                do
                {
                    s = this._tokenEnum.Current.SymbolId;
                    this._errorToken.Value += this._tokenEnum.Current.Value;
                    if (!this._tokenEnum.MoveNext())
                        break;

                } while (this._eosSymbolId != s && !this._stack.Contains(s));

            }
            while (this._stack.Contains((s = this._tokenEnum.Current.SymbolId)) && this._stack.Peek() != s)
                this._stack.Pop();
        }

        protected override void Close()
        {
            this._tokenEnum?.Dispose();
            this._stack.Clear();
            this._errorToken.Symbol = null;
        }
    }
}
