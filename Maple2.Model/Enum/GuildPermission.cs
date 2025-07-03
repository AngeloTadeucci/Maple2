namespace Maple2.Model.Enum;

[Flags]
public enum GuildPermission {
    Default = 1,
    InviteMembers = 2,
    ExpelMembers = 4,
    EditNotice = 8,
    Unknown = 16, // Don't know how this is used, but leader should have it.
    EditRank = 32,
    EditEmblem = 64,
    SendMail = 128,
    StartPvp = 256,
    UseBuff = 512,
    StartMiniGame = 1024,
    SendAlert = 2048,

    All = Default | InviteMembers | ExpelMembers | EditNotice | Unknown | EditRank | EditEmblem | SendMail | StartPvp | UseBuff | StartMiniGame | SendAlert,
}
