using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

using OakBot.Common;
using OakBot.Models;
using OakBot.Services;
using OakBot.Model;
using System.IO;

namespace OakBot.ViewModel
{
    public class SubWelcomeModel : ViewModelBase
    {
        /// <summary>
        /// Appdata Location and Database File Location
        /// </summary>

        private static string dbFile = "C:\\Users\\Flash\\AppData\\Roaming\\OakBot\\DB\\SubDB.db";

        private IChatConnectionService _chat;
        private IBinFileService _bin;



        private SubDB _subDB = new SubDB();
        private SubTrain _train = new SubTrain();





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

            BuildConnectionString(dbFile);


        }

        /// <summary>
        /// Raw Message received event handler, fired on every IRC message received.
        /// </summary>
        private void _chat_RawMessageReceived(object sender, ChatConnectionMessageReceivedEventArgs e)
        {

            // raw message UserNotice
            if (e.ChatMessage.Command == IrcCommand.UserNotice)
            {
                if (e.ChatMessage.NoticeType == NoticeMessageType.Sub)
                {


                    int tier = (int)e.ChatMessage.SubscriptionPlan;



                    if (_subDB.IsSub(e.ChatMessage.UserId))
                    {
                        var sub = _subDB.GetSub(e.ChatMessage.UserId);
                        if (tier > sub.Tier)
                        {
                            _prefix = "+";
                        }
                        else if (tier < sub.Tier)
                        {
                            _prefix = "-";
                        }
                        else
                        {
                            _prefix = "";
                        }

                        Console.Write(_prefix + "Welcome " + sub.Name + ", :Emotes: ");
                    }
                    else
                    {
                        var sub = new Sub()
                        {
                            UserID = e.ChatMessage.UserId,
                            Name = e.ChatMessage.Author,
                            Tier = tier
                        };

                        if (tier > 3)
                        {
                            sub.New = true;
                        }

                        Console.Write(_prefix + "Welcome " + e.ChatMessage.DisplayName + ", :Emotes: ");

                        _subDB.AddSub(sub);
                    }

                }
                if (e.ChatMessage.NoticeType == NoticeMessageType.Resub)
                {
                    // resub

                    // Check if in DB 

                    int tier = (int)e.ChatMessage.SubscriptionPlan;

                    if (_subDB.IsSub(e.ChatMessage.UserId))
                    {
                        var sub = _subDB.GetSub(e.ChatMessage.UserId);
                        if (tier > sub.Tier)
                        {
                            _prefix = "+";
                        }
                        else if (tier < sub.Tier)
                        {
                            _prefix = "-";
                        }
                        else
                        {
                            _prefix = "";
                        }

                        Console.Write(_prefix + "Welcome back " + sub.Name + ", :Emotes: ");
                    }
                    // Add to DB if not there
                    else
                    {
                        var sub = new Sub()
                        {
                            UserID = e.ChatMessage.UserId,
                            Name = e.ChatMessage.DisplayName,
                            Tier = tier
                        };

                        if (tier > 3)
                        {
                            sub.New = true;
                        }

                        Console.Write(_prefix + "Welcome back " + e.ChatMessage.DisplayName + ", :Emotes: ");

                        _subDB.AddSub(sub);
                    }
                }
            }

            // normal chat message
            if (e.ChatMessage.Command == IrcCommand.PrivMsg)
            {
                int tier = (int)e.ChatMessage.SubscriptionPlan;

                // message content
                if (_subDB.IsSub(e.ChatMessage.UserId))
                {
                    var sub = _subDB.GetSub(e.ChatMessage.UserId);
                    if (tier > sub.Tier)
                    {
                        _prefix = "+";
                    }
                    else if (tier < sub.Tier)
                    {
                        _prefix = "-";
                    }
                    else
                    {
                        _prefix = "";
                    }

                    Console.Write(_prefix + "Welcome " + sub.Name + ", :Emotes: ");
                }
                else
                {
                    var sub = new Sub()
                    {                        
                        UserID = e.ChatMessage.UserId,
                        Name = e.ChatMessage.Author,
                        Tier = tier
                    };

                    

                    if (tier > 3)
                    {
                        sub.New = true;
                    }

                    Console.Write(_prefix + "Welcome back" + e.ChatMessage.DisplayName + ", :Emotes: ");

                    _subDB.AddSub(sub);
                }

                switch (e.ChatMessage.Message)
                {
                    case "!start":
                        _train.StartTrain();
                        Console.WriteLine("Train Started...");
                        break;
                    case "!reset":
                        _train.ResetTrain();
                        Console.WriteLine("Train Reset...");
                        break;
                    case "!time":
                        Console.WriteLine("Time Left on Train..." + _train.GetTime());
                        break;
                    case "!check":
                        if (_train.IsTrain())
                        {
                            Console.WriteLine("Train yes");
                        }
                        else
                        {
                            Console.WriteLine("Train no");
                        }
                        break;
                }                
            }
        }

        public void BuildConnectionString(string dbFile)
        {
            if (String.IsNullOrEmpty(Storage.ConnectionString))
            {
                Storage.ConnectionString = string.Format("Data Source={0};Version=3;", dbFile);

                var svc = new SubService();
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
