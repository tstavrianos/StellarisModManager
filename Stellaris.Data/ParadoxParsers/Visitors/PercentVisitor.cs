namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class PercentVisitor : ParadoxBaseVisitor<PercentValue>
    {
        #region Overrides of ParadoxBaseVisitor<PercentValue>

        public override PercentValue VisitPercent(ParadoxParser.PercentContext context)
        {
            return new PercentValue(int.Parse(context.GetText().TrimEnd('%')));
        }

        #endregion
    }
}