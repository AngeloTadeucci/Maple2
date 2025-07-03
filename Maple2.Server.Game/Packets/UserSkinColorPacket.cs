using Maple2.Model.Common;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class UserSkinColorPacket {
    public static ByteWriter Update(IActor<Player> player, SkinColor skinColor) {
        var pWriter = Packet.Of(SendOp.UserSkinColor);
        pWriter.WriteInt(player.ObjectId);
        pWriter.Write<SkinColor>(skinColor);

        return pWriter;
    }
}
