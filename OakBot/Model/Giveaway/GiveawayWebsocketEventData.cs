using System;

using Newtonsoft.Json;

namespace OakBot.Model
{
    /// <summary>
    /// Base class for the giveaway websocket event data, containing module id.
    /// </summary>
    public class GiveawayWebsocketEventBase
    {
        [JsonProperty("module_id")]
        public int ModuleId { get; private set; }

        public GiveawayWebsocketEventBase(int moduleId)
        {
            ModuleId = moduleId;
        } 
    }

    /// <summary>
    /// OPEN giveaway websocket event data.
    /// </summary>
    public class GiveawayWebsocketEventOpen : GiveawayWebsocketEventBase
    {
        [JsonProperty("keyword")]
        public string Keyword { get; private set; }

        [JsonProperty("keyword_ignore_case")]
        public bool KeywordCaseInsensitive { get; private set; }

        [JsonProperty("prize")]
        public string Prize { get; private set; }

        [JsonProperty("following_required")]
        public bool FollowingRequired { get; private set; }

        [JsonProperty("subscriber_only")]
        public bool SubscriberOnly { get; private set; }

        [JsonProperty("has_open_time")]
        public bool HasOpenTime { get; private set; }

        [JsonProperty("open_time_minutes")]
        public int OpenTime { get; private set; }

        [JsonProperty("opened_at")]
        public DateTime OpenedAt { get; private set; }

        [JsonProperty("closing_at")]
        public DateTime ClosingAt { get; private set; }

        /// <summary>
        /// Used for ReOpen to attach current entries count
        /// </summary>
        [JsonProperty("current_entries")]
        public int CurrentEntries { get; private set; }

        public GiveawayWebsocketEventOpen(int moduleId, GiveawayModuleSettings settings, DateTime openedTimestamp, int currentEntries) 
            : base(moduleId)
        {
            Keyword = settings.Keyword;
            KeywordCaseInsensitive = settings.KeywordCaseInsensitive;
            Prize = settings.Prize;
            FollowingRequired = settings.FollowingRequired;
            SubscriberOnly = settings.SubscriberOnly;

            HasOpenTime = settings.OpenTimeMinutes > 0 ? true : false;
            OpenTime = settings.OpenTimeMinutes;

            OpenedAt = openedTimestamp;
            ClosingAt = openedTimestamp.Add(new TimeSpan(0, settings.OpenTimeMinutes, 0));

            CurrentEntries = currentEntries;
        }
    }

    /// <summary>
    /// ENTRY giveaway websocket event data.
    /// </summary>
    public class GiveawayWebsocketEventEntry : GiveawayWebsocketEventBase
    {
        [JsonProperty("entry_display_name")]
        public string EntryDisplayName { get; private set; }

        [JsonProperty("total_entries")]
        public int TotalEntries { get; private set; }

        public GiveawayWebsocketEventEntry(int moduleId, string displayName, int totalEntries)
            : base(moduleId)
        {
            EntryDisplayName = displayName;
            TotalEntries = totalEntries;
        }
    }
    
    /// <summary>
    /// CLOSE giveaway websocket event data.
    /// </summary>
    public class GiveawayWebsocketEventClose : GiveawayWebsocketEventBase
    {
        [JsonProperty("closed_at")]
        public DateTime ClosedAt { get; private set; }

        [JsonProperty("total_entries")]
        public int TotalEntries { get; private set; }

        public GiveawayWebsocketEventClose(int moduleId, int totalEntries, DateTime closedTimestamp)
            : base(moduleId)
        {
            ClosedAt = closedTimestamp;
            TotalEntries = totalEntries;
        }
    }

    /// <summary>
    /// DRAW giveaway websocket event data.
    /// </summary>
    public class GiveawayWebsocketEventDraw : GiveawayWebsocketEventBase
    {
        [JsonProperty("selected_winner")]
        public string SelectedWinner { get; private set; }

        [JsonProperty("drawn_at")]
        public DateTime DrawnAt { get; private set; }

        [JsonProperty("response_time_seconds")]
        public int ResponseTimeSeconds { get; private set; }

        [JsonProperty("redrawing_at")]
        public DateTime RedrawingAt { get; private set; }

        public GiveawayWebsocketEventDraw(int moduleId, GiveawayModuleSettings settings, DateTime drawTimestamp, GiveawayEntry winner)
            : base(moduleId)
        {
            SelectedWinner = winner.DisplayName;
            DrawnAt = drawTimestamp;
            ResponseTimeSeconds = settings.ResponseTimeSeconds;
            RedrawingAt = drawTimestamp.Add(new TimeSpan(0, 0, settings.ResponseTimeSeconds));
        }
    }
}
