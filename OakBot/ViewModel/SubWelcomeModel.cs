using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

using OakBot.Models;


using OakBot.Model;

namespace OakBot.ViewModel
{
    public class SubWelcomeModel : ViewModelBase
    {
        private IChatConnectionService _chat;
        private IBinFileService _bin;        

        private SubDB _subDB = new SubDB();

        private Sub _sub = new Sub(); 

        private string _prefix = "";

        public SubWelcomeModel(IChatConnectionService chat, IBinFileService bin)
        {
            // Register to the shutdown notification
            Messenger.Default.Register<NotificationMessage>(this, "shutdown", (msg) => { _vm_OnShutdown(); });

            // Set refferences to services
            _chat = chat; // Twitch chat service
            _bin = bin;   // Load/Save VM settings .bin files

            // Register to events
            _chat.RawMessageReceived += _chat_RawMessageReceived;

                        
                        
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
                    // Check if in DB 
                    _sub = _subDB.GetSub(e.ChatMessage.UserId);
                    int tier;

                    switch (e.ChatMessage.SubscriptionPlan)
                    {
                        case SubPlan.Prime:
                        case SubPlan.Tier1:
                            tier = 1000;
                            break;

                        case SubPlan.Tier2:
                            tier = 2000;
                            break;

                        case SubPlan.Tier3:
                            tier = 3000;
                            break;

                        default:
                            tier = 1000;
                            break;

                    }

                    if (_sub != null)
                    {
                        if (tier > _sub.Tier)
                        {
                            _prefix = "+";
                        }
                        else if (tier < _sub.Tier)
                        {
                            _prefix = "-";
                        }
                        else
                        {
                            _prefix = "";
                        }

                        Console.Write(_prefix + "Welcome Back " + _sub.Name + ", :Emotes: ");
                    }

                    else
                    {
                        _sub.UserID = e.ChatMessage.UserId;
                        _sub.Name = e.ChatMessage.DisplayName;
                        _sub.Tier = tier;

                        if (tier == 3000)
                        {
                            _sub.New = true;
                        }

                        Console.Write(_prefix + "Welcome " + _sub.Name + ", :Emotes: ");

                        _subDB.AddSub(_sub);
                    }
                
                }
                if (e.ChatMessage.NoticeType == NoticeMessageType.Resub)
                {
                    // resub

                    // Check if in DB 
                    _sub = _subDB.GetSub(e.ChatMessage.UserId);
                    int tier;

                    switch (e.ChatMessage.SubscriptionPlan)
                    {
                        case SubPlan.Prime:
                        case SubPlan.Tier1:
                            tier = 1000;
                            break;

                        case SubPlan.Tier2:
                            tier = 2000;
                            break;

                        case SubPlan.Tier3:
                            tier = 3000;
                            break;

                        default:
                            tier = 1000;
                            break;

                    }

                    if (_sub != null)
                    {
                        if (tier > _sub.Tier)
                        {
                            _prefix = "+";
                        }
                        else if (tier < _sub.Tier)
                        {
                            _prefix = "-";
                        }
                        else
                        {
                            _prefix = "";
                        }

                        Console.Write(_prefix + "Welcome " + _sub.Name + ", :Emotes: ");
                    }

                    else
                    {
                        _sub.UserID = e.ChatMessage.UserId;
                        _sub.Name = e.ChatMessage.DisplayName;
                        _sub.Tier = tier;

                        if (tier == 3000)
                        {
                            _sub.New = true;
                        }

                        Console.Write(_prefix + "Welcome " + _sub.Name + ", :Emotes: ");

                        _subDB.AddSub(_sub);
                    }
                }
            }

            // normal chat message
            if (e.ChatMessage.Command == IrcCommand.PrivMsg)
            {


                // message content
                _sub = _subDB.GetSub(e.ChatMessage.UserId);

                Console.Write(_sub);
                _chat.SendMessage(_sub.Name, false);
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
