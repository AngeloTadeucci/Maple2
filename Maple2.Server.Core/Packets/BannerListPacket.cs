using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Core.Packets;

public static class BannerListPacket {
    public static ByteWriter Load(IList<SystemBanner> banners) {
        var pWriter = Packet.Of(SendOp.BannerList);
        pWriter.WriteShort((short) banners.Count);
        foreach (SystemBanner banner in banners) {
            pWriter.WriteClass<SystemBanner>(banner);
        }

        return pWriter;
    }
}
