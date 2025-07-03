using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model.Widget;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class SurvivalEventPacket {
    public static ByteWriter Update(SurvivalContentsWidget widget) {
        var pWriter = Packet.Of(SendOp.SurvivalEvent);
        pWriter.WriteByte(0); // bool lto enable?
        pWriter.WriteClass<SurvivalContentsWidget>(widget);

        return pWriter;
    }
}
