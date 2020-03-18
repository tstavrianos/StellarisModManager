using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CWTools.CSharp;
using CWTools.Parser;
using CWTools.Process;
using Paradox.Common.Interfaces;
using Splat;

namespace Paradox.Common
{
     /// <summary>
    /// Main Helper class for using the CWTools library to parse general PDX files into a (raw) DTO.
    /// </summary>
    public sealed class CwParserHelper : ICwParserHelper, IEnableLogger {
        private readonly IScriptedVariablesAccessor _scriptedVariablesAccessor;

        static CwParserHelper()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Create a CWParserHelper that will not attempt to resolve in files.
        /// </summary>
        public CwParserHelper() : this(new DummyScriptedVariablesAccessor()) {
        }
        
        /// <summary>
        /// Create a CWParserHelper where the nodes will attempt to resolve variables using the specified <see cref="IScriptedVariablesAccessor"/>.
        /// </summary>
        public CwParserHelper(IScriptedVariablesAccessor scriptedVariablesAccessor) {
            this._scriptedVariablesAccessor = scriptedVariablesAccessor;
        }
        
        /// <inheritdoc />
        public IDictionary<string, CwNode> ParseParadoxFiles(IEnumerable<string> filePaths, bool continueOnFailure = false)
        {
            var result = new Dictionary<string, CwNode>();
            foreach (var paradoxFile in filePaths)
            {
                try {
                    result[paradoxFile] = (this.ParseParadoxFile(paradoxFile));
                }
                catch (Exception e) {
                    if (continueOnFailure) {
                        this.Log().Error(e, "Error parsing file {file}", paradoxFile);
                    }
                    else {
                        throw;
                    }
                }
            }
            return result;
        }

        /// <inheritdoc />
        public CwNode ParseParadoxFile(string filePath)
        {
            // raw parsing
            var parsed = CKParser.parseEventFile(filePath);

            if (!parsed.IsSuccess) throw new Exception(parsed.GetError());
            // this is an extension method in CWTools.CSharp
            var eventFile = parsed.GetResult();

            //"Process" result into nicer format
            var processed = CK2Process.processEventFile(eventFile);

            // marshall this into a more c# fieldy type using the CWTools example
            var marshaled = this.ToMyNode(processed);

            return marshaled;

        }
        
        private CwNode ToMyNode(Node n)
        {
            var leaves = n.AllChildren.Where(x => x.IsLeafC).Select(x => ToMyKeyValue(x.leaf)).ToList();
            var tempAccessor = this._scriptedVariablesAccessor.CreateNew(leaves);
            var nodes = n.AllChildren.Where(x => x.IsNodeC).Select(x => this.ToMyNode(x.node, tempAccessor)).ToList();
            var values = n.AllChildren.Where(x => x.IsLeafValueC).Select(x => x.lefavalue.Key).ToList();
            return new CwNode(n.Key) { Nodes = nodes, Values = values, RawKeyValues = leaves, ScriptedVariablesAccessor = tempAccessor};
        }
        
        private CwNode ToMyNode(Node n, IScriptedVariablesAccessor sa)
        {
            var nodes = n.AllChildren.Where(x => x.IsNodeC).Select(x => this.ToMyNode(x.node, sa)).ToList();
            var leaves = n.AllChildren.Where(x => x.IsLeafC).Select(x => ToMyKeyValue(x.leaf)).ToList();
            var values = n.AllChildren.Where(x => x.IsLeafValueC).Select(x => x.lefavalue.Key).ToList();
            return new CwNode(n.Key) { Nodes = nodes, Values = values, RawKeyValues = leaves, ScriptedVariablesAccessor = sa};
        }


        private static CwKeyValue ToMyKeyValue(Leaf l)
        {
            return new CwKeyValue { Key = l.Key, Value = l.Value.ToRawString() };
        }
    }
}