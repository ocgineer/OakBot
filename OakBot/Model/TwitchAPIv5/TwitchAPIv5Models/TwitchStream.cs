using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OakBot.Model
{
    /// <summary>
    /// Class representing the `Stream` object returned from Twitch APIv5.
    /// </summary>
    public class TwitchStream
    {
        /// <summary>
        /// Id of the stream.
        /// </summary>
        [JsonProperty("_id")]
        public string Id { get; protected set; }

        /// <summary>
        /// Timestamp of the creation of the stream.
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Current set game on the stream.
        /// </summary>
        [JsonProperty("game")]
        public string Game { get; protected set; }

        /// <summary>
        /// ???
        /// </summary>
        [JsonProperty("broadcast_platform")]
        public string BroadcastPlatform { get; protected set; }

        /// <summary>
        /// Primary (first) Twitch Community Id the stream is under.
        /// </summary>
        [JsonProperty("community_id")]
        public string CommunityId { get; protected set; }

        /// <summary>
        /// List of up to three Twitch Communities the stream is under.
        /// </summary>
        [JsonProperty("community_ids")]
        public List<string> CommunityIds { get; protected set; }

        /// <summary>
        /// Amount of viewers the stream currently has.
        /// </summary>
        [JsonProperty("viewers")]
        public int Viewers { get; protected set; }

        /// <summary>
        /// Video height the current stream has in pixels.
        /// </summary>
        [JsonProperty("video_height")]
        public int VideoHeight { get; protected set; }

        /// <summary>
        /// Average frames per seconds the current stream has.
        /// </summary>
        [JsonProperty("average_fps")]
        public float AverageFPS { get; protected set; }

        /// <summary>
        /// Current delay the stream has towards viewers.
        /// </summary>
        [JsonProperty("delay")]
        public int Delay { get; protected set; }

        /// <summary>
        /// ???
        /// </summary>
        [JsonProperty("is_playlist")]
        public bool IsPlaylist { get; protected set; }

        /// <summary>
        /// ???
        /// </summary>
        [JsonProperty("stream_type")]
        public string StreamType { get; protected set; }

        /// <summary>
        /// Preview images of the current stream.
        /// </summary>
        [JsonProperty("preview")]
        public StreamPreviews Preview { get; protected set; }

        /// <summary>
        /// Channel object associated with the current stream.
        /// </summary>
        [JsonProperty("channel")]
        public Channel Channel { get; protected set; }
    }

    public class StreamPreviews
    {
        [JsonProperty("small")]
        public Uri Small { get; protected set; }

        [JsonProperty("medium")]
        public Uri Medium { get; protected set; }

        [JsonProperty("large")]
        public Uri Large { get; protected set; }

        [JsonProperty("template")]
        public Uri Template { get; protected set; }
    }
}

