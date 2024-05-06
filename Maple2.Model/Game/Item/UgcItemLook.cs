using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class UgcItemLook : IByteSerializable, IByteDeserializable {
    public static readonly UgcItemLook Default = new UgcItemLook();

    public long Id;
    public string FileName = string.Empty;
    public string Name = string.Empty;
    public long AccountId;
    public long CharacterId;
    public string Author = string.Empty;
    public long CreationTime;
    public string Url = string.Empty;

    public UgcItemLook Clone() {
        return (UgcItemLook) MemberwiseClone();
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteUnicodeString(Id.ToString()); // UUID (filename)
        writer.WriteUnicodeString(Name); // Name (itemname)
        writer.WriteByte(1); // Needs to be 1 to show the proper UGC icon?
        writer.WriteInt(1);
        writer.WriteLong(AccountId); // AccountId
        writer.WriteLong(CharacterId); // CharacterId
        writer.WriteUnicodeString(Author); // CharacterName
        writer.WriteLong(CreationTime); // CreationTime
        writer.WriteUnicodeString(Url); // URL (no domain)
        writer.WriteByte();
    }

    public void ReadFrom(IByteReader reader) {
        reader.ReadLong();
        FileName = reader.ReadUnicodeString();
        Name = reader.ReadUnicodeString();
        reader.ReadByte();
        reader.ReadInt();
        AccountId = reader.ReadLong();
        CharacterId = reader.ReadLong();
        Author = reader.ReadUnicodeString();
        CreationTime = reader.ReadLong();
        Url = reader.ReadUnicodeString();
        reader.ReadByte();
    }
}
