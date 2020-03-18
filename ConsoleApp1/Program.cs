using System;
using System.IO;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Run(string file)
        {
            if (Path.GetFileName(file) == "HOW_TO_MAKE_NEW_SHIPS.txt") return;
            if (Path.GetFileName(file) == "readme.txt") return;
            if (Path.GetFileName(file) == "README_weapon_component_stat_docs.txt") return;
            if (Path.GetFileName(file) == "README.txt") return;
            if (Path.GetFileName(file).EndsWith("_sc.txt", StringComparison.OrdinalIgnoreCase)) return;
            var text = File.ReadAllText(file, CodePagesEncodingProvider.Instance.GetEncoding(1252));
        }

        static void Main(string[] args)
        {
            var BasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
            if (!Directory.Exists(BasePath)) BasePath = @"C:\usefull\Newfolder\git\StellarisModManager\Stellaris";
            var ModPath = Path.Combine(BasePath, "mod");

            foreach (var file in Directory.EnumerateFiles(ModPath, "*.mod", SearchOption.AllDirectories))
            {
                Run(file);
            }
        }
    }
}
