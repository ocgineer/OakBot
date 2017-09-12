using System;
using Newtonsoft.Json;

namespace OakBot.Model
{
    /// <summary>
    /// Class representing the `Channel` object returned from Twitch APIv5.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Id of the channel.
        /// </summary>
        [JsonProperty("_id")]
        public string Id { get; protected set; }

        /// <summary>
        /// Name of the channel.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; protected set; }

        /// <summary>
        /// Display name of the channel.
        /// </summary>
        [JsonProperty("display_name")]
        public string DisplayName { get; protected set; }

        /// <summary>
        /// Timestamp of the creation of the channel.
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Timestamp of when the channel information was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; protected set; }

        /// <summary>
        /// Uri of the channel.
        /// </summary>
        [JsonProperty("url")]
        public Uri URL { get; protected set; }

        /// <summary>
        /// Language of the caster assosicated with the channel.
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; protected set; }

        /// <summary>
        /// Broadcaster language set on the channel.
        /// </summary>
        [JsonProperty("broadcaster_language")]
        public string BroadcasterLanguage { get; protected set; }

        /// <summary>
        /// The game that is currently set on the channel.
        /// </summary>
        [JsonProperty("game")]
        public string Game { get; protected set; }

        /// <summary>
        /// The status that is currently set on the channel.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; protected set; }

        /// <summary>
        /// Indication if the channel is flagged for mature content.
        /// </summary>
        [JsonProperty("mature")]
        public bool Mature { get; protected set; }

        /// <summary>
        /// Amount of followers the channel currently has.
        /// </summary>
        [JsonProperty("followers")]
        public int Followers { get; protected set; }

        /// <summary>
        /// The amount of views the channel currently has.
        /// </summary>
        [JsonProperty("views")]
        public int Views { get; protected set; }

        /// <summary>
        /// Type of the broadcaster.
        /// </summary>
        [JsonProperty("broadcaster_type")]
        public string BroadcasterType { get; protected set; }

        /// <summary>
        /// Indication if the channel is partnered with Twitch.
        /// </summary>
        [JsonProperty("partner")]
        public bool Partner { get; protected set; }

        /// <summary>
        /// Uri of the set logo of the channel.
        /// </summary>
        [JsonProperty("logo")]
        public Uri Logo { get; protected set; }

        /// <summary>
        /// Uri of the video (offline) banner of the channel.
        /// </summary>
        [JsonProperty("video_banner")]
        public Uri VideoBanner { get; protected set; }

        /// <summary>
        /// Uri of the set banner on the channel.
        /// </summary>
        [JsonProperty("profile_banner")]
        public Uri ProfileBanner { get; protected set; }

        /// <summary>
        /// Color string of the used background color of the banner on the channel.
        /// </summary>
        [JsonProperty("profile_banner_background_color")]
        public string ProfileBannerColor { get; protected set; }

        #region Authenticated GetChannel() Properties

        /// <summary>
        /// Registered email assosiated with the channel.
        /// Only available for the authenticated channel fetch.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; protected set; }

        /// <summary>
        /// Stream-key of the channel.
        /// Only available for the authenticated channel fetch.
        /// </summary>
        [JsonProperty("stream_key")]
        public string StreamKey { get; protected set; }

        #endregion
    }
}
