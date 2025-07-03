using System.ComponentModel;

namespace Maple2.Model.Enum;

public enum ChatType {
    [Description("s_html_chat_normal")]
    Normal = 0,
    // 1: s_html_chat_channel
    // 2: s_html_chat_channel
    [Description("s_html_chat_whisper_from")]
    WhisperFrom = 3,
    [Description("s_html_chat_whisper_to")]
    WhisperTo = 4,
    [Description("s_html_chat_notice: Unable to send whisper.")]
    WhisperFail = 5,
    [Description("s_html_chat_notice: {0} has rejected your whispers.")]
    WhisperReject = 6,
    [Description("s_html_chat_party")]
    Party = 7,
    [Description("s_html_chat_guild")]
    Guild = 8,
    [Description("s_html_chat_img_notice")]
    Notice = 9,
    Command = 10,
    [Description("s_html_chat_world")]
    World = 11,
    [Description("s_html_chat_channel")]
    Channel = 12,
    MeretNoticeAlert = 13, // 14, StringId=53 => Feature 296[MeratMarketClosing]
    [Description("s_html_chat_system_notice")]
    SystemNotice = 15,
    [Description("s_html_chat_super")]
    Super = 16,
    NoticeAlert = 17, // 26
    [Description("s_html_chat_guild_mega_phone")]
    GuildNotice = 18,
    [Description("s_html_chat_system")]
    System = 19, // Guild chat color without [Guild] prefix
    [Description("s_html_chat_club")]
    Club = 20,
    [Description("s_html_chat_ugc_event: It's party time. Click on this message to come to my home and join in the {4} event!")]
    UgcEvent = 22,
    Wedding = 25,
}

public enum AnnounceType {
    [Description("{0} has succeeded in enchanting {1}.")]
    s_itemenchant_success_notice = 1,
    [Description("{0} rolled {2} on their {1}!")]
    s_itemremake_chat_maxoption = 2,
    [Description("{0} has earned {2} from {1}.")]
    s_msg_item_open_item_announce_to_world = 3,
    [Description("{0} got {1} from the Festival House.")]
    s_card_reverse_game_reward_notice = 4,
    [Description("{0} rolled {2} on their {1}!")]
    s_item_merge_chat_maxoption = 5,
    [Description("{0} analyzed {1} and received {2}.")]
    s_msg_item_identified_item_announce_to_world = 6,
}
