
namespace OakBot.Model
{
    public static class Authentication
    {
        public static string AuthenticateTwitch(string username, bool isBot)
        {
            var authwindow = new AuthenticationWindow(username, isBot);
            if (authwindow.ShowDialog() == true)
            {
                System.Console.WriteLine("My Oauth: " + authwindow.Oauth);
                return authwindow.Oauth;
            }
            else
            {
                return null;
            }
        }
    }
}
