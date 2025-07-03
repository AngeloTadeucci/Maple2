using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class UgcItemMusicScore : IByteSerializable, IByteDeserializable {
    public void WriteTo(IByteWriter writer) {
        writer.WriteLong();
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteUnicodeString();
        writer.WriteUnicodeString();
        writer.WriteString();
        writer.WriteInt();
        writer.WriteLong();
        writer.WriteLong();
        writer.WriteUnicodeString();
    }

    public void ReadFrom(IByteReader reader) {
        reader.ReadLong();
        reader.ReadInt();
        reader.ReadInt();
        reader.ReadUnicodeString();
        reader.ReadUnicodeString();
        reader.ReadString();
        reader.ReadInt();
        reader.ReadLong();
        reader.ReadLong();
        reader.ReadUnicodeString();
    }
}
