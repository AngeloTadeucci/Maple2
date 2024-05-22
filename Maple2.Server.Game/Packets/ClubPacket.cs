using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

public static class ClubPacket {
    private enum Mode : byte {
        UpdateClub = 0,
        Establish = 1,
        Create = 2,
        DeleteUnestablishedClub = 5, // maybe only for establishing?
        InviteSentReceipt = 6,
        Invite = 7,
        InviteResponse = 8,
        LeaderInviteResponse = 9,
        LeaveClub = 10,
        ChangeBuffReceipt = 13,
        ClubProposalInviteResponse = 15,
        Disband = 16,
        ConfirmInvite = 17,
        LeaveNotice = 18,
        LoginNotice = 19,
        LogoutNotice = 20,
        AssignNewLeader = 21,
        ChangeBuff = 22,
        UpdateMemberLocation = 23,
        UpdatePlayer = 24,
        Rename = 26,
        UpdateMemberName = 27,
        ErrorNotice = 29,
        Join = 30
    }

    public static ByteWriter Update(Club club) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.UpdateClub);

        club.WriteClubData(pWriter, false);

        pWriter.WriteByte((byte) club.Members.Count);
        foreach (ClubMember member in club.Members) {
            pWriter.WriteByte(2);
            pWriter.Write(club.Id);
            pWriter.WriteClass(member);
        }
        return pWriter;
    }

    public static ByteWriter Create(Club club) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.Create);

        club.WriteClubData(pWriter, true);

        pWriter.WriteByte((byte) club.Members.Count);
        foreach (ClubMember member in club.Members) {
            pWriter.WriteByte(2);
            pWriter.Write(club.Id);
            pWriter.WriteClass(member);
        }
        return pWriter;
    }

    public static ByteWriter ErrorNotice(ClubError errorId) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.ErrorNotice);
        pWriter.WriteByte(1);
        pWriter.Write(errorId);
        return pWriter;
    }


}
