namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using System;

    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class ValueVisitor : ParadoxBaseVisitor<IValue>
    {
        #region Overrides of ParadoxBaseVisitor<IValue>

        public override IValue VisitValue(ParadoxParser.ValueContext context)
        {
            if (context.integer() != null) return context.integer().Accept(new IntegerValueVisitor());
            if (context.percent() != null) return context.percent().Accept(new PercentVisitor());
            if (context.real() != null) return context.real().Accept(new RealVisitor());
            if (context.date() != null) return context.date().Accept(new DateVisitor());
            if (context.@string() != null) return context.@string().Accept(new StringValueVisitor());
            if (context.symbol() != null) return context.symbol().Accept(new SymbolValueVisitor());
            if (context.map() != null) return context.map().Accept(new MapVisitor());
            if (context.array() != null) return context.array().Accept(new ArrayVisitor());
            throw new Exception();
        }

        #endregion
    }
}