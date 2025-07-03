using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class PlayerHostPacket {
    private enum Command : byte {
        UseHongBao = 0,
        GiftHongBao = 2,
        StartMiniGame = 3,
        MiniGameRewardNotice = 4,
        MiniGameRewardReceive = 5,
        AdBalloonWindow = 6,
    }

    public static ByteWriter UseHongBao(HongBao hongBao, int amount) {
        var pWriter = Packet.Of(SendOp.PlayerHost);
        pWriter.Write<Command>(Command.UseHongBao);
        pWriter.WriteInt(hongBao.SourceItemId);
        pWriter.WriteInt(hongBao.ObjectId);
        pWriter.WriteInt(hongBao.ItemId);
        pWriter.WriteInt(amount);
        pWriter.WriteInt(hongBao.MaxUserCount - 1);
        pWriter.WriteUnicodeString(hongBao.Owner.Value.Character.Name);

        return pWriter;
    }

    public static ByteWriter GiftHongBao(FieldPlayer player, HongBao hongBao, int amount) {
        var pWriter = Packet.Of(SendOp.PlayerHost);
        pWriter.Write<Command>(Command.GiftHongBao);
        pWriter.WriteBool(true);
        pWriter.WriteInt(hongBao.SourceItemId);
        pWriter.WriteInt(hongBao.ItemId);
        pWriter.WriteInt(amount);
        pWriter.WriteUnicodeString(hongBao.Owner.Value.Character.Name);
        pWriter.WriteUnicodeString(player.Value.Character.Name);

        return pWriter;
    }

    public static ByteWriter InactiveHongBao() {
        var pWriter = Packet.Of(SendOp.PlayerHost);
        pWriter.Write<Command>(Command.GiftHongBao);
        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter StartMiniGame(string hostName, int mapId) {
        var pWriter = Packet.Of(SendOp.PlayerHost);
        pWriter.Write<Command>(Command.StartMiniGame);
        pWriter.WriteUnicodeString(hostName);
        pWriter.WriteInt(mapId);

        return pWriter;
    }

    public static ByteWriter AdBalloonWindow(InteractBillBoardObject billboard) {
        var pWriter = Packet.Of(SendOp.PlayerHost);
        pWriter.Write<Command>(Command.AdBalloonWindow);
        pWriter.WriteLong(billboard.OwnerAccountId);
        pWriter.WriteLong(billboard.OwnerCharacterId);
        pWriter.WriteUnicodeString(billboard.OwnerPicture);
        pWriter.WriteUnicodeString(billboard.OwnerName);
        pWriter.WriteShort(billboard.OwnerLevel);
        pWriter.Write<JobCode>(billboard.OwnerJobCode);
        pWriter.WriteShort();
        pWriter.WriteUnicodeString(billboard.Title);
        pWriter.WriteUnicodeString(billboard.Description);
        pWriter.WriteBool(billboard.PublicHouse);
        pWriter.WriteLong(billboard.CreationTime);
        pWriter.WriteLong(billboard.ExpirationTime);
        pWriter.WriteLong();

        return pWriter;
    }
}
