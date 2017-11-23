using System;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

using OakBot.ViewModel;
using OakBot.Model;
using System.IO;

namespace OakBot.ViewModel
{
    public class SubWelcomeModel : ViewModelBase
    {
        /// <summary>
        /// Appdata Location and Database File Location
        /// </summary>

        private const string DBFILE = "C:\\Users\\Flash\\AppData\\Roaming\\OakBot\\DB\\SubDB.db";
        private const string TRAINFILE = @"C:\Users\Flash\AppData\Roaming\OakBot\Bin\";

        private IChatConnectionService _chat;        

        private SubDB _subDB = new SubDB();

        private SubTrain _train;
        private SubTrain _trainEnd;

        private System.Timers.Timer _trainStart = new System.Timers.Timer(300000);

        private string _prefix = string.Empty;

        private int _subCount;

        private int _trainCount = 0;
        private int _trainDayHigh = 0;
        private int _trainHigh = 0;

        public SubWelcomeModel(IChatConnectionService chat)
        {
            // Register to the shutdown notification
            Messenger.Default.Register<NotificationMessage>(this, "shutdown", (msg) => { _vm_OnShutdown(); });

            // Set references to services
            _chat = chat; // Twitch chat service

            // Register to events
            _chat.RawMessageReceived += _chat_RawMessageReceived;


            // Initialize Timers
            _trainEnd = new SubTrain(180000, "Two minutes until the edeTRAIN departs!!", chat);
            _train = new SubTrain(300000, "The edeTRAIN has just departed!! edeBRUH", chat);

            _trainStart.AutoReset = false;

            //Build String for Database
            BuildConnectionString(DBFILE);

            if (!Directory.Exists(TRAINFILE))
            {
                Directory.CreateDirectory(TRAINFILE);
                File.WriteAllText(TRAINFILE + "HighestTrain.txt", _trainHigh.ToString());
            }           
               
            
            // Load in Saved Highest Sub Train
            var tr = new StreamReader(TRAINFILE + "HighestTrain.txt");
            _trainHigh = Convert.ToInt32(tr.ReadLine());
            tr.Close();
    }

        /// <summary>
        /// Raw Message received event handler, fired on every IRC message received.
        /// </summary>
        private void _chat_RawMessageReceived(object sender, ChatConnectionMessageReceivedEventArgs e)
        {
            
            // raw message UserNotice
            if (e.ChatMessage.Command == IrcCommand.UserNotice)
            {
                if (e.ChatMessage.NoticeType == NoticeMessageType.Sub || e.ChatMessage.NoticeType == NoticeMessageType.Resub || e.ChatMessage.NoticeType == NoticeMessageType.SubGift)
                {
                    _subCount += 1;

                    int tier;
                    var welcomeMessage = string.Empty;
                    var ID = e.ChatMessage.UserId;
                    var Name = e.ChatMessage.SubscriptionLogin;
                    bool Add = false;

                    var sub = new Sub();                    

                    if ((int)e.ChatMessage.SubscriptionPlan == 0)
                    {
                        tier = 1;
                    }
                    else
                    {
                        tier = (int)e.ChatMessage.SubscriptionPlan;
                    }


                    switch (e.ChatMessage.NoticeType)
                    {
                        case NoticeMessageType.Sub:
                            welcomeMessage = " edeANGEL Welcome " + Name + ", edeWINK edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP";
                            break;

                        case NoticeMessageType.Resub:
                            welcomeMessage = " edeANGEL Welcome back " + Name + ", edeWINK edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP";
                            break;

                        case NoticeMessageType.SubGift:
                            Name = e.ChatMessage.GiftRecipientUserName;
                            ID = e.ChatMessage.GiftRecipientUserID;
                            welcomeMessage = " edeANGEL Welcome " + Name + ", edeWINK edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP (Courtesy of " + e.ChatMessage.SubscriptionLogin + ")";
                            break;
                    }

                    if (_subDB.IsSub(ID))
                    {
                        Add = false;
                        sub = _subDB.GetSub(ID);

                        if (tier != sub.Tier)
                        {
                            sub.Tier = tier;

                            if (tier > sub.Tier)
                            {
                                _prefix = "+";
                            }
                            else if (tier < sub.Tier)
                            {
                                _prefix = "-";
                            }

                            if (tier == 3)
                            {
                                sub.New = true;
                            }
                        }
                        else
                        {
                            _prefix = string.Empty;
                        }

                        if(sub.Name != Name)
                        { 
                            sub.Name = Name;
                        }
                    }
                    else
                    {
                        _prefix = string.Empty;
                        Add = true;                        
                        sub.Name = Name;
                        sub.UserID = ID;
                        sub.Tier = tier;
                        if (tier == 3)
                        {
                            sub.New = true;
                        }
                    }

                    _chat.SendMessage("/me " + _prefix + welcomeMessage, false);

                    if (_train.IsTrain() || _trainStart.Enabled)
                    {
                        _trainCount += 1;
                        _chat.SendMessage("edeTRAIN edeTRAIN edeTRAIN edeTRAIN edeTRAIN " + _trainCount, false);
                        _train.ResetTrain();
                        _trainEnd.ResetTrain();
                    }
                    else
                    {
                        _trainCount = 1;
                        _trainStart.Start();
                    }

                    if (_trainCount > _trainDayHigh)
                    {
                        _trainDayHigh = _trainCount;

                        if (_trainDayHigh > _trainHigh)
                        {
                            _trainHigh = _trainDayHigh;

                            // Save Highest Sub Train
                            var tw = new StreamWriter(TRAINFILE + "HighestTrain.txt");
                            tw.WriteLine(_trainHigh);
                            tw.Close();
                        }
                    }

                    if (sub.New)
                    {
                        _chat.SendMessage("/me " + Name + ", edeO Congrats on the tier 3 sub and your new intro please contact Flash0429, Kushu83 or RedPiIl and we will get you set all setup edeGOOD", false);
                    }                                 

                    if (Add)
                    {
                        _subDB.AddSub(sub);
                    } 
                    else
                    {
                        _subDB.UpdateSub(sub);
                    }               
                }               
            }
            
            // normal chat message
            if (e.ChatMessage.Command == IrcCommand.PrivMsg)
            {
                string[] message = e.ChatMessage.Message.Split();
                if (e.ChatMessage.IsModerator || e.ChatMessage.Badges.Exists(x => x.Contains("broadcaster")))
                {
                    switch (message[0])
                    {
                        case "~subs":
                            _chat.SendMessage("Subs Today: " + _subCount, false);
                            break;

                        case "~train":
                            if (_train.IsTrain())
                            {
                                _chat.SendMessage("Train Length: " + _trainCount + " - Train Departure: " + _train.GetTime() + " - Longest Train Today: " + _trainDayHigh + " - Longest Train All Time: " + _trainHigh, false);
                            }
                            else
                            {
                                _chat.SendMessage("The edeTRAIN has not arrived yet!!", false);
                            }
                            break;

                        case "~set":
                            if (message.Length == 2)
                            {
                                _subCount = Convert.ToInt32(message[1]);
                            }
                            break;

                        case "~test":
                            if (_subDB.IsSub(e.ChatMessage.UserId))
                            {
                                _chat.SendMessage("in", false);
                            }
                            else
                            {
                                var sub = new Sub();

                                sub.UserID = e.ChatMessage.UserId;
                                sub.Name = e.ChatMessage.Author;
                                sub.Tier = 1;

                                _subDB.AddSub(sub);

                            }
                            break;
                    }
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
