namespace Maple2.Model.Enum;

public enum InstanceType : byte {
    none = 0,
    solo = 1,
    channelScale = 2,
    massiveEvent = 3,
    ugcMap = 4,
    GameMaker = 5, //UGD
    GuildEvent = 6,
    GuildPvp = 7, // Not confirmed
    DungeonLobby = 8,
    GuildHouse = 9,
    GuildVsGame = 10, // Not confirmed
    RankingPvp = 11, // Not confirmed
    MapleSurvival = 12, // Not confirmed
    MapleSurvivalSquad = 13, // Not confirmed
    FieldWar = 14,
    WeddingHall = 15,
}
