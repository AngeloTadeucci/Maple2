using System.Numerics;
using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Model.Skill;

public class SkillRecord(SkillMetadata metadata, long castUid, IActor caster) {
    public readonly SkillMetadata Metadata = metadata;
    public SkillMetadataMotion Motion => Metadata.Data.Motions[MotionPoint];
    public SkillMetadataAttack Attack => Motion.Attacks[AttackPoint];

    public int SkillId => Metadata.Id;
    public short Level => Metadata.Level;

    public readonly long CastUid = castUid;
    public long TargetUid;
    public readonly IActor Caster = caster;

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

    public IList<IActor> Targets = new List<IActor>();

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
