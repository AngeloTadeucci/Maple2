using Maple2.Database.Storage;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using static Maple2.Model.Error.ItemSocketScrollError;

namespace Maple2.Server.Game.PacketHandlers;

public class ItemSocketScrollHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.ItemSocketScroll;

    private enum Command : byte {
        Unlock = 1,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Unlock:
                HandleUnlock(session, packet);
                return;
        }
    }

    private void HandleUnlock(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        long scrollUid = packet.ReadLong();

        lock (session.Item) {
            Item? item = session.Item.Inventory.Get(itemUid);
            if (item == null) {
                session.Send(ItemSocketScrollPacket.Error(s_itemsocket_scroll_error_invalid_target));
                return;
            }

            Item? scroll = session.Item.Inventory.Get(scrollUid);
            if (!int.TryParse(scroll?.Metadata.Function?.Parameters, out int scrollId)) {
                session.Send(ItemSocketScrollPacket.Error(s_itemsocket_scroll_error_invalid_scroll));
                return;
            }
            if (!TableMetadata.ItemSocketScrollTable.Entries.TryGetValue(scrollId, out ItemSocketScrollMetadata? metadata)) {
                session.Send(ItemSocketScrollPacket.Error(s_itemsocket_scroll_error_invalid_scroll));
                return;
            }

            ItemSocketScrollError error = IsCompatibleScroll(item, metadata);
            if (error != none) {
                session.Send(ItemSocketScrollPacket.Error(error));
                return;
            }

            // This should never happen
            if (item.Socket == null) {
                session.Send(ItemSocketScrollPacket.Error(s_itemsocket_scroll_error_server_fail_unlock_socket));
                return;
            }
            if (!session.Item.Inventory.Consume(scroll.Uid, 1)) {
                session.Send(ItemSocketScrollPacket.Error(s_itemsocket_scroll_error_server_fail_consume_scroll));
                return;
            }

            item.Socket.UnlockSlots = metadata.SocketCount;
            if (item.Transfer?.RemainTrades > 0) {
                // TODO: Not sure what this is actually supposed to do
                item.Transfer.RemainTrades -= metadata.TradableCountDeduction;
            }
            session.Send(ItemSocketScrollPacket.Unlock(item, true));
        }
    }

    private static ItemSocketScrollError IsCompatibleScroll(Item item, ItemSocketScrollMetadata metadata) {
        if (item.Socket == null) {
            return s_itemsocket_scroll_error_invalid_disable;
        }
        if (item.Socket.UnlockSlots >= item.Socket.MaxSlots) {
            return s_itemsocket_scroll_error_socket_unlock_all;
        }
        if (item.Socket.UnlockSlots >= metadata.SocketCount) {
            return s_itemsocket_scroll_error_already_socket_unlock;
        }
        if (item.Metadata.Limit.Level < metadata.MinLevel || item.Metadata.Limit.Level > metadata.MaxLevel) {
            return s_itemsocket_scroll_error_impossible_level;
        }
        if (!metadata.ItemTypes.Contains(item.Type.Type)) {
            return s_itemsocket_scroll_error_impossible_slot;
        }
        if (!metadata.Rarities.Contains(item.Rarity)) {
            return s_itemsocket_scroll_error_impossible_rank;
        }

        return none;
    }
}
