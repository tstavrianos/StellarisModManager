namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class StringFieldVisitor : ParadoxBaseVisitor<StringField>
    {
        #region Overrides of ParadoxBaseVisitor<StringField>

        public override StringField VisitString(ParadoxParser.StringContext context)
        {
            return new StringField(context.GetText());
        }

        #endregion
    }
}