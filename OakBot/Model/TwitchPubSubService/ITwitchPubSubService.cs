namespace OakBot.Model
{
    public interface ITwitchPubSubService
    {
        void Connect(TwitchCredentials credentials);
        void Close();
    }
}
