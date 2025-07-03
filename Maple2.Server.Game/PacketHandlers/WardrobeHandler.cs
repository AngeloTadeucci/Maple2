using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class WardrobeHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Wardrobe;

    private enum Command : byte {
        Create = 0,
        Equip = 1,
        Reset = 2,
        SetKey = 4,
        Rename = 6,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Create:
                HandleCreate(session, packet);
                return;
            case Command.Equip:
                HandleEquip(session, packet);
                return;
            case Command.Reset:
                HandleReset(session, packet);
                return;
            case Command.SetKey:
                HandleSetKey(session, packet);
                return;
            case Command.Rename:
                HandleRename(session, packet);
                return;
        }
    }

    // creates wardrobe tab from current equips
    private void HandleCreate(GameSession session, IByteReader packet) {
        int index = packet.ReadInt();
        int type = packet.ReadInt();

        if (!session.Config.TryGetWardrobe(index, out Wardrobe? wardrobe)) {
            return;
        }

        wardrobe.Type = type;
        wardrobe.Equips.Clear();
        bool isOutfit = type != 0;
        foreach ((EquipSlot slot, Item equip) in isOutfit ? session.Item.Equips.Outfit : session.Item.Equips.Gear) {
            switch (slot) {
                // Weapons
                case EquipSlot.LH:
                case EquipSlot.RH:
                // Armor
                case EquipSlot.CP:
                case EquipSlot.MT:
                case EquipSlot.CL:
                case EquipSlot.PA:
                case EquipSlot.GL:
                case EquipSlot.SH:
                // Accessories
                case EquipSlot.FH:
                case EquipSlot.EY:
                case EquipSlot.EA:
                case EquipSlot.PD:
                case EquipSlot.RI:
                case EquipSlot.BE:
                    wardrobe.Equips[slot] = new Wardrobe.Equip(equip.Uid, equip.Id, slot, equip.Rarity);
                    break;
            }
        }

        session.Send(WardrobePacket.Load(index, wardrobe));
    }

    private void HandleEquip(GameSession session, IByteReader packet) {
        int index = packet.ReadInt();
        bool isOutfit = packet.ReadInt() != 0;

        if (!session.Config.TryGetWardrobe(index, out Wardrobe? wardrobe)) {
            return;
        }

        foreach (Wardrobe.Equip equip in wardrobe.Equips.Values) {
            session.Item.Equips.Equip(equip.ItemUid, equip.EquipSlot, isOutfit);
        }
    }

    private void HandleReset(GameSession session, IByteReader packet) {
        int index = packet.ReadInt();

        if (session.Config.TryGetWardrobe(index, out Wardrobe? wardrobe)) {
            wardrobe.Equips.Clear();
            session.Send(WardrobePacket.Load(index, wardrobe));
        }
    }

    // This connects a wardrobe with a hotkey setting, which can be configured by KeyTable
    private void HandleSetKey(GameSession session, IByteReader packet) {
        int index = packet.ReadInt();
        int key = packet.ReadInt();

        if (session.Config.TryGetWardrobe(index, out Wardrobe? wardrobe)) {
            wardrobe.KeyId = key;
        }
    }

    private void HandleRename(GameSession session, IByteReader packet) {
        int index = packet.ReadInt();
        string name = packet.ReadUnicodeString();

        if (session.Config.TryGetWardrobe(index, out Wardrobe? wardrobe)) {
            wardrobe.Name = name;
        } else {
            session.Config.AddWardrobe(name);
        }
    }
}
