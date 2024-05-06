using System.Runtime.InteropServices;
using Maple2.Model.Enum;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
public readonly struct NpcDialogue(int id, int index, NpcTalkButton button) {
    public readonly int Id = id; // ScriptId
    public readonly int Index = index; // Index
    public readonly NpcTalkButton Button = button;

}
