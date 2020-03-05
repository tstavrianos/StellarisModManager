namespace PDXModLib.Utilities
{
    using CWTools.CSharp;
    using CWTools.Process;

    internal sealed class CwToolsAdapter
    {

        private CwToolsAdapter(CK2Process.EventRoot eventRoot)
        {
            this.Root = eventRoot;
        }

        private CwToolsAdapter(string error)
        {
            this.ParseError = error;
        }

        public static CwToolsAdapter Parse(string file, string contents)
        {
            var result = CWTools.Parser.CKParser.parseEventString(contents, file);
            return result.IsSuccess ? new CwToolsAdapter(CWTools.Process.CK2Process.processEventFile(result.GetResult())) : new CwToolsAdapter(result.GetError());
        }

        public static CwToolsAdapter Parse(string file)
        {
            var result = CWTools.Parser.CKParser.parseEventFile(file);
            return result.IsSuccess ? new CwToolsAdapter(CWTools.Process.CK2Process.processEventFile(result.GetResult())) : new CwToolsAdapter(result.GetError());
        }

        public CK2Process.EventRoot Root { get; }
        public string ParseError { get; }
    }
}