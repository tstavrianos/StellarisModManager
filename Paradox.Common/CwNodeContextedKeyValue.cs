using System;
using System.Collections.Generic;
using Paradox.Common.Interfaces;

namespace Paradox.Common
{
    internal sealed class CwNodeContextedKeyValue : ICwKeyValue 
    {
        private readonly ICwKeyValue _raw;
        private readonly IScriptedVariablesAccessor _accessor;

        internal CwNodeContextedKeyValue(ICwKeyValue raw, IScriptedVariablesAccessor accessor) {
            this._raw = raw;
            this._accessor = accessor;
        }
        public string Key => this._raw.Key;
        public string Value => this._accessor.GetPotentialValue(this._raw.Value);
        public CwNode ParentNode => this._raw.ParentNode;

        public KeyValuePair<string, string> ToKeyValue() {
            return new KeyValuePair<string, string>(this.Key, this.Value);
        }

        public bool Equals(KeyValuePair<string, string> obj, StringComparison comparison = StringComparison.Ordinal)
        {
            var (key, value) = this.ToKeyValue();
            return key.Equals(obj.Key, comparison) && value.Equals(obj.Value, comparison);
        }
    }
}