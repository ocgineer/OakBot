using System;
using System.Collections.Generic;

namespace OakBot.Model
{
    /// <summary>
    /// Base Argument for Twitch chat events, containing used credentials.
    /// </summary>
    public class TwitchChatEventArgs : EventArgs
    {
        public TwitchCredentials ClientCredentials { get; private set; }

        public TwitchChatEventArgs(TwitchCredentials credentials)
        {
            ClientCredentials = credentials;
        }
    }

    /// <summary>
    /// EventArgs for Twitch chat connected event containing the connected endpoint.
    /// </summary>
    public class TwitchChatConnectedEventArgs : TwitchChatEventArgs
    {
        public TwitchChatConnectedEventArgs(TwitchCredentials credentials) : base(credentials)
        {

        }
    }

    /// <summary>
    /// EventArgs for Twitch chat disconnected event containing the disconnect reason.
    /// </summary>
    public class TwitchChatDisconnectedEventArgs : TwitchChatEventArgs
    {
        public string Reason { get; private set; }

        public TwitchChatDisconnectedEventArgs(TwitchCredentials credentials, string reason) : base(credentials)
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// EventArgs for Twitch chat authentication event containing authentication successful status.
    /// </summary>
    public class TwitchChatAuthenticationEventArgs : TwitchChatEventArgs
    {
        public bool Successfull {  get; private set; }

        public TwitchChatAuthenticationEventArgs(TwitchCredentials credentials, bool successful) : base(credentials)
        {
            Successfull = successful; 
        }
    }

    /// <summary>
    /// EventArgs for Twitch chat message received event containing the parsed <see cref="TwitchChatMessage"/>.
    /// </summary>
    public class TwitchChatMessageReceivedEventArgs : TwitchChatEventArgs
    {
        public TwitchChatMessage ChatMessage { get; private set; }

        public TwitchChatMessageReceivedEventArgs(TwitchCredentials credentials, TwitchChatMessage chatmessage) : base(credentials)
        {
            ChatMessage = chatmessage;
        }
    }

    /// <summary>
    /// EventArgs for Twitch chat opperator changed event containing username and opperator status.
    /// </summary>
    public class TwitchChatOpperatorChangedEventArgs : TwitchChatEventArgs
    {
        public string Username { get; private set; }

        public bool IsOpperator { get; private set; }

        public TwitchChatOpperatorChangedEventArgs(TwitchCredentials credentials, string username, bool isOpperator) : base(credentials)
        {
            Username = username;
            IsOpperator = isOpperator;
        }
    }

    /// <summary>
    /// EventArgs for Twitch chat chatters list received event containing list of usernames.
    /// </summary>
    public class TwitchChatChatterListReceivedEventArgs : TwitchChatEventArgs
    {
        public IList<string> Usernames { get; private set; }

        public TwitchChatChatterListReceivedEventArgs(TwitchCredentials credentials, IList<string> usernames) : base(credentials)
        {
            Usernames = usernames;
        }
    }

    /// <summary>
    /// EventArgs for Twitch chat chatter joined/parted event containing username and joined/parted status.
    /// </summary>
    public class TwitchChatChatterChangedEventArgs : TwitchChatEventArgs
    {
        public string Username { get; private set; }

        public bool HasParted { get; private set; }

        public TwitchChatChatterChangedEventArgs(TwitchCredentials credentials, string username, bool hasParted) : base(credentials)
        {
            Username = username;
            HasParted = hasParted;
        }
    }
}
