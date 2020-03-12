namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using System.Collections.Generic;

    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class ConfigVisitor : ParadoxBaseVisitor<Config>
    {
        #region Overrides of ParadoxBaseVisitor<Config>

        public override Config VisitConfig(ParadoxParser.ConfigContext context)
        {
            var ret = new List<Assignment>();
            var visitor = new AssignmentVisitor();
            foreach (var entry in context.assignment())
            {
                ret.Add(entry.Accept(visitor));
            }
            return new Config(ret);
        }

        #endregion
    }
}