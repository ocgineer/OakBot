using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Model
{
    public class ChatConnectionService : IChatConnectionService
    {
        #region Fields

        private TwitchChatConnection _tcc1;
        private TwitchChatConnection _tcc2;

        private TwitchCredentials _botaccount;
        private TwitchCredentials _casteraccount;

        private string _channel;
        private bool _secure;

        #endregion

        #region Events

        public event EventHandler<ChatConnectionMessageReceivedEventArgs> ChatMessageReceived;
        public event EventHandler<ChatConnectionMessageReceivedEventArgs> RawMessageReceived;
        public event EventHandler<ChatConnectionConnectedEventArgs> Connected;
        public event EventHandler<ChatConnectionAuthenticatedEventArgs> Authenticated;
        public event EventHandler<ChatConnectionDisconnectedEventArgs> Disconnected;

        #endregion

        #region Constructors

        public ChatConnectionService()
        {
            // Initialize Twitch chat connections
            _tcc1 = new TwitchChatConnection();
            _tcc2 = new TwitchChatConnection();

            // Register to events from Twitch chat connections
            _tcc1.Connected += _tcc_Connected;
            _tcc2.Connected += _tcc_Connected;
            _tcc1.Disconnected += _tcc_Disconnected;
            _tcc2.Disconnected += _tcc_Disconnected;
            _tcc1.Authentication += _tcc_Authentication;
            _tcc2.Authentication += _tcc_Authentication;
            _tcc1.ChatMessageReceived += _tcc_ChatMessageReceived;
            _tcc2.ChatMessageReceived += _tcc_ChatMessageReceived;
            _tcc1.RawMessageReceived += _tcc_RawMessageReceived;
            _tcc2.RawMessageReceived += _tcc_RawMessageReceived;
        }

        #endregion

        #region Public Methods

        public void SetJoiningChannel(string channel, bool secure)
        {
            _channel = channel;
            _secure = secure;
        }

        public void SetCredentials(TwitchCredentials credentials)
        {
            if (credentials.IsCaster)
            {
                _casteraccount = credentials;
            }
            else
            {
                _botaccount = credentials;
            }
        }

        public void Connect(bool isCaster)
        {
            if (string.IsNullOrWhiteSpace(_channel))
            {
                return;
            }

            try
            {
                if (!isCaster && _botaccount != null)
                {
                    _tcc1.Connect(_secure, _botaccount);
                }
                else if (isCaster && _casteraccount != null)
                {
                    _tcc2.Connect(_secure, _casteraccount);
                }
            }
            catch { }
        }

        public void SendMessage(string message, bool isCaster)
        {
            if (isCaster)
            {
                _tcc2.SendMessage(message);
            }
            else
            {
                _tcc1.SendMessage(message);
            }
        }

        public void Disconnect(bool isCaster)
        {
            if (isCaster)
            {
                _tcc2.Disconnect();
            }
            else
            {
                _tcc1.Disconnect();
            }
        }

        #endregion

        #region Event Handlers

        private void _tcc_ChatMessageReceived(object sender, TwitchChatMessageReceivedEventArgs e)
        {
            // Caster received message
            if (e.ClientCredentials.IsCaster)
            {
                // Return on anything but the 'bot' messages for loopback
                if (e.ChatMessage.Author != _botaccount.Username)
                {
                    return;
                }
            }

            OnChatMessageReceived(e.ClientCredentials, e.ChatMessage);
        }

        private void _tcc_RawMessageReceived(object sender, TwitchChatMessageReceivedEventArgs e)
        {            
            // Caster received message
            if (e.ClientCredentials.IsCaster)
            {
                // Return on anything but the 'bot' messages for loopback
                if (e.ChatMessage.Author != _botaccount.Username)
                {
                    return;
                }
            }

            OnRawMessageReceived(e.ClientCredentials, e.ChatMessage);
        }

        private void _tcc_Connected(object sender, TwitchChatConnectedEventArgs e)
        {
            // Fire connected event
            OnConnection(e.ClientCredentials);

            // Authenticate to Twitch IRC
            var tcc = (TwitchChatConnection)sender;
            tcc.Authenticate();
        }

        private void _tcc_Authentication(object sender, TwitchChatAuthenticationEventArgs e)
        {
            if (e.Successfull)
            {
                // Fire authentication event
                OnAuthentication(e.ClientCredentials, true);

                // Join channel
                var tcc = (TwitchChatConnection)sender;
                tcc.JoinChannel(_channel);
            }
            else
            {
                // Fire authentication event
                OnAuthentication(e.ClientCredentials, false);

                // Disconnect after failure
                if (e.ClientCredentials.IsCaster)
                {
                    _tcc2.Disconnect();
                }
                else
                {
                    _tcc1.Disconnect();
                }
            }
        }

        private void _tcc_Disconnected(object sender, TwitchChatDisconnectedEventArgs e)
        {
            // Fire disconnection event
            OnDisconnection(e.ClientCredentials, e.Reason);
        }

        #endregion

        #region Protected Virtual

        protected virtual void OnChatMessageReceived(ITwitchAccount account, TwitchChatMessage tcm)
        {
            ChatMessageReceived?.Invoke(this, new ChatConnectionMessageReceivedEventArgs(account, tcm));
        }

        protected virtual void OnRawMessageReceived(ITwitchAccount account, TwitchChatMessage tcm)
        {
            RawMessageReceived?.Invoke(this, new ChatConnectionMessageReceivedEventArgs(account, tcm));
        }

        protected virtual void OnConnection(ITwitchAccount account)
        {
            Connected?.Invoke(this, new ChatConnectionConnectedEventArgs(account));
        }

        protected virtual void OnAuthentication(ITwitchAccount account, bool authenticated)
        {
            Authenticated?.Invoke(this, new ChatConnectionAuthenticatedEventArgs(account, authenticated));
        }

        protected virtual void OnDisconnection(ITwitchAccount account, string reason)
        {
            Disconnected?.Invoke(this, new ChatConnectionDisconnectedEventArgs(account, reason));
        }

        #endregion
    }
}