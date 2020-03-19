using Newtonsoft.Json;

namespace Steam.Common.WebAPI.Responces.ISteamRemoteStorage
{
    public sealed class PublishedFileDetailTag
    {
        [JsonProperty("tag")]
        public string Value { get; set; }
    }
}