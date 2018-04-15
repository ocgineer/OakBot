using System;
using System.Timers;


namespace OakBot.ViewModel
{
    public class CustomTimer : Timer, IDisposable
    {
        private DateTime m_dueTime;

        public CustomTimer(double interval) : base(interval) => this.AutoReset = false;

        protected new void Dispose() => base.Dispose();

        public string GetTimeLeft()
        {
            var time = TimeSpan.FromMilliseconds((m_dueTime - DateTime.Now).TotalMilliseconds);
            return time.ToString(@"mm\:ss");
        }

        public new void Start()
        {
            m_dueTime = DateTime.Now.AddMilliseconds(this.Interval);
            base.Start();
           
        }

        public void Reset()
        {
            this.Stop();
            this.Start();
        }
    }
}