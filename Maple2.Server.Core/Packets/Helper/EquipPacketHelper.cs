using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Core.Packets.Helper;

public static class EquipPacketHelper {
    public static void WriteEquip(this IByteWriter writer, Item equip) {
        writer.WriteInt(equip.Id);
        writer.WriteLong(equip.Uid);
        writer.WriteUnicodeString(equip.EquipSlot().ToString());
        writer.WriteInt(equip.Rarity);
        writer.WriteClass<Item>(equip);
    }

    public static void WriteBadge(this IByteWriter writer, Item badge) {
        if (badge.Badge == null) {
            throw new ArgumentNullException(nameof(badge.Badge));
        }

        writer.Write<BadgeType>(badge.Badge.Type);
        writer.WriteInt(badge.Id);
        writer.WriteLong(badge.Uid);
        writer.WriteInt(badge.Rarity);
        writer.WriteClass<Item>(badge);
    }
}
