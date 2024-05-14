using System.Numerics;
using Maple2.Model.Enum;

namespace Maple2.Server.Game.Model.Skill;

public class DamageRecord {
    public long SkillUid { get; init; }
    public long TargetUid { get; init; } // For Non-Region skills
    public int CasterId { get; init; }

    public int OwnerId;
    public int SkillId;
    public short Level;
    public byte MotionPoint;
    public byte AttackPoint;

    public Vector3 Position;
    public Vector3 Direction;

    public readonly List<DamageRecordTarget> Targets;

    public DamageRecord() {
        Targets = [];
    }
}

public class DamageRecordTarget {
    public int ObjectId { get; init; }
    public Vector3 Position;
    public Vector3 Direction;

    private readonly List<(DamageType, long)> damage;
    public IReadOnlyList<(DamageType Type, long Amount)> Damage => damage;

    public DamageRecordTarget() {
        damage = new List<(DamageType Type, long Amount)>();
    }

    public void AddDamage(DamageType type, long amount) {
        damage.Add((type, amount));
    }
}
