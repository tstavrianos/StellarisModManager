namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using System;

    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class DateVisitor : ParadoxBaseVisitor<DateValue>
    {
        #region Overrides of ParadoxBaseVisitor<DateValue>

        public override DateValue VisitDate(ParadoxParser.DateContext context)
        {
            return new DateValue(DateTime.ParseExact(context.GetText(), "yyyy.MM.dd", null));
        }

        #endregion
    }
}