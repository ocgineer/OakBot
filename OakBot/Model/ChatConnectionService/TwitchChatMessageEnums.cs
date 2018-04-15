namespace OakBot.Model
{
    /// <summary>
    /// Twitch IRC Commands
    /// </summary>
    public enum IrcCommand
    {
        Unknown,            // Unknown command, not in this enum
        Ping,               // Generic PING
        Pong,               // Generic PONG
        RPL_001,            // The first message sent after client registration
        RPL_002,            // 
        RPL_003,            // 
        RPL_004,            // 
        RPL_353,            // List current chatters in a channel
        RPL_366,            // End of chatters list in a channel
        RPL_372,            // End of chatters list
        RPL_375,            //
        RPL_376,            //
        RPL_421,            // Unknown command response
        Join,               // A chatter joined a channel
        Part,               // A chatter left a channel
        Mode,               // Gain/lose moderator (operator) status in a channel
        PrivMsg,            // chatter sends message to a channel
        GlobalUserState,    // On successful login
        RoomState,          // When user joins a channel or a room setting is changed
        UserNotice,         // On subscription or resubscription to a channel
        UserState,          // When user joins a channel or sends a PRIVMSG to a channel
        ClearChat,          // Temporary or permanent ban on a channel
        HostTarget,         // Host starts or stops a message
        Notice,             // General notices from the server
        Reconnect,          // Rejoin channels after a restart
        Whisper
    }

    /// <summary>
    /// Command NOTICE msg-id values.
    /// </summary>
    public enum NoticeMessageType
    {
        // CUSTOM Types
        None,
        AuthBadFormat,      // Used for 'Improperly formatted auth'
        AuthLoginFailed,    // Used for 'Login authentication failed'

        // NOTICE Types
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
        HostOff,
        HostBadHosting,
        HostsRemaining,
        HostsTargetWentOffline,
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
        UnrecognizedCommand,

        // USERNOTICE Types
        Sub,
        Resub,
        SubGift,
        Raid,
        Ritual
    }

    /// <summary>
    /// Twitch chat possible user-type
    /// </summary>
    public enum UserType
    {
        Normal,
        Moderator,
        GlobalMod,
        Admin,
        Staff
    }

    public enum SubPlan
    {
        Prime,
        Tier1,
        Tier2,
        Tier3
    }
}
