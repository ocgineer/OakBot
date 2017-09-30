using System.Windows.Media;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace OakBot.ViewModel
{
    public class StatusBarViewModel : ViewModelBase
    {
        #region Fields

        private SolidColorBrush _botChatConnectionStatus;
        private SolidColorBrush _casterChatConnectionStatus;
        private SolidColorBrush _pubsubConnectionStatus;
        private SolidColorBrush _wsEventServiceStatus;

        #endregion

        #region Constructors

        public StatusBarViewModel()
        {
            // Subscribe to system messages
            Messenger.Default.Register<bool>(this, "SystemChatConnectionChanged", (status) => SystemChatConnectionChanged(status));
            Messenger.Default.Register<bool>(this, "CasterChatConnectionChanged", (status) => CasterChatConnectionChanged(status));
            Messenger.Default.Register<bool>(this, "PubSubConnectionChanged", (status) => PubSubConnectionChanged(status));
            Messenger.Default.Register<bool>(this, "WebSocketEventServiceStatusChanged", (status) => WebSocketEventServiceStatusChanged(status));

            // Initialize brushes
            _botChatConnectionStatus = Brushes.DarkRed;
            _casterChatConnectionStatus = Brushes.DarkRed;
            _pubsubConnectionStatus = Brushes.DarkRed;
            _wsEventServiceStatus = Brushes.DarkRed;
        }

        #endregion

        #region Private Methods

        private void SystemChatConnectionChanged(bool status)
        {
            if (status)
            {
                BotChatConnectionStatus = Brushes.DarkGreen;
            }
            else
            {
                BotChatConnectionStatus = Brushes.DarkRed;
            }
        }

        private void CasterChatConnectionChanged(bool status)
        {
            if (status)
            {
                CasterChatConnectionStatus = Brushes.DarkGreen;
            }
            else
            {
                CasterChatConnectionStatus = Brushes.DarkRed;
            }
        }

        private void PubSubConnectionChanged(bool status)
        {
            if (status)
            {
                PubSubConnectionStatus = Brushes.DarkGreen;
            }
            else
            {
                PubSubConnectionStatus = Brushes.DarkRed;
            }
        }

        private void WebSocketEventServiceStatusChanged(bool status)
        {
            if (status)
            {
                WsEventServiceStatus = Brushes.DarkGreen;
            }
            else
            {
                WsEventServiceStatus = Brushes.DarkRed;
            }
        }

        #endregion

        #region Properties

        public SolidColorBrush BotChatConnectionStatus
        {
            get
            {
                return _botChatConnectionStatus;
            }
            set
            {
                if (value != _botChatConnectionStatus)
                {
                    _botChatConnectionStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SolidColorBrush CasterChatConnectionStatus
        {
            get
            {
                return _casterChatConnectionStatus;
            }
            set
            {
                if (value != _casterChatConnectionStatus)
                {
                    _casterChatConnectionStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SolidColorBrush PubSubConnectionStatus
        {
            get
            {
                return _pubsubConnectionStatus;
            }
            set
            {
                if (value != _pubsubConnectionStatus)
                {
                    _pubsubConnectionStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SolidColorBrush WsEventServiceStatus
        {
            get
            {
                return _wsEventServiceStatus;
            }
            set
            {
                if (value != _wsEventServiceStatus)
                {
                    _wsEventServiceStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
    }
}
