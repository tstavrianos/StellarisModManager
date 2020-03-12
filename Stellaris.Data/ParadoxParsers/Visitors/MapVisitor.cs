namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using System.Collections.Generic;

    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class MapVisitor : ParadoxBaseVisitor<MapValue>
    {
        #region Overrides of ParadoxBaseVisitor<MapValue>

        public override MapValue VisitMap(ParadoxParser.MapContext context)
        {
            var ret = new List<Assignment>();
            var visitor = new AssignmentVisitor();
            foreach (var entry in context.assignment())
            {
                ret.Add(entry.Accept(visitor));
            }
            return new MapValue(ret);
        }

        #endregion
    }
}