using System;
using System.IO;
using System.Text.RegularExpressions;
using Antlr4.Runtime;

namespace ConsoleApp1
{
    using Stellaris.Data.ParadoxParsers.Visitors;

    class Program
    {
        static void Main(string[] args)
        {
            var regex = new Regex(
                "((?<name>.*?)\\=\\\"(?<value>.*?)\\\")|((?<name>.*?)\\=\\s*\\{\\s*(?<value>(\\s*\\\"(?<item>.*?)\\\"\\s*)+)\\s*\\})",
                RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            var BasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
            if (!Directory.Exists(BasePath)) BasePath = @"C:\usefull\Newfolder\git\StellarisModManager\Stellaris";
            var ModPath = Path.Combine(BasePath, "mod");
            foreach (var file in Directory.EnumerateFiles(ModPath, "*.mod"))
            {
                var text = File.ReadAllText(file);
                try
                {
                    Console.WriteLine(file);
                    var lexer = new ParadoxLexer(new AntlrInputStream(text));
                    var commonTokenStream = new CommonTokenStream(lexer);
                    var parser = new ParadoxParser(commonTokenStream);
                    var c = parser.config().Accept(new ConfigVisitor());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{file}, Error: " + ex);
                }
            }
        }
    }
}
