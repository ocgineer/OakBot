namespace OakBot.Model
{
    /// <summary>
    /// Enum with different TcpDisconnection reasons
    /// </summary>
    public enum TcpDisconnectReason
    {
        Unknown,
        ConnectionFailure,
        ReceiveFailure,
        TransmitFailure,
        UserDisconnection,


        ErrorConnecting,
        ErrorReceiveData,
        ErrorTransmitData,

        ManualDisconnected,
        ReconnectRequest
    }
}
