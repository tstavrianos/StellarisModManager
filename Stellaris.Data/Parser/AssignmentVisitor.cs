using Stellaris.Data.Antlr;

namespace Stellaris.Data.Parser
{
    public sealed class AssignmentVisitor : ParadoxBaseVisitor<ConfigAssignment>
    {
        public override ConfigAssignment VisitAssignment(ParadoxParser.AssignmentContext context)
        {
            var field = context.field().GetText().Strip();
            var value = context.value().Accept(new EntryVisitor());
            return new ConfigAssignment(field, value);
        }
    }
}