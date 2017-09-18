using System;
using Newtonsoft.Json;

namespace OakBot.Model
{
    /// <summary>
    /// Class representing the `Follower` object returned from Twitch APIv5.
    /// </summary>
    public class Follower
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
        /// The `User` Model with the follower user information.
        /// </summary>
        [JsonProperty("user")]
        public v5User User { get; protected set; }
    }
}
