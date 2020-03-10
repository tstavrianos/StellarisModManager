using CWTools.Process;
using System.Linq;

namespace PDXModLib.Utility
{
    using System.Collections;
    using System.Collections.Generic;

    public static class CWToolsExtensions
    {
        public static Child Get(this Node node, string key)
        {
            return node.AllChildren.FirstOrDefault(c => c.IsNodeC && c.node.Key == key || c.IsLeafC && c.leaf.Key == key);
        }

        public static bool Exists(this Node node, string key)
        {
            return node.AllChildren.Any(c => c.IsNodeC && c.node.Key == key || c.IsLeafC && c.leaf.Key == key);

        }

        public static string AsString(this Child child)
        {
            if (child.IsNodeC)
                return null;

            if (child.IsCommentC)
                return child.comment;

            if (child.IsLeafC)
                return child.leaf.Value.ToRawString();

            return child.lefavalue.Value.ToRawString();
        }

        public static bool TryGetString(this Node node, string key, ref string value)
        {
            if (node.Exists(key))
            {
                value = node.Get(key).AsString();
                return true;
            }

            return false;
        }

        public static bool TryGetStrings(this Node node, string key, ref IEnumerable<string> value)
        {
            var child = node.Child(key);
            if (child?.Value?.LeafValues != null)
            {
                value = child.Value.LeafValues.Select(s => s.Value.ToRawString()).Where(x => !string.IsNullOrWhiteSpace(x));
                return true;
            }

            return false;
        }
    }
}
