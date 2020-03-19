using System;
using System.IO;
using ReactiveUI;

namespace Paradox.Common
{
    public sealed class  ModDataFile: ReactiveObject
    {
        private string _fullPath;
        private bool _valid;
        private string _parseError;

        public string Filename => Path.GetFileName(this.FullPath);
        public string Folder => Path.GetDirectoryName(this.FullPath);

        public string FullPath
        {
            get => this._fullPath;
            set => this.RaiseAndSetIfChanged(ref this._fullPath, value);
        }

        public bool Valid
        {
            get => this._valid;
            private set => this.RaiseAndSetIfChanged(ref this._valid , value);
        }

        public string ParseError
        {
            get =>this._parseError;
            private set => this.RaiseAndSetIfChanged(ref this._parseError, value);
        } 

        public ModDataFile(string fullPath, Stream stream)
        {
            this.FullPath = fullPath;
            var parser = new CwParserHelper();
            this.Valid = true;
            
            using (var reader = new StreamReader(stream, leaveOpen: true))
            {
                try
                {
                    parser.ParseParadoxString(fullPath, reader.ReadToEnd());
                }
                catch (Exception e)
                {
                    this.Valid = false;
                    this.ParseError = e.Message;
                }
            }
        }
    }
}