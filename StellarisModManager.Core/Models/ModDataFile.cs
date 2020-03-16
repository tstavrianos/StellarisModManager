using ReactiveUI;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using ParseNode = StellarisModManager.Core.Parsers.pck.ParseNode;
using Parser = StellarisModManager.Core.Parsers.Parser;
using Tokenizer = StellarisModManager.Core.Parsers.Tokenizer;

namespace StellarisModManager.Core.Models
{
    public sealed class ModDataFile: ReactiveObject
    {
        private static readonly Logger Log;

        static ModDataFile()
        {
#if DEBUG
            Log = new LoggerConfiguration()//
                .MinimumLevel.Debug()//
                .Enrich.WithExceptionDetails()//
                .Enrich.FromLogContext()//
                .WriteTo.File("ModDataFile.log")//
                .CreateLogger();//
#endif
        }

        private readonly ParseNode _tree;
        private bool _valid;
        private ModData _sourceMod;
        private string _path;

        public bool Valid
        {
            get => this._valid;
            set => this.RaiseAndSetIfChanged(ref this._valid, value);
        }

        public ModData SourceMod
        {
            get => this._sourceMod;
            set => this.RaiseAndSetIfChanged(ref this._sourceMod, value);
        }

        public string Path
        {
            get => this._path;
            set => this.RaiseAndSetIfChanged(ref this._path, value);
        }

        public string Directory => System.IO.Path.GetDirectoryName(this.Path);
        public string Filename => System.IO.Path.GetFileName(this.Path);

        internal ModDataFile(string path, ModData sourceModData, string filename)
        {
            this.Path = path;
            this.SourceMod = sourceModData;

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