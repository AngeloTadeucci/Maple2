using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class ItemMergePacket {
    private enum Command : byte {
        Stage = 0,
        Select = 1,
        Empower = 3,
        Error = 3, // Same as Empower
        Remove = 4,
    }

    public static ByteWriter Stage(ICollection<long> crystals) {
        var pWriter = Packet.Of(SendOp.ItemMerge);
        pWriter.Write<Command>(Command.Stage);
        pWriter.WriteInt(crystals.Count);
        foreach (long uid in crystals) {
            pWriter.WriteLong(uid);
        }

        return pWriter;
    }

    public static ByteWriter Select(ItemMergeTable.Entry metadata, int materialMultiplier = 1) {
        var pWriter = Packet.Of(SendOp.ItemMerge);
        pWriter.Write<Command>(Command.Select);
        pWriter.WriteInt(1); // Step. Hardcoding 1 as there's never more than one
        pWriter.WriteLong(metadata.MesoCost * materialMultiplier);
        pWriter.WriteInt(1); // Needs to be 1 for meso cost to show?
        pWriter.WriteByte((byte) metadata.Materials.Length);
        foreach (ItemComponent ingredient in metadata.Materials) {
            pWriter.WriteInt(ingredient.ItemId);
            pWriter.WriteInt((int) ingredient.Tag);
            pWriter.WriteInt(ingredient.Amount * materialMultiplier);
        }

        pWriter.WriteInt(); // Unknown loop

        // Basic Attributes
        pWriter.WriteInt(metadata.BasicOptions.Count);
        foreach ((BasicAttribute attribute, ItemMergeTable.Option mergeOption) in metadata.BasicOptions) {
            pWriter.WriteShort((short) attribute);
            pWriter.WriteMergeStat(mergeOption);
        }

        // Special Attributes
        pWriter.WriteInt(metadata.SpecialOptions.Count);
        foreach ((SpecialAttribute attribute, ItemMergeTable.Option mergeOption) in metadata.SpecialOptions) {
            pWriter.WriteShort((short) attribute);
            pWriter.WriteMergeStat(mergeOption);
        }

        return pWriter;
    }

    public static ByteWriter Empower(Item item, ItemMergeError error = ItemMergeError.ok) {
        var pWriter = Packet.Of(SendOp.ItemMerge);
        pWriter.Write<Command>(Command.Empower);
        pWriter.Write<ItemMergeError>(error);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteClass<Item>(item);

        return pWriter;
    }

    public static ByteWriter Error(ItemMergeError error) {
        var pWriter = Packet.Of(SendOp.ItemMerge);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<ItemMergeError>(error);

        return pWriter;
    }

    public static ByteWriter Remove(Item item) {
        var pWriter = Packet.Of(SendOp.ItemMerge);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteClass<Item>(item);

        return pWriter;
    }

    private static void WriteMergeStat(this IByteWriter pWriter, ItemMergeTable.Option option) {
        pWriter.WriteInt(option.Values[0].Min);
        pWriter.WriteInt(option.Values[^1].Max);
        pWriter.WriteFloat((float) option.Rates[0].Min / 1000);
        pWriter.WriteFloat((float) option.Rates[^1].Max / 1000);
    }
}
