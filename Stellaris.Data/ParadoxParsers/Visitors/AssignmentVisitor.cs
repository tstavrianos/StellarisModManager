namespace Stellaris.Data.ParadoxParsers.Visitors
{
    using System;

    using Stellaris.Data.ParadoxParsers.Types;

    public sealed class AssignmentVisitor : ParadoxBaseVisitor<Assignment>
    {
        #region Overrides of ParadoxBaseVisitor<Assignment>

        public override Assignment VisitAssignment(ParadoxParser.AssignmentContext context)
        {
            Operator o;
            switch (context.OPERATOR().GetText())
            {
                case "=":
                    o = Operator.Equal;
                    break;
                case "<>":
                    o = Operator.NotEqual;
                    break;
                case ">":
                    o = Operator.Greater;
                    break;
                case "<":
                    o = Operator.Less;
                    break;
                case "<=":
                    o = Operator.LessEqual;
                    break;
                case ">=":
                    o = Operator.GreaterEqual;
                    break;
                default:
                    throw new Exception();
            }

            var field = context.field().Accept(new FieldVisitor());
            var value = context.value().Accept(new ValueVisitor());
            return new Assignment(field, o, value);
        }

        #endregion
    }
}