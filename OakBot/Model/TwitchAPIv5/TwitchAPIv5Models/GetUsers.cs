using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OakBot.Model
{
    /// <summary>
    /// Class representing the `GetUsers` object returned from Twitch APIv5.
    /// </summary>
    public class GetUsers
    {
        /// <summary>
        /// Total Users that got translated from username
        /// </summary>
        [JsonProperty("_total")]
        public int Total { get; protected set; }

        /// <summary>
        /// List of `User` Models that got translated from the given usernames
        /// </summary>
        [JsonProperty("users")]
        public List<v5User> Users { get; protected set; }
    }
}
