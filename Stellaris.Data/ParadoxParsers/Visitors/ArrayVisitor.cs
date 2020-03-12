namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using System.Collections.Generic;

    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class ArrayVisitor : ParadoxBaseVisitor<ArrayValue>
    {
        #region Overrides of ParadoxBaseVisitor<ArrayValue>

        public override ArrayValue VisitArray(ParadoxParser.ArrayContext context)
        {
            var ret = new List<IValue>();
            var visitor = new ValueVisitor();
            foreach (var entry in context.value())
            {
                ret.Add(entry.Accept(visitor));
            }
            return new ArrayValue(ret);
        }

        #endregion
    }
}