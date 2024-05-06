using System.Runtime.InteropServices;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
public readonly struct Emote(int id, long expiryTime = 0) {
    public readonly int Id = id;
    public readonly int Level = 1;
    public readonly long ExpiryTime = expiryTime;

}
