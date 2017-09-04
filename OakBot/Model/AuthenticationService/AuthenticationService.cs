namespace OakBot.Model
{
    public class AuthenticationService : IAuthenticationService
    {
        public string AuthenticateTwitch(string username, bool isBot)
        {
            var authwindow = new AuthenticationWindow(username, isBot);
            if (authwindow.ShowDialog() == true)
            {
                return authwindow.Oauth;
            }
            else
            {
                return null;
            }
        }
    }
}
