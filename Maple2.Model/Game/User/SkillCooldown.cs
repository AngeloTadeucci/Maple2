using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class SkillCooldown(int skillId) : IByteSerializable {
    public readonly int SkillId = skillId;
    public int OriginSkillId { get; init; }
    public long EndTick;

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(SkillId);
        writer.WriteInt(OriginSkillId);
        writer.WriteInt((int) EndTick);
        writer.WriteInt(); // Unknown Tick. Origin Skill tick maybe?
    }
}
