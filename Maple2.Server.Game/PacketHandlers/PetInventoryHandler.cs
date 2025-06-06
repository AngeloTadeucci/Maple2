using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class PetInventoryHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.RequestPetInventory;

    private enum Command : byte {
        Add = 0,
        Remove = 1,
        Move = 3,
        Load = 7,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();

        switch (command) {
            case Command.Add:
                HandleAdd(session, packet);
                return;
            case Command.Remove:
                HandleRemove(session, packet);
                return;
            case Command.Move:
                HandleMove(session, packet);
                return;
            case Command.Load:
                HandleLoad(session);
                return;
        }
    }

    private static void HandleAdd(GameSession session, IByteReader packet) {
        if (session.Pet == null) {
            return;
        }

        long itemUid = packet.ReadLong();
        short slot = packet.ReadShort();
        int amount = packet.ReadInt();

        StringCode code = session.Pet.Add(itemUid, slot, amount);
        if (code != StringCode.s_empty_string) {
            session.Send(NoticePacket.MessageBox(code));
        }
    }

    private static void HandleRemove(GameSession session, IByteReader packet) {
        if (session.Pet == null) {
            return;
        }

        long itemUid = packet.ReadLong();
        short slot = packet.ReadShort();
        int amount = packet.ReadInt();

        StringCode code = session.Pet.Remove(itemUid, slot, amount);
        if (code != StringCode.s_empty_string) {
            session.Send(NoticePacket.MessageBox(code));
        }
    }

    private static void HandleMove(GameSession session, IByteReader packet) {
        if (session.Pet == null) {
            return;
        }

        long itemUid = packet.ReadLong();
        short dstSlot = packet.ReadShort();

        if (!session.Pet.Move(itemUid, dstSlot)) {
            session.Send(NoticePacket.MessageBox(StringCode.s_item_err_Invalid_slot));
        }
    }

    private static void HandleLoad(GameSession session) {
        session.Pet?.LoadInventory();
    }
}
