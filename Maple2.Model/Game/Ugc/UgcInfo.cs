using System.Runtime.InteropServices;
using Maple2.Model.Enum;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 23)]
public readonly struct UgcInfo {
    public readonly UgcType Type;
    private readonly byte Unknown1;
    private readonly byte Unknown2;
    private readonly int Unknown3;
    public readonly long AccountId;
    public readonly long CharacterId;
}
