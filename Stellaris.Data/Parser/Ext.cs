using System;
using System.Collections.Generic;
using Stellaris.Data.Antlr;

namespace Stellaris.Data.Parser
{
    public static class Ext
    {
        internal static string Strip(this string value)
        {
            if(string.IsNullOrWhiteSpace(value)) return string.Empty;
            if (value.Length <= 2) return value;
            if (value[0] == '"' && value[value.Length - 1] == '"')
                return value.Substring(1, value.Length - 2);
            return value;
        }

        public static MapEntry ToBlock(this IEnumerable<ParadoxParser.AssignmentContext> assignmentContexts)
        {
            var ret = new Dictionary<string, IList<IEntry>>();
            var visitor = new AssignmentVisitor();
            foreach (var context in assignmentContexts)
            {
                var (key, value) = context.Accept(visitor);
                if(ret.TryGetValue(key, out var l)) l.Add(value);
                else ret.Add(key, new List<IEntry>{value});
            }

            return new MapEntry(ret);
        }

        public static MapEntry ToConfigBlock(this ParadoxParser.ConfigContext context)
        {
            return new ConfigVisitor().Visit(context);
        }
    }
}