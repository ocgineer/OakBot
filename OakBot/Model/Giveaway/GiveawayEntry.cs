using System;

namespace OakBot.Model
{
    [Serializable]
    public class GiveawayEntry
    {
        public string ChannelId { get; set; }

        public string UserId { get; set; }

        public string DisplayName { get; set; }

        public bool IsSubscriber { get; set; }

        public int Tickets { get; set; }

        public string Prize { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
