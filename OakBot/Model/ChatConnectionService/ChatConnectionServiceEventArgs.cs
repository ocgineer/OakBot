using System;

namespace OakBot.Model
{
    public class ChatConnectionEventArgs : EventArgs
    {
        public ITwitchAccount Account;

        public ChatConnectionEventArgs(ITwitchAccount account) : base()
        {
            Account = account;
        }
    }

    public class ChatConnectionMessageReceivedEventArgs : ChatConnectionEventArgs
    {
        public TwitchChatMessage ChatMessage { get; private set; }
        
        public ChatConnectionMessageReceivedEventArgs(ITwitchAccount account, TwitchChatMessage chatmessage) : base(account)
        {
            ChatMessage = chatmessage;
        }
    }

    public class ChatConnectionConnectedEventArgs : ChatConnectionEventArgs
    {
        public ChatConnectionConnectedEventArgs(ITwitchAccount account) : base(account)
        {

        }
    }

    public class ChatConnectionAuthenticatedEventArgs : ChatConnectionEventArgs
    {
        public bool IsAuthenticated { get; private set; }
        
        public ChatConnectionAuthenticatedEventArgs(ITwitchAccount account, bool authenticated) : base(account)
        {
            IsAuthenticated = authenticated;
        }
    }

    public class ChatConnectionDisconnectedEventArgs : ChatConnectionEventArgs
    {
        public string Reason { get; private set; }

        public ChatConnectionDisconnectedEventArgs(ITwitchAccount account, string reason) : base(account)
        {
            Reason = reason;
        }
    }

    public class ChatConnectionChannelJoinedEventArgs : ChatConnectionEventArgs
    {
        public string Channel { get; private set; }

        public ChatConnectionChannelJoinedEventArgs(ITwitchAccount account, string channel) : base(account)
        {
            Channel = channel;
        }
    }
}
