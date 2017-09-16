namespace OakBot.ViewModel
{
    public class SubTrain 
    {

        private TrainTimer Train;

        public SubTrain(double interval, string message, Model.IChatConnectionService chat)
        {
            Train = new TrainTimer(interval, message, chat);
            
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
