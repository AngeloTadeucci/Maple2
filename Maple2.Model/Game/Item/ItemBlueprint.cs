using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class ItemBlueprint : IByteSerializable, IByteDeserializable {
    public long BlueprintUid;
    public int Length;
    public int Width;
    public int Height;
    public DateTimeOffset CreationTime;
    public int Unknown;
    public long AccountId;
    public long CharacterId;
    public string CharacterName = "";

    public ItemBlueprint() {
        Unknown = 1;
    }

    public ItemBlueprint Clone() {
        return (ItemBlueprint) MemberwiseClone();
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(BlueprintUid);
        writer.WriteInt(Length);
        writer.WriteInt(Width);
        writer.WriteInt(Height);
        writer.WriteLong(CreationTime.ToUnixTimeSeconds());
        writer.WriteInt(Unknown);
        writer.WriteLong(AccountId);
        writer.WriteLong(CharacterId);
        writer.WriteUnicodeString(CharacterName);
    }

    public void ReadFrom(IByteReader reader) {
        BlueprintUid = reader.ReadLong();
        Length = reader.ReadInt();
        Width = reader.ReadInt();
        Height = reader.ReadInt();
        CreationTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadLong());
        Unknown = reader.ReadInt();
        AccountId = reader.ReadLong();
        CharacterId = reader.ReadLong();
        CharacterName = reader.ReadUnicodeString();
    }
}
