namespace OakBot.Model
{
    /// <summary>
    /// Interface for <see cref="TwitchCredentials"/> to hide oauth.
    /// </summary>
    public interface ITwitchAccount
    {
        string Username { get; }
        bool IsCaster { get; }

        string ToString();
    }
}
