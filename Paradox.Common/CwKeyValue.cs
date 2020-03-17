using System;
using System.Collections.Generic;
using Paradox.Common.Interfaces;

namespace Paradox.Common
{
    /// <summary>
    /// A straight key = value entry in the file/node: e.g: tier = 1
    /// </summary>
    public sealed class CwKeyValue : ICwKeyValue 
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public CwNode ParentNode { get; set; }

        public KeyValuePair<string, string> ToKeyValue() { return new KeyValuePair<string, string>(this.Key, this.Value);}
        
        public bool Equals(KeyValuePair<string, string> obj, StringComparison comparison = StringComparison.Ordinal)
        {
            var (key, value) = this.ToKeyValue();
            return key.Equals(obj.Key, comparison) && value.Equals(obj.Value, comparison);
        }
    }
}