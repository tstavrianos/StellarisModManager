using System;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Stellaris.Data.Parsers;
using Stellaris.Data.Parsers.Models;
using Stellaris.Data.Parsers.Tokenizer;

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

        private readonly Config _config;
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
            try
            {
                var lexer = new Tokenizer();
                var tokens = lexer.Tokenize(text);
                var parser = new Parser();
                this._config = parser.Parse(tokens);
            }
            catch (Exception ex)
            {
                Log?.Error(ex, "ModFile");
                this.Valid = false;
            }
        }
    }
}