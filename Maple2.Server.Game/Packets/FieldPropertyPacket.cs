using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class FieldPropertyPacket {
    private enum Command : byte {
        Load = 0,
        Add = 1,
        Remove = 2,
    }

    public static ByteWriter Load(ICollection<IFieldProperty> properties) {
        var pWriter = Packet.Of(SendOp.FieldProperty);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(properties.Count);
        foreach (IFieldProperty property in properties) {
            pWriter.WriteClass<IFieldProperty>(property);
        }

        return pWriter;
    }

    public static ByteWriter Add(IFieldProperty property) {
        var pWriter = Packet.Of(SendOp.FieldProperty);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteClass<IFieldProperty>(property);

        return pWriter;
    }

    public static ByteWriter Remove(FieldProperty property) {
        var pWriter = Packet.Of(SendOp.FieldProperty);
        pWriter.Write<Command>(Command.Remove);
        pWriter.Write<FieldProperty>(property);

        return pWriter;
    }

    public static ByteWriter Background(string ddsPath) {
        var pWriter = Packet.Of(SendOp.ChangeBackground);
        pWriter.WriteString(ddsPath);

        return pWriter;
    }

    public static ByteWriter TimeScale(bool enable, float startScale, float endScale, float duration, byte interpolator) {
        var pWriter = Packet.Of(SendOp.TimeScale);
        pWriter.WriteBool(enable);
        pWriter.WriteFloat(startScale);
        pWriter.WriteFloat(endScale);
        pWriter.WriteFloat(duration);
        pWriter.WriteByte(interpolator);

        return pWriter;
    }
}
