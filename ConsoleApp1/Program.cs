using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Paradox.Common.Parsers.pck;

namespace ConsoleApp1
{
    class Program
    {
        private static void PrintNode(ParseNode node, string ident)
        {
            Console.WriteLine($"{ident}Symbol = {node.Symbol}, Value = {node.Value} ");
            foreach (var child in node.Children) PrintNode(child, ident + '\t');
        }

        static void Run(string file)
        {
            if (Path.GetFileName(file) == "HOW_TO_MAKE_NEW_SHIPS.txt") return;
            if (Path.GetFileName(file) == "readme.txt") return;
            if (Path.GetFileName(file) == "README_weapon_component_stat_docs.txt") return;
            if (Path.GetFileName(file) == "README.txt") return;
            if (Path.GetFileName(file).EndsWith("_sc.txt", StringComparison.OrdinalIgnoreCase)) return;
            var text = File.ReadAllText(file, CodePagesEncodingProvider.Instance.GetEncoding(1252));
            var t = new Tokenizer(text);
            var parser = new Parser(t);
            var tree = parser.ParseReductions(true); // pass true if you want the tree to be trimmed.
            PrintNode(tree, String.Empty);
        }

        static async Task Main(string[] args)
        {
            var BasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
            if (!Directory.Exists(BasePath)) BasePath = @"C:\usefull\Newfolder\git\StellarisModManager\Stellaris";
            var ModPath = Path.Combine(BasePath, "mod");
            //ModPath = @"h:\Steam\steamapps\common\Stellaris\common\";
            var tasks = new List<Task>();

            foreach (var file in Directory.EnumerateFiles(ModPath, "*.mod", SearchOption.AllDirectories))
            {
                //tasks.Add(Task.Run(() => Run(file)));
                Run(file);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
