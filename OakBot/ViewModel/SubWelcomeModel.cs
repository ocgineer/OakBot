using System;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

using OakBot.Model;
using System.IO;
using System.Timers;
using System.Windows.Media;
using System.Linq;

namespace OakBot.ViewModel
{
    public class SubWelcomeModel : ViewModelBase
    {
        /// <summary>
        /// Train File Location and Database File Location
        /// </summary>

        private static readonly string DBFILE = "Data Source= " + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\OakBot\\DB\\MainDB.db;Version=3;";
        private static readonly string TRAINFILE = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData) + "\\OakBot\\Bin";

        private IChatConnectionService chat;

        private DBService.SubService svc;

        private CustomTimer trainStart = new CustomTimer(300000);
        private CustomTimer trainEnd = new CustomTimer(300000);
        private CustomTimer trainWarn = new CustomTimer(180000);

        private CustomTimer raid = new CustomTimer(300000);

        private string prefix = string.Empty;

        private int subCount;

        private int trainCount = 0;
        private int trainDayHigh = 0;
        private int trainHigh = 0;

        private const string mLink = "https://multi.raredrop.co/tedemonster";
        private const string noMulti = "Sorry, No Multi Right Now";
        private string multi = noMulti;
        private string multiMessage = "Watch EdE and Friends Here:";

        public SubWelcomeModel(IChatConnectionService chat)
        {
            // Register to the shutdown notification
            Messenger.Default.Register<NotificationMessage>(this, "shutdown", (msg) => { _vm_OnShutdown(); });

            // Set references to services
            this.chat = chat; // Twitch chat service

            // Register to events
            this.chat.RawMessageReceived += _chat_RawMessageReceived;


            // Initialize Timers
            trainWarn.Elapsed += TrainWarnElapsedAction;
            trainEnd.Elapsed += TrainEndElapsedAction;

            raid.Elapsed += RaidElapsedAction;


            //Create Svc and open Connection to DB
            svc = new DBService.SubService(DBFILE);

            if (!Directory.Exists(TRAINFILE))
            {
                Directory.CreateDirectory(TRAINFILE);
                File.WriteAllText(TRAINFILE + "\\HighestTrain.txt", trainHigh.ToString());
            }                  
            
            // Load in Saved Highest Sub Train
            var tr = new StreamReader(TRAINFILE + "\\HighestTrain.txt");
            trainHigh = Convert.ToInt32(tr.ReadLine());
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
                    subCount += 1;
                    
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
                            welcomeMessage = " edeHI Welcome " + Name + ", edeHYPE Thank you for being our " + subCount + " subscriber today!! edeWINK edePIMP edePIMP";
                            break;

                        case NoticeMessageType.Resub:
                            welcomeMessage = " edeHI Welcome back " + Name + ", edeGOOD Thank you for being our " + subCount + " subscriber today!! edeWINK edePIMP edePIMP";
                            break;

                        case NoticeMessageType.SubGift:
                            Name = e.ChatMessage.GiftRecipientUserName;
                            ID = e.ChatMessage.GiftRecipientUserID;
                            welcomeMessage = " edeHI Welcome " + Name + ", edeHYPE Thank you for being our " + subCount + " subscriber today!! edeWINK edePIMP edePIMP (Courtesy of " + e.ChatMessage.SubscriptionLogin + ") edeLOVE edeLOVE";
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
                                prefix = "+";
                            }
                            else if (tier < sub.Tier)
                            {
                                prefix = "-";
                            }

