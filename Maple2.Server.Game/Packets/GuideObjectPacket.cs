using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class GuideObjectPacket {
    private enum Command : byte {
        Create = 0,
        Remove = 1,
        Sync = 2,
    }

    public static ByteWriter Create(FieldGuideObject guideObject) {
        var pWriter = Packet.Of(SendOp.GuideObject);
        pWriter.Write<Command>(Command.Create);
        pWriter.WriteClass<FieldGuideObject>(guideObject);

        return pWriter;
    }

    public static ByteWriter Remove(FieldGuideObject guideObject) {
        var pWriter = Packet.Of(SendOp.GuideObject);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteInt(guideObject.ObjectId);
        pWriter.WriteLong(guideObject.CharacterId);

        return pWriter;
    }

    public static void GuideObject(this PoolByteWriter buffer, int guideObjectId, params StateSync[] syncStates) {
        buffer.Write<SendOp>(SendOp.GuideObject);
        buffer.Write<Command>(Command.Sync);
        buffer.WriteInt(guideObjectId);
        buffer.WriteByte((byte) syncStates.Length);
        foreach (StateSync entry in syncStates) {
            buffer.WriteClass<StateSync>(entry);
        }
    }
}
