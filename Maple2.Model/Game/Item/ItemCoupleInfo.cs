using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class ItemCoupleInfo : IByteSerializable, IByteDeserializable {
    public static readonly ItemCoupleInfo Default = new ItemCoupleInfo();

    public long CharacterId { get; private set; }
    public string Name { get; private set; }
    public bool IsCreator { get; private set; }

    public ItemCoupleInfo(long characterId = 0, string name = "", bool isCreator = false) {
        CharacterId = characterId;
        Name = name;
        IsCreator = isCreator;
    }

    public ItemCoupleInfo Clone() {
        return (ItemCoupleInfo) MemberwiseClone();
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(CharacterId);
        if (CharacterId != 0) {
            writer.WriteUnicodeString(Name);
            writer.WriteBool(IsCreator);
        }
    }

    public void ReadFrom(IByteReader reader) {
        CharacterId = reader.ReadLong();
        if (CharacterId != 0) {
            Name = reader.ReadUnicodeString();
        }
    }
}
