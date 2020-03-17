using Serilog;
using ReactiveUI;

namespace Paradox.Common.Models
{
    public sealed class ModDataFile: ReactiveObject
    {
        private readonly Parsers.pck.ParseNode _tree;
        private bool _valid;
        private ModData _sourceMod;
        private string _path;
        private readonly ILogger _logger;

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

        internal ModDataFile(string path, ModData sourceModData, string filename, ILogger logger = null)
        {
            this._logger = logger;
            this.Path = path;
            this.SourceMod = sourceModData;

            this.Valid = true;
            var text = System.IO.File.ReadAllText(filename);
            var lexer = new Parsers.Tokenizer(text);
            var parser = new Parsers.Parser(lexer);
            this._tree = parser.ParseReductions(true);
            if (this._tree.Symbol == "assignmentList" || this._tree.Symbol == "valueList")
                return;
            this.Valid = false;
            this._logger?.Error($"{filename} is not valid");
        }
    }
}