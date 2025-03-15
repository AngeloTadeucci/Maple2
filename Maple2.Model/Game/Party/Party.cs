using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game.Party;

public class Party : IByteSerializable {
    private int capacity = Constant.PartyMaxCapacity;
    public int Capacity {
        get => capacity;
        set {
            capacity = Math.Clamp(value, Constant.PartyMinCapacity, Constant.PartyMaxCapacity);
        }
    }

    public required int Id { get; init; }
    public required long LeaderAccountId;
    public required long LeaderCharacterId;
    public required string LeaderName;
    public long CreationTime;
    public bool DungeonSet;
    public int DungeonId = 0;
    public int DungeonLobbyRoomId = 0;
    public readonly ConcurrentDictionary<long, PartyMember> Members;
    public PartyVote? Vote;
    public PartySearch? Search;
    public long LastVoteTime = 0;

    [SetsRequiredMembers]
    public Party(int id, long leaderAccountId, long leaderCharacterId, string leaderName) {
        Id = id;
        LeaderAccountId = leaderAccountId;
        LeaderCharacterId = leaderCharacterId;
        LeaderName = leaderName;
        CreationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Members = new ConcurrentDictionary<long, PartyMember>();
    }

    [SetsRequiredMembers]
    public Party(int id, PartyMember leader) : this(id, leader.AccountId, leader.CharacterId, leader.Name) { }

    public void WriteTo(IByteWriter writer) {
        writer.WriteBool(true); // joining from offline?
        writer.WriteInt(Id);
        writer.WriteLong(LeaderCharacterId);

        writer.WriteByte((byte) Members.Count);
        foreach (PartyMember member in Members.Values) {
            writer.WriteBool(!member.Info.Online);
            writer.WriteClass<PartyMember>(member);
            member.WriteDungeonEligibility(writer);
        }

        writer.WriteBool(DungeonSet);
        writer.WriteInt(DungeonId);
        writer.WriteBool(false); // unk bool
    }
}
