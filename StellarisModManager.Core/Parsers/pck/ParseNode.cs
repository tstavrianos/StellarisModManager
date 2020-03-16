using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StellarisModManager.Core.Parsers.pck
{
    /// <summary>
	/// Represents a node of a parse tree
	/// </summary>
	public sealed class ParseNode
    {
        private int _line;

        private int _column;

        private long _position;
        public ParseAttribute[] AttributeSet { get; set; }

        public object GetAttribute(string name, object @default = null)
        {
            var attrs = this.AttributeSet;
            foreach (var attr in attrs.Where(attr => attr.Name == name))
            {
                return attr.Value;
            }
            return @default;
        }
        /// <summary>
        /// Gets every descendent of this node and itself
        /// </summary>
        /// <param name="result">The collection to fill</param>
        /// <returns>The <paramref name="result"/> or a new collection, filled with the results</returns>
        public IList<ParseNode> FillDescendantsAndSelf(IList<ParseNode> result = null)
        {
            if (null == result) result = new List<ParseNode>();
            result.Add(this);
            var ic = this.Children.Count;
            for (var i = 0; i < ic; ++i)
                this.Children[i].FillDescendantsAndSelf(result);
            return result;
        }
        public static IEnumerable<ParseNode> Select(IEnumerable<ParseNode> axis, string symbol)
        {
            return axis.Where(pn => null != pn && symbol == pn.Symbol);
        }
        public static ParseNode SelectFirst(IEnumerable<ParseNode> axis, string symbol)
        {
            return axis.FirstOrDefault(pn => null != pn && symbol == pn.Symbol);
        }
        public static IEnumerable<ParseNode> Select(IEnumerable<ParseNode> axis, int symbolId)
        {
            return axis.Where(pn => null != pn && symbolId == pn.SymbolId);
        }
        public static ParseNode SelectFirst(IEnumerable<ParseNode> axis, int symbolId)
        {
            return axis.FirstOrDefault(pn => null != pn && symbolId == pn.SymbolId);
        }
        public void SetLocation(int line, int column, long position)
        {
            this._line = line;
            this._column = column;
            this._position = position;
        }
        public int Line
        {
            get
            {
                if (null == this.Value)
                {
                    return 0 < this.Children.Count ? this.Children[0].Line : 0;
                }
                else
                {
                    return this._line;
                }
            }
        }
        public int Column
        {
            get
            {
                if (null == this.Value)
                {
                    return 0 < this.Children.Count ? this.Children[0].Column : 0;
                }
                else
                {
                    return this._column;
                }
            }
        }
        public long Position
        {
            get
            {
                if (null == this.Value)
                {
                    return 0 < this.Children.Count ? this.Children[0].Position : 0;
                }
                else
                {
                    return this._position;
                }
            }
        }

        public int Length
        {
            get
            {
                if (null == this.Value)
                {
                    if (0 >= this.Children.Count) return 0;
                    var c = this.Children.Count - 1;
                    var p = this.Children[c].Position;
                    var l = this.Children[c].Length;
                    return (int)(p - this.Position) + l;
                }
                else
                    return this.Value.Length;
            }
        }
        public bool IsHidden { get; set; }
        public bool IsCollapsed { get; set; }
        public string SubstituteFor { get; set; }
        public int SubstituteForId { get; set; }

        public string Symbol { get; set; }
        public int SymbolId { get; set; }
        public string Value { get; set; }

        public List<ParseNode> Children { get; } = new List<ParseNode>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            _AppendTreeTo(sb, this);
            return sb.ToString();
        }

        private static void _AppendTreeTo(StringBuilder result, ParseNode node)
        {
            // adapted from https://stackoverflow.com/questions/1649027/how-do-i-print-out-a-tree-structure
            var firstStack = new List<ParseNode> {node};

            var childListStack = new List<List<ParseNode>> {firstStack};

            while (childListStack.Count > 0)
            {
                var childStack = childListStack[childListStack.Count - 1];

                if (childStack.Count == 0)
                {
                    childListStack.RemoveAt(childListStack.Count - 1);
                }
                else
                {
                    node = childStack[0];
                    childStack.RemoveAt(0);

                    var indent = "";
                    for (var i = 0; i < childListStack.Count - 1; i++)
                    {
                        indent += (childListStack[i].Count > 0) ? "|  " : "   ";
                    }
                    var s = node.Symbol;
                    if (node.IsCollapsed)
                        s = string.Concat("{", s, "}");
                    if (node.IsHidden)
                        s = string.Concat("(", s, ")");
                    result.Append(string.Concat(indent, "+- ", s, " ", node.Value ?? "").TrimEnd());
                    result.AppendLine();// string.Concat(" at line ", node.Line, ", column ", node.Column, ", position ", node.Position, ", length of ", node.Length));
                    if (node.Children.Count > 0)
                    {
                        childListStack.Add(new List<ParseNode>(node.Children));
                    }
                }
            }
        }
    }
}
