using System.Runtime.InteropServices;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
public readonly struct ChatSticker {
    public int Id { get; init; }
    public long ExpiryTime { get; init; }

    public ChatSticker(int id, long expiration = long.MaxValue) {
        Id = id;
        ExpiryTime = expiration;
    }
}
