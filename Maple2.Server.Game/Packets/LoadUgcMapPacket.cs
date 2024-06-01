using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class LoadUgcMapPacket {
    public static ByteWriter Load(int cubeCount) {
        var pWriter = Packet.Of(SendOp.LoadUgcMap);
        pWriter.WriteInt(cubeCount);
        pWriter.WriteInt();
        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter LoadHome(int cubeCount, Home home) {
        var pWriter = Packet.Of(SendOp.LoadUgcMap);
        pWriter.WriteInt(cubeCount);
        pWriter.WriteInt();
        pWriter.WriteBool(true);
        pWriter.WriteClass<Home>(home);
        pWriter.WriteByte(); // saved designs
        pWriter.WriteByte(); // saved blueprints

        return pWriter;
    }
}
