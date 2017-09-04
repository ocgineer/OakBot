using System;

namespace OakBot.Model
{
    /// <summary>
    /// EventArgs for Tcp Connected event containing connected endpoint address.
    /// </summary>
    internal class TcpConnectedEventArgs : EventArgs
    {
        internal TcpConnectedEventArgs() : base()
        {

        }
    }

    /// <summary>
    /// EventArgs for Tcp Disconnected event containing disconnection reason.
    /// </summary>
    internal class TcpDisconnectedEventArgs : EventArgs
    {
        internal TcpDisconnectReason Reason { get; private set; }

        internal TcpDisconnectedEventArgs(TcpDisconnectReason reason) : base()
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// EventArgs for Tcp MessageReceived event containing received raw message.
    /// </summary>
    internal class TcpMessageReceivedEventArgs : EventArgs
    {
        internal string RawMessage { get; private set; }

        internal TcpMessageReceivedEventArgs(string message) : base()
        {
            RawMessage = message;
        }     
    }

    /// <summary>
    /// EventArgs for Tcp MessageTransmit event containing transmitted message.
    /// </summary>
    internal class TcpMessageTransmittedEventArgs : EventArgs
    {
        internal string Message { get; private set; }

        internal TcpMessageTransmittedEventArgs(string message) : base()
        {
            Message = message;
        }
    }
}
