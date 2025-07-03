using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class SkillCooldown : IByteSerializable {
    public readonly int SkillId;
    public readonly short Level;
    public int GroupId { get; init; }
    public long EndTick;
    public int Charges;

    public SkillCooldown(int skillId, short level) {
        SkillId = skillId;
        Level = level;
        GroupId = 0;
        Charges = 0;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(SkillId);
        writer.WriteInt(GroupId);
        writer.WriteInt((int) EndTick);
        writer.WriteInt(Charges);
    }
}
