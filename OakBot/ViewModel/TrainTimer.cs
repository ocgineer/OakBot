using System;
using System.Timers;


namespace OakBot.ViewModel
{
    public class TrainTimer : Timer
    {
        private DateTime m_dueTime;
        private string _message;

        private Model.IChatConnectionService _chat;

        

        public TrainTimer(double interval, string message, Model.IChatConnectionService chat) : base(interval)
        {            
            this.AutoReset = false;
            _message = message;
            _chat = chat;
            this.Elapsed += this.ElapsedAction;
        }

        protected new void Dispose()
        {
            this.Elapsed -= this.ElapsedAction;
            base.Dispose();
            
        }

        public string TimeLeft
        {
            get
            {                
                var time = TimeSpan.FromMilliseconds((this.m_dueTime - DateTime.Now).TotalMilliseconds);
                return time.ToString(@"mm\:ss");
            }
        }

        public new void Start()
        {
            this.m_dueTime = DateTime.Now.AddMilliseconds(this.Interval);
            base.Start();
        }

        private void ElapsedAction(object sender, ElapsedEventArgs e)
        {
            _chat.SendMessage(_message, false);
        }

    }
}
