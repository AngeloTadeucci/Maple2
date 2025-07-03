using Maple2.PacketLib.Tools;

namespace Maple2.Model.Game;

// gosMicroGameRps
public class StateSyncRps : StateSync {
    public int UnknownRpsInt;
    public byte UnknownRpsByte1;
    public byte UnknownRpsByte2;
    public string? UnknownRpsString;

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt(UnknownRpsInt);
        writer.WriteByte(UnknownRpsByte1);
        writer.WriteByte(UnknownRpsByte2);
        writer.WriteUnicodeString(UnknownRpsString ?? "");
    }

    public override void ReadFrom(IByteReader reader) {
        base.ReadFrom(reader);
        UnknownRpsInt = reader.ReadInt();
        UnknownRpsByte1 = reader.ReadByte();
        UnknownRpsByte2 = reader.ReadByte();
        UnknownRpsString = reader.ReadUnicodeString();
    }
}
