using System.Numerics;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.PacketHandlers;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class InstrumentPacket {
    private enum Command : byte {
        StartImprovise = 0,
        Improvise = 1,
        StopImprovise = 2,
        StartScore = 3,
        StopScore = 4,
        LeaveEnsemble = 6,
        ComposeScore = 8,
        RemainingUse = 9,
        ViewScore = 10,
        Fireworks = 14,
        Unknown = 17,
    }

    public static ByteWriter StartImprovise(FieldInstrument instrument) {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.StartImprovise);
        pWriter.WriteInt(instrument.ObjectId);
        pWriter.WriteInt(instrument.OwnerId);
        pWriter.Write<Vector3>(instrument.Position);
        pWriter.WriteInt(instrument.Value.MidiId);
        pWriter.WriteInt(instrument.Value.PercussionId);

        return pWriter;
    }

    public static ByteWriter Improvise(FieldInstrument instrument, in InstrumentHandler.MidiMessage note) {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.Improvise);
        pWriter.WriteInt(instrument.ObjectId);
        pWriter.WriteInt(instrument.OwnerId);
        pWriter.Write<InstrumentHandler.MidiMessage>(note);

        return pWriter;
    }

    public static ByteWriter StopImprovise(FieldInstrument instrument) {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.StopImprovise);
        pWriter.WriteInt(instrument.ObjectId);
        pWriter.WriteInt(instrument.OwnerId);

        return pWriter;
    }

    public static ByteWriter StartScore(FieldInstrument instrument, Item score) {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.StartScore);
        pWriter.WriteBool(score.Music != null);
        pWriter.WriteInt(instrument.ObjectId);
        pWriter.WriteInt(instrument.OwnerId);
        pWriter.Write<Vector3>(instrument.Position);
        pWriter.WriteInt((int) instrument.StartTick);
        pWriter.WriteInt(instrument.Value.MidiId);
        pWriter.WriteInt(instrument.Value.PercussionId);
        pWriter.WriteBool(instrument.Ensemble);

        if (score.Music != null) {
            pWriter.WriteString(score.Music.Mml);
        } else {
            pWriter.WriteUnicodeString(score.Metadata.Music?.FileName ?? "");
        }

        return pWriter;
    }

    public static ByteWriter StopScore(FieldInstrument instrument) {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.StopScore);
        pWriter.WriteInt(instrument.ObjectId);
        pWriter.WriteInt(instrument.OwnerId);

        return pWriter;
    }

    public static ByteWriter LeaveEnsemble() {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.LeaveEnsemble);

        return pWriter;
    }

    public static ByteWriter ComposeScore(Item item) {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.ComposeScore);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteClass<Item>(item);

        return pWriter;
    }

    public static ByteWriter RemainUses(long scoreUid, int remainUses) {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.RemainingUse);
        pWriter.WriteLong(scoreUid);
        pWriter.WriteInt(remainUses);

        return pWriter;
    }

    public static ByteWriter ViewScore(long itemUid, string mml) {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.ViewScore);
        pWriter.WriteLong(itemUid);
        pWriter.WriteString(mml);

        return pWriter;
    }

    public static ByteWriter Fireworks(int objectId) {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.Fireworks);
        pWriter.WriteInt(objectId);

        return pWriter;
    }

    public static ByteWriter Unknown(byte value) {
        var pWriter = Packet.Of(SendOp.PlayInstrument);
        pWriter.Write<Command>(Command.Unknown);
        pWriter.WriteByte(value);

        return pWriter;
    }
}
