using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game.Club;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class ClubPacket {
    private enum Mode : byte {
        UpdateClub = 0,
        Establish = 1,
        Create = 2,
        DeleteStagedClub = 5,
        Invited = 6,
        Invite = 7,
        AcceptInvite = 8,
        InviteNotification = 9,
        Leave = 10,
        ChangeBuffNotification = 13,
        StagedClubInviteReply = 15,
        Disband = 16,
        NotifyAcceptInvite = 17,
        LeaveNotice = 18,
        NotifyLogin = 19,
        NotifyLogout = 20,
        UpdateLeader = 21,
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

    public static ByteWriter Create(Club club) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.Create);
        pWriter.WriteClass<Club>(club);
        pWriter.WriteByte((byte) club.Members.Count);
        foreach (ClubMember member in club.Members.Values) {
            pWriter.WriteClass<ClubMember>(member);
        }

        return pWriter;
    }

    public static ByteWriter DeleteStagedClub(long clubId, ClubResponse reply) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.DeleteStagedClub);
        pWriter.WriteLong(clubId);
        pWriter.Write<ClubResponse>(reply);

        return pWriter;
    }

    public static ByteWriter Invited(long clubId, string playerName) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.Invited);
        pWriter.WriteLong(clubId);
        pWriter.WriteUnicodeString(playerName);

        return pWriter;
    }

    public static ByteWriter Invite(ClubInvite invite) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.Invite);
        pWriter.WriteClass<ClubInvite>(invite);

        return pWriter;
    }

    public static ByteWriter AcceptInvite(ClubInvite invite) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.AcceptInvite);
        pWriter.WriteClass<ClubInvite>(invite);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter InviteNotification(long clubId, string invitee, bool accept) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.InviteNotification);
        pWriter.WriteLong(clubId);
        pWriter.WriteUnicodeString(invitee);
        pWriter.WriteBool(accept);
        pWriter.WriteByte();

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

    public static ByteWriter Rename(long clubId, string clubName, long timestamp) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.Rename);
        pWriter.WriteLong(clubId);
        pWriter.WriteUnicodeString(clubName);
        pWriter.WriteLong(timestamp);

        return pWriter;
    }

    public static ByteWriter Leave(long clubId, string playerName) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.Leave);
        pWriter.WriteLong(clubId);
        pWriter.WriteUnicodeString(playerName);

        return pWriter;
    }

    public static ByteWriter StagedClubInviteReply(long clubId, ClubResponse reply, string name) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.StagedClubInviteReply);
        pWriter.WriteLong(clubId);
        pWriter.Write<ClubResponse>(reply);
        pWriter.WriteUnicodeString(name);

        return pWriter;
    }

    public static ByteWriter Disband(long clubId, string leaderName) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.Disband);
        pWriter.WriteLong(clubId);
        pWriter.WriteUnicodeString(leaderName);
        pWriter.Write<ClubResponse>(ClubResponse.Disband);

        return pWriter;
    }

    public static ByteWriter NotifyAcceptInvite(ClubMember member, string leaderName) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.NotifyAcceptInvite);
        pWriter.WriteLong(member.ClubId);
        pWriter.WriteUnicodeString(leaderName);
        pWriter.WriteClass<ClubMember>(member);

        return pWriter;
    }

    public static ByteWriter LeaveNotice(long clubId, string playerName) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.LeaveNotice);
        pWriter.WriteLong(clubId);
        pWriter.WriteUnicodeString(playerName);

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

    public static ByteWriter UpdateLeader(long clubId, string oldLeader, string newLeader) {
        var pWriter = Packet.Of(SendOp.Club);
        pWriter.Write(Mode.UpdateLeader);
        pWriter.WriteLong(clubId);
        pWriter.WriteUnicodeString(oldLeader);
        pWriter.WriteUnicodeString(newLeader);
        pWriter.WriteBool(true); // s_club_notify_change_master

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
