using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Serilog;
using Serilog.Core;
using Stellaris.Data.Antlr;
using Stellaris.Data.Parser;

namespace ConsoleApp1
{
    class Program
    {

        static void Main(string[] args)
        {
            var regex = new Regex(
                "((?<name>.*?)\\=\\\"(?<value>.*?)\\\")|((?<name>.*?)\\=\\s*\\{\\s*(?<value>(\\s*\\\"(?<item>.*?)\\\"\\s*)+)\\s*\\})",
                RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
           //var BasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
           var BasePath = @"C:\usefull\Newfolder\git\StellarisModManager\Stellaris";
           var ModPath = Path.Combine(BasePath, "mod");
           foreach (var file in Directory.EnumerateFiles(ModPath, "*.mod"))
           {
               var text = File.ReadAllText(file);
               try
               {
                   var lexer = new ParadoxLexer(new AntlrInputStream(text));
                   var commonTokenStream = new CommonTokenStream(lexer);
                   var parser = new ParadoxParser(commonTokenStream);
                   var c = parser.config().ToConfigBlock();
                   foreach (var c1 in c)
                   {
                       Console.WriteLine($"{c1.Key} =>");
                       foreach (var v in c1.Value)
                       {
                           Console.WriteLine($"\t{v}");
                       }
                   }
               }
               catch (Exception ex)
               {
                   Console.WriteLine("Error: " + ex);
               }
           }
        }
    }
}
