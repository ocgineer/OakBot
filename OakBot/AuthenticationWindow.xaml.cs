using System;
using System.Windows;
using System.Reflection;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

// Add Reference -> Microsoft.mshtml
using mshtml;

namespace OakBot
{
    /// <summary>
    /// Interaction logic for AuthenticationWindow.xaml
    /// </summary>
    public partial class AuthenticationWindow : Window
    {
        #region Fields

        // Twitch API Client ID
        private static readonly string TwitchClientId = "gtpc5vtk1r4u8fm9l45f9kg1fzezrv8";

        /* TWITCH APIv5 OAuth Scopes:
         * channel_check_subscription   Read whether a user is subscribed to your channel.
         * channel_commercial           Trigger commercials on channel.
         * channel_editor               Write channel metadata (game, status, etc).
         * channel_feed_edit            Add posts and reactions to a channel feed.
         * channel_feed_read            View a channel feed.
         * channel_read	                Read nonpublic channel information, including email address and stream key.
         * channel_stream               Reset a channel’s stream key.
         * channel_subscriptions        Read all subscribers to your channel.
         * chat_login                   Log into chat and send messages.
         * user_blocks_edit             Turn on/off ignoring a user.
         * user_blocks_read             Read a user’s list of ignored users.
         * user_follows_edit            Manage a user’s followed channels.
         * user_read                    Read nonpublic user information, like email address.
         * user_subscriptions           Read a user’s subscriptions.
         */

        // Bot Account: Only chat login
        private static readonly string TwitchAuthBot =
            $"https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id={TwitchClientId}&redirect_uri=http://localhost&scope=chat_login&force_verify=true";

        // Streamer Account: chat login and control authorization
        private readonly string TwitchAuthStreamer =
            $"https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id={TwitchClientId}&redirect_uri=http://localhost&scope=channel_check_subscription+channel_commercial+channel_editor+channel_read+channel_subscriptions+chat_login+user_blocks_edit+user_blocks_read+user_read&force_verify=true";

        // The username to authenticate
        private string _username;

        #endregion

        #region Constructors

        // callback action?! > onclose event to return retrieved oauth?!!
        public AuthenticationWindow(string username, bool isBot)
        {
            // Initialize
            InitializeComponent();
            SetInternetOption();

            // Set username to prefill field
            _username = username;

            // Navigate
            if (isBot)
            {
                AuthWebBrowser.Navigate(TwitchAuthBot);
            }
            else
            {
                AuthWebBrowser.Navigate(TwitchAuthStreamer);
            }  
        }

        #endregion

        #region Event Handlers

        private void AuthWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            // If the loaded URI is the login page from Twitch then we can fill
            // the given Twitch Username in the username input field and lock it.
            // This forces users to fill in the correct username beforehand.
            if (e.Uri.Host == "passport.twitch.tv")
            {
                HTMLDocument doc = (HTMLDocument)AuthWebBrowser.Document;

                // Hide SignUp tab
                IHTMLElement signUp = doc.getElementById("signup_tab");
                if (signUp != null)
                    signUp.style.visibility = "hidden";

                // Lock username field and prefill given username
                IHTMLElement htmlElement = doc.getElementById("username");
                if (htmlElement != null && htmlElement is IHTMLInputElement)
                {
                    ((IHTMLInputElement)htmlElement).disabled = true;
                    ((IHTMLInputElement)htmlElement).value = _username;
                }
            }
        }

        private void AuthWebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            // Suppress script errors
            dynamic activeX = AuthWebBrowser.GetType().InvokeMember("ActiveXInstance",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, AuthWebBrowser, new object[] { });

            activeX.Silent = true;
        }

        private void AuthWebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            // If the WebBrowser is navigating to 'localhost' then this means we 
            // have received our Oauth token, and we can cancel navigating.
            if (e.Uri.Host == "localhost")
            {
                this.Oauth = GetTwitchOauthToken(e.Uri.AbsoluteUri);

                // We are done here, close the Authentication Browser
                this.DialogResult = true;
                this.Close();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns the Twitch OAUTH token extracted from the redirected URL.
        /// </summary>
        /// <param name="url">The URL that included the token to extract.</param>
        /// <returns>Extracted OAUTH token from the URL.</returns>
        private string GetTwitchOauthToken(string url)
        {
            // (?:error=(?<error>.*)&)
            // http://localhost/?error=access_denied&error_description=The+user+denied+you+access

            Match token = Regex.Match(url, "access_token=(?<token>[a-zA-Z0-9]+)");
            return token?.Groups["token"].Value;
        }

        /// <summary>
        /// Returns the GameWisp Code extracted from the redirected URL.
        /// </summary>
        /// <param name="url">The URL that included the code to extract.</param>
        /// <returns>Extracted code from the URL.</returns>
        private string GetGameWispCode(string url)
        {
            Match token = Regex.Match(url, "code=(?<token>[a-zA-Z0-9]+)");
            return token.Groups["code"].Value;
        }

        // Get reference to InternetSetOption Method from wininet.dll
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

        /// <summary>
        /// Set Internet Options for this application WebBrowser.
        /// </summary>
        private unsafe void SetInternetOption()
        {
            /* SOURCE: http://msdn.microsoft.com/en-us/library/windows/desktop/aa385328%28v=vs.85%29.aspx
            
            INTERNET_OPTION_END_BROWSER_SESSION (42):
                Flushes entries not in use from the password cache on the hard disk drive.
                Also resets the cache time used when the synchronization mode is once-per-session.
                No buffer is required for this option. This is used by InternetSetOption.
            
            INTERNET_OPTION_SUPPRESS_BEHAVIOR (81):
                A general purpose option that is used to suppress behaviors on a process-wide basis.
                The lpBuffer parameter of the function must be a pointer to a DWORD containing the specific behavior to suppress.
                This option cannot be queried with InternetQueryOption.
                
                    INTERNET_SUPPRESS_COOKIE_PERSIST (3):
                        Suppresses the persistence of cookies, even if the server has specified them as persistent.
                        Version:  Requires Internet Explorer 8.0 or later.
            */

            // Set INTERNET_OPTION_END_BROWSER_SESSION
            InternetSetOption(IntPtr.Zero, 42, IntPtr.Zero, 0);

            // Set INTERNET_OPTION_SUPPRESS_BEHAVIOR
            // with INTERNET_SUPPRESS_COOKIE_PERSIST
            int suppressBehaviorOption = 3;
            InternetSetOption(IntPtr.Zero, 81, new IntPtr(&suppressBehaviorOption), sizeof(int));
        }

        #endregion

        #region Properties

        public string Oauth { get; private set; }

        #endregion
    }
}
