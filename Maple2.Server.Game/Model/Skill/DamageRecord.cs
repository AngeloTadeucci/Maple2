using System.Collections.Concurrent;
using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Model.Skill;

public class DamageRecord {
    public readonly SkillMetadata SkillMetadata;
    public readonly SkillMetadataAttack AttackMetadata;
    public readonly DamagePropertyRecord Properties;
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

    public readonly ConcurrentDictionary<int, DamageRecordTarget> Targets; // Target's ObjectId as key

    public DamageRecord(SkillMetadata skillMetadata, SkillMetadataAttack attackMetadata) {
        SkillMetadata = skillMetadata;
        AttackMetadata = attackMetadata;
        Targets = [];
        Properties = new DamagePropertyRecord {
            Element = skillMetadata.Property.Element,
            SkillId = skillMetadata.Id,
            SkillGroup = skillMetadata.Property.SkillGroup,
            RangeType = skillMetadata.Property.RangeType,
            AttackType = skillMetadata.Property.AttackType,
            CompulsionTypes = attackMetadata.CompulsionTypes,
            Rate = attackMetadata.Damage.Rate,
            Value = attackMetadata.Damage.Value,
        };
    }
}

public class DamageRecordTarget {
    public readonly IActor Target;
    public int ObjectId => Target.ObjectId;
    public Vector3 Position;
    public Vector3 Direction;

    private readonly List<(DamageType, long)> damage;
    public IReadOnlyList<(DamageType Type, long Amount)> Damage => damage;

    public DamageRecordTarget(IActor target) {
        Target = target;
        damage = [];
    }

    public void AddDamage(DamageType type, long amount) {
        damage.Add((type, amount));
    }
}
