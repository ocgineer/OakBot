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

            
        }

        /// <summary>
        /// Raw Message received event handler, fired on every IRC message received.
        /// </summary>
        private void _chat_RawMessageReceived(object sender, ChatConnectionMessageReceivedEventArgs e)
        {
            
        }

        /// <summary>
        /// Shutdown message handler, handle shutdown.
        /// </summary>
        private void _vm_OnShutdown()
        {
            //
        }
    }
}
