using System.Collections;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.GroupChat;
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
    }

    public static ByteWriter Stage(ICollection<Item> crystals) {
        var pWriter = Packet.Of(SendOp.ItemMerge);
        pWriter.Write<Command>(Command.Stage);
        pWriter.WriteInt(crystals.Count);
        foreach (Item crystal in crystals) {
            pWriter.WriteLong(crystal.Uid);
        }

        return pWriter;
    }

    public static ByteWriter Select(ItemMergeSlot metadata, int materialMultiplier = 1) {
        var pWriter = Packet.Of(SendOp.ItemMerge);
        pWriter.Write<Command>(Command.Select);
        pWriter.WriteInt(1); // Step. Hardcoding 1 as there's never more than one
        pWriter.WriteLong(metadata.MesoCost);
        pWriter.WriteInt(1); // Needs to be 1 for meso cost to show?
        pWriter.WriteByte((byte) metadata.Materials.Length);
        foreach (ItemComponent ingredient in metadata.Materials) {
            pWriter.WriteInt(ingredient.ItemId);
            pWriter.WriteInt(); // ?? not rarity
            pWriter.WriteInt(ingredient.Amount * materialMultiplier);
        }

        pWriter.WriteInt(); // Unknown loop

        // Basic Attributes
        pWriter.WriteInt(metadata.BasicOptions.Count);
        foreach ((BasicAttribute attribute, ItemMergeOption mergeOption) in metadata.BasicOptions) {
            pWriter.WriteShort((short) attribute);
            pWriter.WriteMergeStat(mergeOption);
        }

        // Special Attributes
        pWriter.WriteInt(metadata.SpecialOptions.Count);
        foreach ((SpecialAttribute attribute, ItemMergeOption mergeOption) in metadata.SpecialOptions) {
            pWriter.WriteShort((short) attribute);
            pWriter.WriteMergeStat(mergeOption);
        }

        return pWriter;
    }

    private static void WriteMergeStat(this IByteWriter pWriter, ItemMergeOption option) {
        pWriter.WriteInt(option.MinValue);
        pWriter.WriteInt(option.Values[^1]);
        pWriter.WriteFloat(option.MinRate);
        pWriter.WriteFloat(option.Rates[^1]);
    }

}
