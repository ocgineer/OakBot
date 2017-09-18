namespace OakBot.Model
{
    /// <summary>
    /// Enum with different TcpDisconnection reasons
    /// </summary>
    public enum DisconnectReason
    {
        Unknown,
        ConnectionFailure,
        ReceiveFailure,
        TransmitFailure,
        NoPongReceived,
        UserDisconnection,
        ReconnectRequest
    }
}
