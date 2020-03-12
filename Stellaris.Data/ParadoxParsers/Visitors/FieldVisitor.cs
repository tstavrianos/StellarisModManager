namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using System;

    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class FieldVisitor : ParadoxBaseVisitor<IField>
    {
        #region Overrides of ParadoxBaseVisitor<IField>

        public override IField VisitField(ParadoxParser.FieldContext context)
        {
            if (context.@string() != null) return context.@string().Accept(new StringFieldVisitor());
            if (context.symbol() != null) return context.symbol().Accept(new SymbolFieldVisitor());
            throw new Exception();
        }

        #endregion
    }
}