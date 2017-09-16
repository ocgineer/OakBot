using System;
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
        private static string trainFile = @"C:\Users\Flash\AppData\Roaming\OakBot\Bin\HighestTrain.txt";

        private IChatConnectionService _chat;        

        private SubDB _subDB = new SubDB();

        private SubTrain _train;
        private SubTrain _trainEnd;

        private System.Timers.Timer _trainStart = new System.Timers.Timer(300000);

        private string _prefix = "";

        private int _subCount;

        private int _trainCount = 0;
        private int _trainDayHigh = 0;
        private int _trainHigh = 0;

        public SubWelcomeModel(IChatConnectionService chat)
        {
            // Register to the shutdown notification
            Messenger.Default.Register<NotificationMessage>(this, "shutdown", (msg) => { _vm_OnShutdown(); });

            // Set refferences to services
            _chat = chat; // Twitch chat service

            // Register to events
            _chat.RawMessageReceived += _chat_RawMessageReceived;


            // Initialize Timers
            _trainEnd = new SubTrain(240000, "One minute until the edeTRAIN departs!!", chat);
            _train = new SubTrain(300000, "The edeTRAIN has just departed!! edeBRUH", chat);

            _trainStart.AutoReset = false;

            //Build String for Database
            BuildConnectionString(dbFile);

            // Check if Train File Exists others Wises Creates it
            if (!File.Exists(trainFile))
            {
                File.WriteAllText(trainFile, _trainHigh.ToString());
            }
            
            // Load in Saved Highest Sub Train
            var tr = new StreamReader(trainFile);
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
                if (e.ChatMessage.NoticeType == NoticeMessageType.Sub)
                {
                    _subCount += 1;

                    int tier;

                    if ((int)e.ChatMessage.SubscriptionPlan == 0)
                    {
                        tier = 1;
                    }
                    else
                    {
                        tier = (int)e.ChatMessage.SubscriptionPlan;
                    }

                    if (_subDB.IsSub(e.ChatMessage.UserId))
                    {
                        var sub = _subDB.GetSub(e.ChatMessage.UserId);

                        if (tier != sub.Tier)
                        {                            
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
                            
                            if (tier == 3)
                            {
                                sub.New = true;
                            }                            
                        }

                        _chat.SendMessage("/me " + _prefix + " edeANGEL Welcome " + e.ChatMessage.DisplayName + ", edeWINK edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP", false);

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

                        if (sub.New)
                        {
                            _chat.SendMessage("/me " + e.ChatMessage.DisplayName + ", edeO Congrats on the tier 3 sub and your new intro please contact Flash0429, Kushu83 or RedPiIl and we will get you set all setup edeGOOD", false);
                        }

                        if (_prefix != "")
                        {
                            sub.Tier = (int)e.ChatMessage.SubscriptionPlan;
                            _subDB.UpdateSub(sub);
                        }
                    }
                    else
                    {
                        var sub = new Sub()
                        {
                            UserID = e.ChatMessage.UserId,
                            Name = e.ChatMessage.SubscriptionLogin,
                            Tier = tier
                        };                        

                        if (tier == 3)
                        {
                            sub.New = true;
                        }

                        _chat.SendMessage("/me " + _prefix + " edeANGEL Welcome " + e.ChatMessage.DisplayName + ", edeWINK edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP", false);

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

                        if (sub.New)
                        {
                            _chat.SendMessage("/me " + e.ChatMessage.DisplayName + ", edeO Congrats on the tier 3 sub and your new intro please contact Flash0429, Kushu83 or RedPiIl and we will get you set all setup edeGOOD", false);
                        }

                        _subDB.AddSub(sub);

                        
                    }
                    if (_trainCount > _trainDayHigh)
                    {
                        _trainDayHigh = _trainCount;

                        if (_trainDayHigh > _trainHigh)
                        {
                            _trainHigh = _trainDayHigh;

                            // Save Highest Sub Train
                            var tw = new StreamWriter(trainFile);
                            tw.WriteLine(_trainHigh);
                            tw.Close();
                        }
                    }
                }

                if (e.ChatMessage.NoticeType == NoticeMessageType.Resub)
                {
                    _subCount += 1;

                    int tier;

                    if ((int)e.ChatMessage.SubscriptionPlan == 0)
                    {
                        tier = 1;
                    }
                    else
                    {
                        tier = (int)e.ChatMessage.SubscriptionPlan;
                    }                   

                    if (_subDB.IsSub(e.ChatMessage.UserId))
                    {
                        var sub = _subDB.GetSub(e.ChatMessage.UserId);

                        if (tier != sub.Tier)
                        {
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

                            if (tier == 3)
                            {
                                sub.New = true;
                            }
                        }

                        _chat.SendMessage("/me " + _prefix + " edeANGEL Welcome Back " + e.ChatMessage.DisplayName + ", edeWINK edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP", false);

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

                        if (sub.New)
                        {
                            _chat.SendMessage("/me " + e.ChatMessage.DisplayName + ", edeO Congrats on the tier 3 sub and your new intro please contact Flash0429, Kushu83 or RedPiIl and we will get you set all setup edeGOOD", false);
                        }

                        if (_prefix != "")
                        {
                            sub.Tier = (int)e.ChatMessage.SubscriptionPlan;
                            _subDB.UpdateSub(sub);
                        }
                    }
                    else
                    {
                        var sub = new Sub()
                        {
                            UserID = e.ChatMessage.UserId,
                            Name = e.ChatMessage.SubscriptionLogin,
                            Tier = tier
                        };

                        if (tier == 3)
                        {
                            sub.New = true;
                        }

                        _chat.SendMessage("/me " + _prefix + " edeANGEL Welcome Back " + e.ChatMessage.DisplayName + ", edeWINK edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP edePIMP", false);

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

                        if (sub.New)
                        {
                            _chat.SendMessage("/me " + e.ChatMessage.DisplayName + ", edeO Congrats on the tier 3 sub and your new intro please contact Flash0429, Kushu83 or RedPiIl and we will get you set all setup edeGOOD", false);
                        }

                        _subDB.AddSub(sub);
                    }

                    if (_trainCount > _trainDayHigh)
                    {
                        _trainDayHigh = _trainCount;

                        if (_trainDayHigh > _trainHigh)
                        {
                            _trainHigh = _trainDayHigh;

                            // Save Highest Sub Train
                            var tw = new StreamWriter(trainFile);
                            tw.WriteLine(_trainHigh);
                            tw.Close();
                        }
                    }
                }
            }
            
            // normal chat message
            if (e.ChatMessage.Command == IrcCommand.PrivMsg)
            {
                string[] message = e.ChatMessage.Message.Split();
                if (e.ChatMessage.IsModerator)
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
