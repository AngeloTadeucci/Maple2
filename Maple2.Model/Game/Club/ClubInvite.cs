using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Club;

public class ClubInvite : IByteSerializable {

    public long ClubId { get; init; }
    public required string Name;
    public required string LeaderName;
    public required string Invitee;

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(ClubId);
        writer.WriteUnicodeString(Name);
        writer.WriteUnicodeString(LeaderName);
        writer.WriteUnicodeString(Invitee);
    }
}
