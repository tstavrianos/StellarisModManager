namespace PDXModLib.Utilities
{
    using System;
    using System.IO;
    using System.Text;

    using ICSharpCode.SharpZipLib.Zip;

    public interface IModFileSaver: IDisposable
    {
        void Save(string path, Func<Stream> getStream);
        void Save(string path, string text, Encoding encoding);
    } 

    internal sealed class DiskFileSaver : IModFileSaver
    {
        private readonly string _basePath;

        public DiskFileSaver(string basePath)
        {
	        this._basePath = basePath;
        }

        public void Save(string path, Func<Stream> getStream)
        {
            path = Path.Combine(this._basePath, path);
            VerifyDir(path);
			using (var stream = getStream())
			{
				using (var fileS = File.OpenWrite(path))
				{
					stream.CopyTo(fileS);
				}
			}
        }

        public void Save(string path, string text, Encoding encoding)
        {
            path = Path.Combine(this._basePath, path);
            VerifyDir(path);
            File.WriteAllText(path, text, encoding);
        }

        public void Dispose()
        {
            // do nothing, nothing to dispose of
        }

        private static void VerifyDir(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }

    internal sealed class FunctorDataSource : IStaticDataSource
	{
		private readonly Func<Stream> _accessor;

		public FunctorDataSource(Func<Stream> accessor)
		{
			this._accessor = accessor;
		}

		public FunctorDataSource(string contents, Encoding encoding)
		{
			this._accessor = () => new MemoryStream(encoding.GetBytes(contents));
		}

		public Stream GetSource()
		{
			return this._accessor();
		}
	}

    internal class ZipFileSaver : IModFileSaver
    {
        private readonly ZipFile _zipFile;

        public ZipFileSaver(string targetPath)
        {
	        this._zipFile = ZipFile.Create(File.OpenWrite(targetPath));
	        this._zipFile.BeginUpdate(new MemoryArchiveStorage() );
        }

        public void Save(string path, Func<Stream> getStream)
        {
	        this._zipFile.Add(new FunctorDataSource(getStream), path);
        }

        public void Save(string path, string text, Encoding encoding)
        {
	        this._zipFile.Add(new FunctorDataSource(text, encoding), path);
        }

        public void Dispose()
        {
	        this._zipFile.CommitUpdate();
	        this._zipFile?.Close();
        }
	}
}