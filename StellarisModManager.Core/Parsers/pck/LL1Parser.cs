using System;
using System.Collections.Generic;

namespace StellarisModManager.Core.Parsers.pck
{
    public abstract class LL1Parser : IDisposable
    {
        protected bool ShowHidden { get; set; } = false;
        protected bool ShowCollapsed { get; set; } = true;
        protected abstract LLNodeType NodeType { get; }
        protected abstract string Symbol { get; }
        protected abstract int SymbolId { get; }
        protected abstract string Value { get; }
        protected abstract int Line { get; }
        protected abstract int Column { get; }
        protected abstract long Position { get; }
        protected abstract bool Read();
        public abstract void Restart();
        public abstract void Restart(ITokenizer tokenizer);
        public abstract void Restart(IEnumerable<char> input);
        protected abstract void Close();
        protected abstract bool IsHidden { get; }
        protected abstract bool IsCollapsed { get; }
        protected abstract string Substitute { get; }
        protected abstract int SubstituteId { get; }
        protected abstract ParseAttribute[] GetAttributeSet(int symbolId);
        public abstract object GetAttribute(string name, object @default = null);
        void IDisposable.Dispose() { this.Close(); }

        /// <summary>
        /// Parses the from the current position into a parse tree. This will read an entire sub-tree.
        /// </summary>
        /// <param name="trim">Remove non-terminal nodes that have no terminals and collapse nodes that have a single non-terminal child</param>
        /// <param name="transform">Apply transformations indicated in the grammar to the tree</param>
        /// <returns>A <see cref="ParseNode"/> representing the parse tree. The reader's cursor is advanced.</returns>
        public virtual ParseNode ParseSubtree(bool trim = false, bool transform = true)
        {
            var res = false;
            while ((res = this.Read()) && transform && this.IsCollapsed)
            {
            }

            if (!res) return null;
            var nn = this.NodeType;
            if (LLNodeType.EndNonTerminal == nn)
                return null;

            var result = new ParseNode
            {
                AttributeSet = this.GetAttributeSet(this.SymbolId),
                IsHidden = this.IsHidden,
                IsCollapsed = this.IsCollapsed
            };
            // will fetch the original node's attributes in the 
            // case of a substitution. Not sure if that's desired

            var s = this.Substitute;
            if (null != s)
            {
                result.Symbol = s;
                result.SymbolId = this.SubstituteId;
                result.SubstituteFor = this.Symbol;
                result.SubstituteForId = this.SymbolId;
            }
            else
            {
                result.SubstituteForId = -1;
                result.Symbol = this.Symbol;
                result.SymbolId = this.SymbolId;
            }
            switch (nn)
            {
                case LLNodeType.NonTerminal:
                {
                    while (true)
                    {
                        var k = this.ParseSubtree(trim, transform);
                        if (null != k)
                        {
                            if (null != k.Value)
                                result.Children.Add(k);
                            else
                            {
                                if (!trim)
                                {
                                    //if(0<k.Children.Count)
                                    result.Children.Add(k);
                                }
                                else
                                {
                                    if (1 < k.Children.Count)
                                        result.Children.Add(k);
                                    else
                                    {
                                        if (0 < k.Children.Count)
                                        {
                                            result.Children.Add(null == k.Children[0].Value ? k.Children[0] : k);
                                        }
                                    }
                                }
                            }
                        }
                        else
                            break;
                    }
                    return result;
                }
                case LLNodeType.Terminal:
                    result.SetLocation(this.Line, this.Column, this.Position);
                    result.Value = this.Value;
                    return result;
                case LLNodeType.Error:
                    result.SetLocation(this.Line, this.Column, this.Position);
                    result.Value = this.Value;
                    return result;
                default:
                    return null;
            }
        }
    }
}
