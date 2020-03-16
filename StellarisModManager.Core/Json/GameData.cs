using System.Collections.Generic;
using Newtonsoft.Json;

namespace StellarisModManager.Core.Json
{
    public sealed class GameData
    {
        [JsonProperty("modsOrder")]
        public IList<string> ModsOrder { get; set; }
        
        [JsonProperty("isEulaAccepted")]
        public bool IsEulaAccepted { get; set; }
    }
}