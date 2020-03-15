using System;
using System.Collections.Generic;
using System.Text;

namespace Stellaris.Data.Parsers.pck
{
    public abstract class Lalr1Parser : IDisposable
    {
        protected bool ShowHidden { get; set; } = false;
        protected abstract LRNodeType NodeType { get; }
        protected abstract string Symbol { get; }
        protected abstract int SymbolId { get; }
        protected abstract string Value { get; }
        protected abstract string[] RuleDefinition { get; }
        public abstract int[] RuleDefinitionIds { get; }
        public string Rule => LRNodeType.Reduce == this.NodeType ? _ToRuleString(this.RuleDefinition) : null;
        protected abstract int Line { get; }
        protected abstract int Column { get; }
        protected abstract long Position { get; }
        protected abstract bool IsHidden { get; }
        protected abstract bool IsCollapsed { get; }
        protected abstract string Substitute { get; }
        protected abstract int SubstituteId { get; }
        protected abstract ParseAttribute[] GetAttributeSet(int symbolId);
        public abstract object GetAttribute(string name, object @default = null);
        protected abstract bool Read();
        public abstract void Restart(ITokenizer tokenizer);
        public abstract void Restart(IEnumerable<char> input);
        public abstract void Restart();
        protected abstract void Close();
        void IDisposable.Dispose()
            => this.Close();
        public virtual ParseNode ParseReductions(bool trim = false, bool transform = true)
        {
            var rs = new Stack<ParseNode>();
            while (this.Read())
            {
                ParseNode p = null;
                switch (this.NodeType)
                {
                    case LRNodeType.Shift:
                        p = new ParseNode();
                        p.SetLocation(this.Line, this.Column, this.Position);
                        // this will get the original nodes attributes
                        // in the case of a substitution. Still not sure
                        // if that's preferred or not.
                        p.AttributeSet = this.GetAttributeSet(this.SymbolId);
                        var s = this.Substitute;
                        if (null != s)
                        {
                            p.Symbol = s;
                            p.SymbolId = this.SubstituteId;
                            p.SubstituteFor = this.Symbol;
                            p.SubstituteForId = this.SymbolId;
                        }
                        else
                        {
                            p.Symbol = this.Symbol;
                            p.SymbolId = this.SymbolId;
                            p.SubstituteForId = -1;
                        }
                        p.Value = this.Value;
                        p.IsHidden = this.IsHidden;
                        p.IsCollapsed = this.IsCollapsed;
                        rs.Push(p);
                        break;
                    case LRNodeType.Reduce:
                        if (!trim || 2 != this.RuleDefinition.Length)
                        {
                            var d = new List<ParseNode>();
                            p = new ParseNode
                            {
                                AttributeSet = this.GetAttributeSet(this.SymbolId), IsCollapsed = this.IsCollapsed
                            };

                            s = this.Substitute;
                            if (null != s)
                            {
                                p.Symbol = s;
                                p.SymbolId = this.SubstituteId;
                                p.SubstituteFor = this.Symbol;
                                p.SubstituteForId = this.SymbolId;
                            }
                            else
                            {
                                p.Symbol = this.Symbol;
                                p.SymbolId = this.SymbolId;
                                p.SubstituteForId = -1;
                            }

                            for (var i = 1; this.RuleDefinition.Length > i; i++)
                            {
                                var pc = rs.Pop();
                                this._AddChildren(pc, transform, p.Children);
                                if ("#ERROR" == pc.Symbol)
                                    break;
                                // don't count hidden terminals
                                if (pc.IsHidden)
                                    --i;
                            }
                            rs.Push(p);
                        }
                        break;
                    case LRNodeType.Accept:
                        break;
                    case LRNodeType.Error:
                        p = new ParseNode();
                        p.SetLocation(this.Line, this.Column, this.Position);
                        p.Symbol = this.Symbol;
                        p.SubstituteForId = -1;
                        p.Value = this.Value;
                        rs.Push(p);
                        break;
                }
            }
            if (0 == rs.Count)
                return null;
            var result = rs.Pop();
            while ("#ERROR" != result.Symbol && 0 < rs.Count)
                this._AddChildren(rs.Pop(), transform, result.Children);
            return result;
        }

        private void _AddChildren(ParseNode pc, bool transform, IList<ParseNode> result)
        {
            if (!transform)
            {
                result.Insert(0, pc);
                return;
            }
            if (pc.IsCollapsed)
            {
                if (null != pc.Value) return;
                for (int ic = pc.Children.Count, i = ic - 1; 0 <= i; --i)
                    this._AddChildren(pc.Children[i], true, result);
            }
            else
                result.Insert(0, pc);
        }

        private static string _ToRuleString(IReadOnlyList<string> rule)
        {
            var sb = new StringBuilder();
            sb.Append(rule[0]);
            sb.Append(" ->");
            for (var i = 1; i < rule.Count; ++i)
            {
                sb.Append(' ');
                sb.Append(rule[i]);
            }
            return sb.ToString();
        }
    }
}
