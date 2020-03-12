namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class RealVisitor : ParadoxBaseVisitor<RealValue>
    {
        #region Overrides of ParadoxBaseVisitor<RealValue>

        public override RealValue VisitReal(ParadoxParser.RealContext context)
        {
            return new RealValue(double.Parse(context.GetText()));
        }

        #endregion
    }
}