                            if (tier == 3)
                            {
                                sub.New = true;
                            }
                        }
                        else
                        {
                            prefix = string.Empty;
                        }

                        if (sub.Name != Name)
                        {
                            sub.Name = Name;
                        }
                    }
                    else
                    {
                        prefix = string.Empty;
                        Add = true;
                        sub.Name = Name;
                        sub.UserID = ID;
                        sub.Tier = tier;
                        if (tier == 3)
                        {
                            sub.New = true;
                        }
                    }

                    chat.SendMessage("/me " + prefix + " " + welcomeMessage, false);

                    if (trainEnd.Enabled || trainStart.Enabled)
                    {
                        trainCount += 1;
                        chat.SendMessage("edeTRAIN edeTRAIN edeTRAIN edeTRAIN edeTRAIN " + trainCount, false);
                        trainEnd.Reset();
                        trainWarn.Reset();
                    }
                    else
                    {
                        trainCount = 1;
                        trainStart.Start();
                    }

                    if (trainCount > trainDayHigh)
                    {
                        trainDayHigh = trainCount;

                        if (trainDayHigh > trainHigh)
                        {
                            trainHigh = trainDayHigh;

                            // Save Highest Sub Train
                            var tw = new StreamWriter(TRAINFILE + "HighestTrain.txt");
                            tw.WriteLine(trainHigh);
                            tw.Close();
                        }
                    }

                    if (sub.New)
                    {
                        chat.SendMessage("/me " + Name + ", edeHYPE Congrats on the tier 3 sub please contact Flash0429 and he will get you set all setup with your intro!! edeGOOD", false);
                    }

                    if (Add)
                    {
                        AddSub(sub);
                    }
                    else
                    {
                        UpdateSub(sub);
                    }

                    var r = new Random();
                    var color = String.Format("#{0:X6}", r.Next(0x1000000));

                    chat.SendMessage("/color " + color, false);


                }

                if (e.ChatMessage.NoticeType == NoticeMessageType.Raid)
                {
                    chat.SendMessage("/followersoff", false);

                    raid.Start();

                    chat.SendMessage("Thank You " + e.ChatMessage.RaidUserName + " For the raid of " + e.ChatMessage.RaidCount + " viewers! edeCAP edeZORD edeCAP edeZORD edeCAP edeZORD", false);

                }
            }

            // normal chat message
            if (e.ChatMessage.Command == IrcCommand.PrivMsg)
            {
                string[] message = e.ChatMessage.Message.Split();

                var emotes = e.ChatMessage.UsedEmotes;

                

                if (e.ChatMessage.Message.Contains("miistyDab") || e.ChatMessage.Message.Contains("broD"))
                {
                    chat.SendMessage("/timeout " + e.ChatMessage.Author + " 1 No Dabbing", false);
                }

                if (message[0] == "!multi")
                {
                    if (multi.Contains("Sorry, No"))
                    {
                        chat.SendMessage(multi, false);
                    }
                    else
                    {
                        chat.SendMessage(multiMessage + " " + multi + "/Lcolumns", false);
                    }
                }
                
                if (e.ChatMessage.IsModerator || e.ChatMessage.Badges.Exists(x => x.Contains("broadcaster")) || e.ChatMessage.IsSubscriber)
                {
                    switch (message[0])
                    {
                        case "!subs":
                            chat.SendMessage("Subs Today: " + subCount, false);
                            break;

                        case "!train":
                            if (trainEnd.Enabled)
                            {
                                chat.SendMessage("Train Length: " + trainCount + " - Train Departure: " + trainEnd.GetTimeLeft() + " - Longest Train Today: " + trainDayHigh + " - Longest Train All Time: " + trainHigh, false);
                            }
                            else
                            {
                                chat.SendMessage("The edeTRAIN has not arrived yet!!", false);
                            }
                            break;                

                    }
                }

                
                
                if (e.ChatMessage.IsModerator || e.ChatMessage.Badges.Exists(x => x.Contains("broadcaster")))
                {
                    switch (message[0])
                    {
                        case "~set":
                            if (message.Length == 2)
                            {
                                subCount = Convert.ToInt32(message[1]);
                            }
                            break;

                        case "~start":
                            trainStart.Start();
                            break;

                        case "!multiset":
                            SetMulti(message);                            
                            break;

                        case "!multiadd":
                            if (multi.Contains("Sorry, No"))
                            {
                                chat.SendMessage("Sorry, No multi to edit right now, use !multiset to set the multi first", false);
                            }
                            else
                            {
                                AddMulti(message);
                            }
                            break;

                        case "!multidel":
                            if (multi.Contains("Sorry, No"))
                            {
                                chat.SendMessage("Sorry, No multi to edit right now, use !multiset to set the multi first", false);
                            }
                            else
                            {
                                DelMulti(message);
                            }
                            break;

                        case "!multimsg":
                            if (multi.Contains("Sorry, No"))
                            {
                                chat.SendMessage("Sorry, No multi to edit right now, use !multiset to set the multi first", false);
                            }
                            else
                            {
                                multiMessage = string.Join(" ", message.Select(i => i.Trim()).Skip(1));
                                chat.SendMessage("Changed edeGOOD", false);
                            }
                            break;
                        case "!multiclear":
                            if (multi.Contains("Sorry, No"))
                            {
                                chat.SendMessage("Already Cleared edeBRUH", false);
                            }
                            else
                            {
                                multi = noMulti;
                                chat.SendMessage("Multi Cleared edeANGEL", false);
                            }
                            break;
                    }
                }
            }
        }

        private void SetMulti(string[] m)
        {
            var dups = 0;
            var serv = "t";
            if (m.Length >= 2)
            {
                var i = 1;

                multi = mLink;

                while (i < m.Length)
                {
                    switch (m[i].ToLower())
                    {
                        case "twitch":
                            serv = "t";
                            i += 1;
                            break;
                        case "facebook":
                            serv = "f";
                            i += 1;
                            break;
                        case "mixer":
                            serv = "m";
                            i += 1;
                            break;
                        case "youtube":
                            serv = "y";
                            i += 1;
                            break;
                        case "smashcast":
                            serv = "sc";
                            i += 1;
                            break;
                        case "streamme":
                            serv = "sm";
                            i += 1;
                            break;
                        case "douyu":
                            serv = "d";
                            i += 1;
                            break;
                        case "chew":
                            serv = "c";
                            i += 1;
                            break;
                        case "liveedu":
                            serv = "l";
                            i += 1;
                            break;
                        case "mobcrush":
                            serv = "M";
                            i += 1;
                            break;
                        case "gg":
                            serv = "g";
                            i += 1;
                            break;
                        case "ustream":
                            serv = "u";
                            i += 1;
                            break;
                        case "cybertv":
                            serv = "C";
                            i += 1;
                            break;                        
                    }

                    if (!multi.Contains(m[i].ToLower()))
                    {
                        multi += "/" + serv + m[i].ToLower();
                        i += 1;
                    }
                    else
                    {
                        dups += 1;
                        i += 1;
                    }
                }

                chat.SendMessage("Multi Set edeGOOD", false);
                if (dups > 0)
                {
                    chat.SendMessage(dups + " Duplicate casters not added edeWINK", false);
                }
            }
            else
            {
                chat.SendMessage("Please enter a caster name to set multi edeBRUH", false);
            }

            
            
        }

        private void AddMulti(string[] m)
        {
            var dups = 0;
            var serv = "t";
            if (m.Length >= 2)
            {
                var i = 1;     

                while (i < m.Length)
                {
                    switch (m[i].ToLower())
                    {
                        case "twitch":
                            serv = "t";
                            i += 1;
                            break;
                        case "facebook":
                            serv = "f";
                            i += 1;
                            break;
                        case "mixer":
                            serv = "m";
                            i += 1;
                            break;
                        case "youtube":
                            serv = "y";
                            i += 1;
                            break;
                        case "smashcast":
                            serv = "sc";
                            i += 1;
                            break;
                        case "streamme":
                            serv = "sm";
                            i += 1;
                            break;
                        case "douyu":
                            serv = "c";
                            i += 1;
                            break;
                        case "chew":
                            serv = "d";
                            i += 1;
                            break;
                        case "liveedu":
                            serv = "l";
                            i += 1;
                            break;
                        case "mobcrush":
                            serv = "M";
                            i += 1;
                            break;
                        case "gg":
                            serv = "g";
                            i += 1;
                            break;
                        case "ustream":
                            serv = "u";
                            i += 1;
                            break;
                        case "cybertv":
                            serv = "C";
                            i += 1;
                            break;                        
                    }

                    if (!multi.Contains(m[i].ToLower()))
                    {
                        multi += "/" + serv + m[i].ToLower();
                        i += 1;
                    }
                    else
                    {
                        dups += 1;
                        i += 1;
                    }
                }

                chat.SendMessage("Multi Set edeGOOD", false);
                if (dups > 0)
                {
                    chat.SendMessage(dups + " Duplicate casters not added edeWINK", false);
                }
            }
            else
            {
                chat.SendMessage("Please enter a caster name to add to multi edeBRUH", false);
            }

            
        }

        private void DelMulti(string[] m)
        {
            var serv = "t";
            if (m.Length >= 2)
            {
                var i = 1;

                while (i < m.Length)
                {
                    switch (m[i].ToLower())
                    {
                        case "twitch":
                            serv = "t";
                            i += 1;
                            break;
                        case "facebook":
                            serv = "f";
                            i += 1;
                            break;
                        case "mixer":
                            serv = "m";
                            i += 1;
                            break;
                        case "youtube":
                            serv = "y";
                            i += 1;
                            break;
                        case "smashcast":
                            serv = "sc";
                            i += 1;
                            break;
                        case "streamme":
                            serv = "sm";
                            i += 1;
                            break;
                        case "douyu":
                            serv = "c";
                            i += 1;
                            break;
                        case "chew":
                            serv = "d";
                            i += 1;
                            break;
                        case "liveedu":
                            serv = "l";
                            i += 1;
                            break;
                        case "mobcrush":
                            serv = "M";
                            i += 1;
                            break;
                        case "gg":
                            serv = "g";
                            i += 1;
                            break;
                        case "ustream":
                            serv = "u";
                            i += 1;
                            break;
                        case "cybertv":
                            serv = "C";
                            i += 1;
                            break;                        
                    }

                    if (m[i].ToLower() == "edemonster")
                    {
                        chat.SendMessage("Cannot remove channel name from multi link edeBRUH", false);
                        i += 1;
                    }
                    else
                    {
                        multi = multi.Replace("/" + serv + m[i].ToLower(), "");
                        i += 1;
                    }
                }

                chat.SendMessage("Removed edeFEELS ", false);
            }
            else
            {
                chat.SendMessage("Please enter a caster name to remove from multi edeBRUH", false);
            }
            
        }

        private void AddSub(Sub newSub) => svc.Add(newSub);

        private void UpdateSub(Sub existingSub) => svc.Update(existingSub);

        private Sub GetSub(string id) => svc.GetById(id);

        private bool IsSub(string id) => string.IsNullOrEmpty((svc.GetById(id)).Name) ? false : true;

        private void TrainEndElapsedAction(object sender, ElapsedEventArgs e) => chat.SendMessage("The edeTRAIN has just departed!! edeBRUH", false);

        private void TrainWarnElapsedAction(object sender, ElapsedEventArgs e) => chat.SendMessage("Two minutes until the edeTRAIN departs!!", false);

        private void RaidElapsedAction(object sender, ElapsedEventArgs e) => chat.SendMessage("/followers 5m", false);



        /// <summary>
        /// Shutdown message handler, handle shutdown.
        /// </summary>
        private void _vm_OnShutdown()
        {
            
        }
    }
}
