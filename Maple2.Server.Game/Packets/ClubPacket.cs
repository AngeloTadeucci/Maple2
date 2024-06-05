using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class ClubPacket {
    private enum Mode : byte {
        UpdateClub = 0,
        Establish = 1,
        Load = 2,
        DeleteStagedClub = 5,
        InviteSentReceipt = 6,
        Invite = 7,
        InviteResponse = 8,
        LeaderInviteReply = 9,
        LeaveClub = 10,
        ChangeBuffReceipt = 13,
        StagedClubInviteReply = 15,
        Disband = 16,
        ConfirmInvite = 17,
        LeaveNotice = 18,
        NotifyLogin = 19,
        NotifyLogout = 20,
        AssignNewLeader = 21,
        ChangeBuff = 22,
        UpdateMemberMap = 23,
        UpdateMember = 24,
        Rename = 26,
        UpdateMemberName = 27,
        ErrorNotice = 29,
        Join = 30,
    }

    public static ByteWriter Update(Club club) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.UpdateClub);
        pWriter.WriteClass<Club>(club);
        pWriter.WriteByte((byte) club.Members.Count);
        foreach (ClubMember member in club.Members.Values) {
            pWriter.WriteClass<ClubMember>(member);
        }

        return pWriter;
    }

    public static ByteWriter Establish(Club club) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.Establish);
        pWriter.WriteLong(club.Id);
        pWriter.WriteUnicodeString(club.Name);

        return pWriter;
    }

    public static ByteWriter Load(Club club) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.Load);
        pWriter.WriteClass<Club>(club);
        pWriter.WriteByte((byte) club.Members.Count);
        foreach (ClubMember member in club.Members.Values) {
            pWriter.WriteClass<ClubMember>(member);
        }

        return pWriter;
    }

    public static ByteWriter DeleteStagedClub(long clubId, ClubInviteReply reply) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.DeleteStagedClub);
        pWriter.WriteLong(clubId);
        pWriter.Write<ClubInviteReply>(reply);

        return pWriter;
    }

    public static ByteWriter UpdateMember(ClubMember member) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.UpdateMember);
        pWriter.WriteLong(member.ClubId);
        pWriter.WriteUnicodeString(member.Name);
        ClubMember.WriteInfo(pWriter, member);

        return pWriter;
    }

    public static ByteWriter StagedClubInviteReply(long clubId, ClubInviteReply reply, string name) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.StagedClubInviteReply);
        pWriter.WriteLong(clubId);
        pWriter.Write<ClubInviteReply>(reply);
        pWriter.WriteUnicodeString(name);

        return pWriter;
    }

    public static ByteWriter NotifyLogin(long clubId, string memberName) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.NotifyLogin);
        pWriter.WriteLong(clubId);
        pWriter.WriteUnicodeString(memberName);

        return pWriter;
    }

    public static ByteWriter NotifyLogout(long clubId, string memberName, long lastLoginTime) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.NotifyLogout);
        pWriter.WriteLong(clubId);
        pWriter.WriteUnicodeString(memberName);
        pWriter.WriteLong(lastLoginTime);

        return pWriter;
    }

    public static ByteWriter UpdateMemberMap(long clubId, string memberName, int mapId) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.UpdateMemberMap);
        pWriter.WriteLong(clubId);
        pWriter.WriteUnicodeString(memberName);
        pWriter.WriteInt(mapId);

        return pWriter;
    }

    public static ByteWriter UpdateMemberName(string oldName, string newName) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.UpdateMemberName);
        pWriter.WriteUnicodeString(oldName);
        pWriter.WriteUnicodeString(newName);

        return pWriter;
    }

    public static ByteWriter Error(ClubError error) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.ErrorNotice);
        pWriter.WriteByte(1);
        pWriter.Write<ClubError>(error);

        return pWriter;
    }

    public static ByteWriter Join(ClubMember member, string clubName) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.Join);
        pWriter.WriteLong(member.ClubId);
        pWriter.WriteUnicodeString(member.Name);
        pWriter.WriteUnicodeString(clubName);

        return pWriter;
    }
}
