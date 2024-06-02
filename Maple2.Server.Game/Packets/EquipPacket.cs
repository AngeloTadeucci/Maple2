using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class EquipPacket {
    public static ByteWriter EquipItem(IActor<Player> player, Item item, byte type) {
        var pWriter = Packet.Of(SendOp.ItemPutOn);
        pWriter.WriteInt(player.ObjectId);
        pWriter.WriteInt(item.Id);
        pWriter.WriteLong(item.Uid);
        pWriter.Write<EquipSlot>(item.EquipSlot());
        pWriter.WriteInt(item.Rarity);
        pWriter.WriteByte(type); // 0, 1, 2 (rest invalid)
        pWriter.WriteClass<Item>(item);

        return pWriter;
    }

    public static ByteWriter UnequipItem(IActor<Player> player, Item item) {
        var pWriter = Packet.Of(SendOp.ItemPutOff);
        pWriter.WriteInt(player.ObjectId);
        pWriter.WriteLong(item.Uid);

        return pWriter;
    }

    private enum BadgeCommand : byte {
        Equip = 0,
        Unequip = 1,
    }

    public static ByteWriter EquipBadge(IActor<Player> player, Item item) {
        var pWriter = Packet.Of(SendOp.BadgeEquip);
        pWriter.Write<BadgeCommand>(BadgeCommand.Equip);
        pWriter.WriteInt(player.ObjectId);
        pWriter.WriteInt(item.Id);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteInt(item.Rarity);
        pWriter.Write<BadgeType>(item.Badge?.Type ?? BadgeType.None);
        pWriter.WriteClass<Item>(item);

        return pWriter;
    }

    public static ByteWriter UnequipBadge(IActor<Player> player, BadgeType slot) {
        var pWriter = Packet.Of(SendOp.BadgeEquip);
        pWriter.Write<BadgeCommand>(BadgeCommand.Unequip);
        pWriter.WriteInt(player.ObjectId);
        pWriter.Write<BadgeType>(slot);

        return pWriter;
    }
}
