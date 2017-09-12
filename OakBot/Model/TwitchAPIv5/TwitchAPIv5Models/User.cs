using System;
using Newtonsoft.Json;

namespace OakBot.Model
{
    /// <summary>
    /// Class representing the `User` object returned from Twitch APIv5.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Id of the user.
        /// </summary>
        [JsonProperty("_id")]
        public string Id { get; protected set; }

        /// <summary>
        /// Name of the user (lower-case).
        /// </summary>
        [JsonProperty("name")]
        public string Username { get; protected set; }

        /// <summary>
        /// Display name of the user.
        /// </summary>
        [JsonProperty("display_name")]
        public string DisplayName { get; protected set; }

        /// <summary>
        /// Type of the user.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; protected set; }

        /// <summary>
        /// Timestamp of when the user was created.
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Timestamp of when the user information was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; protected set; }

        /// <summary>
        /// Biography of the user.
        /// </summary>
        [JsonProperty("bio")]
        public string Bio { get; protected set; }

        /// <summary>
        /// Uri of user logo.
        /// </summary>
        [JsonProperty("logo")]
        public Uri Logo { get; protected set; }

        #region Authenticated GetUser() Properties

        /// <summary>
        /// The set email of the user.
        /// Only available for the authenticated user fetch.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; protected set; }

        /// <summary>
        /// Indication if the set email is verfied.
        /// Only available for the authenticated user fetch.
        /// </summary>
        [JsonProperty("email_verified")]
        public bool IsEmailVerified { get; protected set; }

        /// <summary>
        /// The notification settings of the user.
        /// Only available for the authenticated user fetch.
        /// </summary>
        [JsonProperty("notifications")]
        public UserNotifications Notifications { get; protected set; }

        /// <summary>
        /// Indication if the user is partnered with Twitch.
        /// Only available for the authenticated user fetch.
        /// </summary>
        [JsonProperty("partnered")]
        public bool Partnered { get; protected set; }

        /// <summary>
        /// Indication if the user has connected to Twitter.
        /// Only available for the authenticated user fetch.
        /// </summary>
        [JsonProperty("twitter_connected")]
        public bool TwitterConnected { get; protected set; }

        #endregion
    }

    /// <summary>
    /// Class representing the `Nofitications` object included in the authenticated GetUser.
    /// </summary>
    public class UserNotifications
    {
        /// <summary>
        /// Indication if email notifications is enabled.
        /// </summary>
        [JsonProperty("email")]
        public bool Email { get; protected set; }

        /// <summary>
        /// Indication if push notifications is enabled.
        /// </summary>
        [JsonProperty("push")]
        public bool Push { get; protected set; }
    }
}
