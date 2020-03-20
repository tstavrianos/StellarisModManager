using System.Collections.Generic;
using System.IO;
using System.Linq;
using Paradox.Common.Extensions;
using Paradox.Common.Interfaces;
using Splat;

namespace Paradox.Common
{
    /// <summary>
    /// Manages scripted variables from the core game and mods.
    /// </summary>
    public class ScriptedVariableAccessor : IScriptedVariablesAccessor, IEnableLogger
    {
        private IDirectoryWalker DirectoryWalker { get; }
        private ICwParserHelper CwParserHelper { get; }

        private readonly IDictionary<string, string> _variables;

        public ScriptedVariableAccessor(StellarisDirectoryHelper stellarisDirectoryHelper, bool continueOnError = false) :
            this(stellarisDirectoryHelper, new StellarisDirectoryHelper[] { }, continueOnError)
        {
        }

        public ScriptedVariableAccessor(StellarisDirectoryHelper stellarisDirectoryHelper,
            IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers, bool continueOnError = false) :
            this(stellarisDirectoryHelper, modDirectoryHelpers, new DirectoryWalker(), new CwParserHelper(), continueOnError)
        {
        }

        internal ScriptedVariableAccessor(StellarisDirectoryHelper stellarisDirectoryHelper,
            IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers,
            IDirectoryWalker directoryWalker,
            ICwParserHelper cwParserHelper, bool continueOnError = false, ILogger logger = null)
        {
            this.DirectoryWalker = directoryWalker;
            this.CwParserHelper = cwParserHelper;

            this._variables = new Dictionary<string, string>();
            foreach (var directoryHelper in StellarisDirectoryHelper.CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers))
            {
                if (Directory.Exists(directoryHelper.ScriptedVariables))
                {
                    var modVariables = this.ParseScriptedVariables(directoryHelper.ScriptedVariables, continueOnError);
                    this._variables.PutAll(modVariables);
                }
                else
                {
                    this.Log().Debug("{0} does not contain scripted variables", directoryHelper.ModName);
                }

            }
        }

        private ScriptedVariableAccessor(IEnumerable<CwKeyValue> keyValues, ICwParserHelper cwParserHelper)
        {
            this.DirectoryWalker = null;
            this.CwParserHelper = cwParserHelper;
            this._variables = new Dictionary<string, string>();
            keyValues.Where(kv => IsVariable(kv.Key)).ForEach(kv => this._variables[kv.Key] = kv.Value);
        }

        /// <inheritdoc />
        public string GetPotentialValue(string rawValue)
        {
            if (rawValue == null || !IsVariable(rawValue) || !this._variables.ContainsKey(rawValue)) return rawValue;
            var value = this._variables[rawValue];
            return IsVariable(value) ? this.GetPotentialValue(value) : value;

        }

        /*private bool Contains(string key) {
            return this._variables.ContainsKey(key);
        }*/

        public static bool IsVariable(string key)
        {
            return key.StartsWith('@');
        }

        public void AddAdditionalFileVariables(CwNode node)
        {
            node.RawKeyValues.Where(kv => IsVariable(kv.Key)).ForEach(kv => this._variables[kv.Key] = kv.Value);
        }

        public IScriptedVariablesAccessor CreateNew(IEnumerable<CwKeyValue> keyValues)
        {
            return new DelegatingScriptedVariablesAccessor(new ScriptedVariableAccessor(keyValues, this.CwParserHelper), this);
        }


        private sealed class DelegatingScriptedVariablesAccessor : IScriptedVariablesAccessor
        {
            private readonly IScriptedVariablesAccessor _primary;
            private readonly IScriptedVariablesAccessor _fallback;

            internal DelegatingScriptedVariablesAccessor(IScriptedVariablesAccessor primary, IScriptedVariablesAccessor fallback)
            {
                this._primary = primary;
                this._fallback = fallback;
            }
            public void Dispose()
            {
            }

            public string GetPotentialValue(string rawValue)
            {
                var potentialValue = this._primary.GetPotentialValue(rawValue);
                return potentialValue == rawValue ? this._fallback.GetPotentialValue(rawValue) : potentialValue;
            }

            public IScriptedVariablesAccessor CreateNew(IEnumerable<CwKeyValue> node)
            {
                return new DelegatingScriptedVariablesAccessor(this._primary.CreateNew(node), this._fallback);
            }

            public void AddAdditionalFileVariables(CwNode node)
            {
                this._primary.AddAdditionalFileVariables(node);
            }
        }

        private Dictionary<string, string> ParseScriptedVariables(string scriptedVariableDir, bool continueOnError = false)
        {
            var techFiles = this.DirectoryWalker.FindFilesInDirectoryTree(scriptedVariableDir, StellarisDirectoryHelper.TextMask);
            var parsedTechFiles = this.CwParserHelper.ParseParadoxFiles(techFiles, continueOnError);
            var result = new Dictionary<string, string>();
            foreach (var file in parsedTechFiles.Values)
            {
                // top level nodes are files, so we process the immediate children of each file, which is the individual variables.
                this.AddAdditionalFileVariables(file);
            }
            return result;
        }

        public void Dispose()
        {
        }
    }
}