using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class SkillCooldown : IByteSerializable {
    public readonly int SkillId;
    public int OriginSkillId { get; init; }
    public long EndTick;

    public SkillCooldown(int skillId) {
        SkillId = skillId;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(SkillId);
        writer.WriteInt(OriginSkillId);
        writer.WriteInt((int) EndTick);
        writer.WriteInt(); // Unknown Tick. Origin Skill tick maybe?
    }
}
