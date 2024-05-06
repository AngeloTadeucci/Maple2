using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class Quest(QuestMetadata metadata) : IByteSerializable {
    public int Id => Metadata.Id;
    public readonly QuestMetadata Metadata = metadata;

    public QuestState State;
    public int CompletionCount;
    public long StartTime;
    public long EndTime;
    public bool Track;
    public SortedDictionary<int, Condition> Conditions = new();

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.Write<QuestState>(State);
        writer.WriteInt(CompletionCount);
        writer.WriteLong(StartTime);
        writer.WriteLong(EndTime);
        writer.WriteBool(Track);

        writer.WriteInt(Conditions.Count);
        foreach (Condition condition in Conditions.Values) {
            writer.WriteInt(condition.Counter);
        }
    }

    public class Condition(ConditionMetadata metadata) {
        public readonly ConditionMetadata Metadata = metadata;
        public int Counter;

    }
}
