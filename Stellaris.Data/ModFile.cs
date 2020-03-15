using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Stellaris.Data.Parsers;
using Stellaris.Data.Parsers.pck;

namespace Stellaris.Data
{
    public sealed class ModFile
    {
        private static readonly Logger Log;

        static ModFile()
        {
#if DEBUG
            Log = new LoggerConfiguration()//
                .MinimumLevel.Debug()//
                .Enrich.WithExceptionDetails()//
                .Enrich.FromLogContext()//
                .WriteTo.File("ModFile.log")//
                .CreateLogger();//
#endif
        }

        private readonly ParseNode _tree;
        public bool Valid { get; }
        public Mod SourceMod { get; }

        public string Path { get; }

        public string Directory => System.IO.Path.GetDirectoryName(this.Path);
        public string Filename => System.IO.Path.GetFileName(this.Path);

        public ModFile(string path, Mod sourceMod, string filename)
        {
            this.Path = path;
            this.SourceMod = sourceMod;

            this.Valid = true;
            var text = System.IO.File.ReadAllText(filename);
            var lexer = new Tokenizer(text);
            var parser = new Parser(lexer);
            this._tree = parser.ParseReductions(true);
            if (this._tree.Symbol == "assignmentList" || this._tree.Symbol == "valueList")
                return;
            this.Valid = false;
            Log?.Error($"{filename} is not valid");
        }
    }
}