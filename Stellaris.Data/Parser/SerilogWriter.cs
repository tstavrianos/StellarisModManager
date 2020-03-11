using System.IO;
using System.Text;

namespace Stellaris.Data.Parser
{
    public class SerilogWriter: TextWriter
    {
        public override Encoding Encoding { get; }
    }
}