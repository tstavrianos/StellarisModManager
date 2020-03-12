namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using System;

    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class SymbolFieldVisitor : ParadoxBaseVisitor<SymbolField>
    {
        #region Overrides of ParadoxBaseVisitor<IField>

        public override SymbolField VisitSymbol(ParadoxParser.SymbolContext context)
        {
            if (context.STRING() != null) return new SymbolField(context.Accept(new StringSymbolVisitor()));
            if (context.INT() != null) return new SymbolField(context.Accept(new IntegerSymbolVisitor()));
            if (context.SYMBOL() != null) return new SymbolField(new Symbol(context.GetText()));
            throw new Exception();
        }

        #endregion
    }
}