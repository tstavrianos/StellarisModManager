namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using System;

    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class SymbolValueVisitor : ParadoxBaseVisitor<SymbolValue>
    {
        #region Overrides of ParadoxBaseVisitor<SymbolValue>

        public override SymbolValue VisitSymbol(ParadoxParser.SymbolContext context)
        {
            if (context.STRING() != null) return new SymbolValue(context.Accept(new StringSymbolVisitor()));
            if (context.INT() != null) return new SymbolValue(context.Accept(new IntegerSymbolVisitor()));
            if (context.SYMBOL() != null) return new SymbolValue(new Symbol(context.GetText()));
            throw new Exception();
        }

        #endregion
    }
}