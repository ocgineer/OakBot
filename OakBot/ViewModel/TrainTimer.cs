using System;
using System.Timers;


namespace OakBot.ViewModel
{
    public class TrainTimer : Timer
    {
        private DateTime m_dueTime;

        //private SubWelcomeModel Main

        public TrainTimer(double interval) : base(interval)
        {
            this.Elapsed += this.ElapsedAction;
            this.AutoReset = false;
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
            Console.WriteLine("testing ");
        }
    }
}
