using System.Runtime.InteropServices;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
public readonly struct ChatSticker(int id, long expiration = long.MaxValue) {
    public int Id { get; init; } = id;
    public long ExpiryTime { get; init; } = expiration;

}
