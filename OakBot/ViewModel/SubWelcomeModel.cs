using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using GalaSoft.MvvmLight;

using GalaSoft.MvvmLight.Messaging;

using OakBot.Model;

namespace OakBot.ViewModel
{
    public class SubWelcomeModel : ViewModelBase
    {
        private IChatConnectionService _chat;
        private IBinFileService _bin;
        

        private SubDB _subDB;

        private string _prefix;
        

        public SubWelcomeModel(IChatConnectionService chat, IBinFileService bin)
        {
            // Register to the shutdown notification
            Messenger.Default.Register<NotificationMessage>(this, "shutdown", (msg) => { _vm_OnShutdown(); });

            // Set refferences to services
            _chat = chat; // Twitch chat service
            _bin = bin;   // Load/Save VM settings .bin files

            // Register to events
            _chat.RawMessageReceived += _chat_RawMessageReceived;

            // Initialize private database
            _subDB = new SubDB();

            _prefix = "";

            
        }

        /// <summary>
        /// Raw Message received event handler, fired on every IRC message received.
        /// </summary>
        private void _chat_RawMessageReceived(object sender, ChatConnectionMessageReceivedEventArgs e)
        {
           

            if (e.ChatMessage.Command == IrcCommand.UserNotice)
            {
                if (e.ChatMessage.NoticeType == NoticeMessageType.Sub)
                {
                    // Check if Sub 
                }
                if (e.ChatMessage.NoticeType == NoticeMessageType.Resub)
                {
                    // resub
                }
            }

            // normal chat message
            if (e.ChatMessage.Command == IrcCommand.PrivMsg)
            {
                // message content
                string message = e.ChatMessage.Message;

                // Add user to database
                

            }
        }

        /// <summary>
        /// Shutdown message handler, handle shutdown.
        /// </summary>
        private void _vm_OnShutdown()
        {
            
        }
    }
}
