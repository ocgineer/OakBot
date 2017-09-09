using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace OakBot.ViewModel
{
    public class SubTrain
    {

        private TrainTimer Train;

        public SubTrain()
        {
            Train = new TrainTimer(5000);
            
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
            return Train.TimeLeft;
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

        


    }
}
