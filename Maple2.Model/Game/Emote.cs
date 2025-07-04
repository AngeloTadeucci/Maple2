using System.Runtime.InteropServices;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
public readonly struct Emote {
    public readonly int Id;
    public readonly int Level = 1;
    public readonly long ExpiryTime;

    public Emote(int id, long expiryTime = 0) {
        Id = id;
        ExpiryTime = expiryTime;
    }
}
