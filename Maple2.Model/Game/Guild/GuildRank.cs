using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class GuildRank : IByteSerializable, IByteDeserializable {
    public required byte Id;
    public required string Name;
    public GuildPermission Permission = GuildPermission.Default;

    public void WriteTo(IByteWriter writer) {
        writer.WriteByte(Id);
        writer.WriteUnicodeString(Name);
        writer.Write<GuildPermission>(Permission);
    }

    public void ReadFrom(IByteReader reader) {
        Id = reader.ReadByte();
        Name = reader.ReadUnicodeString();
        Permission = reader.Read<GuildPermission>();
    }
}
