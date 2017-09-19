using System;
using Newtonsoft.Json;

namespace OakBot.Model
{
    /// <summary>
    /// Class representing the `Follows` object returned from Twitch APIv5.
    /// </summary>
    public class v5Follows
    {
        /// <summary>
        /// Timestamp of the follow.
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Indication if user has notifications enabled.
        /// </summary>
        [JsonProperty("notifications")]
        public bool Notifications { get; protected set; }

        /// <summary>
        /// The `Channel` Model with followed channel information.
        /// </summary>
        [JsonProperty("channel")]
        public Channel Channel { get; protected set; }
    }
}
