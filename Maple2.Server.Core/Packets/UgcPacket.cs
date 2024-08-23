using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Game.Ugc;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Core.Packets;

public static class UgcPacket {
    private enum Command : byte {
        Upload = 2,
        UpdatePath = 4,
        EnableBanner = 7,
        UpdateBanner = 8,
        ProfilePicture = 11,
        UpdateItem = 13,
        UpdateFurnishing = 14,
        UpdateMount = 15,
        SetEndpoint = 17,
        LoadBanner = 18,
        ReserveBanners = 20,
    }

    public static ByteWriter Upload(UgcResource ugc) {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.Upload);
        pWriter.Write<UgcType>(ugc.Type);
        pWriter.WriteLong(ugc.Id);
        pWriter.WriteUnicodeString(ugc.Id.ToString());

        return pWriter;
    }

    public static ByteWriter UpdatePath(UgcResource ugc) {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.UpdatePath);
        pWriter.Write<UgcType>(ugc.Type);
        pWriter.WriteLong(ugc.Id);
        pWriter.WriteUnicodeString(ugc.Path);

        return pWriter;
    }

    public static ByteWriter SetEndpoint(Uri uri, Locale locale = Locale.NA) {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.SetEndpoint);
        pWriter.WriteUnicodeString($"{uri.Scheme}://{uri.Authority}/ws.asmx?wsdl");
        pWriter.WriteUnicodeString($"{uri.Scheme}://{uri.Authority}");
        pWriter.WriteUnicodeString(locale.ToString().ToLower());
        pWriter.Write<Locale>(locale);

        return pWriter;
    }

    public static ByteWriter ProfilePicture(Player player) {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.ProfilePicture);
        pWriter.WriteInt(player.ObjectId);
        pWriter.WriteLong(player.Character.Id);
        pWriter.WriteUnicodeString(player.Character.Picture);

        return pWriter;
    }

    public static ByteWriter ProfilePicture(Character character) {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.ProfilePicture);
        pWriter.WriteInt();
        pWriter.WriteLong(character.Id);
        pWriter.WriteUnicodeString(character.Picture);

        return pWriter;
    }

    public static ByteWriter UpdateItem(int objectId, Item item, long createPrice, UgcType ugcType) {
        var pWriter = Packet.Of(SendOp.Ugc);
        switch (ugcType) {
            case UgcType.Item:
                pWriter.Write<Command>(Command.UpdateItem);
                break;
            case UgcType.Furniture:
                pWriter.Write<Command>(Command.UpdateFurnishing);
                break;
            case UgcType.Mount:
                pWriter.Write<Command>(Command.UpdateMount);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ugcType), ugcType, null);
        }

        pWriter.WriteInt(objectId);

        pWriter.WriteLong(item.Uid);
        pWriter.WriteInt(item.Id);
        pWriter.WriteInt(item.Amount);
        pWriter.WriteUnicodeString(item.Template!.Name);
        pWriter.WriteByte(1);
        pWriter.WriteLong(createPrice);
        pWriter.WriteByte();

        pWriter.WriteClass<UgcItemLook>(item.Template);

        return pWriter;
    }

    public static ByteWriter LoadBanners(List<UgcBanner> banners) {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.LoadBanner);

        int counter1 = 0;
        pWriter.WriteInt(counter1);
        for (int i = 0; i < counter1; i++) {
            bool flagA = false;
            pWriter.WriteBool(flagA); // CUgcBannerRollingImage
            if (flagA) {
                pWriter.WriteLong();
                pWriter.WriteUnicodeString();
                pWriter.WriteByte();
                pWriter.WriteInt();
                pWriter.WriteLong();
                pWriter.WriteLong();
                pWriter.WriteUnicodeString();
                pWriter.WriteUnicodeString();
                pWriter.WriteUnicodeString();
            }
        }

        pWriter.WriteInt(banners.Count);
        foreach (UgcBanner ugcBanner in banners) {
            pWriter.WriteLong(ugcBanner.Id);
            BannerSlot? activeSlot = ugcBanner.Slots.FirstOrDefault(x => x.Active);
            pWriter.WriteBool(activeSlot is not null);
            if (activeSlot?.Template is null) {
                continue;
            }

            pWriter.WriteActiveBannerSlot(activeSlot);
        }

        pWriter.WriteInt(banners.Count);
        foreach (UgcBanner ugcBanner in banners) {
            pWriter.WriteUgcBanner(ugcBanner.Id, ugcBanner.Slots);
        }

        return pWriter;
    }

    public static ByteWriter ActivateBanner(UgcBanner ugcBanner) {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.EnableBanner);
        pWriter.WriteLong(ugcBanner.Id);
        BannerSlot? activeSlot = ugcBanner.Slots.FirstOrDefault(x => x.Active);
        pWriter.WriteBool(activeSlot is not null);
        if (activeSlot?.Template is null) {
            return pWriter;
        }

        pWriter.WriteActiveBannerSlot(activeSlot);

        return pWriter;
    }

    public static ByteWriter UpdateBanner(UgcBanner ugcBanner) {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.UpdateBanner);
        pWriter.WriteUgcBanner(ugcBanner.Id, ugcBanner.Slots);

        return pWriter;
    }

    public static ByteWriter ReserveBannerSlots(long bannerId, List<BannerSlot> bannerSlots) {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.ReserveBanners);
        pWriter.WriteLong(bannerId);
        pWriter.WriteInt(bannerSlots.Count);
        foreach (BannerSlot slot in bannerSlots) {
            pWriter.WriteClass<BannerSlot>(slot);
        }

        return pWriter;
    }

    private static void WriteUgcBanner(this ByteWriter pWriter, long bannerId, List<BannerSlot> banners) {
        pWriter.WriteLong(bannerId);
        pWriter.WriteInt(banners.Count);
        foreach (BannerSlot bannerSlot in banners) {
            long bannerSlotDate = long.Parse($"{bannerSlot.Date}00000") + bannerSlot.Hour; // yes. this is stupid. Who approved this?
            pWriter.WriteLong(bannerSlotDate);
            pWriter.WriteUnicodeString(bannerSlot.Template?.Author ?? string.Empty);
            pWriter.WriteBool(true); //  true = reserved, false = awaiting reservation, not sure when false is used
        }
    }

    private static void WriteActiveBannerSlot(this ByteWriter pWriter, BannerSlot activeSlot) {
        pWriter.Write(UgcType.Banner);
        pWriter.WriteInt(2);
        pWriter.WriteLong(activeSlot.Template!.AccountId);
        pWriter.WriteLong(activeSlot.Template.CharacterId);
        pWriter.WriteUnicodeString(); // unknown
        pWriter.WriteUnicodeString(activeSlot.Template.Author);
        pWriter.WriteLong(activeSlot.Template.Id);
        pWriter.WriteUnicodeString(activeSlot.Template.Id.ToString());
        pWriter.WriteByte(3);
        pWriter.WriteByte(1);
        pWriter.WriteLong(activeSlot.BannerId);
        byte loopCounter = 1; // Usually it's all slots but only one can be active at a time, so why have more than one?
        pWriter.WriteByte(loopCounter);
        for (byte i = 0; i < loopCounter; i++) {
            pWriter.WriteClass<BannerSlot>(activeSlot);
        }

        pWriter.WriteUnicodeString(activeSlot.Template.Url);
    }
}
