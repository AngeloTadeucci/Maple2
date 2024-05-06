using System.Collections.Generic;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class SkillMacro(string name, long keyId, HashSet<int>? skills = null) : IByteSerializable, IByteDeserializable {
    public string Name { get; private set; } = name;
    public long KeyId { get; private set; } = keyId;
    public IReadOnlyCollection<int> Skills => skills;

    private HashSet<int> skills = skills ?? [];

    public void WriteTo(IByteWriter writer) {
        writer.WriteUnicodeString(Name);
        writer.WriteLong(KeyId);
        writer.WriteInt(Skills.Count);

        foreach (int skillId in Skills) {
            writer.WriteInt(skillId);
        }
    }

    public void ReadFrom(IByteReader reader) {
        skills = []; // Clear any existing settings

        Name = reader.ReadUnicodeString();
        KeyId = reader.ReadLong();
        int count = reader.ReadInt();
        for (int i = 0; i < count; i++) {
            skills.Add(reader.ReadInt());
        }
    }
}
