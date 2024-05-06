using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class ItemCustomMusicScore : IByteSerializable, IByteDeserializable {
    public int Length;
    public int Instrument;
    public string Title = string.Empty;
    public string Author = string.Empty;
    public long AuthorId; // AccountId
    public bool IsLocked; // true=>s_writemusic_error_cant_edit
    public string Mml = string.Empty;

    public ItemCustomMusicScore Clone() {
        return (ItemCustomMusicScore) MemberwiseClone();
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Length);
        writer.WriteInt(Instrument);
        writer.WriteUnicodeString(Title);
        writer.WriteUnicodeString(Author);
        writer.WriteInt(1);
        writer.WriteLong(AuthorId);
        writer.WriteBool(IsLocked);
        writer.WriteLong();
        writer.WriteLong();
    }

    public void ReadFrom(IByteReader reader) {
        Length = reader.ReadInt();
        Instrument = reader.ReadInt();
        Title = reader.ReadUnicodeString();
        Author = reader.ReadUnicodeString();
        reader.ReadInt();
        AuthorId = reader.ReadLong();
        IsLocked = reader.ReadBool();
        reader.ReadLong();
        reader.ReadLong();
    }
}
