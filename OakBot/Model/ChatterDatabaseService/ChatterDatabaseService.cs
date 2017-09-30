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

        #endregion

        #region Events

        public event EventHandler<ChattersListUpdatedEventArgs> ChattersListUpdated;

        #endregion

        #region Constructors

        public ChatterDatabaseService(IChatConnectionService chatService)
        {
            // This service depends on chat connection service to expand functionality.
            _chatService = chatService;
            _chatService.ChatMessageReceived += _chatService_ChatMessageReceived;

            // Initialize chatters list
            _chatters = new List<ChatterAPI>();

            // Initialize timer to refresh chatters list
            _timerRefreshChatters = new System.Timers.Timer(60000);
            _timerRefreshChatters.Elapsed += _timerRefreshChatters_Elapsed;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the service.
        /// Used after successful connection to the chat.
        /// </summary>
        /// <param name="channel"></param>
        public void StartService(string channel)
        {
            _channel = channel;
            _timerRefreshChatters.Start();
            RefreshChattersList();
        }

        /// <summary>
        /// Stop the service.
        /// Used on disconnection of the chat.
        /// </summary>
        public void StopService()
        {
            _timerRefreshChatters.Stop();
            _chatters.Clear();
            ChattersListUpdated?.Invoke(this, new ChattersListUpdatedEventArgs(_chatters));
        }

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
