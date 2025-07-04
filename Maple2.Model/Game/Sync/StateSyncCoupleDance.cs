using Maple2.PacketLib.Tools;

namespace Maple2.Model.Game;

// gosMicroGameCoupleDance
public sealed class StateSyncCoupleDance : StateSync {
    public int UnknownCoupleDanceInt;
    public bool UnknownCoupleDanceBool;
    public string? UnknownCoupleDanceString;

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt(UnknownCoupleDanceInt);
        writer.WriteBool(UnknownCoupleDanceBool);
        writer.WriteUnicodeString(UnknownCoupleDanceString ?? "");
    }

    public override void ReadFrom(IByteReader reader) {
        base.ReadFrom(reader);
        UnknownCoupleDanceInt = reader.ReadInt();
        UnknownCoupleDanceBool = reader.ReadBool();
        UnknownCoupleDanceString = reader.ReadUnicodeString();
    }
}
