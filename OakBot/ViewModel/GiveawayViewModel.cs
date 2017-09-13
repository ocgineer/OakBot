using System;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;

using OakBot.Model;

namespace OakBot.ViewModel
{
    public class GiveawayViewModel : ViewModelBase
    {
        #region Fields

        // Services
        private readonly IChatConnectionService _chatService;
        private readonly IWebSocketEventService _wsEventService;
        private readonly int _moduleId;
        private Random _rndGen;

        // Required items
        private Timer _timerOpen;
        private Timer _timerResponse;
        private Timer _timerElapsed;
        private DateTime _timestampDraw;
        private DateTime _timestampOpened;
        private DateTime _timestampClosed;
        private TimeSpan _elapsedOpenTime;
        private TimeSpan _elapsedNoResponseTime;
        private TimeSpan _elapsedInterval;
        private bool _isActive;
        private bool _isClosing;
        private bool _isHavingEntries;
        private bool _isDrawingWinner;
        private bool _WinnerHasReplied;
        private List<GiveawayEntry> _drawList;
        private GiveawayEntry _selectedWinner;
        private ObservableCollection<GiveawayEntry> _listEntries;
        private ObservableCollection<GiveawayEntry> _listWinners;
        private ObservableCollection<TwitchChatMessage> _listMessagesWinner;

        /* TODO: Make and Use SFX Service /w NAUDIO */
        private MediaPlayer _mediaPlayer; 

        // This modules settings
        GiveawayModuleSettings _moduleSettings;

        // Command interfacing for binding
        private ICommand _cmdOpen;
        private ICommand _cmdReOpen;
        private ICommand _cmdClose;
        private ICommand _cmdDraw;
        private ICommand _cmdRedraw;
        private ICommand _cmdClearWinners;
        private ICommand _cmdCancelTimer;
        private ICommand _cmdRemoveEntry;
        private ICommand _cmdRemoveWinner;
        private ICommand _cmdSelectAudioFile;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new Giveaway module, seperate from other modules.
        /// </summary>
        /// <param name="id">Id of the module, must be unique and can't be lower than 1.</param>
        /// <param name="rndGen">A preseeded pseudo random generator to be used in the module.</param>
        /// <param name="ccs">Chat Service to receive and send messages.</param>
        /// <param name="wes">WebSocket Event Service to push out giveaway events to websocket.</param>
        public GiveawayViewModel(int id, Random rndGen, IChatConnectionService ccs, IWebSocketEventService wes)
        {
            // Store references
            _moduleId = id;
            _rndGen = rndGen;
            _chatService = ccs;
            _wsEventService = wes;

            // Initialize collections and lists
            _listEntries = new ObservableCollection<GiveawayEntry>();
            _listWinners = new ObservableCollection<GiveawayEntry>();
            _listMessagesWinner = new ObservableCollection<TwitchChatMessage>();

            // Initialize MediaPlayer
            _mediaPlayer = new MediaPlayer();

            // Set initial timestamps and spans
            _timestampOpened = new DateTime(0);
            _timestampClosed = new DateTime(0);
            _timestampDraw = new DateTime(0);
            _elapsedOpenTime = new TimeSpan(0);
            _elapsedNoResponseTime = new TimeSpan(0);
            _elapsedInterval = new TimeSpan(0, 0, 1);

            // Initialize timers
            _timerOpen = new Timer() { AutoReset = false };
            _timerOpen.Elapsed += _TimerOpen_Elapsed;
            _timerResponse = new Timer() { AutoReset = false };
            _timerResponse.Elapsed += _TimerResponse_Elapsed;
            _timerElapsed = new Timer(_elapsedInterval.TotalMilliseconds);
            _timerElapsed.Elapsed += _TimerElapsed_Elapsed;
            _timerElapsed.Start();

            // Set a 'winner' to display
            _selectedWinner = new GiveawayEntry { DisplayName = "WINNER" };
            _WinnerHasReplied = true;
            
            // Load in previous saved settings else use new default settings
            var loaded = BinaryFile.ReadBinFile($"GivewawayModule{ID}");
            if (loaded != null && loaded is GiveawayModuleSettings)
            {
                // Success, set last saved settings and set previous winners
                _moduleSettings = (GiveawayModuleSettings)loaded;
                if (_moduleSettings.SavedWinnersList?.Count > 0)
                {
                    _listWinners = new ObservableCollection<GiveawayEntry>(_moduleSettings.SavedWinnersList);
                }
            }
            else
            {
                // Failure, load defaults
                _moduleSettings = new GiveawayModuleSettings();
            }

            // Transmit WS event
            _wsEventService.SendRegisteredEvent("GIVEAWAY_DONE",
                new GiveawayWebsocketEventBase(_moduleId));

            // Register to service events and system broadcast messages
            _chatService.ChatMessageReceived += _chatService_ChatMessageReceived;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Open a new giveaway if one is not already started or is in the process of drawing a winner.
        /// </summary>
        /// <param name="reopen">Does not clear entries list if set to true.</param>
        private void Open(bool reopen = false)
        {
            // Ignore if giveaway is already active else set active
            if (_isActive || _isClosing || _isDrawingWinner)
            {
                return;
            }

            // Set control flags
            IsActive = true;

            // Clear 'previous' winner messages if any
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                _listMessagesWinner.Clear();
            });

