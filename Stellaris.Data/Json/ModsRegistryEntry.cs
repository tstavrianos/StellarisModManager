using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Stellaris.Data.Json
{
    public sealed class ModsRegistryEntry
    {
        [JsonProperty("gameRegistryId")]
        public string GameRegistryId;

        [JsonProperty("source")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SourceType Source;
        
        [JsonProperty("steamid")]
        public string SteamId;
        
        [JsonProperty("displayName")]
        public string DisplayName;
        
        [JsonProperty("tags")]
        public IList<string> Tags;
        
        [JsonProperty("requiredVersion")]
        public string RequiredVersion;
        
        [JsonProperty("dirPath")]
        public string DirPath;
        
        [JsonProperty("archivePath")]
        public string ArchivePath;

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusType Status;
        
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("timeUpdated")]
        public int? TimeUpdated;

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl;

        [JsonProperty("cause")]
        public string Cause;

        [JsonProperty("thumbnailPath")]
        public string ThumbnailPath;
    }
}