using System;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.CommandWpf;

using OakBot.Model;

namespace OakBot.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        // Services
        private readonly IChatConnectionService _ccs;
        private readonly IWebSocketEventService _wse;
        private readonly ITwitchPubSubService _pss;
        private readonly IChatterDatabaseService _cds;

        private MainSettings _mainSettings;

        private TwitchCredentials _botCredentials;
        private TwitchCredentials _casterCredentials;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IChatConnectionService ccs, IWebSocketEventService wse,
            ITwitchPubSubService pss, IChatterDatabaseService cds)
        {          
            Title = "OakBot - YATB";

            // Set dependency injection references
            _ccs = ccs;
            _wse = wse;
            _pss = pss;
            _cds = cds;

            // Register for chat connection service events
            _ccs.Authenticated += _ccs_Authenticated;
            _ccs.Disconnected += _ccs_Disconnected;

            // Load settings
            var loaded = BinaryFile.ReadEncryptedBinFile("LoginSettings");
            if (loaded != null && loaded is MainSettings)
            {
                // Success, set last saved settings
                _mainSettings = (MainSettings)loaded;

                // Set loaded settings values through properties for UI
                ChannelName = _mainSettings.Channel;
                BotUsername = _mainSettings.BotUsername;
                CasterUsername = _mainSettings.CasterUsername;
                IsUsingSSL = _mainSettings.UseSecureConnection;

                // Set bot credentials
                if (!string.IsNullOrWhiteSpace(_mainSettings.BotOauthKey))
                {
                    IsBotOauthSet = true;
                    _botCredentials = new TwitchCredentials(
                        _mainSettings.BotUsername, _mainSettings.BotOauthKey, false);
                }

                // Set caster credentials
                if (!string.IsNullOrWhiteSpace(_mainSettings.CasterOauthKey))
                {
                    IsCasterOauthSet = true;
                    _casterCredentials = new TwitchCredentials(
                        _mainSettings.CasterUsername, _mainSettings.CasterOauthKey, true);

                    // Try connection PubSub
                    _pss.Connect(_casterCredentials);
                }
            }
            else
            {
                // Failure, load defaults
                _mainSettings = new MainSettings();
            }

            // Start WebSocket Event Service
            _wse.StartService(1337, "oakbotapitest");

        }

        #endregion

        #region Private Methods

        private void OnSettingsChanged()
        {
            BinaryFile.WriteEncryptedBinFile("LoginSettings", _mainSettings);
            _ccs.SetJoiningChannel(_mainSettings.Channel, _isUsingSSL);
        }

        private void ConnectDisconnectChat(bool isCaster)
        {
            if (isCaster)
            {
                if (IsCasterConnected)
                {
                    _ccs.Disconnect(true);
                }
                else
                {
                    // Credentials should be present
                    if (_casterCredentials != null)
                    {
                        // Set credentials
                        _ccs.SetCredentials(_casterCredentials);

                        // Connect
                        _ccs.Connect(true);
                    }
                }
            }
            else
            {
                if (IsBotConnected)
                {
                    if (IsCasterConnected)
                    {
                        ConnectDisconnectChat(true);
                    }

                    // Disconnect chat service
                    _ccs.Disconnect(false);

                    // stop database service
                    _cds.StopService();
                }
                else
                {
                    // Credentials should be present
                    if (_botCredentials != null)
                    {
                        // Set channel
                        _ccs.SetJoiningChannel(ChannelName, true);

                        // Set credentials
                        _ccs.SetCredentials(_botCredentials);

                        // Connect chat service
                        _ccs.Connect(false);

                        // Start database service
                        _cds.StartService(ChannelName);
                    }
                }
            }
        }

        private bool CanConnectDisconnectExecute(bool isCaster)
        {
            // Return false if channel name is not valid
            if (!Regex.IsMatch(ChannelName ?? string.Empty, @"^[a-z0-9][a-z0-9_]{3,24}$"))
                return false;

            if (isCaster)
            {
                // Return false if caster oauth is not set
                if (!IsCasterOauthSet)
                    return false;

                // Return result for valid given caster username
                return Regex.IsMatch(CasterUsername ?? string.Empty, @"^[a-z0-9][a-z0-9_]{3,24}$");
            }
            else
            {
                // Return false if bot oauth is not set
                if (!IsBotOauthSet)
                    return false;

                // Return false for valid given bot username
                return Regex.IsMatch(BotUsername ?? string.Empty, @"^[a-z0-9][a-z0-9_]{3,24}$");
            }
        }

        #endregion

        #region Event Handlers

        private void _ccs_Authenticated(object sender, ChatConnectionAuthenticatedEventArgs e)
        {
            if (e.IsAuthenticated)
            {
                // Set connected state for UI
                if (e.Account.IsCaster)
                {
                    IsCasterConnected = true;
                }
                else
                {
                    IsBotConnected = true;
                    _wse.BroadcastEvent("OAKBOT_CHAT_CONNECTED", new { name = e.Account.Username });
                }
            }
        }

        private void _ccs_Disconnected(object sender, ChatConnectionDisconnectedEventArgs e)
        {
            // Set flag for UI notification
            if (e.Account.IsCaster)
            {
                IsCasterConnected = false;
            }
            else
            {
                IsBotConnected = false;
                _wse.BroadcastEvent("OAKBOT_CHAT_DISCONNECTED", new { name = e.Account.Username });
            }
        }

        #endregion

        #region General Properties

        public string Title { get; private set; }

        private ICommand _cmdOnClose;
        public ICommand CmdOnClose
        {
            get
            {
                return _cmdOnClose ??
                    (_cmdOnClose = new RelayCommand<CancelEventArgs>(
                        args =>
                        {
                            var res = MessageBox.Show("Are you sure to shutdown OakBot?",
                                "OakBot - Shutdown Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (res == MessageBoxResult.Yes)
                            {
                                // Disconnect caster
                                if (IsCasterConnected)
                                    _ccs.Disconnect(true);

                                // Disconnect bot
                                if (IsBotConnected)
                                    _ccs.Disconnect(false);

                                // Send a shutting down message
                                Messenger.Default.Send(new NotificationMessage("User initiated shutdown."), "shutdown");

                                // Continue shutting down
                                args.Cancel = false;
                            }
                            else
                            {
                                // Cancel shutting down
                                args.Cancel = true;
                            }
                        }
                    ));
            }
        }

        #endregion

        #region Authentication and Chat Connect Properties

        private string _channelName;
        public string ChannelName
        {
            get
            {
                return _channelName;
            }
            set
            {
                var lowered = value.ToLower();
                if (lowered != _channelName)
                {
                    _channelName = lowered;
                    RaisePropertyChanged();

                    _mainSettings.Channel = lowered;
                    OnSettingsChanged();
                }
            }
        }

        private string _botUsername;
        public string BotUsername
        {
            get
            {
                return _botUsername;
            }
            set
            {
                var lowered = value.ToLower();
                if (lowered != _botUsername)
                {
                    _botUsername = lowered;
                    RaisePropertyChanged();

                    _mainSettings.BotUsername = lowered;
                    OnSettingsChanged();
                }
            }
        }

        private bool _isBotOauthSet;
        public bool IsBotOauthSet
        {
            get
            {
                return _isBotOauthSet;
            }
            private set
            {
                _isBotOauthSet = value;
                RaisePropertyChanged();
            }
        }

        private ICommand _cmdAuthBot;
        public ICommand CmdAuthBot
        {
            get
            {
                return _cmdAuthBot ??
                    (_cmdAuthBot = new RelayCommand(
                        () =>
                        {
                            string res = Authentication.AuthenticateTwitch(BotUsername, true);
                            if (res != null)
                            {
                                _mainSettings.BotOauthKey = res;
                                OnSettingsChanged();

                                IsBotOauthSet = true;
                                _botCredentials = new TwitchCredentials(
                                    _mainSettings.BotUsername, _mainSettings.BotOauthKey, false);
                            }
                        },
                        () =>
                        {
                            return (!string.IsNullOrWhiteSpace(BotUsername) &&
                                Regex.IsMatch(BotUsername, @"^[a-z0-9][a-z0-9_]{3,24}$"));
                        }
                    ));
            }
        }

        private string _casterUsername;
        public string CasterUsername
        {
            get
            {
                return _casterUsername;
            }
            set
            {
                var lowered = value.ToLower();
                if (lowered != _casterUsername)
                {
                    _casterUsername = lowered;
                    RaisePropertyChanged();

                    _mainSettings.CasterUsername = lowered;
                    OnSettingsChanged();
                }
            }
        }

        private bool _isCasterOauthSet;
        public bool IsCasterOauthSet
        {
            get
            {
                return _isCasterOauthSet;
            }
            private set
            {
                _isCasterOauthSet = value;
                RaisePropertyChanged();
            }
        }

        private ICommand _cmdAuthCaster;
        public ICommand CmdAuthCaster
        {
            get
            {
                return _cmdAuthCaster ??
                    (_cmdAuthCaster = new RelayCommand(
                        () =>
                        {
                            string res = Authentication.AuthenticateTwitch(CasterUsername, false);
                            if (res != null)
                            {
                                _mainSettings.CasterOauthKey = res;
                                OnSettingsChanged();

                                IsCasterOauthSet = true;
                                _casterCredentials = new TwitchCredentials(
                                    _mainSettings.CasterUsername, _mainSettings.CasterOauthKey, true);
                            }
                        },
                        () =>
                        {
                            return (!string.IsNullOrWhiteSpace(CasterUsername) &&
                                Regex.IsMatch(CasterUsername, @"^[a-z0-9][a-z0-9_]{3,24}$"));
                        }
                    ));
            }
        }

        private bool _isUsingSSL;
        public bool IsUsingSSL
        {
            get
            {
                return _isUsingSSL;
            }
            set
            {
                if (value != _isUsingSSL)
                {
                    _isUsingSSL = value;
                    RaisePropertyChanged();

                    _mainSettings.UseSecureConnection = value;
                    OnSettingsChanged();
                }
            }
        }

        private bool _isBotConnected;
        public bool IsBotConnected
        {
            get
            {
                return _isBotConnected;
            }
            private set
            {
                if (value != _isBotConnected)
                {
                    _isBotConnected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICommand _cmdConnectBot;
        public ICommand CmdConnectBot
        {
            get
            {
                return _cmdConnectBot ?? (_cmdConnectBot = new RelayCommand(
                        () => ConnectDisconnectChat(false),
                        () => CanConnectDisconnectExecute(false)
                    ));
            }
        }

        private bool _isCasterConnected;
        public bool IsCasterConnected
        {
            get
            {
                return _isCasterConnected;
            }
            private set
            {
                if (value != _isCasterConnected)
                {
                    _isCasterConnected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICommand _cmdConnectCaster;
        public ICommand CmdConnectCaster
        {
            get
            {
                return _cmdConnectCaster ?? (_cmdConnectCaster = new RelayCommand(
                        () => ConnectDisconnectChat(true),
                        () => CanConnectDisconnectExecute(true)
                    ));
            }
        }

        #endregion

        private ICommand _cmdTestButton1;
        public ICommand CmdTestButton1
        {
            get
            {
                return _cmdTestButton1 ??
                    (_cmdTestButton1 = new RelayCommand(
                        () =>
                        {

                        }));
            }
        }
    }
}