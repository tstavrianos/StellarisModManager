using Stellaris.Data.Antlr;

namespace Stellaris.Data.Parser
{
    public sealed class ConfigVisitor : ParadoxBaseVisitor<MapEntry>
    {
        public override MapEntry VisitConfig(ParadoxParser.ConfigContext context)
        {
            return context.assignment().ToBlock();
        }
    }
}