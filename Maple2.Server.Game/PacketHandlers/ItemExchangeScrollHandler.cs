using Maple2.Database.Storage;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ItemExchangeScroll : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.ItemExchangeScroll;

    private enum Command : byte {
        Exchange = 1,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required ItemMetadataStorage ItemMetadata { private get; init; }
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Exchange:
                HandleExchange(session, packet);
                break;
        }
    }

    private void HandleExchange(GameSession session, IByteReader packet) {
        long scrollItemUid = packet.ReadLong();
        long unk = packet.ReadLong(); // item uid for upgrading exchange type?
        int quantity = packet.ReadInt();

        Item? scrollItem = session.Item.Inventory.Get(scrollItemUid);

        if (scrollItem == null) {
            session.Send(ItemExchangeScrollPacket.Error(ItemExchangeScrollError.s_itemslot_exchange_scroll_invalid));
            return;
        }

        if (!int.TryParse(scrollItem.Metadata.Function?.Parameters, out int scrollId) || !TableMetadata.ItemExchangeScrollTable.Entries.TryGetValue(scrollId, out ItemExchangeScrollMetadata? scrollMetadata)) {
            session.Send(ItemExchangeScrollPacket.Error(ItemExchangeScrollError.s_itemslot_exchange_unknown));
            return;
        }

        if (session.Currency.CanAddMeso(-scrollMetadata.RequiredMeso * quantity) != -scrollMetadata.RequiredMeso * quantity) {
            session.Send(ItemExchangeScrollPacket.Error(ItemExchangeScrollError.s_itemslot_exchange_money_invalid));
            return;
        }

        lock (session.Item) {
            // Check if player has enough scrolls
            List<Item> scrolls = session.Item.Inventory.Find(scrollMetadata.RecipeScroll.ItemId, scrollMetadata.RecipeScroll.Rarity).ToList();
            if (scrolls.Sum(x => x.Amount) < scrollMetadata.RecipeScroll.Amount * quantity) {
                session.Send(ItemExchangeScrollPacket.Error(ItemExchangeScrollError.s_itemslot_exchange_count_invalid));
                return;
            }

            // Check and consume required items
            if (!session.Item.Inventory.ConsumeItemComponents(scrollMetadata.RequiredItems, quantity)) {
                session.Send(ItemExchangeScrollPacket.Error(ItemExchangeScrollError.s_itemslot_exchange_count_invalid));
                return;
            }

            // Consume scrolls
            int remainingScrolls = scrollMetadata.RecipeScroll.Amount * quantity;
            foreach (Item scroll in scrolls) {
                int consume = Math.Min(remainingScrolls, scroll.Amount);
                if (!session.Item.Inventory.Consume(scroll.Uid, consume)) {
                    Logger.Fatal("Failed to consume item {ItemUid}", scroll.Uid);
                    throw new InvalidOperationException($"Fatal: Consuming item: {scroll.Uid}");
                }
            }
        }
        Item? rewardItem = session.Item.CreateItem(scrollMetadata.RewardItem.ItemId, scrollMetadata.RewardItem.Rarity, scrollMetadata.RewardItem.Amount * quantity);
        if (rewardItem == null) {
            return;
        }

        session.Currency.Meso -= scrollMetadata.RequiredMeso;
        if (!session.Item.Inventory.Add(rewardItem, true)) {
            session.Item.MailItem(rewardItem);
        }
        session.Send(ItemExchangeScrollPacket.Error(ItemExchangeScrollError.s_itemslot_exchange_ok));
    }
}
