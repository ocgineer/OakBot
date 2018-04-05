using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Model
{
    public interface IChatConnectionService
    {
        event EventHandler<ChatConnectionMessageReceivedEventArgs> ChatMessageReceived;
        event EventHandler<ChatConnectionMessageReceivedEventArgs> RawMessageReceived;
        event EventHandler<ChatConnectionConnectedEventArgs> Connected;
        event EventHandler<ChatConnectionAuthenticatedEventArgs> Authenticated;
        event EventHandler<ChatConnectionChannelJoinedEventArgs> ChannelJoined;
        event EventHandler<ChatConnectionDisconnectedEventArgs> Disconnected;

        void Connect(bool isCaster);
        void Disconnect(bool isCaster);

        void SetJoiningChannel(string channel, bool secure);
        void SetCredentials(TwitchCredentials credentials);

        void SendMessage(string message, bool isCaster);

        string GetChannelName();

    }
}
