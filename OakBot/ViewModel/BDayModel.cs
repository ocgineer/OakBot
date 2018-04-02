using System;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

using OakBot.Model;
using System.Linq;


namespace OakBot.ViewModel
{   
    public class BDayModel : ViewModelBase
    {
        /// <summary>
        /// Birthday Model to shoutout Subs on their birthday
        /// </summary>
        

        // Database Location
        private static readonly string DBFILE = "Data Source= " + Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData) + "\\OakBot\\DB\\MainDB.db;Version=3;";

        private IChatConnectionService chat;

        private DBService.BDayService svc;

        private string names = String.Empty;        

        public BDayModel(IChatConnectionService chat)
        {
            // Register to the shutdown notification
            Messenger.Default.Register<NotificationMessage>(this, "shutdown", (msg) => { _vm_OnShutdown(); });

            // Set references to services
            this.chat = chat; // Twitch chat service

            // Register to events
            this.chat.RawMessageReceived += _chat_RawMessageReceived;

            //Create Svc and open Connection to DB
            svc = new DBService.BDayService(DBFILE);
        }

        private void _chat_RawMessageReceived(object sender, ChatConnectionMessageReceivedEventArgs e)
        {
            if (e.ChatMessage.Command == IrcCommand.PrivMsg )
            {
                if (e.ChatMessage.IsSubscriber)
                {
                    var length = e.ChatMessage.Message.Split().Length;
                    var command = e.ChatMessage.Message.Split().First();
                    var msg = string.Join(" ", e.ChatMessage.Message.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).Skip(1));
                    var ID = e.ChatMessage.UserId;
                    var bday = new BDay();

                    if (command.ToLower() == "!mybday")
                    {
                        var IsDate = DateTime.TryParse(msg, out DateTime date);

                        if ((length == 2 || length == 3) && !Exist(ID) && IsDate)
                        {
                            bday.UserID = e.ChatMessage.UserId;
                            bday.Name = e.ChatMessage.Author;
                            bday.Date = date.ToString();
                            AddBDay(bday);
                            chat.SendMessage("edeGOOD " + e.ChatMessage.Author, false);
                        }
                        else if ((length < 2 || length > 3) || !IsDate)
                        {
                            chat.SendMessage("Correct Use is '!mybday (date)' Month and Day only! Examples: 10/3, 10-3, 10.3 or October 3 Either way works edeWINK", false);
                        }
                        else if (Exist(ID))
                        {

                        }
                    }
                    else if (Exist(ID))
                    {
                        if (!names.Contains(e.ChatMessage.UserId))
                        {
                            bday = GetUser(ID);

                            names += e.ChatMessage.UserId + " ";

                            if (DateTime.Parse(bday.Date).Month == DateTime.Today.Month && DateTime.Parse(bday.Date).Day == DateTime.Today.Day)
                            {
                                chat.SendMessage("/me edeUP edeUP edeUP Happy Birthday " + e.ChatMessage.Author + " edeUP edeUP edeUP", false);
                            }
                        }
                    }
                }
                else
                {
                    var first = e.ChatMessage.Message.Split().First();

                    if (first.ToLower() == "!mybday")
                    {
                        chat.SendMessage("Sorry " + e.ChatMessage.Author + ", !mybday is a Subscriber only feature. edeWINK", false);
                    }
                }
            }
        }

        private bool Exist(string id) => string.IsNullOrEmpty((svc.GetById(id)).Name) ? false : true;
        private void AddBDay(BDay newBDay) => svc.Add(newBDay);
        private BDay GetUser(string id) => svc.GetById(id);

        private void _vm_OnShutdown()
        {

        }        
    }
}
