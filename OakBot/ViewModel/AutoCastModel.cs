using System;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

using OakBot.Model;

namespace OakBot.ViewModel
{
    public class AutoCastModel
    {
        private IChatConnectionService _chat;

        public AutoCastModel(IChatConnectionService chat)
        {
            // Register to the shutdown notification
            Messenger.Default.Register<NotificationMessage>(this, "shutdown", (msg) => { _vm_OnShutdown(); });

            // Set references to services
            _chat = chat; // Twitch chat service

            // Register to events
            _chat.RawMessageReceived += _chat_RawMessageReceived;
        }

        private void _chat_RawMessageReceived(object sender, ChatConnectionMessageReceivedEventArgs e)
        {
            if (e.ChatMessage.Command == IrcCommand.PrivMsg)
            {
            }
        }

        private void _vm_OnShutdown()
        {

        }
    }
}
