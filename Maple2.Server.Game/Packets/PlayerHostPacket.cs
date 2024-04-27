using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class PlayerHostPacket {
    private enum Command : byte {
        UseHongBao = 0,
        HongBaoGiftNotice = 2,
        StartMiniGame = 3,
        MiniGameRewardNotice = 4,
        MiniGameRewardReceive = 5,
        AdBalloonWindow = 6,
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
