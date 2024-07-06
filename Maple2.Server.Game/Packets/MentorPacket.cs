using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class MentorPacket {
    private enum Command : byte {
        UpdateRole = 1,
        MenteeInvitations = 2,
        MyList = 3,
        AssignReturningUser = 4,
        SendMenteeInvite = 5,
        AcceptMentorInvite = 6,
        DeclineMentorInvite = 7,
        AssignMentor = 8,
        Load = 9,
        Unknown10 = 10,
        Unknown11 = 11,
        Unknown12 = 12,
        ReceiveMenteeInvite = 14,
        Unknown15 = 15,
        Unknown16 = 16,
    }

    public static ByteWriter UpdateRole(MentorRole role, int objectId) {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.UpdateRole);
        pWriter.Write<MentorRole>(role);
        pWriter.WriteInt(objectId);

        return pWriter;
    }

    public static ByteWriter MenteeInvitations() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.MenteeInvitations);

        int count = 2;
        pWriter.WriteInt(count);
        for (int i = 0; i < count; i++) {
            pWriter.WriteLong(4);
            pWriter.WriteLong(6);
            pWriter.WriteLong(1111);
            pWriter.WriteUnicodeString("Test1");
            pWriter.WriteShort(11);
            pWriter.WriteUnicodeString("Test1B");
            pWriter.WriteInt(1);
            pWriter.WriteByte(2);
            pWriter.WriteInt(1);
        }

        return pWriter;
    }

    /// <summary>
    /// If player is a mentee, the list is of mentors. If player is a mentor, the list is of mentees.
    /// </summary>
    public static ByteWriter MyList() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.MyList);

        int count = 1;
        pWriter.WriteInt(count);
        for (int i = 0; i < count; i++) {
            pWriter.WriteLong();
            pWriter.WriteLong();
            pWriter.WriteLong();
            pWriter.WriteUnicodeString("Test2");
            pWriter.WriteShort();
            pWriter.WriteUnicodeString("Test2B");
            pWriter.WriteInt();
            pWriter.WriteByte();
            pWriter.WriteInt();
        }

        return pWriter;
    }

    public static ByteWriter AssignReturningUser() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.AssignReturningUser);

        return pWriter;
    }

    public static ByteWriter MenteeInvite(long characterId, string name) {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.SendMenteeInvite);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(name);

        return pWriter;
    }

    // s_mentoring_invite_approve_mentor
    public static ByteWriter AcceptMentorInvite() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.AcceptMentorInvite);
        pWriter.WriteLong();

        return pWriter;
    }

    // s_mentoring_invite_decline_mentor
    public static ByteWriter DeclineMentorInvite() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.DeclineMentorInvite);
        pWriter.WriteLong();

        return pWriter;
    }

    public static ByteWriter AssignMentor() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.AssignMentor);

        return pWriter;
    }

    public static ByteWriter Load() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.Load);

        return pWriter;
    }

    public static ByteWriter Unknown10() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.Unknown10);
        pWriter.WriteInt(324234);
        pWriter.WriteLong(DateTime.Now.ToEpochSeconds());

        return pWriter;
    }

    public static ByteWriter Unknown11() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.Unknown11);
        pWriter.WriteInt(1);
        pWriter.WriteInt(2);
        pWriter.WriteLong(5);

        return pWriter;
    }

    public static ByteWriter Unknown12() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.Unknown12);
        pWriter.WriteLong(69);
        pWriter.WriteLong(420);

        return pWriter;
    }

    public static ByteWriter ReceiveMenteeInvite() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.ReceiveMenteeInvite);
        pWriter.WriteUnicodeString(); // Inviter name
        pWriter.WriteLong();
        pWriter.WriteLong();
        pWriter.WriteLong();
        pWriter.WriteUnicodeString();
        pWriter.WriteShort();
        pWriter.WriteUnicodeString();
        pWriter.WriteInt();
        pWriter.WriteByte();
        pWriter.WriteInt();


        return pWriter;
    }

    public static ByteWriter Unknown15(int count) {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.Unknown15);

        pWriter.WriteInt(count);
        for (int i = 0; i < count; i++) {
            pWriter.WriteLong(50000);
            pWriter.WriteByte(1);
        }

        return pWriter;
    }

    public static ByteWriter Unknown16() {
        var pWriter = Packet.Of(SendOp.Mentor);
        pWriter.Write<Command>(Command.Unknown16);
        pWriter.WriteBool(true); // true = Character is not a returning user
        pWriter.WriteInt();

        return pWriter;
    }
}
