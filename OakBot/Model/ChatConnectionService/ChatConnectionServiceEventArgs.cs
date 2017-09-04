using System;

namespace OakBot.Model
{
    public class ChatConnectionBaseEventArgs : EventArgs
    {
        public ITwitchAccount Account;

        public ChatConnectionBaseEventArgs(ITwitchAccount account) : base()
        {
            Account = account;
        }
    }

    public class ChatConnectionMessageReceivedEventArgs : ChatConnectionBaseEventArgs
    {
        public TwitchChatMessage ChatMessage { get; private set; }
        
        public ChatConnectionMessageReceivedEventArgs(ITwitchAccount account, TwitchChatMessage chatmessage) : base(account)
        {
            ChatMessage = chatmessage;
        }
    }

    public class ChatConnectionConnectedEventArgs : ChatConnectionBaseEventArgs
    {
        public ChatConnectionConnectedEventArgs(ITwitchAccount account) : base(account)
        {

        }
    }

    public class ChatConnectionAuthenticatedEventArgs : ChatConnectionBaseEventArgs
    {
        public bool IsAuthenticated { get; private set; }
        
        public ChatConnectionAuthenticatedEventArgs(ITwitchAccount account, bool authenticated) : base(account)
        {
            IsAuthenticated = authenticated;
        }
    }

    public class ChatConnectionDisconnectedEventArgs : ChatConnectionBaseEventArgs
    {
        public string Reason { get; private set; }

        public ChatConnectionDisconnectedEventArgs(ITwitchAccount account, string reason) : base(account)
        {
            Reason = reason;
        }
    }
}
