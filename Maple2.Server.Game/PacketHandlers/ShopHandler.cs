using Maple2.Database.Storage;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ShopHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Shop;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required GameStorage GameStorage { private get; init; }
    // ReSharper restore All
    #endregion

    private enum Command : byte {
        PurchaseBuyBack = 3,
        Buy = 4,
        Sell = 5,
        InstantRestock = 9,
        Refresh = 10,
        LoadNew = 13, // Possibly unable to function in game
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.PurchaseBuyBack:
                HandlePurchaseBuyBack(session, packet);
                return;
            case Command.Buy:
                HandleBuy(session, packet);
                return;
            case Command.Sell:
                HandleSell(session, packet);
                return;
            case Command.InstantRestock:
                HandleInstantRestock(session, packet);
                return;
            case Command.Refresh:
                HandleRefresh(session);
                return;
        }
    }

    private void HandlePurchaseBuyBack(GameSession session, IByteReader packet) {
        int id = packet.ReadInt();

        session.Shop.PurchaseBuyBack(id);
    }

    private void HandleBuy(GameSession session, IByteReader packet) {
        int shopItemId = packet.ReadInt();
        int quantity = packet.ReadInt();

        session.Shop.Buy(shopItemId, quantity);
    }

    private void HandleSell(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        int quantity = packet.ReadInt();

        session.Shop.Sell(itemUid, quantity);
    }

    private void HandleInstantRestock(GameSession session, IByteReader packet) {
        packet.ReadInt(); // cost

        session.Shop.InstantRestock();
    }

    private void HandleRefresh(GameSession session) {
        session.Shop.Refresh();
    }
}
