using System.Threading.Tasks;

namespace OakBot.Model
{
    public interface ITwitchPubSubService
    {
        Task<bool> Connect(TwitchCredentials credentials);
        void Close();
        Task Reconnect();
    }
}
