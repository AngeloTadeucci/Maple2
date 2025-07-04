using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class LevelUpPacket {
    public static ByteWriter LevelUp(IActor<Player> player) {
        var pWriter = Packet.Of(SendOp.LevelUp);
        pWriter.WriteInt(player.ObjectId);
        pWriter.WriteShort(player.Value.Character.Level);

        return pWriter;
    }
}
