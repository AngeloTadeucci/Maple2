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
        GameEvent? gameEvent = session.FindEvent<BlueMarble>();
        if (gameEvent == null) {
            return;
        }
        var blueMarble = (gameEvent.EventInfo as BlueMarble)!;
        blueMarble.Tiles = blueMarble.Tiles.OrderBy(tile => tile.Position).ToList();

        int freeRollValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyFreeRollAmount, gameEvent.Id, gameEvent.EndTime).Int();
        int totalTileValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyTotalTileCount, gameEvent.Id, gameEvent.EndTime).Int();
        int ticketAmount = session.Item.Inventory.Find(Constant.MapleopolyTicketItemId)
            .Sum(ticket => ticket.Amount);

        session.Send(MapleopolyPacket.Load(blueMarble.Tiles, totalTileValue, freeRollValue, ticketAmount));
    }

    private void HandleRoll(GameSession session) {
        GameEvent? gameEvent = session.FindEvent<BlueMarble>();
        if (gameEvent == null) {
            return;
        }

        int freeRollValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyFreeRollAmount, gameEvent.Id, gameEvent.EndTime).Int();
        int totalTileValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyTotalTileCount, gameEvent.Id, gameEvent.EndTime).Int();

        if (freeRollValue > 0) {
            freeRollValue--;
        } else {
            if (!session.Item.Inventory.ConsumeItemComponents(new List<ItemComponent> {
                    new(ItemId: Constant.MapleopolyTicketItemId,
                        Amount: Constant.MapleopolyTicketCostCount,
                        Rarity: Constant.MapleopolyTicketRarity,
                        Tag: ItemTag.None),
                })) {
                session.Send(MapleopolyPacket.Error(MapleopolyError.s_bluemarble_result_consume_fail));
                return;
            }
        }

        // roll two dice
        int dice1 = Random.Shared.Next(1, 7);
        int dice2 = Random.Shared.Next(1, 7);

        totalTileValue += dice1 + dice2;
        if (dice1 == dice2) {
            freeRollValue++;
        }

        session.GameEventUserValue.Set(gameEvent.Id, GameEventUserValueType.MapleopolyFreeRollAmount, freeRollValue);
        session.GameEventUserValue.Set(gameEvent.Id, GameEventUserValueType.MapleopolyTotalTileCount, totalTileValue);
        session.Send(MapleopolyPacket.Roll(totalTileValue, dice1, dice2));
    }

    private void HandleResult(GameSession session) {
        GameEvent? gameEvent = session.FindEvent<BlueMarble>();
        if (gameEvent == null) {
            return;
        }
        var blueMarble = (gameEvent.EventInfo as BlueMarble)!;

        int freeRollValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyFreeRollAmount, gameEvent.Id, gameEvent.EndTime).Int();
        int totalTileValue = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyTotalTileCount, gameEvent.Id, gameEvent.EndTime).Int();

        int currentTilePosition = totalTileValue % blueMarble.Tiles.Count;

        BlueMarbleTile tile = blueMarble.Tiles[currentTilePosition];
        switch (tile.Type) {
            case BlueMarbleTileType.Item:
            case BlueMarbleTileType.TreasureTrove:
                Item? item = session.Item.CreateItem(tile.Item.ItemId, tile.Item.ItemRarity, tile.Item.ItemAmount);
                if (item == null) {
                    // TODO: Error packet?
                    break;
                }
                if (!session.Item.Inventory.Add(item, true)) {
                    session.Item.MailItem(item);
                }
                break;
            case BlueMarbleTileType.Backtrack:
                totalTileValue -= tile.MoveAmount;
                break;
            case BlueMarbleTileType.MoveForward:
                totalTileValue += tile.MoveAmount;
                break;
            case BlueMarbleTileType.RoundTrip:
                totalTileValue += blueMarble.Tiles.Count;
                break;
            case BlueMarbleTileType.GoToStart:
                totalTileValue += blueMarble.Tiles.Count - currentTilePosition;
                break;
            case BlueMarbleTileType.Start:
                // Do nothing
                break;
            case BlueMarbleTileType.Lose:
            case BlueMarbleTileType.RollAgain:
            case BlueMarbleTileType.Trap:
            default:
                Logger.Warning("Unhandled Mapleopoly tile type: {TileType}", tile.Type);
                break;
        }

        int totalNewTrips = totalTileValue / blueMarble.Tiles.Count;
        RewardNewTrips(session, gameEvent, totalNewTrips);

        session.GameEventUserValue.Set(gameEvent.Id, GameEventUserValueType.MapleopolyTotalTrips, totalTileValue);
        session.Send(MapleopolyPacket.Result(tile, totalTileValue, freeRollValue));
    }

    private void RewardNewTrips(GameSession session, GameEvent gameEvent, int totalNewTrips) {
        int trips = session.GameEventUserValue.Get(GameEventUserValueType.MapleopolyTotalTrips, gameEvent.Id, gameEvent.EndTime).Int();
        var blueMarble = (gameEvent.EventInfo as BlueMarble)!;
        while (trips < totalNewTrips) {
            trips++;

            // Check if there's any item to give for every 1 trip
            BlueMarbleEntry entry1 = blueMarble.Entries.FirstOrDefault(entry => entry.TripAmount == 0);
            if (entry1 != default) {
                Item? trip0Item = session.Item.CreateItem(entry1.Item.ItemId);
                if (trip0Item != null && !session.Item.Inventory.Add(trip0Item, true)) {
                    session.Item.MailItem(trip0Item);
                }
            }

            // Check if there's any other item to give for hitting a specific number of trips
            BlueMarbleEntry entry2 = blueMarble.Entries.FirstOrDefault(entry => entry.TripAmount == trips);
            if (entry2 != default) {
                Item? tripItem = session.Item.CreateItem(entry2.Item.ItemId);
                if (tripItem != null && !session.Item.Inventory.Add(tripItem, true)) {
                    session.Item.MailItem(tripItem);
                }
            }
        }

        session.GameEventUserValue.Set(gameEvent.Id, GameEventUserValueType.MapleopolyTotalTrips, trips);
    }
}
