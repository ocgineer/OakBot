namespace OakBot.Model
{
    public interface IWebSocketEventService
    {
        bool StartService(int port, string token);
        void ChangeApiToken(string token);
        void BroadcastEvent(string eventName, object data);
        void SendRegisteredEvent(string eventName, object data);
    }
}
