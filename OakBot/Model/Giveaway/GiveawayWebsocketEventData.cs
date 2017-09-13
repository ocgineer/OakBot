using System;

using Newtonsoft.Json;

namespace OakBot.Model
{
    public class GiveawayWebsocketEventData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("module_id")]
        public int ModuleId { get; set; }

        #region OPEN event data

        [JsonProperty("keyword")]
        public string Keyword { get; set; }

        [JsonProperty("keyword_ignore_case")]
        public bool KeywordCaseInsensitive { get; set; }

        [JsonProperty("prize")]
        public string Prize { get; set; }

        [JsonProperty("following_required")]
        public bool FollowingRequired { get; set; }

        [JsonProperty("subscriber_only")]
        public bool SubscriberOnly { get; set; }

        [JsonProperty("has_open_time")]
        public bool HasOpenTime { get; set; }

        [JsonProperty("open_time_minutes")]
        public int OpenTime { get; set; }

        [JsonProperty("opened_at")]
        public DateTime OpenedAt { get; set; }

        [JsonProperty("closing_at")]
        public DateTime ClosingAt { get; set; }

        #endregion

        #region CLOSE event data

        [JsonProperty("closed_at")]
        public DateTime ClosedAt { get; set; }

        [JsonProperty("total_entries")]
        public int TotalEntries { get; set; }

        #endregion

        #region DRAW event data

        [JsonProperty("selected_winner")]
        public string SelectedWinner { get; set; }

        [JsonProperty("drawn_at")]
        public DateTime DrawnAt { get; set; }

        [JsonProperty("has_response_time")]
        public bool HasResponseTime { get; set; }

        [JsonProperty("response_time_seconds")]
        public int ResponseTimeSeconds { get; set; }

        [JsonProperty("redrawing_at")]
        public DateTime RedrawingAt { get; set; }

        #endregion

        #region DONE event data

        [JsonProperty("done")]
        public bool Done { get; set; }

        #endregion

        /// <summary>
        /// Constructor used to create OPEN event data.
        /// </summary>
        /// <param name="moduleId">Module Id creating the event.</param>
        /// <param name="settings">Module settings to create data object.</param>
        /// <param name="openedTimestamp">Opened timestamp of the giveaway.</param>
        public GiveawayWebsocketEventData(int moduleId, GiveawayModuleSettings settings, DateTime openedTimestamp)
        {
            Type = "OPEN";
            ModuleId = moduleId;

            Keyword = settings.Keyword;
            KeywordCaseInsensitive = settings.KeywordCaseInsensitive;
            Prize = settings.Prize;
            FollowingRequired = settings.FollowingRequired;
            SubscriberOnly = settings.SubscriberOnly;

            HasOpenTime = settings.OpenTimeMinutes > 0 ? true : false;
            OpenTime = settings.OpenTimeMinutes;

            OpenedAt = openedTimestamp;
            ClosingAt = openedTimestamp.Add(new TimeSpan(0, settings.OpenTimeMinutes, 0));
        }

        /// <summary>
        /// Constructor used to create CLOSE event data.
        /// </summary>
        /// <param name="moduleId">Module Id creating the event.</param>
        /// <param name="totalEntries">Total entries in the giveaway.</param>
        /// <param name="closedTimestamp">Closure timestamp of the giveaway.</param>
        public GiveawayWebsocketEventData(int moduleId, int totalEntries, DateTime closedTimestamp)
        {
            Type = "CLOSE";
            ModuleId = moduleId;

            ClosedAt = closedTimestamp;
            TotalEntries = totalEntries;
        }

        /// <summary>
        /// Constructor used to create DRAW event data.
        /// </summary>
        /// <param name="moduleId">Module Id creating the event.</param>
        /// <param name="settings">Module settings to create data object.</param>
        /// <param name="winner">Selected winner of the giveaway</param>
        public GiveawayWebsocketEventData(int moduleId, GiveawayModuleSettings settings, DateTime drawTimestamp, GiveawayEntry winner)
        {
            Type = "DRAW";
            ModuleId = moduleId;

            SelectedWinner = winner.DisplayName;
            HasResponseTime = settings.ResponseTimeSeconds < 10 ? false : true;
            ResponseTimeSeconds = settings.ResponseTimeSeconds;
            RedrawingAt = drawTimestamp.Add(new TimeSpan(0, 0, settings.ResponseTimeSeconds, 0));
        }

        /// <summary>
        /// Constructor used to create DONE event data.
        /// </summary>
        /// <param name="moduleId">Module Id creating the event.</param>
        public GiveawayWebsocketEventData(int moduleId)
        {
            Type = "DONE";
            ModuleId = moduleId;

            Done = true;
        }
    }
}
