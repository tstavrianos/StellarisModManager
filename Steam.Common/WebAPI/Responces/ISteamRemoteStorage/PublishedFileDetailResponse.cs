using System.Collections.Generic;
using Newtonsoft.Json;

namespace Steam.Common.WebAPI.Responces.ISteamRemoteStorage
{
    public sealed class PublishedFileDetailResponse
    {
        [JsonProperty("result")]
        public int Result { get; set; }
        [JsonProperty("resultcount")]
        public int ResultCount { get; set; }
        [JsonProperty("publishedfiledetails")]
        public IList<PublishedFileDetail> PublishedFileDetails { get; set; }
    }
}