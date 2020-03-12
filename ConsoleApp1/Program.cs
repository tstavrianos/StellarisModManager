using System;
using System.IO;
using Stellaris.Data.Parsers.Tokenizer;
using Parser = Stellaris.Data.Parsers.Parser;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var BasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
            if (!Directory.Exists(BasePath)) BasePath = @"C:\usefull\Newfolder\git\StellarisModManager\Stellaris";
            var ModPath = Path.Combine(BasePath, "mod");
            var s = new SimpleRegexTokenizer();
            var p = new Parser();
            foreach (var file in Directory.EnumerateFiles(ModPath, "*.mod"))
            {
                Console.WriteLine(file);
                var text = File.ReadAllText(file);
                var b = s.Tokenize(text);
                var c = p.Parse(b);
            }
        }
    }
}
