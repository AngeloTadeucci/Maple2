using System.Collections.Generic;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class SkillTab(string name) : IByteSerializable, IByteDeserializable {
    public long Id;
    public string Name = name;
    public Dictionary<int, int> Skills = new();

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteUnicodeString(Name);

        writer.WriteInt(Skills.Count);
        foreach ((int skillId, int points) in Skills) {
            writer.WriteInt(skillId);
            writer.WriteInt(points);
        }
    }

    public void ReadFrom(IByteReader reader) {
        Id = reader.ReadLong();
        Name = reader.ReadUnicodeString();
        Skills = new Dictionary<int, int>();

        int count = reader.ReadInt();
        for (int i = 0; i < count; i++) {
            int skillId = reader.ReadInt();
            int points = reader.ReadInt();
            Skills.Add(skillId, points);
        }
    }
}
