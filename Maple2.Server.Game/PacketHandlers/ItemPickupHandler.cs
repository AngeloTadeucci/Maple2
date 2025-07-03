using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ItemPickupHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.RequestItemPickup;

    public override void Handle(GameSession session, IByteReader packet) {
        int objectId = packet.ReadInt();
        packet.ReadByte();

        if (session.Field == null) {
            return;
        }

        // Ensure item exists.
        if (!session.Field.TryGetItem(objectId, out FieldItem? fieldItem)) {
            return;
        }

        if (fieldItem.ReceiverId != 0 && fieldItem.ReceiverId != session.CharacterId) {
            return;
        }

        // Currency items are handled differently
        if (fieldItem.Value.IsCurrency()) {
            // Remove objectId from Field, make sure item still exists (multiple looters)
            if (!session.Field.PickupItem(session.Player, objectId, out Item? item)) {
                return;
            }

            session.Item.Inventory.Add(item);
            return;
        }

        lock (session.Item) {
            if (!session.Item.Inventory.CanAdd(fieldItem)) {
                return;
            }

            // Remove objectId from Field, make sure item still exists (multiple looters)
            if (!session.Field.PickupItem(session.Player, objectId, out Item? item)) {
                return;
            }

            session.ConditionUpdate(ConditionType.item_pickup, counter: item.Amount, codeLong: item.Id);

            // Items with Pick up effects are not added to player inventory.
            if (item.Metadata.AdditionalEffects.Any(buff => buff.PickUpEffect)) {
                foreach (ItemMetadataAdditionalEffect additionalEffect in item.Metadata.AdditionalEffects.Where(buff => buff.PickUpEffect)) {
                    session.Player.Buffs.AddBuff(session.Player, session.Player, additionalEffect.Id, additionalEffect.Level, session.Field.FieldTick);
                }
                return;
            }

            item.Slot = -1;
            if (session.Item.Inventory.Add(item, true) && item.Metadata.Limit.TransferType == TransferType.BindOnLoot) {
                session.Item.Bind(item);
            }
        }
    }
}