            // Behavior and message on open or reopen action
            if (reopen)
            {
                // Send a Chat Message with reopened info
                string message = $"A giveaway for '{Prize}' has reopened";
                message += OpenTimeMinutes > 0 ? $" for another {OpenTimeMinutes} minutes! " : "! ";
                message += IsFollowRequired ? "Following the channel is required to be eligible to win. " : "";
                message += $"Type in '{Keyword}' to enter this giveaway, if you already have entered before you don't have to type again.";
                _chatService.SendMessage(message, false);
            }
            else
            {
                // Set control flag
                IsHavingEntries = false;

                // Set 'winner' to display
                SelectedWinner = new GiveawayEntry { DisplayName = "WINNER" };
                ElapsedNoResponseTime = new TimeSpan(0);

                // Clear entries list on UI thread
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    _listEntries.Clear();
                });

                // Send new giveaway opened message
                string message = $"A giveaway for '{Prize}' has opened";
                message += OpenTimeMinutes > 0 ? $" for {OpenTimeMinutes} minutes! " : "! ";
                message += IsFollowRequired ? "Following the channel is required to be eligible to win. " : "";
                message += $"Type in '{Keyword}' to enter this giveaway.";
                _chatService.SendMessage(message, false);
            }

            // Set opened timestamp and reset elapsed open timespan
            _timestampOpened = DateTime.UtcNow;
            ElapsedOpenTime = new TimeSpan(0);

            // Start open timer if required
            if (OpenTimeMinutes > 0)
            {
                _timerOpen.Interval = (OpenTimeMinutes * 60000);
                _timerOpen.Start();
            }

            // Transmit WS event
            _wsEventService.SendRegisteredEvent("GIVEAWAY_OPENED",
                new GiveawayWebsocketEventOpen(_moduleId, _moduleSettings, _timestampOpened));

            // Save current settings values to file
            SaveSettings();
        }

        /// <summary>
        /// Open a new giveaway with new keyword, prize and optional open- and response-time.
        /// </summary>
        /// <param name="keyword">New keyword to be used to enter.</param>
        /// <param name="prize">New prize to be given away.</param>
        /// <param name="openTime">Optional: Giveaway open time in minutes.</param>
        /// <param name="responseTime">Optional: Winner response time in seconds.</param>
        private void Open(string keyword, string prize, int? openTime = null, int? responseTime = null)
        {
            // Ignore if giveaway is already active else set active
            if (_isActive || _isClosing || _isDrawingWinner)
            {
                return;
            }

            // Set new keyword and prize
            Keyword = keyword;
            Prize = prize;

            // Set Open- (and Response-) time if set.
            OpenTimeMinutes = openTime ??
                _moduleSettings.OpenTimeMinutes;
            ResponseTimeSeconds = responseTime ??
                _moduleSettings.ResponseTimeSeconds;

            // Open giveaway normally
            this.Open(false);
        }
     
        /// <summary>
        /// Closes an active giveaway and prepares a draw list to draw from.
        /// Ignores if no giveaway is active or is in the process of closing.
        /// </summary>
        private void Close()
        {
            // Ignore if giveaway is not active
            if (!_isActive || _isClosing)
            {
                return;
            }

            // Set control flags
            IsClosing = true;
            IsActive = false;
            
            // Set timestamp of closure and stop open timer
            _timerOpen.Stop();
            _timestampClosed = DateTime.UtcNow;
            
            // Send a Chat Message with closure info
            _chatService.SendMessage($"The giveaway for '{Prize}' has now been closed. A total of {_listEntries.Count} eligible viewer(s) have entered.", false);

            // Transmit WS event
            _wsEventService.SendRegisteredEvent("GIVEAWAY_CLOSED",
                new GiveawayWebsocketEventClose(_moduleId, _listEntries.Count, _timestampClosed));

            // Clone entries list to a draw list and shuffle one time
            _drawList = new List<GiveawayEntry>(_listEntries);
            _drawList.Shuffle(_rndGen);

            // If subscriber luck is set, get all entires that are subscriber
            // and randomly insert the 'amount of entires' extra in the draw-list
            if (_moduleSettings.SubscriberLuck > 1)
            {
                foreach (GiveawayEntry SubEntry in _listEntries.Where(x => x.IsSubscriber))
                {
                    for (int i = 1; i < _moduleSettings.SubscriberLuck; i++)
                    {
                        _drawList.Insert(_rndGen.Next(0, _drawList.Count), SubEntry);
                    }
                }
            }

            // Shuffle draw list twice more
            _drawList.Shuffle(_rndGen, 2);

            // Done Closing up, unlock
            IsClosing = false;
        }

        /// <summary>
        /// Draws a winner from the created drawlist. Will close the giveaway is still active.
        /// </summary>
        /// <param name="redraw">Indication if the previous winner should be redrawn.</param>
        private async void Draw(bool redraw = false)
        {
            // Ignore if giveaway is closing up or no entries left
            // also ignore it if system is already is drawing a winner
            if (_isClosing || _isDrawingWinner || !_isHavingEntries)
            {
                return;
            }

            // Set control flags
            IsDrawingWinner = true;

            // Stop response timer while determining a new winner
            _timerResponse.Stop();
            ElapsedNoResponseTime = new TimeSpan(0);

            // Close first if still active
            Close();

            // Remove the 'previous' winner from the winners list if this is
            // a redraw and the winner has replied by message or default win
            if (redraw && _WinnerHasReplied)
            {
                // Remove previous replied winner from winners list
                GiveawayEntry previous = SelectedWinner;
                await DispatcherHelper.RunAsync(() =>
                {
                    _listWinners.Remove(previous);
                });

                // Save changed winners list after winners list has updated
                SaveSettings();
            }

            // Clear 'previous' winners response messages
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                _listMessagesWinner.Clear();
            });

            // If there are no more entries to draw from...
            if (_listEntries.Count == 0)
            {                
                // Set control flags
                IsDrawingWinner = false;
                IsHavingEntries = false;

                // For UI
                SelectedWinner = new GiveawayEntry { DisplayName = "NOBODY" };
                WinnerHasReplied = true;

                // Send chat message with info
                _chatService.SendMessage("There are no entries left to draw from so nobody has won.", false);

                // Transmit WS event
                _wsEventService.SendRegisteredEvent("GIVEAWAY_DONE",
                    new GiveawayWebsocketEventBase(_moduleId));

                // Stop Draw function
                return;
            }

            // Draw random winner from the shuffled drawing list
            SelectedWinner = _drawList[_rndGen.Next(0, (_drawList.Count * 4) % _drawList.Count)];

            // Remove picked winner from the draw list and entries list
            _drawList.RemoveAll(x => x == SelectedWinner);
            DispatcherHelper.CheckBeginInvokeOnUI(() => 
            {
                _listEntries.Remove(SelectedWinner);
            });

            // If follow required validate follow with Twitch API
            // Skips follow check if selected winner is a subscriber
            if (_moduleSettings.FollowingRequired && !SelectedWinner.IsSubscriber)
            {
                try
                {
                    // Check if user is following the channel via Twitch API
                    var following = await TwitchAPI.CheckUserFollowsByChannel(SelectedWinner.UserId, SelectedWinner.ChannelId);
                    if (following == null)
                    {
                        // Silently redraw
                        IsDrawingWinner = false;
                        Draw(true);
                        return;
                    }
                }
                catch
                {
                    // Api Error, allow win due to not be able to verify follow
                }
            }

            // Set timestamp of the draw
            _timestampDraw = DateTime.UtcNow;

            // Require response timer and/or exclude subs from having to respond
            if ((_moduleSettings.ExcludeSubscriberToRespond && SelectedWinner.IsSubscriber) || _moduleSettings.ResponseTimeSeconds < 10)
            {
                // Send chat message of the new winner, no reply required
                string message = $"{SelectedWinner} you have won the giveaway for '{Prize}'!";
                _chatService.SendMessage(message, false);

                // Control, winner does not have to reply
                WinnerHasReplied = true;

                // Add winner to winners list
                await DispatcherHelper.RunAsync(() =>
                {
                    _listWinners.Add(SelectedWinner);
                });

                // Transmit WS event
                _wsEventService.SendRegisteredEvent("GIVEAWAY_DONE",
                    new GiveawayWebsocketEventBase(_moduleId));

                // Save current winners list after winners list is done updating
                SaveSettings();
            }
            else
            {
                // Send chat message of the new winner, timed response
                string message = $"{SelectedWinner} you have won the giveaway for '{Prize}'! ";
                message += $"Please reply within {ResponseTimeSeconds} seconds in the chat to claim your prize. ";
                _chatService.SendMessage(message, false);

                // Start response timers and set control flag
                _timerResponse.Interval = (ResponseTimeSeconds * 1000);
                _timerResponse.Start();
                WinnerHasReplied = false;

                // Transmit WS event
                _wsEventService.SendRegisteredEvent("GIVEAWAY_DRAW",
                    new GiveawayWebsocketEventDraw(_moduleId, _moduleSettings, _timestampDraw, _selectedWinner));
            }

            // Done drawing a winner, unlock
            IsDrawingWinner = false;
        }

        /// <summary>
        /// Validates person trying to enter and adds to entries list if eligible.
        /// </summary>
        /// <param name="message"></param>
        private async void AddEntry(TwitchChatMessage message)
        {
            // If the person is already in the entry list ignore the entry
            if (_listEntries.Any(x => x.UserId == message.UserId))
            {
                return;
            }

            // If winners cannot reenter ignore if person is already a winner 
            if (!IsWinnersCanReEnter && _listWinners.Any(x => x.UserId == message.UserId))
            {
                return;
            }

            // If the giveaway is for subscribers only ignore non subscribers
            if (_moduleSettings.SubscriberOnly && !message.IsSubscriber)
            {
                return;
            }

            // Control Flags
            IsHavingEntries = true;

            // Eligible to enter giveaway
            await DispatcherHelper.RunAsync(() =>
            {
                _listEntries.Add(new GiveawayEntry
                {
                    ChannelId = message.ChannelId,
                    UserId = message.UserId,
                    DisplayName = message.DisplayName,
                    IsSubscriber = message.IsSubscriber,
                    Tickets = message.IsSubscriber ? SubscriberLuck : 1,
                    Prize = Prize
                });
            });

            // Send entry data after done adding to entries list
            _wsEventService.SendRegisteredEvent("GIVEAWAY_ENTRY",
                new GiveawayWebsocketEventEntry(_moduleId, message.DisplayName, _listEntries.Count));
        }

        /// <summary>
        /// Clears the winners list.
        /// </summary>
        private void ClearWinners()
        {
            // Clear winners list
            _listWinners.Clear();

            // Save changed winners list
            SaveSettings();
        }

        /// <summary>
        /// Stops either the open timer or response timer, whichever is active.
        /// Open timer; stops timer and sends out a notification message to chat.
        /// Response timer; stops response timer and adds the selected winner to the winners list.
        /// </summary>
        private void StopOpenResponseTimer()
        {
            // Stop open timer if active
            if (_isActive && _timerOpen.Enabled)
            {
                _timerOpen.Stop();
            }

            // Stop reponse timer if active
            if (!_WinnerHasReplied && _timerResponse.Enabled)
            {
                // Stop response timer and mark selected winner as won
                _timerResponse.Stop();
                WinnerHasReplied = true;

                // Add selected winner to the winners list
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    _listWinners.Add(_selectedWinner);
                });
            }
        }

        /// <summary>
        /// Removes a <see cref="GiveawayEntry"/> from either the entries list or winners list.
        /// </summary>
        /// <param name="item">Item to be removed from selected list.</param>
        /// <param name="fromWinnersList">True if item is to be removed from the winners list.</param>
        private void RemoveSelectedItem(GiveawayEntry item, bool fromWinnersList)
        {
            if (item != null)
            {
                if (fromWinnersList)
                {
                    // Remove item from winners list
                    _listWinners.Remove(item);

                    // Save changed winners list
                    SaveSettings();
                }
                else
                {
                    // Remove item from entries list
                    _listEntries.Remove(item);
                    if (_listEntries.Count == 0)
                    {
                        IsHavingEntries = false;
                    }
                }
            }
        }

        /// <summary>
        /// Save <see cref="_moduleSettings"/> to file after adding winners list.
        /// </summary>
        private void SaveSettings()
        {
            // Set winners into module settings and save settings to file
            _moduleSettings.SavedWinnersList = new List<GiveawayEntry>(_listWinners);
            BinaryFile.WriteBinFile($"GivewawayModule{ID}", _moduleSettings);
        }
        
        /// <summary>
        /// Opens a File Dialog to select a audio file.
        /// </summary>
        private void DlgSelectSoundFile()
        {
            /* TODO: Implement this with more featuers and reusability across the bot;
             * - Select audio file
             * - Set volume wanted
             * - Preview the selected sound file with set volume
             */

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Sound File (*.wav, *.mp3, *.m4a)|*.wav;*.wave;*.mp3;*.m4a";
            openFileDialog.InitialDirectory =
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\OakBot";

            if (openFileDialog.ShowDialog() == true)
            {
                _moduleSettings.SelectedAudioFile = openFileDialog.FileName;
            }
            else
            {
                _moduleSettings.SelectedAudioFile = null;
            }

            openFileDialog = null;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Elapsed interval timer, update UI timespans and announce timeleft/playsound.
        /// </summary>
        private async void _TimerElapsed_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Update open timer
            if (_isActive)
            {
                // Update the timer display on the ui thread
                await DispatcherHelper.RunAsync(() =>
                {
                    ElapsedOpenTime = _elapsedOpenTime.Add(_elapsedInterval);
                });

                // Do not contune if there is no open timer running
                if (!_timerOpen.Enabled)
                {
                    return;
                }

                // Calculate the time left before closure in seconds
                int openTimeSeconds = (_moduleSettings.OpenTimeMinutes * 60);
                int timeLeft = (openTimeSeconds - (int)_elapsedOpenTime.TotalSeconds);

                // Announce half time left
                if (_moduleSettings.AnnounceTimeLeft && timeLeft == (openTimeSeconds / 2))
                {
                    _chatService.SendMessage($"{timeLeft} seconds left before the giveaway for {Prize} is closed! Type in {Keyword} to enter the giveaway!", false);
                }

                // Announce 10 seconds left
                if (_moduleSettings.AnnounceTimeLeft && timeLeft == 10)
                {
                    _chatService.SendMessage("10 seconds left before the giveaway is closed!", false);
                }

                // Play sound at 10 seconds left
                if (_moduleSettings.PlaySound && timeLeft == 10)
                {
                    if (_moduleSettings.SelectedAudioFile != null)
                    {
                        /* TODO: OakBot SFX system to enqueue sound files / volumes to a
                         * SFX service to control all audio output flows
                         */

                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            _mediaPlayer.Open(new Uri(_moduleSettings.SelectedAudioFile));
                            _mediaPlayer.Play();
                        });
                    }
                }
            }

            // Update no response timer
            if (!_WinnerHasReplied && _isHavingEntries)
            {
                // Update the timer display on the ui thread
                await DispatcherHelper.RunAsync(() =>
                {
                    ElapsedNoResponseTime = _elapsedNoResponseTime.Add(_elapsedInterval);
                });
            }
        }

        /// <summary>
        /// Set Giveaway open time elapsed; close the giveaway disallowing further entries.
        /// </summary>
        private void _TimerOpen_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_isActive && !_isClosing)
            {
                // Close giveaway
                Close();

                // Auto draw if enabled and has entries
                if (_moduleSettings.AutoDraw && _isHavingEntries)
                {
                    Draw(false);
                }
            }
        }

        /// <summary>
        /// Set winners reply time elapsed; redraw a new winner.
        /// </summary>
        private void _TimerResponse_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_isActive && !_isDrawingWinner && !_WinnerHasReplied)
            {
                // Redraw winner, 'previous' winner did not reply
                Draw(true);
            }
        }

        /// <summary>
        /// Chat MessageReceived; handle chat command and entries for 'keyword' giveaway.
        /// </summary>
        private async void _chatService_ChatMessageReceived(object sender, ChatConnectionMessageReceivedEventArgs e)
        {
            // Can only be entered with the giveaway is active, keyword can be set to be case sensitive
            if (_isActive && e.ChatMessage.Message.StartsWith(_moduleSettings.Keyword,
                IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
            {
                // Let the AddEntry handle if the person is eligiable to enter
                AddEntry(e.ChatMessage);
            }

            // Winners messages
            if (!_isActive && _selectedWinner.UserId == e.ChatMessage.UserId)
            {
                // Winners first message
                if (!_WinnerHasReplied)
                {
                    // Set winner has replied
                    WinnerHasReplied = true;

                    // Stop response timer
                    _timerResponse.Stop();

                    // Transmit WS event
                    _wsEventService.SendRegisteredEvent("GIVEAWAY_DONE",
                        new GiveawayWebsocketEventBase(_moduleId));

                    // Add winner to winners list
                    await DispatcherHelper.RunAsync(() =>
                    {
                        _listWinners.Add(SelectedWinner);
                    });

                    // Save winner to file after winners list has been updated
                    SaveSettings();
                }

                // Add any message from winner to overview
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    _listMessagesWinner.Add(e.ChatMessage);
                });
            }

            // Command structure
            if (e.ChatMessage.Message.StartsWith("!raffle", StringComparison.InvariantCultureIgnoreCase)
                // Only moderators and the broadcaster can use the !raffle command
                /* TODO: Other rights when implemented in the bot */
                && (e.ChatMessage.IsModerator || e.ChatMessage.Badges.Exists(x => x.Contains("broadcaster"))))

            {
                // split the command on whitespaces to retrieve the arguments of the command
                string[] cmdArgs = e.ChatMessage.Message.Split((char[])null, 3, StringSplitOptions.RemoveEmptyEntries);

                // First giveaway module (id = 1) can be adddessed with !raffle and no id.
                // If module has another id than 1 it needs to be specifically addressed with !raffle<id>
                string addressing = string.IsNullOrEmpty(cmdArgs[0].Substring(7)) ? "1" : cmdArgs[0].Substring(7);
                if (addressing != _moduleId.ToString())
                {
                    return;
                }

                // Module is addressed check what arguments the !raffle command has
                if (cmdArgs.Length > 2 && cmdArgs[1].Equals("open", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] openArgs = cmdArgs[2].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                    switch (openArgs.Length)
                    {
                        // !raffle open <keyword>|<description>
                        // Open a new giveaway with new keyword and description
                        case 2:
                            Open(openArgs[0], openArgs[1]);
                            break;

                        // !raffle open <keyword>|<description>|<run-time-min>
                        // Open a new giveaway with new keyword, description, and overrides runtime
                        case 3:
                            if (!Int32.TryParse(openArgs[2], out int parsedRunTime))
                            {
                                // Invalid time given, set to disabled open time
                                parsedRunTime = 0;
                            }
                            Open(openArgs[0], openArgs[1], parsedRunTime);
                            break;

                        // !raffle open <keyword>|<description>|<run-time-min>|<response-time-sec>
                        // Open a new giveaway with new keyword, description, overrides runtime, and overrides responsetime
                        case 4:
                            int parsedResponseTime;
                            if (!Int32.TryParse(openArgs[2], out int parsedRunTime2))
                            {
                                // Invalid number given, set to disabled open time
                                parsedRunTime2 = 0;
                            }   
                            if (!Int32.TryParse(openArgs[3], out parsedResponseTime))
                            {
                                // Invalid number given, set to disable response time
                                parsedResponseTime = 0;
                            }  
                            Open(openArgs[0], openArgs[1], parsedRunTime2, parsedResponseTime);
                            break;
                    }
                }
                // Single argument used
                else if (cmdArgs.Length == 2)
                {
                    if (cmdArgs[1].Equals("open", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Open();
                    }
                    else if (cmdArgs[1].Equals("reopen", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Open(true);
                    }
                    else if (cmdArgs[1].Equals("close", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Close();
                    }
                    else if (cmdArgs[1].Equals("draw", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Draw();
                    }
                    else if (cmdArgs[1].Equals("redraw", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Draw(true);
                    }
                    else if (cmdArgs[1].Equals("clearwinners", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.ClearWinners();
                    }
                    else if (cmdArgs[1].Equals("stoptimer", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.StopOpenResponseTimer();
                    }
                }
            }
        }

        #endregion

        #region Control Properties

        /// <summary>
        /// Id of the giveaway module.
        /// </summary>
        public int ID
        {
            get
            {
                return _moduleId;
            }
        }

        /// <summary>
        /// Indication if the giveaway is opened.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            private set
            {
                if (value != _isActive)
                {
                    _isActive = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Indication if the giveaway is in the process of closing up.
        /// </summary>
        public bool IsClosing
        {
            get
            {
                return _isClosing;
            }
            private set
            {
                if (value != _isClosing)
                {
                    _isClosing = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Indication if the giveaway has any entries.
        /// </summary>
        public bool IsHavingEntries
        {
            get
            {
                return _isHavingEntries;
            }
            private set
            {
                if (value != _isHavingEntries)
                {
                    _isHavingEntries = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Indication if the giveaway is in the process of drawing a winner.
        /// </summary>
        public bool IsDrawingWinner
        {
            get
            {
                return _isDrawingWinner;
            }
            private set
            {
                if (value != _isDrawingWinner)
                {
                    _isDrawingWinner = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Indication if the winner has replied to claim the prize.
        /// </summary>
        public bool WinnerHasReplied
        {
            get
            {
                return _WinnerHasReplied;
            }
            private set
            {
                if (value != _WinnerHasReplied)
                {
                    _WinnerHasReplied = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Settings Properties

        /// <summary>
        /// Keyword to be used to enter a 'keyword'-type giveaway.
        /// </summary>
        public string Keyword
        {
            get
            {
                return _moduleSettings.Keyword;
            }
            set
            {
                _moduleSettings.Keyword = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Determines if the keyword to enter a 'keyword'-type is case-sensitive.
        /// </summary>
        public bool IgnoreCase
        {
            get
            {
                return _moduleSettings.KeywordCaseInsensitive;
            }
            set
            {
                _moduleSettings.KeywordCaseInsensitive = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The prize description of the item to be won.
        /// </summary>
        public string Prize
        {
            get
            {
                return _moduleSettings.Prize;
            }
            set
            {
                _moduleSettings.Prize = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Amount of minutes the giveaway will be open for.
        /// </summary>
        public int OpenTimeMinutes
        {
            get
            {
                return _moduleSettings.OpenTimeMinutes;
            }
            set
            {
                _moduleSettings.OpenTimeMinutes = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Determines if a winner is automatically drawn on closure.
        /// </summary>
        public bool IsAutoDraw
        {
            get
            {
                return _moduleSettings.AutoDraw;
            }
            set
            {
                _moduleSettings.AutoDraw = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Determines if the 50% and 10s time left of the giveaway will be announced.
        /// </summary>
        public bool IsAnnounceTimeLeft
        {
            get
            {
                return _moduleSettings.AnnounceTimeLeft;
            }
            set
            {
                _moduleSettings.AnnounceTimeLeft = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Determines if a sound will play on 10 seconds time left of the giveaway.
        /// </summary>
        public bool IsPlaySound
        {
            get
            {
                return _moduleSettings.PlaySound;
            }
            set
            {
                _moduleSettings.PlaySound = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Amount of 'total' tickets subscribers get from entering the giveaway.
        /// </summary>
        public int SubscriberLuck
        {
            get
            {
                return _moduleSettings.SubscriberLuck;
            }
            set
            {
                _moduleSettings.SubscriberLuck = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Determines if the winner must be following the channel to be eligable.
        /// </summary>
        public bool IsFollowRequired
        {
            get
            {
                return _moduleSettings.FollowingRequired;
            }
            set
            {
                _moduleSettings.FollowingRequired = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Determines if the giveaway is only for subscribers.
        /// </summary>
        public bool IsSubscriberOnly
        {
            get
            {
                return _moduleSettings.SubscriberOnly;
            }
            set
            {
                _moduleSettings.SubscriberOnly = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Determines if previous winners can reenter the giveaway.
        /// </summary>
        public bool IsWinnersCanReEnter
        {
            get
            {
                return _moduleSettings.WinnersCanEnter;
            }
            set
            {
                _moduleSettings.WinnersCanEnter = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Amount of seconds a winner has to response before a new winner is drawn automatically.
        /// </summary>
        public int ResponseTimeSeconds
        {
            get
            {
                return _moduleSettings.ResponseTimeSeconds;
            }
            set
            {
                _moduleSettings.ResponseTimeSeconds = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Determines if subscribers do not have to reply if response time is set.
        /// </summary>
        public bool IsExcludeSubsToResponse
        {
            get
            {
                return _moduleSettings.ExcludeSubscriberToRespond;
            }
            set
            {
                _moduleSettings.ExcludeSubscriberToRespond = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Runtime and Winner Properties

        /// <summary>
        /// Elapsed time since opening of the giveaway.
        /// </summary>
        public TimeSpan ElapsedOpenTime
        {
            get
            {
                return _elapsedOpenTime;
            }
            private set
            {
                _elapsedOpenTime = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Elapsed time since drawing a winner till a reply from a winner.
        /// </summary>
        public TimeSpan ElapsedNoResponseTime
        {
            get
            {
                return _elapsedNoResponseTime;
            }
            private set
            {
                _elapsedNoResponseTime = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The selected winner that won the giveaway.
        /// </summary>
        public GiveawayEntry SelectedWinner
        {
            get
            {
                return _selectedWinner;
            }
            private set
            {
                _selectedWinner = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Collection Properties

        /// <summary>
        /// List of people that entered the giveaway.
        /// </summary>
        public ObservableCollection<GiveawayEntry> Entries
        {
            get
            {
                return _listEntries;
            }
        }

        /// <summary>
        /// List of people that have won a giveaway.
        /// </summary>
        public ObservableCollection<GiveawayEntry> Winners
        {
            get
            {
                return _listWinners;
            }
        }

        /// <summary>
        /// Messages of a selected winner to display seperately.
        /// </summary>
        public ObservableCollection<TwitchChatMessage> WinnerMessages
        {
            get
            {
                return _listMessagesWinner;
            }
        }

        #endregion

        #region Command Properties
        
        public ICommand CmdOpen
        {
            get
            {
                return _cmdOpen ??
                    (_cmdOpen = new RelayCommand(
                        () => Open(),
                        () => { return (!IsActive && !IsClosing && !IsDrawingWinner); }
                    ));
            }
        }

        public ICommand CmdReOpen
        {
            get
            {
                return _cmdReOpen ??
                    (_cmdReOpen = new RelayCommand(
                        () => Open(true),
                        () => { return (!IsActive && !IsClosing && !IsDrawingWinner); }
                    ));
            }
        }

        public ICommand CmdClose
        {
            get
            {
                return _cmdClose ??
                    (_cmdClose = new RelayCommand(
                        () => Close(),
                        () => { return (IsActive && !IsClosing); }
                    ));
            }
        }

        public ICommand CmdDraw
        {
            get
            {
                return _cmdDraw ??
                    (_cmdDraw = new RelayCommand(
                        () => Draw(),
                        () => { return (!IsClosing && !IsDrawingWinner && IsHavingEntries); }
                    ));
            }
        }

        public ICommand CmdRedraw
        {
            get
            {
                return _cmdRedraw ??
                    (_cmdRedraw = new RelayCommand(
                        () => Draw(true),
                        () => { return (!IsClosing && !IsDrawingWinner && IsHavingEntries); }
                    ));
            }
        }

        public ICommand CmdClearWinners
        {
            get
            {
                return _cmdClearWinners ??
                    (_cmdClearWinners = new RelayCommand(
                        () =>
                        {
                            ClearWinners();
                        },
                        () =>
                        {
                            return _listWinners.Count > 0;
                        }));
            }
        }

        public ICommand CmdCancelTimer
        {
            get
            {
                return _cmdCancelTimer ??
                    (_cmdCancelTimer = new RelayCommand(
                        () => StopOpenResponseTimer(),
                        () =>
                        {
                            return (_timerOpen.Enabled || _timerResponse.Enabled);
                        }));
            }
        }

        public ICommand CmdRemoveEntry
        {
            get
            {
                return _cmdRemoveEntry ??
                    (_cmdRemoveEntry = new RelayCommand<GiveawayEntry>(
                        (item) => RemoveSelectedItem(item, false)));
            }
        }

        public ICommand CmdRemoveWinner
        {
            get
            {
                return _cmdRemoveWinner ??
                    (_cmdRemoveWinner = new RelayCommand<GiveawayEntry>(
                        (item) => RemoveSelectedItem(item, true)));
            }
        }

        public ICommand CmdSelectAudioFile
        {
            get
            {
                return _cmdSelectAudioFile ??
                    (_cmdSelectAudioFile = new RelayCommand(
                        () => DlgSelectSoundFile(),
                        () =>
                        {
                            return IsPlaySound; 
                        }
                    ));
            }
        }
        
        #endregion
    }
}
