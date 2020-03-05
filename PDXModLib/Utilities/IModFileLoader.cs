namespace PDXModLib.Utilities
{
    using System.IO;

    using ICSharpCode.SharpZipLib.Zip;

    interface IModFileLoader
    {
        Stream OpenStream();
    }
    
    public sealed class DiskFileLoader : IModFileLoader
    {
        private readonly string _path;

        public Stream OpenStream()
        {
            return File.OpenRead(this._path);
        }

        public DiskFileLoader(string path)
        {
            this._path = path;
        }
    }

    public sealed class ZipFileLoader : IModFileLoader
    {
        private readonly ZipFile _file;
        private readonly ZipEntry _zipEntry;

        public Stream OpenStream()
        {
            return this._file.GetInputStream(this._zipEntry);
        }

        public ZipFileLoader(ZipFile file, ZipEntry zipEntry)
        {
            this._file = file;
            this._zipEntry = zipEntry;
        }
    }
}