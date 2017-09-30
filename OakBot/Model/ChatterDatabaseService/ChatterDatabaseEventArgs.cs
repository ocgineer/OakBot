using System;
using System.Collections.Generic;

namespace OakBot.Model
{
    public class ChattersListUpdatedEventArgs : EventArgs
    {
        public List<ChatterAPI> Chatters { get; private set; }

        public ChattersListUpdatedEventArgs(List<ChatterAPI> chatters)
        {
            Chatters = chatters;
        }

    }
}
