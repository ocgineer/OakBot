using System.Timers;

namespace OakBot.ViewModel
{
    public class SubTrain 
    {

        private CustomTimer Train;

        public SubTrain(double interval)
        {
            Train = new CustomTimer(interval);
            
        }

        public bool IsTrain()
        {
            if (Train.Enabled)
            {                
                return true;
            }
            else
            {                
                return false;
            }
        }

        public string GetTime()
        {
            return Train.GetTimeLeft();
        }

        public void ResetTrain()
        {
            Train.Stop();
            Train.Start();
        }

        public void StartTrain()
        {
            Train.Start();
        }

        public void SetElapsed(ElapsedEventHandler e)
        {
            Train.Elapsed += e;
        }
    }
}
