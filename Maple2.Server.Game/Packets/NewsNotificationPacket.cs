using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class NewsNotificationPacket {
    // 0 = WebBrowser
    // 1 = VisitorsBookDialog
    // 2 = NoticeDialog
    // 5 = WebBrowserPopup
    public static ByteWriter OpenUi(int type) {
        var pWriter = Packet.Of(SendOp.NewsNotification);
        pWriter.WriteByte();
        pWriter.WriteUnicodeString("86BFAEA2-DC42-4AEA-ADD5-D234E8810E08");
        pWriter.WriteBool(true);
        pWriter.WriteInt(type);

        return pWriter;
    }

    public static ByteWriter ResponseToken(string token) {
        var pWriter = Packet.Of(SendOp.NewsNotification);
        pWriter.WriteByte();
        pWriter.WriteUnicodeString(token);
        pWriter.WriteBool(false);
        pWriter.WriteInt(); // Doesn't seem to be used

        return pWriter;
    }
}
