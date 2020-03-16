using System.Runtime.Serialization;

namespace Paradox.Common.Json
{
    public enum StatusType
    {
        [EnumMember(Value = "ready_to_play")]
        ReadyToPlay,
        [EnumMember(Value = "invalid_mod")]
        InvalidMod
    }
}