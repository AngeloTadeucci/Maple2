using System.Runtime.InteropServices;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 26)]
public readonly struct Emote {
    public readonly int Id;
    public readonly int Level = 1;
    public readonly bool Unknown1;
    public readonly long ExpiryTime;
    public readonly long UnknownTime;
    public readonly bool Unknown2;

    public Emote(int id, long expiryTime = 0) {
        Id = id;
        ExpiryTime = expiryTime;
    }
}
