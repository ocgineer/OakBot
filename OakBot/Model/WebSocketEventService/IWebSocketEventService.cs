namespace OakBot.Model
{
    public interface IWebSocketEventService
    {
        void SetApiToken(string token);
        void BroadcastEvent(string eventName, object data);
        void SendRegisteredEvent(string eventName, object data);
    }
}
