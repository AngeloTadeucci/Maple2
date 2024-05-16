using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Model.Skill;

public class SkillRecord {
    public readonly SkillMetadata Metadata;
    public SkillMetadataMotion Motion => Metadata.Data.Motions[MotionPoint];
    public SkillMetadataAttack Attack => Motion.Attacks[AttackPoint];

    public int SkillId => Metadata.Id;
    public short Level => Metadata.Level;

    public readonly long CastUid;
    public long TargetUid;
    public readonly IActor Caster;

    public int ServerTick;
    public byte MotionPoint { get; private set; }
    public byte AttackPoint { get; private set; }

    public Vector3 Position;
    public Vector3 ImpactPosition;
    public Vector3 Direction; // Sometimes Rotation
    public Vector3 Rotation;
    public float Rotate2Z;
    public bool Unknown;

    public bool IsHold;
    public int HoldInt;
    public string HoldString = string.Empty;

    public IList<IActor> Targets;

    public SkillRecord(SkillMetadata metadata, long castUid, IActor caster) {
        Metadata = metadata;
        CastUid = castUid;
        Caster = caster;
        Targets = new List<IActor>();
    }

    public int GetSpCost() {
        if (Metadata.Data.Consume.Stat.TryGetValue(BasicAttribute.Spirit, out long spiritCost)) {
            return (int) spiritCost;
        }
        return 0;
    }

    public int GetStaCost() {
        if (Metadata.Data.Consume.Stat.TryGetValue(BasicAttribute.Stamina, out long staminaCost)) {
            return (int) staminaCost;
        }
        return 0;
    }

    public bool TrySetMotionPoint(byte motionPoint) {
        if (Metadata.Data.Motions.Length <= motionPoint) {
            return false;
        }

        MotionPoint = motionPoint;
        return true;
    }

    public bool TrySetAttackPoint(byte attackPoint) {
        if (Motion.Attacks.Length <= attackPoint) {
            return false;
        }

        AttackPoint = attackPoint;
        return true;
    }

    public override string ToString() {
        return $"Uid:{CastUid}, SkillId:{SkillId}, Level:{Level}, MotionPoint:{MotionPoint}, AttackPoint:{AttackPoint}\n"
               + $"- Position:{Position}\n"
               + $"- Rotation:{Rotation}\n"
               + $"- Direction:{Direction}";
    }
}
