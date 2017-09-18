using System.Collections.Generic;
using Newtonsoft.Json;

namespace OakBot.Model
{
    /// <summary>
    /// Base response of the Twitch API containing a collection of response data model.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TwitchApiResponse<T> where T : TwitchApiResponseData
    {
        [JsonProperty("data")]
        internal List<T> Data { get; private set; }

        [JsonProperty("pagination")]
        internal Pagination Pagination { get; private set; }
    }

    /// <summary>
    /// Pagination response from the Twitch API containing a cursor to fetch next batches of data.
    /// </summary>
    internal class Pagination
    {
        [JsonProperty("cursor")]
        internal string Cursor { get; private set; }
    }
}
