using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class SkillPointPacket {
    public static ByteWriter Sources(SkillPoint skillPoints) {
        var pWriter = Packet.Of(SendOp.SkillPoint);
        pWriter.WriteClass<SkillPoint>(skillPoints);

        return pWriter;
    }
}
