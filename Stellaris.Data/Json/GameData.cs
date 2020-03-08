using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stellaris.Data.Json
{
    public sealed class GameData
    {
        [JsonProperty("modsOrder")]
        public IList<string> ModsOrder { get; set; }
        
        [JsonProperty("isEulaAccepted")]
        public bool IsEulaAccepted { get; set; }
    }
}