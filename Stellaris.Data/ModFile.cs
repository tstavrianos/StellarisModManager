namespace Stellaris.Data
{
    public sealed class ModFile
    {
        public string Path { get; }
        public Mod SourceMod  { get; }

        public ModFile(string path, Mod sourceMod )
        {
            this.Path = path;
            this.SourceMod  = sourceMod;
        }
    }
}