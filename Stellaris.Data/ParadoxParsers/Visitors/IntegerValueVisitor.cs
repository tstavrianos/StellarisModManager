namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class IntegerValueVisitor : ParadoxBaseVisitor<IntegerValue>
    {
        #region Overrides of ParadoxBaseVisitor<IntegerValue>

        public override IntegerValue VisitInteger(ParadoxParser.IntegerContext context)
        {
            return new IntegerValue(long.Parse(context.GetText()));
        }

        #endregion
    }
}