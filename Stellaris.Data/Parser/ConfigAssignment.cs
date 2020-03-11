using System;

namespace Stellaris.Data.Parser
{
    public sealed class ConfigAssignment: Tuple<string, IEntry> {
        public ConfigAssignment(string item1, IEntry item2) : base(item1, item2)
        {
        }
    }
}