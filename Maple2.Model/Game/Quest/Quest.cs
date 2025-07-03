using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class Quest : IByteSerializable {
    public int Id => Metadata.Id;
    public readonly QuestMetadata Metadata;

    public QuestState State;
    public int CompletionCount;
    public long StartTime;
    public long EndTime;
    public bool Track;
    public SortedDictionary<int, Condition> Conditions;

    public Quest(QuestMetadata metadata) {
        Metadata = metadata;
        Conditions = new SortedDictionary<int, Condition>();
    }

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

    public class Condition {
        public readonly ConditionMetadata Metadata;
        public int Counter;

        public Condition(ConditionMetadata metadata) {
            Metadata = metadata;
        }
    }
}
