using System.Runtime.InteropServices;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 21)]
public readonly record struct KeyBind(int KeyCode, int OptionType, long OptionGuid, int Unknown1, byte Priority);

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
public readonly record struct QuickSlot(int SkillId, int ItemId = 0, long ItemUid = 0) {
    public QuickSlot(Item item) : this(0, item.Id, item.Uid) { }
}
