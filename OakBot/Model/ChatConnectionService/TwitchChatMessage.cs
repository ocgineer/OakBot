using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace OakBot.Model
{
    public class TwitchChatMessage
    {
        // Depending on the load on the chat, compiling regex can be justified
        // Using Lazy loading that the compile only happens first time it is used.
        private static Lazy<Regex> IrcParserRegex =
            new Lazy<Regex>(() => new Regex(@"^(?:@(?<tags>[^\ ]*)\ )?(?::(?:(?<nick>[^@!\ ]*)!)?(?<host>[^\ ]*)\ )?(?<command>[^\ ]+)(?<middle>(?:\ [^:\ ][^\ ]*){0,14})(?:\ :?(?<trailing>.*))?$", RegexOptions.Compiled));
        private static Lazy<Regex> TagsParserRegex =
            new Lazy<Regex>(() => new Regex(@"(?:(?<key>[a-zA-Z0-9-]+)=(?<value>[^; ]*))"));


        #region Constructors

        /// <summary>
        /// Creates a new Chat message instance after parsing a given raw IRC message.
        /// </summary>
        /// <param name="raw">The received raw IRC message to parse.</param>
        public TwitchChatMessage(string raw)
        {
            // Set base info
            RawMessage = raw;
            Timestamp = DateTime.Now;
            IsSystemMessage = false;

            // Parse the received raw irc message
            Match ircMessage = IrcParserRegex.Value.Match(RawMessage);

            // Set base IRC message values
            Author = ircMessage.Groups["nick"].Value;
            Host = ircMessage.Groups["host"].Value;
            Args = ircMessage.Groups["middle"].Value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            Message = ircMessage.Groups["trailing"].Value;
            switch (ircMessage.Groups["command"].Value)
            {
                case "PING":
                    Command = IrcCommand.Ping;
                    break;
                case "PONG":
                    Command = IrcCommand.Pong;
                    break;
                case "001":
                    Command = IrcCommand.RPL_001;
                    break;
                case "002":
                    Command = IrcCommand.RPL_002;
                    break;
                case "003":
                    Command = IrcCommand.RPL_003;
                    break;
                case "004":
                    Command = IrcCommand.RPL_004;
                    break;
                case "353":
                    Command = IrcCommand.RPL_353;
                    break;
                case "366":
                    Command = IrcCommand.RPL_366;
                    break;
                case "372":
                    Command = IrcCommand.RPL_372;
                    break;
                case "375":
                    Command = IrcCommand.RPL_375;
                    break;
                case "376":
                    Command = IrcCommand.RPL_376;
                    break;
                case "421":
                    Command = IrcCommand.RPL_421;
                    break;
                case "JOIN":
                    Command = IrcCommand.Join;
                    break;
                case "PART":
                    Command = IrcCommand.Part;
                    break;
                case "MODE":
                    Command = IrcCommand.Mode;
                    break;
                case "PRIVMSG":
                    Command = IrcCommand.PrivMsg;
                    break;
                case "GLOBALUSERSTATE":
                    Command = IrcCommand.GlobalUserState;
                    break;
                case "ROOMSTATE":
                    Command = IrcCommand.RoomState;
                    break;
                case "USERNOTICE":
                    Command = IrcCommand.UserNotice;
                    break;
                case "USERSTATE":
                    Command = IrcCommand.UserState;
                    break;
                case "CLEARCHAT":
                    Command = IrcCommand.ClearChat;
                    break;
                case "HOSTTARGET":
                    Command = IrcCommand.HostTarget;
                    break;
                case "NOTICE":
                    Command = IrcCommand.Notice;
                    if (NoticeType == NoticeMessageType.None)
                    {
                        // Check for error message to set error type
                        if (Message == "Login authentication failed")
                            NoticeType = NoticeMessageType.AuthLoginFailed;
                        if (Message == "Improperly formatted auth")
                            NoticeType = NoticeMessageType.AuthBadFormat;
                    }
                    break;
                case "RECONNECT":
                    Command = IrcCommand.Reconnect;
                    break;
                case "WHISPER":
                    Command = IrcCommand.Whisper;
                    break;
                default:
                    Command = IrcCommand.Unknown;
                    break;
            }

            // Set IRCv3 tags
            if (ircMessage.Groups["tags"].Success)
            {
                // Itterate over all tags key=value matches
                foreach (Match tag in TagsParserRegex.Value.Matches(ircMessage.Groups["tags"].Value))
                {                
                    // Set available IRCv3 tag properties
                    switch (tag.Groups["key"].Value)
                    {
                        // Shared tags
                        case "user-id":
                            UserId = tag.Groups["value"].Value;
                            break;
                
                        case "user-type":
                            switch (tag.Groups["value"].Value)
                            {
                                case "mod": UserType = UserType.Moderator; break;
                                case "global_mod": UserType = UserType.GlobalMod; break;
                                case "admin": UserType = UserType.Admin; break;
                                case "staff": UserType = UserType.Staff; break;
                                default: UserType = UserType.Normal; break;
                            }
                            break;
                
                        case "display-name":
                            DisplayName = string.IsNullOrEmpty(tag.Groups["value"].Value) ? Author : tag.Groups["value"].Value;
                            break;
                
                        case "color":
                            Color = string.IsNullOrEmpty(tag.Groups["value"].Value) ? "#1CC9BA" : tag.Groups["value"].Value;
                            break;
                
                        case "turbo":
                            IsTurbo = tag.Groups["value"].Value == "1";
                            break;
                
                        case "mod":
                            IsModerator = tag.Groups["value"].Value == "1";
                            break;
                
                        case "subscriber":
                            IsSubscriber = tag.Groups["value"].Value == "1";
                            break;
                
                        case "badges":
                            Badges = new List<string>(tag.Groups["value"].Value.Split(','));
                            break;
                
                        case "emotes":
                            UsedEmotes = new List<string>(tag.Groups["value"].Value.Split('/'));
                            break;
                
                        case "room-id":
                            ChannelId = tag.Groups["value"].Value;
                            break;
                
                        // PRIVMSG tags
                        case "id":
                            MessageId = tag.Groups["value"].Value;
                            break;
                
                        case "bits":
                            Bits = Int32.Parse(tag.Groups["value"].Value);
                            break;
                
                        // CLEARCHAT tags
                        case "ban-duration":
                            BanDuration = string.IsNullOrEmpty(tag.Groups["value"].Value) ? -1 : Int32.Parse(tag.Groups["value"].Value);
                            break;
                
                        case "ban-reason":
                            BanReason = tag.Groups["value"].Value;
                            break;
                
                        // (GLOBAL)USERSTATE tags
                        case "emote-sets":
                            EmoteSets = new List<string>(tag.Groups["value"].Value.Split(','));
                            break;
                
                        // ROOMSTATE tags
                        case "broadcaster-language":
                            ChatLanguage = tag.Groups["value"].Value;
                            break;
                
                        case "r9k":
                            ChatR9KMode = tag.Groups["value"].Value == "1";
                            break;
                
                        case "slow":
                            ChatSlowMode = Int32.Parse(tag.Groups["value"].Value);
                            break;
                
                        case "subs-only":
                            ChatSubsOnly = tag.Groups["value"].Value == "1";
                            break;
                
                        // (USER)NOTICE tags
                        case "msg-id":
                            switch (tag.Groups["value"]?.Value)
                            {
                                // NOTICE msg-id tag
                                case "subs_on": NoticeType = NoticeMessageType.SubsOn; break;
                                case "already_subs_on": NoticeType = NoticeMessageType.SubsAlreadyOn; break;
                                case "subs_off": NoticeType = NoticeMessageType.SubsOff; break;
                                case "already_subs_off": NoticeType = NoticeMessageType.SubsAlreadyOff; break;
                                case "slow_on": NoticeType = NoticeMessageType.SlowOn; break;
                                case "slow_off": NoticeType = NoticeMessageType.SlowOff; break;
                                case "r9k_on": NoticeType = NoticeMessageType.R9kOn; break;
                                case "already_r9k_on": NoticeType = NoticeMessageType.R9kAlreadyOn; break;
                                case "r9k_off": NoticeType = NoticeMessageType.R9kOff; break;
                                case "already_r9k_off": NoticeType = NoticeMessageType.R9kAlreadyOff; break;
                                case "host_on": NoticeType = NoticeMessageType.HostOn; break;
                                case "host_off": NoticeType = NoticeMessageType.HostOff; break;
                                case "bad_host_hosting": NoticeType = NoticeMessageType.HostBadHosting; break;
                                case "hosts_remaining": NoticeType = NoticeMessageType.HostsRemaining; break;
                                case "host_target_went_offline": NoticeType = NoticeMessageType.HostsTargetWentOffline; break;
                                case "emote_only_on": NoticeType = NoticeMessageType.EmoteOnlyOn; break;
                                case "already_emote_only_on": NoticeType = NoticeMessageType.EmoteOnlyAlreadyOn; break;
                                case "emote_only_off": NoticeType = NoticeMessageType.EmoteOnlyOff; break;
                                case "already_emote_only_off": NoticeType = NoticeMessageType.EmoteOnlyAlreadyOff; break;
                                case "msg_channel_suspended": NoticeType = NoticeMessageType.ChannelSuspended; break;
                                case "timeout_success": NoticeType = NoticeMessageType.TimeoutSuccess; break;
                                case "untimeout_success": NoticeType = NoticeMessageType.UntimeoutSuccess; break;
                                case "ban_success": NoticeType = NoticeMessageType.BanSuccess; break;
                                case "unban_success": NoticeType = NoticeMessageType.UnbanSuccess; break;
                                case "bad_unban_no_ban": NoticeType = NoticeMessageType.UnbanNotBanned; break;
                                case "already_banned": NoticeType = NoticeMessageType.BanAlreadyBanned; break;
                                case "unrecognized_cmd": NoticeType = NoticeMessageType.UnrecognizedCommand; break;
                                // USERSNOTICE msg-id tag
                                case "sub": NoticeType = NoticeMessageType.Sub; break;
                                case "resub": NoticeType = NoticeMessageType.Resub; break;
                                case "subgift": NoticeType = NoticeMessageType.SubGift; break;
                                case "ritual": NoticeType = NoticeMessageType.Ritual; break;
                                case "raid": NoticeType = NoticeMessageType.Raid; break;
                            }
                            break;
                
                        // USERNOTICE Tags
                        case "login":
                            SubscriptionLogin = tag.Groups["value"].Value;
                            break;
                
                        case "msg-param-months":
                            SubscriptionMonths = Int32.Parse(tag.Groups["value"].Value);
                            break;
                
                        case "msg-param-sub-plan":
                            switch (tag.Groups["value"].Value)
                            {
                                case "3000": SubscriptionPlan = SubPlan.Tier3; break;
                                case "2000": SubscriptionPlan = SubPlan.Tier2; break;
                                case "1000": SubscriptionPlan = SubPlan.Tier1; break;
                                case "Prime": SubscriptionPlan = SubPlan.Prime; break;
                            }
                            break;
                
                        case "msg-param-sub-plan-name":
                            SubscriptionPlanName = tag.Groups["value"].Value;
                            break;
                
                        case "system-msg":
                            SubscriptionSystemMessage = tag.Groups["value"].Value.Replace(@"\s", " ");
                            break;

                        // USERNOTICE SubGift Tags
                        case "msg-param-recipient-display-name":
                            GiftRecipientDisplayName = tag.Groups["value"].Value; break;

                        case "msg-param-recipient-user-name":
                            GiftRecipientUserName = tag.Groups["value"].Value; break;

                        case "msg-param-recipient-id":
                            GiftRecipientUserID = tag.Groups["value"].Value; break;

                        case "msg-param-sender-count":
                            GiftSenderCount = tag.Groups["value"].Value; break;

                        // USERNOTICE Raid tags
                        case "msg-param-displayName":
                            RaidDisplayName = tag.Groups["value"].Value; break;

                        case "msg-param-login":
                            RaidUserName = tag.Groups["value"].Value; break;

                        case "msg-param-viewerCount":
                            RaidCount = tag.Groups["value"].Value; break;

                        // USERNOTICE Ritual Tags
                        case "msg-param-ritual-name":
                            NewUser = tag.Groups["value"].Value; break;
                    }
                }

            }
        }

        /// <summary>
        /// Constructor to manually create a (system) chat message.
        /// </summary>
        /// <param name="message">System Message.</param>
        public TwitchChatMessage(string message, string systemname)
        {
            Timestamp = DateTime.Now;
            IsSystemMessage = true;

            Author = systemname.ToLower();
            DisplayName = systemname;
            Color = "#fcc244";

            Message = message;
        }

        #endregion

        #region General Properties

        /// <summary>
        /// Timestamp of the received message.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Short time string notation.
        /// </summary>
        public string ShortTime { get { return Timestamp.ToShortTimeString(); } }

        /// <summary>
        /// Raw message string received from IRC.
        /// </summary>
        public string RawMessage { get; private set; }

        /// <summary>
        /// If it is an internal bots system message.
        /// </summary>
        public bool IsSystemMessage { get; private set; }

        #endregion

        #region Base IRC Properties

        /// <summary>
        /// IRC author, author username of the received message.
        /// </summary>
        public string Author { get; private set; }

        /// <summary>
        /// IRC host.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// IRC command.
        /// </summary>
        public IrcCommand Command { get; private set; }

        /// <summary>
        /// IRC command arguments.
        /// </summary>
        public string[] Args { get; private set; }

        /// <summary>
        /// IRC message.
        /// </summary>
        public string Message { get; private set; }

        #endregion

        #region IRCv3 Shared Tags Properties

        /// <summary>
        /// The user’s ID.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// The user’s type.
        /// </summary>
        public UserType UserType { get; private set; }

        /// <summary>
        /// The user’s display name, falls back to <see cref="Author"/> if never set.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// User's set color, falls back to #2E8B57 if never set.
        /// </summary>
        public string Color { get; private set; }

        /// <summary>
        /// The user has Turbo or not.
        /// </summary>
        public bool IsTurbo { get; private set; }

        /// <summary>
        /// The user is moderator or not.
        /// </summary>
        public bool IsModerator { get; private set; }

        /// <summary>
        /// The user is subscriber or not.
        /// </summary>
        public bool IsSubscriber { get; private set; }

        /// <summary>
        /// List of badges the user has.
        /// </summary>
        public List<string> Badges { get; private set; }

        /// <summary>
        /// List of emotes ids used in the message.
        /// </summary>
        public List<string> UsedEmotes { get; private set; }

        /// <summary>
        /// Channel Id of the channel the message was send in.
        /// </summary>
        public string ChannelId { get; private set; }

        /// <summary>
        /// Displaynme, UserName, and UserID of Recipient for A sub gift the sender goes to Normal values
        /// </summary>
        public string GiftRecipientDisplayName { get; private set; }

        public string GiftRecipientUserName { get; private set; }

        public string GiftRecipientUserID { get; private set; }

        ///<summary>
        /// Number of times user has gifted a sub
        /// </summary>
        public string GiftSenderCount { get; private set; }

        /// <summary>
        /// Name of channel and Number of Viewers for a Raid
        /// </summary>
        public string RaidUserName { get; private set; }

        public string RaidDisplayName { get; private set; }

        public string RaidCount { get; private set; }

        /// <summary>
        /// Name of New Chatter
        /// </summary>
        public string NewUser { get; private set; }

        #endregion

        #region IRCv3 PRIVMSG Tag Properties

        /// <summary>
        /// PRIVMSG tag; The user has used bits in the channel if set.
        /// </summary>
        public int Bits { get; private set; }

        /// <summary>
        /// PRIVMSG tag; Unique Id for the received message.
        /// </summary>
        public string MessageId { get; private set; }

        #endregion

        #region IRCv3 CLEARCHAT Tag Properties

        /// <summary>
        /// CLEARCHAT tag; Duration of the timeout, in seconds. If -1 it is a permanent ban.
        /// </summary>
        public int BanDuration { get; private set; }

        /// <summary>
        /// CLEARCHAT tag; The moderator’s reason for the timeout or ban.
        /// </summary>
        public string BanReason { get; private set; }

        #endregion

        #region IRCv3 (GLOBAL)USERSTATE Tag Properties

        /// <summary>
        /// (GLOBAL)USERSTATE tag; The emote sets the user can use.
        /// </summary>
        public IList<string> EmoteSets { get; private set; }

        #endregion

        #region IRCv3 ROOMSTATE Tag Properties

        /// <summary>
        /// ROOMSTATE tag; The chat language when broadcaster language mode is enabled.
        /// </summary>
        public string ChatLanguage { get; private set; }

        /// <summary>
        /// ROOMSTATE tag; Chat is in R9K mode.
        /// </summary>
        public bool ChatR9KMode { get; private set; }

        /// <summary>
        /// ROOMSTATE tag; Chat delay in seconds between messages.
        /// </summary>
        public int ChatSlowMode { get; private set; }

        /// <summary>
        /// ROOMSTATE tag; Chat is in subscriber only mode.
        /// </summary>
        public bool ChatSubsOnly { get; private set; }

        #endregion

        #region IRCv3 (USER)NOTICE Tag Properties

        /// <summary>
        /// (USER)NOTICE tag; The type of notice.
        /// See <see cref="NoticeMessageType"/> for available types.
        /// </summary>
        public NoticeMessageType NoticeType { get; private set; }

        #endregion

        #region IRCv3 USERNOTICE Tag Properties

        /// <summary>
        /// USERNOTICE tag; The name of the user who sent the notice.
        /// </summary>
        public string SubscriptionLogin { get; private set; }
        
        /// <summary>
        /// USERNOTICE tag; The number of consecutive months the user has
        /// subscribed for, in a <see cref="NoticeMessageType.Resub"/> notice.
        /// </summary>
        public int SubscriptionMonths { get; private set; }

        /// <summary>
        /// USERNOTICE tag; The type of subscription plan being used.
        /// See <see cref="SubPlan"/> for available types.
        /// </summary>
        public SubPlan SubscriptionPlan { get; private set; }

        /// <summary>
        /// USERNOTICE tag; The display name of the subscription plan.
        /// </summary>
        public string SubscriptionPlanName { get; private set; }

        /// <summary>
        /// USERNOTICE tag; The message printed in chat along with this notice.
        /// </summary>
        public string SubscriptionSystemMessage { get; private set; }

        #endregion
    }
}
