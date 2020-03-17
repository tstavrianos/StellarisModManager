using System.Collections.Generic;
using Paradox.Common.Interfaces;

namespace Paradox.Common
{
    public sealed class DummyScriptedVariablesAccessor : IScriptedVariablesAccessor {
        /// <summary>
        /// Always returns the passed in value.
        /// </summary>
        public string GetPotentialValue(string rawValue) {
            return rawValue;
        }

        public IScriptedVariablesAccessor CreateNew(IEnumerable<CwKeyValue> node) {
            return this;
        }

        public void AddAdditionalFileVariables(CwNode node) {
        }

        public void Dispose() {
        }
    }

}