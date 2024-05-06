using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class ItemPet : IByteSerializable, IByteDeserializable {
    public string Name = string.Empty;
    public long Exp;
    public int EvolvePoints;
    public short Level = 1;
    public bool HasItems;

    public short RenameRemaining = 1;

    public ItemPet Clone() {
        return (ItemPet) MemberwiseClone();
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteUnicodeString(Name);
        writer.WriteLong(Exp);
        writer.WriteInt(EvolvePoints);
        writer.WriteInt(Level);
        writer.WriteBool(HasItems);
    }

    public void ReadFrom(IByteReader reader) {
        Name = reader.ReadUnicodeString();
        Exp = reader.ReadLong();
        EvolvePoints = reader.ReadInt();
        Level = (short) reader.ReadInt();
        HasItems = reader.ReadBool();
    }
}
