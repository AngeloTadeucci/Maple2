using System.ComponentModel;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class GuildInvite : IByteSerializable, IByteDeserializable {
    public enum Response : byte {
        Accept = 0,
        [Description("{0} has rejected the guild invitation.")]
        RejectInvite = 1,
        [Description("{0} cannot receive the guild invitation at the moment.")]
        RejectLogout = 2,
        [Description("{0} cannot receive the guild invitation at the moment.")]
        RejectTimeout = 3,
        [Description("Enter at least 2 letters.")]
        InvalidName = byte.MaxValue,
    }

    public long GuildId { get; set; }
    public string GuildName { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(GuildId);
        writer.WriteUnicodeString(GuildName);
        writer.WriteUnicodeString();
        writer.WriteUnicodeString(SenderName);
        writer.WriteUnicodeString(ReceiverName);
    }

    public void ReadFrom(IByteReader reader) {
        GuildId = reader.ReadLong();
        GuildName = reader.ReadUnicodeString();
        reader.ReadUnicodeString();
        SenderName = reader.ReadUnicodeString();
        ReceiverName = reader.ReadUnicodeString();
    }
}
