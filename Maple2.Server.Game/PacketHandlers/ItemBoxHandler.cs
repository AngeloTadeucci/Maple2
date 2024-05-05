using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ItemBoxHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RequestItemBox;

    public override void Handle(GameSession session, IByteReader packet) {
        int itemId = packet.ReadInt();
        short unk = packet.ReadShort();
        int count = packet.ReadInt();
        if (!int.TryParse(packet.ReadUnicodeString(), out int index)) {
            return;
        }

        Item? item = session.Item.Inventory.Find(itemId).FirstOrDefault();
        if (item == null) {
            return;
        }

        ItemBoxError error = session.ItemBox.Open(item, count, index);
        session.Send(ItemBoxPacket.Open(itemId, session.ItemBox.BoxCount, error));
        session.ItemBox.Reset();
    }
}
