using System.Collections.Generic;

namespace Paradox.Common.Interfaces
{
    public interface ICwKeyValue {
        string Key { get; }
        string Value { get; }
        CwNode ParentNode { get; }

        KeyValuePair<string, string> ToKeyValue();
    }
}