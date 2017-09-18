using System;
using Newtonsoft.Json;

namespace OakBot.Model
{
    internal class Follows : TwitchApiResponseData
    {
        /// <summary>
        /// ID of the user following the ToId user.
        /// </summary>
        [JsonProperty("from_id")]
        internal string FromId { get; private set; }

        /// <summary>
        /// ID of the user being followed by the FromId user.
        /// </summary>
        [JsonProperty("to_id")]
        internal string ToId { get; private set; }

        /// <summary>
        /// Date and time when the FromId user followed the ToId user.
        /// </summary>
        [JsonProperty("followed_at")]
        internal DateTime FollowedAt { get; private set; }
    }
}
