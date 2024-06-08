using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Club;

public class Club : IByteSerializable {

    public long Id { get; init; }
    public required string Name;
    public required long LeaderId;
    public ClubMember Leader;
    public long CreationTime;
    public ClubState State = ClubState.Staged;
    public int BuffId;
    public long NameChangeCooldown;

    public ConcurrentDictionary<long, ClubMember> Members;

    [SetsRequiredMembers]
    public Club(long id, string name, long leaderId) {
        Id = id;
        Name = name;
        LeaderId = leaderId;

        Members = new ConcurrentDictionary<long, ClubMember>();
        Leader = null!;
    }

    [SetsRequiredMembers]
    public Club(long id, string name, ClubMember leader) : this(id, name, leader.Info.CharacterId) {
        Leader = leader;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteUnicodeString(Name);
        writer.WriteLong(Leader.Info.AccountId);
        writer.WriteLong(Leader.Info.CharacterId);
        writer.WriteUnicodeString(Leader.Info.Name);
        writer.WriteLong(CreationTime);
        writer.Write<ClubState>(State);
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteLong(NameChangeCooldown);
    }
}



