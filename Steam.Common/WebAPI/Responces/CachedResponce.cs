using System;
using Newtonsoft.Json;

namespace Steam.Common.WebAPI.Responces
{
    public sealed class CachedResponce<T>
    {
        [JsonProperty("lastFetched")]
        public DateTime LastFetched { get; set; }
        [JsonProperty("responce")]
        public T Responce { get; private set; }

        public static CachedResponce<T> FromResponce(T responce)
        {
            return new CachedResponce<T>{LastFetched = DateTime.Now, Responce = responce};
        }

    }
}