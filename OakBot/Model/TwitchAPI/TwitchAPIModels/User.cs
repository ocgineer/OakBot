using System;
using Newtonsoft.Json;

namespace OakBot.Model
{
    internal class User : TwitchApiResponseData
    {
        /// <summary>
        /// User’s broadcaster type: "partner", "affiliate", or "".
        /// </summary>
        [JsonProperty("broadcaster_type")]
        internal string BroadcasterType { get; private set; }

        /// <summary>
        /// User's channel description.
        /// </summary>
        [JsonProperty("description")]
        internal string Description { get; private set; }

        /// <summary>
        /// User's display name.
        /// </summary>
        [JsonProperty("display_name")]
        internal string DisplayName { get; private set; }

        /// <summary>
        /// User’s email address. Returned if the request includes the user:read:edit scope.
        /// </summary>
        [JsonProperty("email")]
        internal string Email { get; private set; }

        /// <summary>
        /// User's ID.
        /// </summary>
        [JsonProperty("id")]
        internal string Id { get; private set; }

        /// <summary>
        /// User's login name.
        /// </summary>
        [JsonProperty("login")]
        internal string Login { get; private set; }

        /// <summary>
        /// URL of the user's offline image.
        /// </summary>
        [JsonProperty("offline_image_url")]
        internal Uri OfflineImageUri { get; private set; }

        /// <summary>
        /// URL of the user's profile image.
        /// </summary>
        [JsonProperty("profile_image_url")]
        internal Uri ProfileImageUri { get; private set; }

        /// <summary>
        /// User’s type: "staff", "admin", "global_mod", or "".
        /// </summary>
        [JsonProperty("type")]
        internal string Type { get; private set; }

        /// <summary>
        /// Number of users following this user.
        /// </summary>
        [JsonProperty("view_count")]
        internal int ViewCount { get; private set; }
    }
}
