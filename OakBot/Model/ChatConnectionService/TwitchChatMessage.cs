using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace OakBot.Model
{
    public enum IrcCommand
    {
        Unknown,
        PrivMsg,
        Notice,
        Ping,
        Pong,
        Join,
        Part,
        HostTarget,
        ClearChat,
        UserState,
        GlobalUserState,
        Nick,
        Pass,
        Cap,
        RPL_001,
        RPL_002,
        RPL_003,
        RPL_004,
        RPL_353,
        RPL_366,
        RPL_372,
        RPL_375,
        RPL_376,
        Whisper,
        RoomState,
        Reconnect,
        ServerChange,
        UserNotice,
        Mode
    }

    public enum UserType
    {
        None,
        Moderator,
        GlobalMod,
        Admin,
        Staff,
    }

    public enum NoticeMsgId
    {
        None,
        Resub,              // Only used in USERNOTICE resubs
        SubsOn,
        SubsAlreadyOn,
        SubsOff,
        SubsAlreadyOff,
        SlowOn,
        SlowOff,
        R9kOn,
        R9kAlreadyOn,
        R9kOff,
        R9kAlreadyOff,
        HostOn,
        HostAlready,
        HostOff,
        HostsRemaining,
        EmoteOnlyOn,
        EmoteOnlyAlreadyOn,
        EmoteOnlyOff,
        EmoteOnlyAlreadyOff,
        ChannelSuspended,
        TimeoutSuccess,
        UntimeoutSuccess,
        BanSuccess,
        BanAlreadyBanned,
        UnbanSuccess,
        UnbanNotBanned,
        UnknownCommand,
        AuthFailed          // Used in a bad authentication
    }

    public class TwitchChatMessage
    {
        #region Private Fields

        // Trackable and Diagnostics
        private string _rawMessage;
        private string _receivedOnAccount;
        private DateTime _timestamp;
        private Dictionary<string, string> Tags;

        // Basic IRC Message
        private string _author;
        private string _host;
        private IrcCommand _command;
        private string[] _arguments;
        private string _message;

        // IRCv3 TAGS
        private string _channelId;
        private string _messageId;
        private List<string> _badges;
        private string _userId;
        private string _color;
        private string _displayName;
        private string _emotes;
        private bool _moderator;
        private bool _subscriber;
        private bool _turbo;
        private UserType _userType;

        // Bits additonal TAGS
        private int _bits;

        // NOTICE additional TAGS
        private NoticeMsgId _noticeMsgId;   // msg-id : Type of the notice

        // USERNOTICE additional TAGS
        private int _noticeMonths;          // msg-param-months : number of consecutive months subscribed
        private string _noticeMessage;      // system-msg : the message printed in chat along with this notice
        private string _noticeLogin;        // login : username of the resubscriber

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor to be used to parse received IRC message.
        /// </summary>
        /// <param name="rawMessage">Received IRC message.</param>
        /// <param name="receivedOn">Account that received the message.</param>
        public TwitchChatMessage(string rawMessage, string receivedOnAccount)
        {
            _rawMessage = rawMessage;
            _receivedOnAccount = receivedOnAccount;
            _timestamp = DateTime.Now;

            _badges = new List<string>();
            Tags = new Dictionary<string, string>();

            // First get all IRCv3 tags if starts with @
            if (_rawMessage.StartsWith("@"))
            {
                // Itterate over tag matches contained in MatchCollection
                foreach (Match tag in Regex.Matches(_rawMessage, @"(?<key>[\w-]+)=(?<value>[\w:#,-\/]*);?"))
                {
                    // Add current raw tag to the Tags collection
                    Tags[tag.Groups["key"].Value] = tag.Groups["value"].Value;

                    // Set specific tag property
                    switch (tag.Groups["key"].Value)
                    {
                        case "badges":
                            foreach (string badge in tag.Groups["value"].Value.Split(','))
                            {
                                _badges.Add(badge);
                            }
                            break;

                        case "id":
                            _messageId = tag.Groups["value"].Value;
                            break;

                        case "room-id":
                            _channelId = tag.Groups["value"].Value;
                            break;

                        case "color":
                            _color = tag.Groups["value"].Value;
                            break;

                        case "display-name":
                            _displayName = tag.Groups["value"].Value;
                            break;

                        case "user-id":
                            _userId = tag.Groups["value"].Value;
                            break;

                        case "emotes":
                            _emotes = tag.Groups["value"].Value;
                            break;

                        case "mod":
                            _moderator = tag.Groups["value"].Value == "1";
                            break;

                        case "subscriber":
                            _subscriber = tag.Groups["value"].Value == "1";
                            break;

                        case "turbo":
                            _turbo = tag.Groups["value"].Value == "1";
                            break;

                        case "user-type":
                            switch (tag.Groups["value"].Value)
                            {
                                case "mod":         _userType = UserType.Moderator; break;
                                case "global_mod":  _userType = UserType.GlobalMod; break;
                                case "admin":       _userType = UserType.Admin;     break;
                                case "staff":       _userType = UserType.Staff;     break;
                                default:            _userType = UserType.None;      break;
                            }
                            break;

                        // Bits additional TAGS
                        case "bits":
                            _bits = Int32.Parse(tag.Groups["value"].Value);
                            break;

                        // NOTICE additional TAGS
                        case "msg-id":
                            #region Switch NoticeMsgId
                            switch (tag.Groups["value"].Value)
                            {
                                case "resub": _noticeMsgId = NoticeMsgId.Resub; break;
                                case "subs_on": _noticeMsgId = NoticeMsgId.SubsOn; break;
                                case "already_subs_on": _noticeMsgId = NoticeMsgId.SubsAlreadyOn; break;
                                case "subs_off": _noticeMsgId = NoticeMsgId.SubsOff; break;
                                case "already_subs_off": _noticeMsgId = NoticeMsgId.SubsAlreadyOff; break;
                                case "slow_on": _noticeMsgId = NoticeMsgId.SlowOn; break;
                                case "slow_off": _noticeMsgId = NoticeMsgId.SlowOff; break;
                                case "r9k_on": _noticeMsgId = NoticeMsgId.R9kOn; break;
                                case "already_r9k_on": _noticeMsgId = NoticeMsgId.R9kAlreadyOn; break;
                                case "r9k_off": _noticeMsgId = NoticeMsgId.R9kOff; break;
                                case "already_r9k_off": _noticeMsgId = NoticeMsgId.R9kAlreadyOff; break;
                                case "host_on": _noticeMsgId = NoticeMsgId.HostOn; break;
                                case "bad_host_hosting": _noticeMsgId = NoticeMsgId.HostAlready; break;
                                case "host_off": _noticeMsgId = NoticeMsgId.HostOff; break;
                                case "hosts_remaining": _noticeMsgId = NoticeMsgId.HostsRemaining; break;
                                case "emote_only_on": _noticeMsgId = NoticeMsgId.EmoteOnlyOn; break;
                                case "already_emote_only_on": _noticeMsgId = NoticeMsgId.EmoteOnlyAlreadyOn; break;
                                case "emote_only_off": _noticeMsgId = NoticeMsgId.EmoteOnlyOff; break;
                                case "already_emote_only_off": _noticeMsgId = NoticeMsgId.EmoteOnlyAlreadyOff; break;
                                case "msg_channel_suspended": _noticeMsgId = NoticeMsgId.ChannelSuspended; break;
                                case "timeout_success": _noticeMsgId = NoticeMsgId.TimeoutSuccess; break;
                                case "untimeout_success": _noticeMsgId = NoticeMsgId.UntimeoutSuccess; break;
                                case "ban_success": _noticeMsgId = NoticeMsgId.BanSuccess; break;
                                case "unban_success": _noticeMsgId = NoticeMsgId.UnbanSuccess; break;
                                case "bad_unban_no_ban": _noticeMsgId = NoticeMsgId.UnbanNotBanned; break;
                                case "already_banned": _noticeMsgId = NoticeMsgId.BanAlreadyBanned; break;
                                case "unrecognized_cmd": _noticeMsgId = NoticeMsgId.UnknownCommand; break;
                            }
                            #endregion
                            break;

                        // USERNOTICCE additional TAGS
                        case "msg-param-months":
                            _noticeMonths = Int32.Parse(tag.Groups["value"].Value);
                            break;

                        case "system-msg":
                            _noticeMessage = tag.Groups["value"].Value.Replace(@"\s", " ");
                            break;

                        case "login":
                            _noticeLogin = tag.Groups["value"].Value;
                            break;
                    }
                }
            }

            // Parse the Basic IRC Messages
            Match ircMessage = Regex.Match(_rawMessage,
                @"(?<!\S)(?::(?:(?<author>\w+)!)?(?<host>\S+) )?(?<command>\w+)(?: (?<args>.+?))?(?: :(?<message>.+))?$");

            _author = ircMessage.Groups["author"].Value;
            _host = ircMessage.Groups["host"].Value;
            _arguments = ircMessage.Groups["args"].Value?.Split((char[])null);
            _message = ircMessage.Groups["message"].Value;

            switch (ircMessage.Groups["command"].Value)
            {
                case "PRIVMSG":
                    _command = IrcCommand.PrivMsg;
                    break;
                case "NOTICE":
                    _command = IrcCommand.Notice;

                    // Set a Authentication Failed notice message as the message
                    // received on these event do not contain a notice message.
                    if (_message == "Login authentication failed" || _message == "Improperly formatted auth")
                    {
                        _noticeMsgId = NoticeMsgId.AuthFailed;
                    }
                    break;
                case "PING":
                    _command = IrcCommand.Ping;
                    break;
                case "PONG":
                    _command = IrcCommand.Pong;
                    break;
                case "HOSTTARGET":
                    _command = IrcCommand.HostTarget;
                    break;
                case "CLEARCHAT":
                    _command = IrcCommand.ClearChat;
                    break;
                case "USERSTATE":
                    _command = IrcCommand.UserState;
                    break;
                case "GLOBALUSERSTATE":
                    _command = IrcCommand.GlobalUserState;
                    break;
                case "NICK":
                    _command = IrcCommand.Nick;
                    break;
                case "JOIN":
                    _command = IrcCommand.Join;
                    break;
                case "PART":
                    _command = IrcCommand.Part;
                    break;
                case "PASS":
                    _command = IrcCommand.Pass;
                    break;
                case "CAP":
                    _command = IrcCommand.Cap;
                    break;
                case "001":
                    _command = IrcCommand.RPL_001;
                    break;
                case "002":
                    _command = IrcCommand.RPL_002;
                    break;
                case "003":
                    _command = IrcCommand.RPL_003;
                    break;
                case "004":
                    _command = IrcCommand.RPL_004;
                    break;
                case "353":
                    _command = IrcCommand.RPL_353;
                    break;
                case "366":
                    _command = IrcCommand.RPL_366;
                    break;
                case "372":
                    _command = IrcCommand.RPL_372;
                    break;
                case "375":
                    _command = IrcCommand.RPL_375;
                    break;
                case "376":
                    _command = IrcCommand.RPL_376;
                    break;
                case "WHISPER":
                    _command = IrcCommand.Whisper;
                    break;
                case "SERVERCHANGE":
                    _command = IrcCommand.ServerChange;
                    break;
                case "RECONNECT":
                    _command = IrcCommand.Reconnect;
                    break;
                case "ROOMSTATE":
                    _command = IrcCommand.RoomState;
                    break;
                case "USERNOTICE":
                    _command = IrcCommand.UserNotice;
                    break;
                case "MODE":
                    _command = IrcCommand.Mode;
                    break;
                default:
                    _command = IrcCommand.Unknown;
                    break;
            }
        }

        /// <summary>
        /// Constructor to manually create a (system) chat message.
        /// </summary>
        /// <param name="author">Author of the message.</param>
        /// <param name="message">Message content.</param>
        public TwitchChatMessage(string message, string author, string displayName)
        {
            _timestamp = DateTime.Now;
            _message = message;
            _author = author;
            _displayName = displayName;
        }

        #endregion

        #region Properties

        //*************************************************************
        // Additional Properties
        //*************************************************************
        public DateTime Timestamp
        {
            get
            {
                return _timestamp;
            }
        }

        public string ShortTime
        {
            get
            {
                return _timestamp.ToShortTimeString();
            }
        }

        public string RawMessage
        {
            get
            {
                return _rawMessage;
            }
        }

        public string ReceivedOnAccount
        {
            get
            {
                return _receivedOnAccount;
            }
        }

        //*************************************************************
        // Base IRC Properties
        //*************************************************************
        public string Author
        {
            get
            {
                return _author;
            }
        }

        public IrcCommand Command
        {
            get
            {
                return _command;
            }
        }

        public string[] Args
        {
            get
            {
                return _arguments;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        //*************************************************************
        // IRCv3 Properties
        //*************************************************************
        public string ChannelId
        {
            get
            {
                return _channelId;
            }
        }

        public string UserId
        {
            get
            {
                return _userId;
            }
        }

        public UserType UserType
        {
            get
            {
                return _userType;
            }
        }

        public string DisplayName
        {
            get
            {
                // Fall back to the author name if display name is unavailable
                return string.IsNullOrEmpty(_displayName) ? this.Author : _displayName;
            }
        }

        public string NameColor
        {
            get
            {
                return string.IsNullOrEmpty(_color) ? "#2E8B57" : _color;
            }
        }

        public bool IsModerator
        {
            get
            {
                return _moderator;
            }
        }

        public bool IsSubscriber
        {
            get
            {
                return _subscriber;
            }
        }

        public List<string> Badges
        {
            get
            {
                return _badges;
            }
        }

        //*************************************************************
        // IRCv3 NOTICE and USERNOTICE Properties
        //*************************************************************
        public NoticeMsgId NoticeType
        {
            get
            {
                return _noticeMsgId;
            }
        }

        public string NoticeMessage
        {
            get
            {
                return _noticeMessage;
            }
        }

        #endregion
    }

}
