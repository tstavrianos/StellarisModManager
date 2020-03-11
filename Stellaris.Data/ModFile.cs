using System;
using System.IO;
using Antlr4.Runtime;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Stellaris.Data.Antlr;
using Stellaris.Data.Parser;

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

        private readonly MapEntry _mapEntry;
        public bool Valid { get; private set; }
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
                var lexer = new ParadoxLexer(new AntlrInputStream(text));
                var commonTokenStream = new CommonTokenStream(lexer);
                var parser = new ParadoxParser(commonTokenStream);
                this._mapEntry = parser.config().ToConfigBlock();
            }
            catch (Exception ex)
            {
                Log?.Error(ex, "ModFile");
                this.Valid = false;
            }
        }
    }
}