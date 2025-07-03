using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class KeyTableHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.KeyTable;

    private enum Command : byte {
        SetGameKeyBind = 1,
        SetKeyBind = 2,
        MoveQuickSlot = 3,
        AddQuickSlot = 4,
        RemoveQuickSlot = 5,
        Unknown6 = 6,
        Unknown7 = 7,
        SetActiveHotBar = 8,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.SetGameKeyBind:
            case Command.SetKeyBind:
                SetKeyBinds(session, packet);
                return;
            case Command.MoveQuickSlot:
            case Command.AddQuickSlot:
                SetQuickSlot(session, packet);
                return;
            case Command.RemoveQuickSlot:
                RemoveQuickSlot(session, packet);
                return;
            case Command.Unknown6:
                break;
            case Command.Unknown7:
                break;
            case Command.SetActiveHotBar:
                SetActiveHotBar(session, packet);
                return;
        }
    }

    private void SetKeyBinds(GameSession session, IByteReader packet) {
        int count = packet.ReadInt();
        for (int i = 0; i < count; i++) {
            var keyBind = packet.Read<KeyBind>();
            session.Config.SetKeyBind(keyBind);
        }
    }

    private void SetQuickSlot(GameSession session, IByteReader packet) {
        short index = packet.ReadShort();
        if (!session.Config.TryGetHotBar(index, out HotBar? targetHotBar)) {
            Logger.Warning("Invalid HotBar: {Index}", index);
            return;
        }

        var quickSlot = packet.Read<QuickSlot>();
        if (quickSlot.ItemUid != 0) {
            Item? item = session.Item.Inventory.Get(quickSlot.ItemUid);
            if (item == null || item.Id != quickSlot.ItemId) {
                Logger.Error("Assigning invalid item to QuickSlot");
                return;
            }
        }

        int targetSlot = packet.ReadInt();
        targetHotBar.MoveQuickSlot(targetSlot, quickSlot);
        session.Config.LoadHotBars();
    }

    private void RemoveQuickSlot(GameSession session, IByteReader packet) {
        short index = packet.ReadShort();
        if (!session.Config.TryGetHotBar(index, out HotBar? targetHotBar)) {
            Logger.Warning("Invalid HotBar: {Index}", index);
            return;
        }

        int skillId = packet.ReadInt();
        long itemUid = packet.ReadLong();
        if (targetHotBar.RemoveQuickSlot(skillId, itemUid)) {
            session.Config.LoadHotBars();
        }
    }

    private void SetActiveHotBar(GameSession session, IByteReader packet) {
        short hotBarIndex = packet.ReadShort();

        session.Config.SetActiveHotBar(hotBarIndex);
    }
}
