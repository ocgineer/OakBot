using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OakBot.Model
{
    /// <summary>
    /// TwitchChatConnection used TcpConnection and handles The Twitch IRC connection
    /// Parses received raw TCP messages into TwitchChatMessages and actions on it
    /// </summary>
    public class TwitchChatConnection
    {
        #region Private Fields

        // Twitch IRC transmit rate limits;
        // non-opperator: 20 messages per 30 sec => 300
        // opperator: 100 messages per 30 sec => 1500
        // Verified bot: 50 messages per 30 sec => 600
        private static readonly int RATELIMIT_MOD = 300;
        private static readonly int RATELIMIT_NOMOD = 1500;
        //private static readonly int RATELIMIT_VERBOT = 600;

        private TcpConnection _tcpClient;
        private TwitchCredentials _credentials;
        private string _joinedChannel;

        #endregion

        #region Events

        public event EventHandler<TwitchChatConnectedEventArgs> Connected;
        public event EventHandler<TwitchChatOpperatorChangedEventArgs> OpperatorChange;
        public event EventHandler<TwitchChatDisconnectedEventArgs> Disconnected;
        public event EventHandler<TwitchChatAuthenticationEventArgs> Authentication;
        public event EventHandler<TwitchChatMessageReceivedEventArgs> ChatMessageReceived;
        public event EventHandler<TwitchChatMessageReceivedEventArgs> RawMessageReceived;
        public event EventHandler<TwitchChatChatterListReceivedEventArgs> ChatterListReceived;
        public event EventHandler<TwitchChatChatterChangedEventArgs> ChatterChanged;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a non connected TwitchChatConnection
        /// </summary>
        public TwitchChatConnection()
        {
            // Initiate the TcpConnection and hook events
            _tcpClient = new TcpConnection();
            _tcpClient.Connected += _tcpClient_Connected;
            _tcpClient.Disconnected += _tcpClient_Disconnected;
            _tcpClient.MessageReceived += _tcpClient_MessageReceived;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start connection to the Twitch IRC Chat.
        /// </summary>
        /// <param name="secure">Secure SSL connection or not.</param>
        public async Task Connect(bool secure, TwitchCredentials credentials)
        {
            _credentials = credentials;
            
            // Set used port and connect to Twitch IRC connection
            int port = secure ? 443 : 6667;
            await _tcpClient.ConnectAsync("irc.chat.twitch.tv", port, secure, RATELIMIT_NOMOD);
        }

        public void Authenticate()
        {
            // Enqueue PASS and NICK messages to authenticated to Twitch IRC
            _tcpClient.EnqueueMessage("PASS oauth:" + _credentials.OAuth, true);
            _tcpClient.EnqueueMessage("NICK " + _credentials.Username, true);
        }
        
        /// <summary>
        /// Disconnect from the Twitch IRC Chat.
        /// </summary>
        public void Disconnect()
        {
            // Disconnected from TCP with a manual disconnection reason
            _tcpClient.Disconnect(TcpDisconnectReason.ManualDisconnected);

            // Reset the ratelimit to ensure correct ratelimit on reconnection
            _tcpClient.RateLimit = RATELIMIT_NOMOD;

            // Clear channel
            _joinedChannel = null;
        }

        /// <summary>
        /// Join a Twitch IRC channel
        /// </summary>
        /// <param name="channel">Channel name to join</param>
        public void JoinChannel(string channel)
        {
            _tcpClient.EnqueueMessage("JOIN #" + channel.ToLower(), true);
            _joinedChannel = channel;
        }

        /// <summary>
        /// Part from a Twitch IRC channel
        /// </summary>
        /// <param name="channel">Channel name to part</param>
        public void PartChannel(string channel)
        {
            if (channel == _joinedChannel)
            {
                _tcpClient.EnqueueMessage("PART #" + channel.ToLower());
                _joinedChannel = null;
            }
        }

        /// <summary>
        /// Send a chat message to the joined channel
        /// </summary>
        /// <param name="message">Chat message as string</param>
        public void SendMessage(string message)
        {
            _tcpClient.EnqueueMessage("PRIVMSG #" + _joinedChannel + " :" + message);
        }

        /// <summary>
        /// Send a raw message to the IRC connection
        /// </summary>
        /// <param name="rawMessage">Raw message as string</param>
        public void SendRawIrcMessage(string rawMessage)
        {
            _tcpClient.EnqueueMessage(rawMessage);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler for TcpConnection.Connected
        /// Will request capabilities after connection
        /// </summary>
        private void _tcpClient_Connected(object sender, TcpConnectedEventArgs e)
        {
            // Enqueue CAP to Request Twitch Capabilities
            _tcpClient.EnqueueMessage("CAP REQ :twitch.tv/membership twitch.tv/commands twitch.tv/tags", true);

            // Fire Connected event
            OnConnected();
        }

        /// <summary>
        /// Event handler for TcpConnection.Disconnected
        /// </summary>
        private void _tcpClient_Disconnected(object sender, TcpDisconnectedEventArgs e)
        {
            // Fire Disconnected Event
            OnDisconnected(e.Reason);
        }
        
        /// <summary>
        /// Event handler for TcpConnection.MessageReceived
        /// Parsed the received raw message in a TwitchChatMessage
        /// Switches on the received command and executes what is needed
        /// </summary>
        private void _tcpClient_MessageReceived(object sender, TcpMessageReceivedEventArgs e)
        {
            // Create a TwitchMessage from the received message
            TwitchChatMessage twitchMsg = new TwitchChatMessage(e.RawMessage);

            OnRawMessageReceived(twitchMsg);
            
            #if DEBUG
            Console.WriteLine(twitchMsg.RawMessage);
            #endif

            switch (twitchMsg.Command)
            {
                // Received PRIVMSG
                // Invoke message received event
                case IrcCommand.PrivMsg:
                    OnChatMessageReceived(twitchMsg);
                    break;

                // Received PING
                // Send a PONG back
                case IrcCommand.Ping:
                    _tcpClient.EnqueueMessage(twitchMsg.RawMessage.Replace("PING", "PONG"), true);
                    break;

                // Received Reconnection request
                // Disconnect the connected TCP client with a reqonnect request reason
                case IrcCommand.Reconnect:
                    _tcpClient.Disconnect(TcpDisconnectReason.ReconnectRequest);
                    break;

                // Received 376, last message of the MOTD after authentication
                // Used to assume successful authentication to the Twitch IRC
                case IrcCommand.RPL_001:
                    OnAuthentication(true);
                    break;

                // Resubscription notification
                case IrcCommand.UserNotice:
                    /* TODO */
                    break;

                // Received general IRC Notice
                /* TODO: Expand this https://dev.twitch.tv/docs/v5/guides/irc/#notice */
                case IrcCommand.Notice:
                    switch (twitchMsg.NoticeType)
                    {
                        // Authentication failed
                        // Invoke the authentication event with a failed authentication status
                        case NoticeMessageType.AuthBadFormat:
                            OnAuthentication(false);
                            break;
                        case NoticeMessageType.AuthLoginFailed:
                            OnAuthentication(false);
                            break;
                    }
                    break;
                
                // Received MODE
                // Invoke opperator changed event and if its this client adjust TCP RateLimit
                case IrcCommand.Mode:
                    if (twitchMsg.Args[1] == "+o")
                    {
                        OnOpperatorChange(twitchMsg.Args[2], true);

                        // Change rate limit if bot/caster-account changed opperator status
                        if (_credentials.Username == twitchMsg.Args[2])
                            _tcpClient.RateLimit = RATELIMIT_MOD;
                    }
                    else if (twitchMsg.Args[1] == "-o")
                    {
                        OnOpperatorChange(twitchMsg.Args[2], false);

                        // Change rate limit if bot/caster-account changed opperator status
                        if (_credentials.Username == twitchMsg.Args[2])
                            _tcpClient.RateLimit = RATELIMIT_NOMOD;
                    }

                    break;

                /* 
                 * TODO: Check better way to receive the viewers, as apperantly the JOIN/PART/ROOMLIST
                 * do not work if the user has over 1000 chatters, and ROOMLIST is not always triggering
                 * usage of API endpoint (undocumented) http://tmi.twitch.tv/group/user/{caster_name}/chatters
                 */

                // Chatter joined channel
                case IrcCommand.Join:
                    OnChatterJoinPart(twitchMsg.Author, false);
                    break;

                // Chatter parted channel
                case IrcCommand.Part:
                    OnChatterJoinPart(twitchMsg.Author, true);
                    break;

                // Received roomlist
                case IrcCommand.RPL_353:
                    OnChatterListReceived(twitchMsg.Message.Split(null));
                    break;

            } // End switch on IrcCommand

        }

        #endregion

        #region Protected Virtual

        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, new TwitchChatConnectedEventArgs(_credentials));
        }

        protected virtual void OnDisconnected(TcpDisconnectReason reason)
        {
            string msg = string.Empty;
            switch (reason)
            {
                case TcpDisconnectReason.ErrorConnecting:
                    msg = "An error occured while connecting.";
                    break;
                case TcpDisconnectReason.ErrorReceiveData:
                    msg = "An error occured while receiving data.";
                    break;
                case TcpDisconnectReason.ErrorTransmitData:
                    msg = "An error occured while transmitting data.";
                    break;
                case TcpDisconnectReason.ManualDisconnected:
                    msg = "Manual disconnected from the chat.";
                    break;
                case TcpDisconnectReason.ReconnectRequest:
                    msg = "Twitch IRC Server requested to reconnect.";
                    break;
            }

            Disconnected?.Invoke(this, new TwitchChatDisconnectedEventArgs(_credentials, msg));
        }
        
        protected virtual void OnAuthentication(bool success)
        {
            Authentication?.Invoke(this, new TwitchChatAuthenticationEventArgs(_credentials, success));
        }

        protected virtual void OnChatMessageReceived(TwitchChatMessage msg)
        {
            ChatMessageReceived?.Invoke(this, new TwitchChatMessageReceivedEventArgs(_credentials, msg));
        }

        protected virtual void OnRawMessageReceived(TwitchChatMessage msg)
        {
            RawMessageReceived?.Invoke(this, new TwitchChatMessageReceivedEventArgs(_credentials, msg));
        }

        protected virtual void OnOpperatorChange(string username, bool isOpperator)
        {
            OpperatorChange?.Invoke(this, new TwitchChatOpperatorChangedEventArgs(_credentials, username, isOpperator));
        }
        
        protected virtual void OnChatterListReceived(IList<string> usernames)
        {
            ChatterListReceived?.Invoke(this, new TwitchChatChatterListReceivedEventArgs(_credentials, usernames));
        }

        protected virtual void OnChatterJoinPart(string username, bool hasParted)
        {
            ChatterChanged?.Invoke(this, new TwitchChatChatterChangedEventArgs(_credentials, username, hasParted));
        }

        #endregion

    }
}