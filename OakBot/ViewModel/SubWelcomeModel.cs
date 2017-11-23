using System;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

using OakBot.Model;
using System.IO;
using System.Timers;

namespace OakBot.ViewModel
{
    public class SubWelcomeModel : ViewModelBase
    {
        /// <summary>
        /// Appdata Location and Database File Location
        /// </summary>

        private const string DBFILE = "Data Source=C:\\Users\\Flash\\AppData\\Roaming\\OakBot\\DB\\SubDB.db;Version=3;";
        private const string TRAINFILE = @"C:\Users\Flash\AppData\Roaming\OakBot\Bin\";

        private IChatConnectionService _chat;

        private DBService DBsvc;

        private SubTrain _trainStart = new SubTrain(300000);
        private SubTrain _trainEnd = new SubTrain(300000);
        private SubTrain _trainWarn = new SubTrain(180000);        

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
            _trainWarn.SetElapsed(TrainWarnElapsedAction);
            _trainEnd.SetElapsed(TrainEndElapsedAction);


            //Create Svc and open Connection to DB
            DBsvc = new DBService(DBFILE);

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

                    if (IsSub(ID))
                    {
                        Add = false;
                        sub = GetSub(ID);

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

                    if (_trainEnd.IsTrain() || _trainStart.IsTrain())
                    {
                        _trainCount += 1;
                        _chat.SendMessage("edeTRAIN edeTRAIN edeTRAIN edeTRAIN edeTRAIN " + _trainCount, false);
                        _trainEnd.ResetTrain();
                        _trainWarn.ResetTrain();
                    }
                    else
                    {
                        _trainCount = 1;
                        _trainStart.StartTrain();
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
                        AddSub(sub);
                    } 
                    else
                    {
                        UpdateSub(sub);
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
                            if (_trainEnd.IsTrain())
                            {
                                _chat.SendMessage("Train Length: " + _trainCount + " - Train Departure: " + _trainEnd.GetTime() + " - Longest Train Today: " + _trainDayHigh + " - Longest Train All Time: " + _trainHigh, false);
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
                            if (IsSub(e.ChatMessage.UserId))
                            {
                                _chat.SendMessage("in", false);
                            }
                            else
                            {
                                var sub = new Sub();

                                sub.UserID = e.ChatMessage.UserId;
                                sub.Name = e.ChatMessage.Author;
                                sub.Tier = 1;

                                AddSub(sub);

                            }
                            break;
                    }
                }

            }
        }                       

        private void AddSub(Sub newSub) => DBsvc.Add(newSub);

        private void UpdateSub(Sub existingSub) => DBsvc.Update(existingSub);

        private Sub GetSub(string id) => DBsvc.GetById(id);

        private bool IsSub(string id) => string.IsNullOrEmpty((DBsvc.GetById(id)).Name) ? false : true;

        private void TrainEndElapsedAction(object sender, ElapsedEventArgs e) => _chat.SendMessage("The edeTRAIN has just departed!! edeBRUH", false);

        private void TrainWarnElapsedAction(object sender, ElapsedEventArgs e) => _chat.SendMessage("Two minutes until the edeTRAIN departs!!", false);



        /// <summary>
        /// Shutdown message handler, handle shutdown.
        /// </summary>
        private void _vm_OnShutdown()
        {
            
        }
    }
}
