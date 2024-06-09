using System;
using Maple2.Model.Enum;
using Maple2.Model.Game;
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

    public static ByteWriter UpdateItem(int objectId, Item item, long createPrice) {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.UpdateItem);
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

    public static ByteWriter LoadBanners() {
        var pWriter = Packet.Of(SendOp.Ugc);
        pWriter.Write<Command>(Command.LoadBanner);

        pWriter.WriteInt(); // count
        pWriter.WriteInt(); // count
        pWriter.WriteInt(); // count

        return pWriter;
    }
}
