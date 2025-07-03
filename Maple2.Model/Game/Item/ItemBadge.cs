using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class ItemBadge : IByteSerializable, IByteDeserializable {
    private const int TRANSPARENCY_COUNT = 10;

    public readonly int Id;
    public readonly BadgeType Type;
    public readonly bool[] Transparency;
    public int PetSkinId;

    public ItemBadge(int id, bool[]? transparency = null) {
        Id = id;
        Type = (Id / 100000) switch {
            701 => (Id % 10) switch {
                0 => BadgeType.PetSkin,
                1 => BadgeType.Transparency,
                _ => BadgeType.AutoGather,
            },
            702 => BadgeType.ChatBubble,
            703 => BadgeType.NameTag,
            704 => BadgeType.Damage,
            705 => BadgeType.Tombstone,
            706 => BadgeType.SwimTube,
            707 => BadgeType.Fishing,
            708 => BadgeType.Buddy,
            709 => BadgeType.Effect,
            _ => BadgeType.None,
        };

        if (transparency is not { Length: TRANSPARENCY_COUNT }) {
            Transparency = new bool[TRANSPARENCY_COUNT];
        } else {
            Transparency = transparency;
        }
    }

    public ItemBadge Clone() {
        bool[] transparency = new bool[TRANSPARENCY_COUNT];
        for (int i = 0; i < TRANSPARENCY_COUNT; i++) {
            transparency[i] = Transparency[i];
        }
        return new ItemBadge(Id, transparency) {
            PetSkinId = PetSkinId,
        };
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteByte(1);
        writer.WriteByte((byte) Type);
        writer.WriteUnicodeString(Id.ToString());

        switch (Type) {
            case BadgeType.Transparency: // Flags for each slot
                foreach (bool toggle in Transparency) {
                    writer.WriteBool(toggle);
                }
                break;
            case BadgeType.PetSkin: // PetId for skin
                writer.WriteInt(PetSkinId);
                break;
        }
    }

    public void ReadFrom(IByteReader reader) {
        reader.ReadByte();
        reader.ReadByte(); // Type
        reader.ReadUnicodeString(); // String ItemId

        switch (Type) {
            case BadgeType.Transparency: // Flags for each slot
                for (int i = 0; i < TRANSPARENCY_COUNT; i++) {
                    Transparency[i] = reader.ReadBool();
                }
                break;
            case BadgeType.PetSkin: // PetId for skin
                PetSkinId = reader.ReadInt();
                break;
        }
    }
}
