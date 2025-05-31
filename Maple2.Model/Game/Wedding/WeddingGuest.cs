using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Wedding;

public readonly struct WeddingGuest : IByteSerializable {
    public static readonly WeddingGuest Default = new WeddingGuest {
        AccountId = 0,
        CharacterId = 0,
        Name = string.Empty,
    };
    public long AccountId { get; init; }
    public long CharacterId { get; init; }
    public string Name { get; init; }

    public WeddingGuest() {
        AccountId = 0;
        CharacterId = 0;
        Name = string.Empty;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(AccountId);
        writer.WriteLong(CharacterId);
        writer.WriteUnicodeString(Name ?? string.Empty);
    }
}
