using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class Guild : IByteSerializable {
    public byte Capacity = 60;

    public required long Id { get; init; }
    public required string Name;
    public required long LeaderAccountId;
    public required long LeaderCharacterId;
    public required string LeaderName;

    public string Emblem = string.Empty;
    public string Notice = string.Empty;
    public long CreationTime;
    public AchievementInfo AchievementInfo;
    public GuildFocus Focus;
    public int Experience;
    public int Funds;
    public int HouseRank;
    public int HouseTheme;

    public readonly ConcurrentDictionary<long, GuildMember> Members;
    public required IList<GuildRank> Ranks { get; init; }
    public required IList<GuildBuff> Buffs { get; init; }
    public required IList<GuildEvent> Events { get; init; }
    public required IList<GuildPoster> Posters { get; init; }
    public required IList<GuildNpc> Npcs { get; init; }
    public required IList<RewardItem> Bank { get; init; }

    [SetsRequiredMembers]
    public Guild(long id, string name, long leaderAccountId, long leaderCharacterId, string leaderName) {
        Id = id;
        Name = name;
        LeaderAccountId = leaderAccountId;
        LeaderCharacterId = leaderCharacterId;
        LeaderName = leaderName;

        Members = new ConcurrentDictionary<long, GuildMember>();
        Ranks = new List<GuildRank>();
        Buffs = new List<GuildBuff>();
        Events = new List<GuildEvent>();
        Posters = new List<GuildPoster>();
        Npcs = new List<GuildNpc>();
        Bank = new List<RewardItem>();
    }

    [SetsRequiredMembers]
    public Guild(long id, string name, GuildMember leader) : this(id, name, leader.AccountId, leader.CharacterId, leader.Name) { }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteUnicodeString(Name);
        writer.WriteUnicodeString(Emblem);
        writer.WriteByte(Capacity);
        writer.WriteUnicodeString();
        writer.WriteUnicodeString(Notice);
        writer.WriteLong(LeaderAccountId);
        writer.WriteLong(LeaderCharacterId);
        writer.WriteUnicodeString(LeaderName);
        writer.WriteLong(CreationTime);
        writer.WriteByte(1);
        writer.WriteInt(1000);
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteByte(1);
        writer.Write<GuildFocus>(Focus);
        writer.WriteInt(Experience);
        writer.WriteInt(Funds);
        writer.WriteBool(false);
        writer.WriteInt();

        writer.WriteByte((byte) Members.Count);
        foreach (GuildMember member in Members.Values) {
            writer.WriteClass<GuildMember>(member);
        }
        writer.WriteByte((byte) Ranks.Count);
        foreach (GuildRank rank in Ranks) {
            writer.WriteClass<GuildRank>(rank);
        }
        writer.WriteByte((byte) Buffs.Count);
        foreach (GuildBuff buff in Buffs) {
            writer.Write<GuildBuff>(buff);
        }
        writer.WriteByte((byte) Events.Count);
        foreach (GuildEvent @event in Events) {
            writer.Write<GuildEvent>(@event);
        }

        writer.WriteInt(HouseRank);
        writer.WriteInt(HouseTheme);

        writer.WriteInt(Posters.Count);
        foreach (GuildPoster poster in Posters) {
            writer.WriteClass<GuildPoster>(poster);
        }
        writer.WriteByte((byte) Npcs.Count);
        foreach (GuildNpc npc in Npcs) {
            writer.Write<GuildNpc>(npc);
        }

        writer.WriteBool(false); // GuildNpcShopProducts

        writer.WriteInt(Bank.Count);
        foreach (RewardItem item in Bank) {
            writer.Write<RewardItem>(item);
        }

        writer.WriteInt();
        writer.WriteUnicodeString();
        writer.WriteLong();
        writer.WriteLong();
        for (int i = 0; i < 7; i++) {
            writer.WriteInt();
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
public readonly record struct GuildBuff(int Id, int Level, long ExpiryTime);

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
public readonly record struct GuildEvent(int Id, int Value);

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
public readonly record struct GuildNpc(GuildNpcType Type, int Level);
