using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;
using static Maple2.Model.Error.ItemSocketError;

namespace Maple2.Server.Game.Packets;

public static class ItemSocketPacket {
    public enum Command : byte {
        UnlockSocket = 1,
        StageUnlockSocket = 3,
        UpgradeGemstone = 5,
        StageUpgradeGemstone = 7,
        EquipGemstone = 9,
        UnequipGemstone = 11,
        TransferSocket = 13,
        Unknown = 16,
        Error = 18,
    }

    public static ByteWriter UnlockSocket(bool success, Item item) {
        if (item.Socket == null) {
            return Error(error: s_itemsocketsystem_error_socket_unlock_all);
        }

        var pWriter = Packet.Of(SendOp.ItemSocketSystem);
        pWriter.Write<Command>(Command.UnlockSocket);
        pWriter.WriteBool(success);
        pWriter.WriteLong(item.Uid);
        if (success) {
            pWriter.WriteByte(item.Socket.MaxSlots);
            pWriter.WriteClass<ItemSocket>(item.Socket);
            // Used for mutating item trade info?
            pWriter.WriteBool(item.Transfer != null);
            if (item.Transfer != null) {
                pWriter.WriteClass<ItemTransfer>(item.Transfer);
            }
        }

        return pWriter;
    }

    public static ByteWriter StageUnlockSocket(Item item, float rate) {
        var pWriter = Packet.Of(SendOp.ItemSocketSystem);
        pWriter.Write<Command>(Command.StageUnlockSocket);
        pWriter.WriteLong();
        pWriter.Write<sbyte>(-1);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteFloat(rate);

        return pWriter;
    }

    public static ByteWriter UpgradeGemstone(long uid, bool success, Item gem) {
        var pWriter = Packet.Of(SendOp.ItemSocketSystem);
        pWriter.Write<Command>(Command.UpgradeGemstone);
        pWriter.WriteLong(uid);
        pWriter.WriteByte();
        pWriter.WriteBool(success);
        pWriter.WriteByte(Constant.GemstoneGrade);
        pWriter.WriteLong(gem.Uid);
        pWriter.WriteBool(true);
        if (true) {
            pWriter.WriteGemstone(gem);
        }

        return pWriter;
    }

    public static ByteWriter UpgradeGemstone(long uid, bool success, long clientUid, ItemGemstone gem) {
        var pWriter = Packet.Of(SendOp.ItemSocketSystem);
        pWriter.Write<Command>(Command.UpgradeGemstone);
        pWriter.WriteLong(uid);
        pWriter.WriteByte();
        pWriter.WriteBool(success);
        pWriter.WriteByte(Constant.GemstoneGrade);
        pWriter.WriteLong(clientUid);
        pWriter.WriteBool(true);
        if (true) {
            pWriter.WriteGemstone(gem);
        }

        return pWriter;
    }

    public static ByteWriter StageUpgradeGemstone(long uid, sbyte slot, long gemUid, float rate) {
        var pWriter = Packet.Of(SendOp.ItemSocketSystem);
        pWriter.Write<Command>(Command.StageUpgradeGemstone);
        pWriter.WriteLong(uid);
        pWriter.Write<sbyte>(slot);
        pWriter.WriteLong(gemUid);
        pWriter.WriteFloat(rate);

        return pWriter;
    }

    public static ByteWriter EquipGemstone(long itemUid, byte slot, ItemGemstone? gem) {
        var pWriter = Packet.Of(SendOp.ItemSocketSystem);
        pWriter.Write<Command>(Command.EquipGemstone);
        pWriter.WriteLong(itemUid);
        pWriter.WriteByte(slot);

        pWriter.WriteBool(gem != null);
        if (gem != null) {
            pWriter.WriteGemstone(gem);
        }

        return pWriter;
    }

    public static ByteWriter UnequipGemstone(long itemUid, byte slot) {
        var pWriter = Packet.Of(SendOp.ItemSocketSystem);
        pWriter.Write<Command>(Command.UnequipGemstone);
        pWriter.WriteLong(itemUid);
        pWriter.WriteByte(slot);

        return pWriter;
    }

    public static ByteWriter TransferSocket() {
        var pWriter = Packet.Of(SendOp.ItemSocketSystem);
        pWriter.Write<Command>(Command.TransferSocket);

        return pWriter;
    }

    public static ByteWriter Unknown() {
        var pWriter = Packet.Of(SendOp.ItemSocketSystem);
        pWriter.Write<Command>(Command.Unknown);

        return pWriter;
    }

    public static ByteWriter Error(byte code = 0, ItemSocketError error = s_itemsocketsystem_error_server_default) {
        var pWriter = Packet.Of(SendOp.ItemSocketSystem);
        pWriter.Write<Command>(Command.Error);
        pWriter.WriteByte(code);
        pWriter.Write<ItemSocketError>(error);

        return pWriter;
    }

    // Gemstone from inventory
    private static void WriteGemstone(this IByteWriter writer, Item gem) {
        writer.WriteInt(gem.Id);
        writer.WriteBool(gem.Transfer?.Binding != null);
        if (gem.Transfer?.Binding != null) {
            writer.WriteClass<ItemBinding>(gem.Transfer.Binding);
        }

        writer.WriteBool(gem.IsLocked);
        if (gem.IsLocked) {
            writer.WriteByte();
            writer.WriteLong(gem.UnlockTime);
        }
    }

    // Gemstone in socket
    private static void WriteGemstone(this IByteWriter writer, ItemGemstone gem) {
        writer.WriteInt(gem.ItemId);
        writer.WriteBool(gem.Binding != null);
        if (gem.Binding != null) {
            writer.WriteClass<ItemBinding>(gem.Binding);
        }

        writer.WriteBool(gem.IsLocked);
        if (gem.IsLocked) {
            writer.WriteByte();
            writer.WriteLong(gem.UnlockTime);
        }
    }
}
