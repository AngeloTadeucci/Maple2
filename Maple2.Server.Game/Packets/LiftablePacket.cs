using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class LiftablePacket {
    private enum Command : byte {
        BatchUpdate = 0,
        Update = 2,
        Add = 3,
        Remove = 4,
    }

    public static ByteWriter Update(ICollection<FieldLiftable> liftables) {
        var pWriter = Packet.Of(SendOp.Liftable);
        pWriter.Write<Command>(Command.BatchUpdate);
        pWriter.WriteInt(liftables.Count);
        foreach (FieldLiftable liftable in liftables) {
            pWriter.WriteString(liftable.EntityId);
            pWriter.WriteByte();
            pWriter.WriteInt(liftable.Count);
            pWriter.Write<LiftableState>(liftable.State);
            pWriter.WriteUnicodeString(liftable.Value.MaskQuestId);
            pWriter.WriteUnicodeString(liftable.Value.MaskQuestState);
            pWriter.WriteUnicodeString(liftable.Value.EffectQuestId);
            pWriter.WriteUnicodeString(liftable.Value.EffectQuestState);
            pWriter.WriteBool(true);
        }

        return pWriter;
    }

    public static ByteWriter Update(FieldLiftable liftable) {
        var pWriter = Packet.Of(SendOp.Liftable);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteString(liftable.EntityId);
        pWriter.WriteByte();
        pWriter.WriteInt(liftable.Count);
        pWriter.Write<LiftableState>(liftable.State);

        return pWriter;
    }

    public static ByteWriter Add(FieldLiftable liftable) {
        var pWriter = Packet.Of(SendOp.Liftable);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteString(liftable.EntityId);
        pWriter.WriteInt(liftable.Count);
        pWriter.WriteUnicodeString(liftable.Value.MaskQuestId);
        pWriter.WriteUnicodeString(liftable.Value.MaskQuestState);
        pWriter.WriteUnicodeString(liftable.Value.EffectQuestId);
        pWriter.WriteUnicodeString(liftable.Value.EffectQuestState);
        pWriter.WriteBool(true); // UseEffect

        return pWriter;
    }

    public static ByteWriter Remove(string entityId) {
        var pWriter = Packet.Of(SendOp.Liftable);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteString(entityId);

        return pWriter;
    }
}
