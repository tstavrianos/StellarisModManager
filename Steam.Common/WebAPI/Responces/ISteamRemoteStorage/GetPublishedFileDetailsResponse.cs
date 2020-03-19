using Newtonsoft.Json;

namespace Steam.Common.WebAPI.Responces.ISteamRemoteStorage
{
    public sealed class GetPublishedFileDetailsResponse
    {
        [JsonProperty("response")]
        public PublishedFileDetailResponse Response { get; set; }
    }
}