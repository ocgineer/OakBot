using System.Collections.ObjectModel;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.CommandWpf;

using OakBot.Model;

namespace OakBot.ViewModel
{
    public class ConsoleViewModel : ViewModelBase
    {
        #region Fields

        private readonly IChatConnectionService _chatService;
        private readonly LimitedObservableCollection<TwitchChatMessage> _chatMessages;
        private readonly ObservableCollection<ITwitchAccount> _chatAccounts;

        private bool _systemConnected;
        private string _messageToSend;
        private ITwitchAccount _selectedAccount;
        private ICommand _cmdSendMessage;

        #endregion

        #region Constructors

        public ConsoleViewModel(IChatConnectionService chatService)
        {
            // Dependancy Injection
            _chatService = chatService;

            // Initialize collections
            _chatMessages = new LimitedObservableCollection<TwitchChatMessage>(500);
            _chatAccounts = new ObservableCollection<ITwitchAccount>();

            // Register for chatService Event
            _chatService.Connected += _chatService_Connected;
            _chatService.Authenticated += _chatService_Authenticated;
            _chatService.Disconnected += _chatService_Disconnected;
            _chatService.ChatMessageReceived += _chatService_ChatMessageReceived;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add a <see cref="TwitchChatMessage"/> to the <see cref="LimitedObservableCollection{T}"/>
        /// via the help of the UI dispatcher as collection was created on the UI thread.
        /// </summary>
        /// <param name="tcm"></param>
        private void AddChatMessage(TwitchChatMessage tcm)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                _chatMessages.AddAndTrim(tcm);
            });
        }

        /// <summary>
        /// Adds a authenticated <see cref="ITwitchAccount"/> to the account collection via the
        /// help of the UI dispatcher, and pre-selects the account if it's the first account.
        /// </summary>
        /// <param name="account"></param>
        private void AddChatAccount(ITwitchAccount account)
        {
            // Add account to collection
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                // Add
                _chatAccounts.Add(account);

                // If only account, pre-select it
                if (_chatAccounts.Count == 1)
                {
                    SelectedAccount = account;
                }
            });
        }

        /// <summary>
        /// Removes a disconnected <see cref="ITwitchAccount"/> from the account collection via
        /// the help of the UI dispatcher, and selects new account if current got removed.
        /// </summary>
        /// <param name="account"></param>
        private void RemoveChatAccount(ITwitchAccount account)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                // Remove
                _chatAccounts.Remove(account);

                // Selected account got disconnected
                if (SelectedAccount == null && _chatAccounts.Count != 0)
                {
                    SelectedAccount = _chatAccounts[0];
                }
            });
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Chat Service Connected Event Handler.
        /// Adds connection system message to chat message collection.
        /// </summary>
        private void _chatService_Connected(object sender, ChatConnectionConnectedEventArgs e)
        {
            AddChatMessage(new TwitchChatMessage(
                string.Format("{0} is successfully connected to Twitch chat server. Authenticating now.", e.Account.Username),
                "OakBot")
            );
        }

        /// <summary>
        /// Chat Service Authenticated Event Handler.
        /// Add authenticated account to collection to be used as message sender on success.
        /// Adds authentication successful or failure system message to chat message collection.
        /// </summary>
        private void _chatService_Authenticated(object sender, ChatConnectionAuthenticatedEventArgs e)
        {
            if (e.IsAuthenticated)
            {
                // Add authenticated account to chat account collection
                AddChatAccount(e.Account);

                // Add success message in console
                AddChatMessage(new TwitchChatMessage(
                    string.Format("{0} successfully authenticated, have fun!", e.Account.Username),
                    "Oakbot")
                );

                // If bot account is authenticated
                if (!e.Account.IsCaster)
                {
                    // Set system connected status
                    IsSystemConnected = true;
                }
            }
            else
            {
                // Add failure message in console
                AddChatMessage(new TwitchChatMessage(
                    string.Format("{0} failed to authenticate, please relink with Twitch!", e.Account.Username),
                    "Oakbot")
                );
            }
        }

        /// <summary>
        /// Chat Service Disconnected Event Handler.
        /// Removes disconnected account from collection to be used as message sender.
        /// Adds disconnection system message to chat message collection.
        /// </summary>
        private void _chatService_Disconnected(object sender, ChatConnectionDisconnectedEventArgs e)
        {
            // Remove disconnected account from chat account collection
            RemoveChatAccount(e.Account);

            // Add disconnection message to console
            AddChatMessage(new TwitchChatMessage(
                string.Format("{0} disconnected from the chat.", e.Account.Username),
                "OakBot"));

            // If bot is disconnected
            if (!e.Account.IsCaster)
            {
                // Set system connected status
                IsSystemConnected = false;
            }
        }

        /// <summary>
        /// Chat Service ChatMessageReceived Event Handler.
        /// Adds received message to chat message collection.
        /// </summary>
        private void _chatService_ChatMessageReceived(object sender, ChatConnectionMessageReceivedEventArgs e)
        {
            // Add received message to limited collection
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                _chatMessages.AddAndTrim(e.ChatMessage);
            });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indication if the system is connected and authenticated to Twitch chat for bindings.
        /// </summary>
        public bool IsSystemConnected
        {
            get
            {
                return _systemConnected;
            }
            private set
            {
                _systemConnected = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// The <see cref="TwitchChatMessage"/> collection for bindings.
        /// </summary>
        public LimitedObservableCollection<TwitchChatMessage> ChatMessages
        {
            get
            {
                return _chatMessages;
            }
        }

        /// <summary>
        /// The connected <see cref="ITwitchAccount"/> chat accounts collection for bindings.
        /// </summary>  
        public ObservableCollection<ITwitchAccount> ChatAccounts
        {
            get
            {
                return _chatAccounts;
            }
        }

        /// <summary>
        /// The selected account to send chat message as for bindings.
        /// </summary>
        public ITwitchAccount SelectedAccount
        {
            get
            {
                return _selectedAccount;
            }
            set
            {
                if (value != _selectedAccount)
                {
                    _selectedAccount = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Message to send to chat for bindings.
        /// </summary>
        public string MessageToSend
        {
            get
            {
                return _messageToSend;
            }
            private set
            {
                _messageToSend = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Command binding to send given message to with selected account.
        /// This will also clear the message to send towards the UI.
        /// </summary>
        public ICommand CmdSendMessage
        {
            get
            {
                return _cmdSendMessage ??
                    (_cmdSendMessage = new RelayCommand<string>(
                        (msg) =>
                        {
                            _chatService.SendMessage(msg, SelectedAccount.IsCaster);
                            MessageToSend = string.Empty;
                        },
                        (msg) =>
                        {
                            // Can execute when not null or whitespace
                            return !string.IsNullOrWhiteSpace(msg) && SelectedAccount != null;
                        }
                    ));
            }
        }

        #endregion

    }
}
