namespace OakBot.Model
{
    public interface IAuthenticationService
    {
        string AuthenticateTwitch(string username, bool isBot);
    }
}
