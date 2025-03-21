using System.Runtime.InteropServices;
using Maple2.Model.Enum;

namespace Maple2.Model.Game.Dungeon;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
public struct DungeonUserResult {
    public readonly long CharacterId;
    public readonly DungeonAccumulationRecordType RecordType;
    public readonly int Value;
    public DungeonGrade Grade = DungeonGrade.None;

    public DungeonUserResult(long characterId, DungeonAccumulationRecordType recordType, int value) {
        CharacterId = characterId;
        RecordType = recordType;
        Value = value;
    }
}
