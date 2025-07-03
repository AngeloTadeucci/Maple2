using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class MailPacket {
    private enum Command : byte {
        Load = 0,
        Send = 1,
        Read = 2,
        Returned = 3,
        Collect = 10,
        CollectRead = 11,
        AdBill = 12,
        Deleted = 13,
        Notify = 14,
        NotifyTemporary = 15,
        StartList = 16,
        EndList = 17,
        Error = 20,
        Gift = 22,
    }

    public static ByteWriter Load(ICollection<Mail> mails) {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.Load);

        pWriter.WriteInt(mails.Count);
        foreach (Mail mail in mails) {
            pWriter.WriteClass<Mail>(mail);
        }

        return pWriter;
    }

    public static ByteWriter Send(long mailId) {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.Send);
        pWriter.WriteLong(mailId);

        return pWriter;
    }

    public static ByteWriter Read(Mail mail) {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.Read);
        pWriter.WriteLong(mail.Id);
        pWriter.WriteLong(mail.ReadTime);

        return pWriter;
    }

    // s_mail_return
    // - Mail has been returned.
    public static ByteWriter Returned() {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.Returned);
        pWriter.WriteLong();

        return pWriter;
    }

    public static ByteWriter Collect(long mailId, bool success = true) {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.Collect);
        pWriter.WriteLong(mailId);
        pWriter.WriteBool(success);
        if (success) {
            pWriter.WriteByte(); // assert(<= 4)
            pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); // Collection time
        }

        return pWriter;
    }

    public static ByteWriter CollectRead(Mail mail) {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.CollectRead);
        pWriter.WriteLong(mail.Id);
        pWriter.WriteLong(mail.ReadTime);

        return pWriter;
    }

    public static ByteWriter AdBill(Mail mail) {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.AdBill);
        pWriter.WriteLong(mail.Id);
        pWriter.WriteLong(); // timestamp

        return pWriter;
    }

    public static ByteWriter Deleted(long mailId) {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.Deleted);
        pWriter.WriteLong(mailId);

        return pWriter;
    }

    public static ByteWriter Notify(int unreadCount, bool alert = true) {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.Notify);
        pWriter.WriteInt(unreadCount);
        pWriter.WriteBool(alert);
        pWriter.WriteInt(unreadCount);

        return pWriter;
    }

    // s_mail_period_item_include
    // - Mail has arrived with a temporary item attached
    // s_mail_period_item_include_chat
    // - Mail has arrived with a temporary item attached.\nBe sure to claim it before it expires!
    public static ByteWriter NotifyTemporary() {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.NotifyTemporary);

        return pWriter;
    }

    public static ByteWriter StartList() {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.StartList);

        return pWriter;
    }

    public static ByteWriter EndList() {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.EndList);

        return pWriter;
    }

    public static ByteWriter Error(MailError error, byte code = 0) {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.Error);
        pWriter.WriteByte(code); // used in default case
        pWriter.Write<MailError>(error);

        return pWriter;
    }

    // s_mail_notify_recieve_gift
    // - You received a gift from {0}!
    // s_mail_get_gift_item
    // - {1} has sent you a gift. Check your mailbox.
    public static ByteWriter Gift(long mailId) {
        var pWriter = Packet.Of(SendOp.Mail);
        pWriter.Write<Command>(Command.Gift);
        pWriter.WriteUnicodeString();
        pWriter.WriteByte();
        pWriter.WriteInt();
        pWriter.WriteByte();
        pWriter.WriteInt();
        pWriter.WriteString();
        pWriter.WriteUnicodeString();

        return pWriter;
    }
}
