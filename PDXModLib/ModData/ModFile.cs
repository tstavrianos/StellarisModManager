using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using PDXModLib.Utility;
using static CWTools.Process.CK2Process;

namespace PDXModLib.ModData
{
    public abstract class ModFile
    {
        private static readonly Encoding NoBomEncoding = new UTF8Encoding(false, true);
        private static readonly string[] SCExtensions = { ".gfx", ".gui", ".txt" };

        private static readonly string[] CodeExtensions = { ".lua" };

        private static readonly string[] LocalisationExtensions = { ".yml" };

        public string Path { get; set; }

        public string Directory => System.IO.Path.GetDirectoryName(this.Path);
        public string Filename => System.IO.Path.GetFileName(this.Path);


        internal virtual Encoding BoMEncoding { get; } = NoBomEncoding;

        public Mod SourceMod { get; }

        protected ModFile(string path, Mod sourceMod)
        {
            this.Path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            this.SourceMod = sourceMod;
        }

        public abstract string RawContents { get; }

        internal static ModFile Load(IModFileLoader loader, string path, Mod sourceMod)
        {
            if (SCExtensions.Contains(System.IO.Path.GetExtension(path).ToLower()))
            {
                return new SCModFile(loader, path, sourceMod);
            }

            if (CodeExtensions.Contains(System.IO.Path.GetExtension(path).ToLower()))
            {
                return new CodeModFile(loader, path, sourceMod);
            }

            if (LocalisationExtensions.Contains(System.IO.Path.GetExtension(path).ToLower()))
            {
                return new LocalisationFile(loader, path, sourceMod);
            }

            return new BinaryModFile(loader, path, sourceMod);
        }

        protected static string NormalizeLineEndings(string source)
        {
            return Regex.Replace(source, @"\r\n|\n\r|\n|\r", Environment.NewLine);
        }

        public virtual void Save(IModFileSaver saver)
        {
            saver.Save(this.Path, this.RawContents, this.BoMEncoding);
        }

        public override string ToString()
        {
            return this.Filename;
        }
    }

    internal class SCModFile : ModFile
    {
        private readonly IModFileLoader _loader;
        private string _rawContents;
        internal EventRoot Contents { get; private set; }

        public override string RawContents
        {
            get
            {
                if (this._rawContents == null)
                {
                    this._rawContents = NormalizeLineEndings(this.LoadSCFileContents(this._loader));
                }
                return this._rawContents;
            }
        }

        internal bool ParseError { get; private set; }
        internal string ParseErrorMessage { get; private set; }

        public SCModFile(IModFileLoader loader, string path, Mod sourceMod)
            : base(path, sourceMod)
        {
            this._loader = loader;
        }

        private string LoadSCFileContents(IModFileLoader loader)
        {
            using (var stream = loader.OpenStream())
            {
                using (var sr = new StreamReader(stream))
                {
                    var contents = sr.ReadToEnd();

                    var adapter = CWToolsAdapter.Parse(this.Path, contents);

                    this.ParseError = adapter.ParseError != null;
                    this.ParseErrorMessage = adapter.ParseError;
                    this.Contents = adapter.Root;
                    return contents;
                }
            }
        }
    }

    internal class CodeModFile : ModFile
    {
        private readonly IModFileLoader _loader;
        public string Contents { get; set; }
        private string _rawContents;

        public override string RawContents
        {
            get
            {
                if (this._rawContents == null)
                {
                    using (var stream = this._loader.OpenStream())
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            this._rawContents = NormalizeLineEndings(sr.ReadToEnd());
                        }
                    }
                }
                return this._rawContents;
            }
        }

        public CodeModFile(IModFileLoader loader, string path, Mod sourceMod)
            : base(path, sourceMod)
        {
            this._loader = loader;
        }
    }

    internal class LocalisationFile : ModFile
    {
        private readonly IModFileLoader _loader;
        public string Contents { get; set; }
        private string _rawContents;

        internal override Encoding BoMEncoding { get; } = Encoding.UTF8;

        public override string RawContents
        {
            get
            {
                if (this._rawContents == null)
                {
                    using (var stream = this._loader.OpenStream())
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            this._rawContents = NormalizeLineEndings(sr.ReadToEnd());
                        }
                    }
                }
                return this._rawContents;
            }
        }

        public LocalisationFile(IModFileLoader loader, string path, Mod sourceMod)
            : base(path, sourceMod)
        {
            this._loader = loader;
        }
    }

    internal class BinaryModFile : ModFile
    {
        private readonly IModFileLoader _loader;
        public string Contents => "BinaryModFile";
        public override string RawContents => this.Contents;

        public BinaryModFile(IModFileLoader loader, string path, Mod sourceMod)
            : base(path, sourceMod)
        {
            this._loader = loader;
        }

        public override void Save(IModFileSaver saver)
        {
            saver.Save(this.Path, this._loader.OpenStream);
        }
    }

    public class MergedModFile : ModFile
    {
        string contents;
        private readonly List<ModFile> _sourceFiles;

        internal override Encoding BoMEncoding { get; }

        public override string RawContents => this.GetContents();

        public IReadOnlyList<ModFile> SourceFiles => this._sourceFiles;

        public int SourceFileCount => this.SourceFiles.Count;

        public bool Resolved => this.contents != null ? this.SourceFileCount == 0 : this.SourceFileCount == 1;

        public MergedModFile(string path, IEnumerable<ModFile> source, Mod sourceMod)
            : base(path, sourceMod)
        {
            this._sourceFiles = source.ToList();
            this.BoMEncoding = this._sourceFiles[0].BoMEncoding;
        }

        public void SaveResult(string toSave)
        {
            this.contents = toSave;
        }

        public void RemoveSourceFile(ModFile file)
        {
            this._sourceFiles.Remove(file);
        }

        public override void Save(IModFileSaver saver)
        {
            if (this.Resolved)
                base.Save(saver);
            else
            {
                var extension = System.IO.Path.GetExtension(this.Path);
                var newPath = System.IO.Path.ChangeExtension(this.Path, $"{extension}.mzip");
                saver.Save(newPath, this.CreateMergeZip);
            }
        }

        private string GetContents()
        {
            if (this.contents != null)
                return this.contents;

            if (this.SourceFileCount == 1)
                return this.SourceFiles[0].RawContents;

            return null;
        }

        private MemoryStream CreateMergeZip()
        {
            var result = new MemoryStream();
            using (var saver = new MergeZipFileSaver(result))
            {
                foreach (var sourceFile in this.SourceFiles)
                {
                    sourceFile.Save(saver);
                }
            }

            return result;
        }

        private class MergeZipFileSaver : IModFileSaver
        {
            private int _index;
            private readonly ZipFile _zipFile;

            public MergeZipFileSaver(Stream outputStream)
            {
                this._zipFile = ZipFile.Create(outputStream);
                this._zipFile.BeginUpdate(new MemoryArchiveStorage());
            }

            public void Save(string path, Func<Stream> getStream)
            {
                this._zipFile.Add(new FunctorDataSource(getStream), this.GetPath(path));
            }

            public void Save(string path, string text, Encoding encoding)
            {
                this._zipFile.Add(new FunctorDataSource(text, encoding), this.GetPath(path));
            }

            public void Dispose()
            {
                this._zipFile.CommitUpdate();
                this._zipFile?.Close();
            }

            private string GetPath(string path)
            {
                var filename = System.IO.Path.GetFileName(path);
                var i = this._index++;
                return $"{i:00}/{filename}";
            }
        }
    }
}
