namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class StringSymbolVisitor : ParadoxBaseVisitor<StringSymbol>
    {
        #region Overrides of ParadoxBaseVisitor<StringSymbol>

        public override StringSymbol VisitSymbol(ParadoxParser.SymbolContext context)
        {
            return new StringSymbol(context.GetText());
        }

        #endregion
    }
}