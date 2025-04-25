using System.Collections.Concurrent;
using System.Numerics;
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
    public long ItemUid;

    public ConcurrentDictionary<int, IActor> Targets;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillRecord"/> class for a specific skill cast by an actor.
    /// </summary>
    /// <param name="metadata">The metadata describing the skill.</param>
    /// <param name="castUid">A unique identifier for this skill cast instance.</param>
    /// <param name="caster">The actor who is casting the skill.</param>
    public SkillRecord(SkillMetadata metadata, long castUid, IActor caster) {
        Metadata = metadata;
        CastUid = castUid;
        Caster = caster;
        Targets = [];
    }

    /// <summary>
    /// Attempts to set the current motion point index for the skill if the specified value is within the valid range.
    /// </summary>
    /// <param name="motionPoint">The motion point index to set.</param>
    /// <returns>True if the motion point was set successfully; otherwise, false.</returns>
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
