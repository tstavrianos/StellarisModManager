using System.Runtime.Serialization;

namespace Paradox.Common.Json
{
    public enum SourceType
    {
        [EnumMember(Value = "local")]
        Local,
        [EnumMember(Value = "steam")]
        Steam
    }
}