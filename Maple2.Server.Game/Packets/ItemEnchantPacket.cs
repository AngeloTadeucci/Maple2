using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class ItemEnchantPacket {
    private enum Command : byte {
        StageItem = 5,
        UpdateExp = 6,
        UpdateCharges = 7,
        UpdateCatalysts = 8,
        Refund = 9,
        Success = 10,
        Failure = 11,
        Error = 12,
        Unknown15 = 15,
        Unknown16 = 16,
    }

    public static ByteWriter StageItem(EnchantType type, Item item, ICollection<IngredientInfo> catalysts, IDictionary<BasicAttribute, BasicOption> statDeltas, in EnchantRates rates, int requiredCount) {
        var pWriter = Packet.Of(SendOp.ItemEnchant);
        pWriter.Write<Command>(Command.StageItem);
        pWriter.WriteShort((short) type);
        pWriter.WriteLong(item.Uid);

        pWriter.WriteByte((byte) catalysts.Count);
        foreach (IngredientInfo catalyst in catalysts) {
            pWriter.Write<IngredientInfo>(catalyst);
        }

        pWriter.WriteShort();

        pWriter.WriteInt(statDeltas.Count);
        foreach ((BasicAttribute attribute, BasicOption delta) in statDeltas) {
            pWriter.WriteShort((short) attribute);
            pWriter.Write<BasicOption>(delta);
        }

        if (type is EnchantType.Ophelia) {
            pWriter.WriteClass<EnchantRates>(rates);
            pWriter.WriteLong();
            pWriter.WriteLong();
            pWriter.WriteBool(true);
        }

        // Copies required
        if (requiredCount > 0) {
            pWriter.WriteInt(item.Id);
            pWriter.WriteShort((short) item.Rarity);
            pWriter.WriteInt(requiredCount);
        } else {
            pWriter.WriteInt();
            pWriter.WriteShort();
            pWriter.WriteInt();
        }

        return pWriter;
    }

    public static ByteWriter UpdateExp(long itemUid, int exp) {
        var pWriter = Packet.Of(SendOp.ItemEnchant);
        pWriter.Write<Command>(Command.UpdateExp);
        pWriter.WriteLong(itemUid);
        pWriter.WriteInt(exp);

        return pWriter;
    }

    public static ByteWriter UpdateCharges(ICollection<long> fodder, int charges, int fodderWeight, in EnchantRates rates) {
        var pWriter = Packet.Of(SendOp.ItemEnchant);
        pWriter.Write<Command>(Command.UpdateCharges);
        pWriter.WriteInt(charges);
        pWriter.WriteInt(fodderWeight);
        pWriter.WriteInt(fodder.Count);
        foreach (long itemUid in fodder) {
            pWriter.WriteLong(itemUid);
        }

        pWriter.WriteClass<EnchantRates>(rates);

        return pWriter;
    }

    public static ByteWriter UpdateFodder(ICollection<long> fodder) {
        var pWriter = Packet.Of(SendOp.ItemEnchant);
        pWriter.Write<Command>(Command.UpdateCatalysts);
        pWriter.WriteInt(fodder.Count);
        pWriter.WriteInt(fodder.Count);
        foreach (long ingredient in fodder) {
            pWriter.WriteLong(ingredient);
        }

        return pWriter;
    }

    // s_itemenchant_enchant_exp_refund_ok
    // - Enchantment experience reset and material refund complete.
    public static ByteWriter Refund() {
        var pWriter = Packet.Of(SendOp.ItemEnchant);
        pWriter.Write<Command>(Command.Refund);

        return pWriter;
    }

    public static ByteWriter Success(Item item, IDictionary<BasicAttribute, BasicOption> statDeltas) {
        var pWriter = Packet.Of(SendOp.ItemEnchant);
        pWriter.Write<Command>(Command.Success);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteClass<Item>(item);

        pWriter.WriteInt(statDeltas.Count);
        foreach ((BasicAttribute attribute, BasicOption delta) in statDeltas) {
            pWriter.WriteShort((short) attribute);
            pWriter.Write<BasicOption>(delta);
        }

        return pWriter;
    }

    public static ByteWriter Failure(Item item, int addCharges) {
        var pWriter = Packet.Of(SendOp.ItemEnchant);
        pWriter.Write<Command>(Command.Failure);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteClass<Item>(item);

        pWriter.WriteInt();
        pWriter.WriteInt();
        pWriter.WriteLong();
        pWriter.WriteInt(addCharges);

        return pWriter;
    }

    public static ByteWriter Error(ItemEnchantError error) {
        var pWriter = Packet.Of(SendOp.ItemEnchant);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<ItemEnchantError>(error);

        return pWriter;
    }

    public static ByteWriter Unknown15(long itemUid) {
        var pWriter = Packet.Of(SendOp.ItemEnchant);
        pWriter.Write<Command>(Command.Unknown15);
        pWriter.WriteLong(itemUid);

        return pWriter;
    }

    public static ByteWriter Unknown16() {
        var pWriter = Packet.Of(SendOp.ItemEnchant);
        pWriter.Write<Command>(Command.Unknown16);

        return pWriter;
    }
}
