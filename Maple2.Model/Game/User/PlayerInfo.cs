using System;
using System.Numerics;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class PlayerInfo(CharacterInfo character, string homeName, AchievementInfo achievementInfo) : CharacterInfo(character), IPlayerInfo, IByteSerializable {
    // Home/Plot
    public string HomeName { get; set; } = string.IsNullOrWhiteSpace(homeName) ? "Unknown" : homeName;
    public int PlotMapId { get; set; }
    public int PlotNumber { get; set; }
    public int ApartmentNumber { get; set; }
    public long PlotExpiryTime { get; set; }
    // Trophy
    public AchievementInfo AchievementInfo { get; set; } = achievementInfo;

    public static implicit operator PlayerInfo(Player player) {
        return new PlayerInfo(player, player.Home.Name, player.Character.AchievementInfo) {
            PlotMapId = player.Home.PlotMapId,
            PlotNumber = player.Home.PlotNumber,
            ApartmentNumber = player.Home.ApartmentNumber,
            PlotExpiryTime = player.Home.PlotExpiryTime,
            AchievementInfo = player.Character.AchievementInfo,
        };
    }

    public PlayerInfo Clone() {
        return (PlayerInfo) MemberwiseClone();
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(AccountId);
        writer.WriteLong(CharacterId);
        writer.WriteUnicodeString(Name);
        writer.Write<Gender>(Gender);
        writer.WriteByte(1);
        writer.WriteLong();
        writer.WriteInt();
        writer.WriteInt(MapId);
        writer.WriteInt(MapId);
        writer.WriteInt(PlotMapId);
        writer.WriteShort(Level);
        writer.WriteShort(Channel);
        writer.WriteInt((int) Job.Code());
        writer.Write<Job>(Job);
        writer.WriteInt((int) CurrentHp);
        writer.WriteInt((int) TotalHp);
        writer.WriteShort();
        writer.WriteLong();
        writer.WriteLong();
        writer.WriteLong();
        writer.WriteInt();
        writer.Write<Vector3>(default);
        writer.WriteInt(GearScore);
        writer.Write<SkinColor>(default);
        writer.WriteLong();
        writer.Write<AchievementInfo>(default);
        writer.WriteLong();
        writer.WriteUnicodeString();
        writer.WriteUnicodeString(Motto);
        writer.WriteUnicodeString(Picture);
        writer.WriteByte();
        writer.WriteByte();
        writer.WriteClass<Mastery>(new Mastery());
        writer.WriteUnicodeString();
        writer.WriteLong();
        writer.WriteLong();
        writer.WriteLong();
        writer.WriteInt();
        writer.WriteByte();
        writer.WriteBool(false);
        writer.WriteLong();
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        writer.WriteInt();
        writer.WriteLong();
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteShort();
        writer.WriteLong();
    }
}

public class CharacterInfo(long accountId, long characterId, string name, string motto, string picture, Gender gender, Job job, short level) {
    public long AccountId { get; } = accountId;
    public long CharacterId { get; } = characterId;

    public string Name { get; set; } = name;
    public string Motto { get; set; } = motto;
    public string Picture { get; set; } = picture;
    public Gender Gender { get; set; } = gender;
    public Job Job { get; set; } = job;
    public short Level { get; set; } = level;
    public int GearScore { get; set; }
    // Health
    public long CurrentHp { get; set; }
    public long TotalHp { get; set; }
    // Location
    public int MapId { get; set; }
    public short Channel { get; set; }

    public long UpdateTime { get; set; }
    public bool Online => Channel != 0;

    public CharacterInfo(CharacterInfo other) : this(other.AccountId, other.CharacterId, other.Name, other.Motto, other.Picture, other.Gender, other.Job, other.Level) {
        MapId = other.MapId;
        Channel = other.Channel;
    }

    public static implicit operator CharacterInfo(Player player) {
        return new CharacterInfo(
            accountId: player.Account.Id,
            characterId: player.Character.Id,
            name: player.Character.Name,
            motto: player.Character.Motto,
            picture: player.Character.Picture,
            gender: player.Character.Gender,
            job: player.Character.Job,
            level: player.Character.Level) {
            MapId = player.Character.MapId,
            Channel = player.Character.Channel,
        };
    }


}
