using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paradox.Common.Json
{
    public sealed class DlcLoad
    {
        [JsonProperty("disabled_dlcs")]
        public IList<string> DisabledDlcs { get; set; }
        
        [JsonProperty("enabled_mods")]
        public IList<string> EnabledMods { get; set; }
    }
}