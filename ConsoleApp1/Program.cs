using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellaris.Data.Parsers.Tokenizer;
using Parser = Stellaris.Data.Parsers.Parser;

namespace ConsoleApp1
{
    class Program
    {
        static void Run(string file)
        {
            var s = new Tokenizer();
            var p = new Parser();
            if(Path.GetFileName(file) == "HOW_TO_MAKE_NEW_SHIPS.txt")return;
            if(Path.GetFileName(file) == "readme.txt")return;
            if(Path.GetFileName(file) == "README_weapon_component_stat_docs.txt")return;
            if(Path.GetFileName(file) == "README.txt")return;
            if(Path.GetFileName(file).EndsWith("_sc.txt", StringComparison.OrdinalIgnoreCase)) return;
            try
            {
                var text = File.ReadAllText(file, CodePagesEncodingProvider.Instance.GetEncoding(1252));
                var b = s.Tokenize(text);
                if (b.Count <= 1) return;
                var c = p.Parse(b);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{file}, {e}");
            }
        }
        
        static async Task Main(string[] args)
        {
            var BasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
            if (!Directory.Exists(BasePath)) BasePath = @"C:\usefull\Newfolder\git\StellarisModManager\Stellaris";
            var ModPath = Path.Combine(BasePath, "mod");
            var tasks = new List<Task>();

            foreach (var file in Directory.EnumerateFiles(ModPath, "*.txt", SearchOption.AllDirectories))
            {
                tasks.Add(Task.Run(() => Run(file)));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
