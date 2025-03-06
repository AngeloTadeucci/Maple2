using System.Collections.Concurrent;
using System.Collections.Generic;
using Maple2.Model.Enum;

namespace Maple2.Model.Game;

public interface IPlayerInfo {
    public long AccountId { get; }
    public long CharacterId { get; }

    public string Name { get; set; }
    public string Motto { get; set; }
    public string Picture { get; set; }
    public Gender Gender { get; set; }
    public Job Job { get; set; }
    public short Level { get; set; }
    public int GearScore { get; set; }
    public long PremiumTime { get; set; }
    public List<long> ClubIds { get; set; }
    // Health
    public long CurrentHp { get; set; }
    public long TotalHp { get; set; }
    // Location
    public int MapId { get; set; }
    public short Channel { get; set; }
    // Home
    public string HomeName { get; set; }
    public int PlotMapId { get; set; }
    public int PlotNumber { get; set; }
    public int ApartmentNumber { get; set; }
    public long PlotExpiryTime { get; set; }
    // Trophy
    public AchievementInfo AchievementInfo { get; set; }
    // Dungeon
    public Dictionary<int, DungeonEnterLimit> DungeonEnterLimits { get; set; }
    // Guild
    public long GuildId { get; set; }
    public string GuildName { get; set; }

    // Timestamp
    public long UpdateTime { get; set; }
    public long LastOnlineTime { get; set; }
}
