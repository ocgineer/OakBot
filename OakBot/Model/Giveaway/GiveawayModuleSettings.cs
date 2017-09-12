using System;
using System.Collections.Generic;

namespace OakBot.Model
{
    [Serializable]
    public class GiveawayModuleSettings
    {
        public string Keyword { get; set; }

        public bool KeywordCaseInsensitive { get; set; }

        public string Prize { get; set; }

        public int OpenTimeMinutes { get; set; }

        public bool AutoDraw { get; set; }

        public bool AnnounceTimeLeft { get; set; }

        public bool PlaySound { get; set; }

        public string SelectedAudioFile { get; set; }

        public int SubscriberLuck { get; set; }

        public bool FollowingRequired { get; set; }

        public bool SubscriberOnly { get; set; }

        public bool WinnersCanEnter { get; set; }

        public int ResponseTimeSeconds { get; set; }

        public bool ExcludeSubscriberToRespond { get; set; }

        public List<GiveawayEntry> SavedWinnersList { get; set; }

        public GiveawayModuleSettings()
        {
            Keyword = "!item";
            KeywordCaseInsensitive = true;
            Prize = "a beautiful item";
            OpenTimeMinutes = 10;
            AutoDraw = true;
            AnnounceTimeLeft = true;
            PlaySound = false;
            SubscriberLuck = 2;
            FollowingRequired = false;
            SubscriberOnly = false;
            WinnersCanEnter = false;
            ResponseTimeSeconds = 60;
            ExcludeSubscriberToRespond = true;
            SavedWinnersList = null;
        }
    }
}
