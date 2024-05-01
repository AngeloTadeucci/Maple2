using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools;

namespace Maple2.Server.Game.PacketHandlers;

public class BonusGameHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.BonusGame;

    private enum Command : byte {
        Load = 0,
        Spin = 2,
        Close = 3,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    public required ServerTableMetadataStorage ServerTableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Load:
                HandleLoad(session, packet);
                break;
            case Command.Spin:
                HandleSpin(session);
                break;
        }
    }

    private void HandleLoad(GameSession session, IByteReader packet) {
        int gameId = packet.ReadInt();

        if (!ServerTableMetadata.BonusGameTable.Games.TryGetValue(gameId, out BonusGameTable.Game? game) ||
            !ServerTableMetadata.BonusGameTable.Drops.TryGetValue(gameId, out BonusGameTable.Drop? drop)) {
            return;
        }

        session.BonusGameId = gameId;
        session.Send(BonusGamePacket.Load(drop.Items.Select(item => item.ItemComponent).ToList()));
    }

    private void HandleSpin(GameSession session) {
        if (session.NpcScript?.State == null) {
            session.BonusGameId = 0;
            return;
        }

        if (!ServerTableMetadata.BonusGameTable.Games.TryGetValue(session.BonusGameId, out BonusGameTable.Game? game) ||
            !ServerTableMetadata.BonusGameTable.Drops.TryGetValue(session.BonusGameId, out BonusGameTable.Drop? drop)) {
            session.BonusGameId = 0;
            return;
        }

        // Find out how many spins the player is choosing
        if (!session.ServerTableMetadata.ScriptConditionTable.Entries.TryGetValue(session.NpcScript.Npc.Value.Id, out Dictionary<int, ScriptConditionMetadata>? scriptConditions) ||
            !scriptConditions.TryGetValue(session.NpcScript.State.Id, out ScriptConditionMetadata? scriptCondition)) {
            session.BonusGameId = 0;
            return;
        }

        // We're getting only the first in the list, assuming it will always be only a list of one item
        int spins = scriptCondition.Items.FirstOrDefault().Key.Amount;

        int summedWeight = drop.Items.Sum(item => item.Probability);

        var dropItems = new WeightedSet<(BonusGameTable.Drop.Item, int)>(); // Item, index on wheel
        for (int i = 0; i < drop.Items.Length; i++) {
            dropItems.Add((drop.Items[i], i), drop.Items[i].Probability);
        }


        IList<KeyValuePair<Item, int>> rewardedItems = new List<KeyValuePair<Item, int>>(); // Item, Index on wheel
        for (int spin = 0; spin < spins; spin++) {
            (BonusGameTable.Drop.Item DropItem, int Index) result = dropItems.Get();
            Item? createdItem = session.Item.CreateItem(result.DropItem.ItemComponent.ItemId, result.DropItem.ItemComponent.Rarity, result.DropItem.ItemComponent.Amount);
            if (createdItem == null) {
                break;
            }

            if (!session.Item.Inventory.ConsumeItemComponents(new[] {game.ConsumeItem})) {
                // TODO: Close the bonus game if items count is 0
                break;
            }

            if (!session.Item.Inventory.Add(createdItem, true)) {
                session.Item.MailItem(createdItem);
            }
            rewardedItems.Add(new KeyValuePair<Item, int>(createdItem, result.Index));
        }

        session.Send(BonusGamePacket.Spin(rewardedItems));
        session.BonusGameId = 0;
    }
}
