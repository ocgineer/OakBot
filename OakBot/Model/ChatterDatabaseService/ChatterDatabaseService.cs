using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Model
{
    public class ChatterDatabaseService : IChatterDatabaseService
    {
        #region Fields

        private readonly IChatConnectionService _chatService;

        private System.Timers.Timer _timerRefreshChatters;
        private List<ChatterAPI> _chatters;
        private string _channel;

        private object _lockChattersList;

        #endregion

        #region Events

        public event EventHandler<ChattersListUpdatedEventArgs> ChattersListUpdated;

        #endregion

        #region Constructors

        public ChatterDatabaseService(IChatConnectionService chatService)
        {
            // This service depends on chat connection service to expand functionality.
            // Also it lets this service opperate independently without any control needed.
            _chatService = chatService;
            _chatService.ChannelJoined += _chatService_ChannelJoined;
            _chatService.Disconnected += _chatService_Disconnected;
            _chatService.ChatMessageReceived += _chatService_ChatMessageReceived;

            // Initialize chatters list
            _chatters = new List<ChatterAPI>();
            _lockChattersList = new object();

            // Initialize timer to refresh chatters list from API
            _timerRefreshChatters = new System.Timers.Timer(60000);
            _timerRefreshChatters.Elapsed += _timerRefreshChatters_Elapsed;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Get a new chatters list from the unoffical unsupported API endpoint.
        /// </summary>
        private async void RefreshChattersList()
        {
            try
            {
                _chatters = new List<ChatterAPI>(await TwitchAPI.GetChannelChatters(_channel));
                ChattersListUpdated?.Invoke(this, new ChattersListUpdatedEventArgs(_chatters));
            }
            catch
            {
                //
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Chat Service Channel Joined Event Handler.
        /// </summary>
        private void _chatService_ChannelJoined(object sender, ChatConnectionChannelJoinedEventArgs e)
        {
            // If bot account joined a channel
            if (!e.Account.IsCaster)
            {
                // Set channel and start service
                _channel = e.Channel;
                _timerRefreshChatters.Start();
                RefreshChattersList();
            }
        }

        /// <summary>
        /// Chat Service Disconnected Event Handler.
        /// </summary>
        private void _chatService_Disconnected(object sender, ChatConnectionDisconnectedEventArgs e)
        {
            // If the bot account disconnected
            if (!e.Account.IsCaster)
            {
                // Stop this service
                _timerRefreshChatters.Stop();
                lock (_lockChattersList)
                {
                    _chatters.Clear();
                    ChattersListUpdated?.Invoke(this, new ChattersListUpdatedEventArgs(_chatters));
                }
            }
        }

        /// <summary>
        /// Chat Service Message Received Event Handler.
        /// </summary>
        private void _chatService_ChatMessageReceived(object sender, ChatConnectionMessageReceivedEventArgs e)
        {
            //
        }

        /// <summary>
        /// Refresh Chatters Timer Elapsed Event Handler.
        /// </summary>
        private void _timerRefreshChatters_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RefreshChattersList();
        }

        #endregion
    }
}
