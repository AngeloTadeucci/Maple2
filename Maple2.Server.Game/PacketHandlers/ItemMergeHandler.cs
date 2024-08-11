using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ItemMergeHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.ItemMerge;

    private enum Command : byte {
        Stage = 0,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required Lua.Lua Lua { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Stage:
                HandleStage(session, packet);
                break;
        }
    }

    private void HandleStage(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();

        Item? item = session.Item.Inventory.Get(itemUid);
        if (item == null) {
            item = session.Item.Equips.Get(itemUid);
            if (item == null) {
                // TODO: Error
                return;
            }
        }
        int type = item.Type.Type;

        List<long> catalystUids = new();
        foreach ((int id, Dictionary<int, ItemMergeSlot> options) in session.ServerTableMetadata.ItemMergeTable.Entries) {
            Item? catalyst = session.Item.Inventory.Find(id).FirstOrDefault();
            if (catalyst == null) {
                continue;
            }

            if (catalyst.Metadata.Limit.Level < item.Metadata.Limit.Level) {
                continue;
            }

            if (!options.TryGetValue(type, out ItemMergeSlot? mergeSlot)) {
                continue;
            }

            catalystUids.Add(catalyst.Uid);
        }

        session.Send(ItemMergePacket.Stage(catalystUids));
    }
}
