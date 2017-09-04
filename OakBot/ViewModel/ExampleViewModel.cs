using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OakBot.Model;

namespace OakBot.ViewModel
{
    public class ExampleViewModel
    {
        private IChatConnectionService _chat;
        private IBinFileService _bin;

        public ExampleViewModel(IChatConnectionService chat, IBinFileService bin)
        {
            // Set ref to services
            _chat = chat; // Twitch chat service
            _bin = bin;   // Load/Save VM settings .bin files

            // Register to events
            _chat.RawMessageReceived += _chat_RawMessageReceived;
        }

        private void _chat_RawMessageReceived(object sender, ChatConnectionMessageReceivedEventArgs e)
        {
            Console.WriteLine(e.ChatMessage.RawMessage);
        }
    }
}
