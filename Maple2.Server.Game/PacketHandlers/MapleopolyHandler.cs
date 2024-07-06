using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class MapleopolyHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Mapleopoly;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required ItemMetadataStorage ItemMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    private enum Command : byte {
        Load = 0,
        Roll = 1,
        Result = 3,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Load:
                HandleLoad(session);
                return;
            case Command.Roll:
                HandleRoll(session);
                return;
            case Command.Result:
                HandleResult(session);
                return;
        }
    }

    private void HandleLoad(GameSession session) {
        GameEvent? gameEvent = session.FindEvent(GameEventType.BlueMarble);
        if (gameEvent?.Metadata.Data is not BlueMarble blueMarble) {
            return;
        }

        int freeRollValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyFreeRollAmount, gameEvent.Id, gameEvent.EndTime).Int();
        int totalSlotValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyTotalSlotCount, gameEvent.Id, gameEvent.EndTime).Int();
        int ticketAmount = session.Item.Inventory.Find(blueMarble.RequiredItem.ItemId)
            .Sum(ticket => ticket.Amount);

        session.Send(MapleopolyPacket.Load(blueMarble.Slots, blueMarble.RequiredItem.ItemId, totalSlotValue, freeRollValue, ticketAmount));
    }

    private void HandleRoll(GameSession session) {
        GameEvent? gameEvent = session.FindEvent(GameEventType.BlueMarble);
        if (gameEvent?.Metadata.Data is not BlueMarble blueMarble) {
            return;
        }

        int freeRollValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyFreeRollAmount, gameEvent.Id, gameEvent.EndTime).Int();
        int totalSlotValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyTotalSlotCount, gameEvent.Id, gameEvent.EndTime).Int();

        if (freeRollValue > 0) {
            freeRollValue--;
        } else {
            if (!session.Item.Inventory.ConsumeItemComponents(new List<ItemComponent> {
                    blueMarble.RequiredItem,
                })) {
                session.Send(MapleopolyPacket.Error(MapleopolyError.s_bluemarble_result_consume_fail));
                return;
            }
        }

        // roll two dice
        int dice1 = Random.Shared.Next(1, 7);
        int dice2 = Random.Shared.Next(1, 7);

        totalSlotValue += dice1 + dice2;
        if (dice1 == dice2) {
            freeRollValue++;
        }

        session.GameEventUserValue.Set(gameEvent.Id, GameEventUserValueType.MapleopolyFreeRollAmount, freeRollValue);
        session.GameEventUserValue.Set(gameEvent.Id, GameEventUserValueType.MapleopolyTotalSlotCount, totalSlotValue);
        session.Send(MapleopolyPacket.Roll(totalSlotValue, dice1, dice2));
    }

    private void HandleResult(GameSession session) {
        GameEvent? gameEvent = session.FindEvent(GameEventType.BlueMarble);
        if (gameEvent?.Metadata.Data is not BlueMarble blueMarble) {
            return;
        }

        int freeRollValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyFreeRollAmount, gameEvent.Id, gameEvent.EndTime).Int();
        int totalSlotValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyTotalSlotCount, gameEvent.Id, gameEvent.EndTime).Int();

        int currentSlotPosition = totalSlotValue % blueMarble.Slots.Length;

        BlueMarble.Slot slot = blueMarble.Slots[currentSlotPosition];
        switch (slot.Type) {
            case BlueMarbleSlotType.Item:
            case BlueMarbleSlotType.Paradise:
                Item? item = session.Field.ItemDrop.CreateItem(slot.Item.ItemId, slot.Item.Rarity, slot.Item.Amount);
                if (item == null) {
                    // TODO: Error packet?
                    break;
                }
                if (!session.Item.Inventory.Add(item, true)) {
                    session.Item.MailItem(item);
                }
                break;
            case BlueMarbleSlotType.Backward:
                totalSlotValue -= slot.MoveAmount;
                break;
            case BlueMarbleSlotType.Forward:
                totalSlotValue += slot.MoveAmount;
                break;
            case BlueMarbleSlotType.WorldTour:
                totalSlotValue += blueMarble.Slots.Length;
                break;
            case BlueMarbleSlotType.GoToStarting:
                totalSlotValue += blueMarble.Slots.Length - currentSlotPosition;
                break;
            case BlueMarbleSlotType.Start:
                // Do nothing
                break;
            case BlueMarbleSlotType.Lose:
            case BlueMarbleSlotType.Roll:
            case BlueMarbleSlotType.Trap:
            default:
                Logger.Warning("Unhandled Mapleopoly slot type: {SlotType}", slot.Type);
                break;
        }

        int totalNewTrips = totalSlotValue / blueMarble.Slots.Length;
        RewardNewTrips(session, gameEvent, totalNewTrips);

        session.GameEventUserValue.Set(gameEvent.Id, GameEventUserValueType.MapleopolyTotalTrips, totalSlotValue);
        session.Send(MapleopolyPacket.Result(slot, totalSlotValue, freeRollValue));
    }

    private void RewardNewTrips(GameSession session, GameEvent gameEvent, int totalNewTrips) {
        int trips = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyTotalTrips, gameEvent.Id, gameEvent.EndTime).Int();
        var blueMarble = (gameEvent.Metadata.Data as BlueMarble)!;
        while (trips < totalNewTrips) {
            trips++;

            // Check if there's any item to give for every 1 trip
            BlueMarble.Round? entry1 = blueMarble.Rounds.FirstOrDefault(entry => entry.RoundCount == 0);
            if (entry1 != default) {
                Item? trip0Item = session.Field.ItemDrop.CreateItem(entry1.Item.ItemId);
                if (trip0Item != null && !session.Item.Inventory.Add(trip0Item, true)) {
                    session.Item.MailItem(trip0Item);
                }
            }

            // Check if there's any other item to give for hitting a specific number of trips
            BlueMarble.Round? entry2 = blueMarble.Rounds.FirstOrDefault(entry => entry.RoundCount == trips);
            if (entry2 != default) {
                Item? tripItem = session.Field.ItemDrop.CreateItem(entry2.Item.ItemId);
                if (tripItem != null && !session.Item.Inventory.Add(tripItem, true)) {
                    session.Item.MailItem(tripItem);
                }
            }
        }

        session.GameEventUserValue.Set(gameEvent.Id, GameEventUserValueType.MapleopolyTotalTrips, trips);
    }
}
