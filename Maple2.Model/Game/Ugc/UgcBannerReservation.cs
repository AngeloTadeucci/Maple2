using System.Runtime.InteropServices;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 36)]
public readonly struct UgcBannerReservation {
    public readonly long Uid;
    public readonly int Unknown1;
    public readonly long Id;
    public readonly int Date;
    public readonly int Hour;
    public readonly long Unknown2;
}
