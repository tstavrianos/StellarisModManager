using System;
using System.Linq;
using Stellaris.Data.Antlr;

namespace Stellaris.Data.Parser
{
    public sealed class EntryVisitor : ParadoxBaseVisitor<IEntry>
    {
        public override IEntry VisitSymbol(ParadoxParser.SymbolContext context)
        {
            switch (context.GetText().ToLower())
            {
                case "true": return new BoolEntry(true);
                case "false": return new BoolEntry(false);
                default:
                    return new StringEntry(context.GetText());
            }
        }

        public override IEntry VisitString(ParadoxParser.StringContext context)
        {
            return new StringEntry(context.GetText().Strip());
        }

        public override IEntry VisitInteger(ParadoxParser.IntegerContext context)
        {
            return new IntegerEntry(long.Parse(context.GetText()));
        }

        public override IEntry VisitReal(ParadoxParser.RealContext context)
        {
            return new RealEntry(double.Parse(context.GetText()));
        }

        public override IEntry VisitDate(ParadoxParser.DateContext context)
        {
            return new DateEntry(DateTime.ParseExact(context.GetText(), "yyyy.MM.dd", null));
        }

        public override IEntry VisitPercent(ParadoxParser.PercentContext context)
        {
            return new PercentEntry(int.Parse(context.GetText().TrimEnd('%')));
        }

        public override IEntry VisitArray(ParadoxParser.ArrayContext context)
        {
            var ret = context.value().Select(it => it.Accept(this)).ToList();
            return new ArrayEntry(ret);
        }

        public override IEntry VisitMap(ParadoxParser.MapContext context)
        {
            return context.assignment().ToBlock();
        }
    }
}