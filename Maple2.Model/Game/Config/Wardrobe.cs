using System.Collections.Generic;
using System.Runtime.InteropServices;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class Wardrobe : IByteSerializable {
    public int Type;
    public int KeyId;
    public string Name;
    public readonly IDictionary<EquipSlot, Equip> Equips;

    public Wardrobe(int type, string name) {
        Type = type;
        Name = name;
        Equips = new Dictionary<EquipSlot, Equip>();
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Type);
        writer.WriteInt(KeyId);
        writer.WriteUnicodeString(Name);

        writer.WriteInt(Equips.Count);
        foreach (Equip equip in Equips.Values) {
            writer.Write<Equip>(equip);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 20)]
    public readonly struct Equip {
        public readonly long ItemUid;
        public readonly int ItemId;
        private readonly int Slot;
        public readonly int Rarity;

        public EquipSlot EquipSlot => (EquipSlot) Slot;

        public Equip(long itemUid, int itemId, EquipSlot slot, int rarity) {
            ItemUid = itemUid;
            ItemId = itemId;
            Slot = (int) slot;
            Rarity = rarity;
        }

        public override string ToString() => $"WardrobeEquip({ItemUid}, {ItemId}, {Slot}, {Rarity})";
    }
}
