using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class ItemBinding : IByteSerializable, IByteDeserializable {
    public static readonly ItemBinding Default = new ItemBinding();

    public long CharacterId { get; private set; }
    public string Name { get; private set; }

    public ItemBinding(long characterId = 0, string name = "") {
        CharacterId = characterId;
        Name = name;
    }

    public ItemBinding Clone() {
        return (ItemBinding) MemberwiseClone();
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(CharacterId);
        writer.WriteUnicodeString(Name);
    }

    public void ReadFrom(IByteReader reader) {
        CharacterId = reader.ReadLong();
        Name = reader.ReadUnicodeString();
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(this, obj)) return true;
        if (!(obj is ItemBinding other)) return false;
        return CharacterId == other.CharacterId && Name == other.Name;
    }

    public override int GetHashCode() {
        return HashCode.Combine(CharacterId, Name);
    }
}
