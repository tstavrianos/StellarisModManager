namespace PDXModLib.Utilities
{
    using System.Linq;

    using CWTools.Process;

    public static class CwToolsExtensions
    {
        public static Child Get(this Node node, string key)
        {
            return node.AllChildren.FirstOrDefault(c => c.IsNodeC  && c.node.Key == key || c.IsLeafC && c.leaf.Key == key);
        }

        public static string AsString(this Child child)
        {
            if (child.IsNodeC)
                return null;

            if (child.IsCommentC)
                return child.comment;

            return child.IsLeafC ? child.leaf.Value.ToRawString() : child.lefavalue.Value.ToRawString();
        }
    }
}
