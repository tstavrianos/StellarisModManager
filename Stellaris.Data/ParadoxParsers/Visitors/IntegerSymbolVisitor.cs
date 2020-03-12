namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class IntegerSymbolVisitor : ParadoxBaseVisitor<IntegerSymbol>
    {
        #region Overrides of ParadoxBaseVisitor<IntegerSymbol>

        public override IntegerSymbol VisitInteger(ParadoxParser.IntegerContext context)
        {
            return new IntegerSymbol(long.Parse(context.GetText()));
        }

        #endregion
    }
}