namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class StringValueVisitor : ParadoxBaseVisitor<StringValue>
    {
        #region Overrides of ParadoxBaseVisitor<StringValue>

        public override StringValue VisitString(ParadoxParser.StringContext context)
        {
            return new StringValue(context.GetText());
        }

        #endregion
    }
}