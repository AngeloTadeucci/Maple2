using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class ItemEnchant(
    int enchants = 0,
    int enchantExp = 0,
    byte enchantCharges = 1,
    bool tradeable = true,
    int charges = 0,
    Dictionary<BasicAttribute, BasicOption>? basicOptions = null)
    : IByteSerializable, IByteDeserializable {
    public static readonly ItemEnchant Default = new ItemEnchant();

    public int Enchants { get; set; } = enchants;
    public int EnchantExp { get; set; } = enchantExp;
    // Enchant based peachy charges, otherwise always require 10 charges
    public byte EnchantCharges { get; set; } = enchantCharges;
    public bool Tradeable { get; private set; } = tradeable;
    public int Charges { get; set; } = charges;

    public readonly Dictionary<BasicAttribute, BasicOption> BasicOptions = basicOptions ?? new Dictionary<BasicAttribute, BasicOption>();

    public ItemEnchant Clone() {
        return new ItemEnchant(Enchants, EnchantExp, EnchantCharges, Tradeable, Charges, new Dictionary<BasicAttribute, BasicOption>(BasicOptions));
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Enchants);
        writer.WriteInt(EnchantExp);
        writer.WriteByte(EnchantCharges);
        writer.WriteLong();
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteBool(Tradeable);
        writer.WriteInt(Charges);

        writer.WriteByte((byte) BasicOptions.Count);
        foreach ((BasicAttribute type, BasicOption option) in BasicOptions) {
            writer.WriteInt((int) type);
            writer.Write<BasicOption>(option);
        }
    }

    public void ReadFrom(IByteReader reader) {
        Enchants = reader.ReadInt();
        EnchantExp = reader.ReadInt();
        EnchantCharges = reader.ReadByte();
        reader.ReadLong();
        reader.ReadInt();
        reader.ReadInt();
        Tradeable = reader.ReadBool();
        Charges = reader.ReadInt();

        byte count = reader.ReadByte();
        for (int i = 0; i < count; i++) {
            var type = (BasicAttribute) reader.ReadInt();
            BasicOptions[type] = reader.Read<BasicOption>();
        }
    }
}
