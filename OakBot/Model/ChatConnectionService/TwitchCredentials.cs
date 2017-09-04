namespace OakBot.Model
{
    public class TwitchCredentials : ITwitchAccount
    {
        public string Username { get; private set; }
        public string OAuth { get; private set; }
        public bool IsCaster { get; private set; }

        public TwitchCredentials(string username, string oauth, bool isCaster)
        {
            Username = username;
            OAuth = oauth;
            IsCaster = isCaster;
        }

        public override string ToString()
        {
            return Username;
        }
    }
}
