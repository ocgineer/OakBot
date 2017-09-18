using System;

namespace OakBot.Model
{
    /// <summary>
    /// EventArgs for Tcp Connected event containing connected endpoint address.
    /// </summary>
    internal class IrcTcpConnectedEventArgs : EventArgs
    {
        internal IrcTcpConnectedEventArgs() : base()
        {

        }
    }

    /// <summary>
    /// EventArgs for Tcp Disconnected event containing disconnection reason.
    /// </summary>
    internal class IrcTcpDisconnectedEventArgs : EventArgs
    {
        internal DisconnectReason Reason { get; private set; }

        internal IrcTcpDisconnectedEventArgs(DisconnectReason reason) : base()
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// EventArgs for Tcp MessageReceived event containing received raw message.
    /// </summary>
    internal class IrcTcpMessageEventArgs : EventArgs
    {
        internal string RawMessage { get; private set; }

        internal IrcTcpMessageEventArgs(string message) : base()
        {
            RawMessage = message;
        }     
    }
}
