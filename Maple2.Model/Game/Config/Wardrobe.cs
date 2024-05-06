using System.Collections.Generic;
using System.Runtime.InteropServices;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class Wardrobe(int type, string name) : IByteSerializable {
    public int Type = type;
    public int KeyId;
    public string Name = name;
    public readonly IDictionary<EquipSlot, Equip> Equips = new Dictionary<EquipSlot, Equip>();

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
    public readonly struct Equip(long itemUid, int itemId, EquipSlot slot, int rarity) {
        public readonly long ItemUid = itemUid;
        public readonly int ItemId = itemId;
        private readonly int Slot = (int) slot;
        public readonly int Rarity = rarity;

        public EquipSlot EquipSlot => (EquipSlot) Slot;

        public override string ToString() => $"WardrobeEquip({ItemUid}, {ItemId}, {Slot}, {Rarity})";
    }
}
