using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Skill;
using Maple2.Tools.VectorMath;
using Maple2.Server.Game.Packets;
using Maple2.Tools.Collision;
using Serilog;
using Maple2.Database.Storage;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Model.ActorStateComponent;
using Maple2.Server.Game.Util;

namespace Maple2.Server.Game.Model;

/// <summary>
/// Actor is an entity that can engage in combat.
/// </summary>
/// <typeparam name="T">The type contained by this object</typeparam>
public abstract class Actor<T> : IActor<T>, IDisposable {

    protected readonly ILogger Logger = Log.ForContext<T>();
    public NpcMetadataStorage NpcMetadata { get; init; }

    public FieldManager Field { get; }
    public T Value { get; }

    public virtual StatsManager Stats { get; }

    protected readonly ConcurrentDictionary<int, DamageRecordTarget> DamageDealers = new();

    public int ObjectId { get; }
    public virtual Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
    public virtual Vector3 Rotation {
        get => Transform.RotationAnglesDegrees;
        set => Transform.RotationAnglesDegrees = value;
    }
    public Transform Transform { get; init; }
    public AnimationState AnimationState { get; init; }
    public SkillState SkillState { get; init; }

    public virtual bool IsDead { get; protected set; }
    public abstract IPrism Shape { get; }

    public virtual BuffManager Buffs { get; }

    protected Actor(FieldManager field, int objectId, T value, string modelName, NpcMetadataStorage npcMetadata) {
        Field = field;
        ObjectId = objectId;
        Value = value;
        Buffs = new BuffManager(this);
        Transform = new Transform();
        NpcMetadata = npcMetadata;
        AnimationState = new AnimationState(this, modelName);
        SkillState = new SkillState(this);
        Stats = new StatsManager(this);
    }

    protected Actor(FieldManager field, int objectId, T value, string modelName) {
        Field = field;
        ObjectId = objectId;
        Value = value;
        Buffs = new BuffManager(this);
        Transform = new Transform();
        AnimationState = new AnimationState(this, modelName);
        SkillState = new SkillState(this);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) { }

    public virtual void ApplyEffect(IActor caster, IActor owner, SkillEffectMetadata effect, bool notifyField = true) {
        Debug.Assert(effect.Condition != null);

        foreach (SkillEffectMetadata.Skill skill in effect.Skills) {
            Buffs.AddBuff(caster, owner, skill.Id, skill.Level, Field.FieldTick, notifyField: notifyField);
        }
    }

    public virtual void ApplyDamage(IActor caster, DamageRecord damage, SkillMetadataAttack attack) {
        if (attack.Damage.Count <= 0) {
            return;
        }

        var targetRecord = new DamageRecordTarget {
            ObjectId = ObjectId,
            Position = caster.Position,
            Direction = caster.Rotation, // Idk why this is wrong
        };

        long damageAmount = 0;
        for (int i = 0; i < attack.Damage.Count; i++) {
            Reflect(caster);
            if (attack.Damage.IsConstDamage) {
                targetRecord.AddDamage(DamageType.Normal, attack.Damage.Value);
                damageAmount -= attack.Damage.Value;
            } else {
                (DamageType damageTypeResult, double damageResult) = DamageCalculator.CalculateDamage(caster, this, damage.Properties);
                targetRecord.AddDamage(damageTypeResult, (long) damageResult);
                damageAmount -= (long) damageResult;
            }
        }

        if (damageAmount != 0) {
            long positiveDamage = damageAmount * -1;
            if (!DamageDealers.TryGetValue(caster.ObjectId, out DamageRecordTarget? record)) {
                record = new DamageRecordTarget();
                DamageDealers.TryAdd(caster.ObjectId, record);
            }
            record.AddDamage(DamageType.Normal, positiveDamage);
            Stats.Values[BasicAttribute.Health].Add(damageAmount);
            Field.Broadcast(StatsPacket.Update(this, BasicAttribute.Health));
        }

        damage.Targets.Add(targetRecord);
    }

    public virtual void Reflect(IActor target) {
        if (Buffs.Reflect == null || Buffs.Reflect.Counter >= Buffs.Reflect.Metadata.Count) {
            return;
        }
        ReflectRecord record = Buffs.Reflect;

        if (record.Metadata.Rate is not 1 && record.Metadata.Rate < Random.Shared.NextDouble()) {
            return;
        }

        record.Counter++;
        if (record.Counter >= record.Metadata.Count) {
            Buffs.Remove(record.SourceBuffId);
        }
        target.Buffs.AddBuff(this, target, record.Metadata.EffectId, record.Metadata.EffectLevel, Field.FieldTick);

        // TODO: Reflect should also amend the target's damage record from Reflect.ReflectValues and ReflectRates
    }

    public virtual void TargetAttack(SkillRecord record) {
        if (record.Targets.Count == 0) {
            return;
        }

        var damage = new DamageRecord(record.Metadata, record.Attack) {
            CasterId = record.Caster.ObjectId,
            TargetUid = record.TargetUid,
            OwnerId = record.Caster.ObjectId,
            SkillId = record.SkillId,
            Level = record.Level,
            AttackPoint = record.AttackPoint,
            MotionPoint = record.MotionPoint,
            Position = record.ImpactPosition,
            Direction = record.Direction,
        };

        foreach (IActor target in record.Targets) {
            target.ApplyDamage(this, damage, record.Attack);
        }

        Field.Broadcast(SkillDamagePacket.Damage(damage));

        long startTick = Field.FieldTick;
        foreach (SkillEffectMetadata effect in record.Attack.Skills) {
            if (effect.Condition != null) {
                foreach (IActor actor in record.Targets) {
                    IActor owner = GetTarget(effect.Condition.Target, record.Caster, actor);
                    if (effect.Condition.Condition.Check(record.Caster, owner, actor)) {
                        actor.ApplyEffect(record.Caster, owner, effect, startTick);
                    }
                }
            } else if (effect.Splash != null) {
                Field.AddSkill(record.Caster, effect, [
                    record.Caster.Position,
                ], record.Caster.Rotation);
            }
        }
    }

    public virtual void ApplyEffect(IActor caster, IActor target, SkillEffectMetadata effect, long startTick) {
        foreach (SkillEffectMetadata.Skill skill in effect.Skills) {
            Buffs.AddBuff(caster, target, skill.Id, skill.Level, startTick);
        }
    }

    private IActor GetTarget(SkillEntity entity, IActor caster, IActor target) {
        return entity switch {
            SkillEntity.Target => target,
            SkillEntity.Owner => target,
            SkillEntity.Caster => caster,
            _ => throw new NotImplementedException(),
        };
    }

    public virtual void Update(long tickCount) {
        if (IsDead) return;

        if (Stats.Values[BasicAttribute.Health].Current <= 0) {
            IsDead = true;
            OnDeath();
            return;
        }

        AnimationState.Update(tickCount);
        Buffs.Update(tickCount);
    }

    public virtual void KeyframeEvent(string keyName) { }

    public virtual SkillRecord? CastSkill(int id, short level, long uid = 0, byte motionPoint = 0) {
        if (!Field.SkillMetadata.TryGet(id, level, out SkillMetadata? metadata)) {
            Logger.Error("Invalid skill use: {SkillId},{Level}", id, level);
            return null;
        }

        var record = new SkillRecord(metadata, uid, this);
        record.Position = Position;
        record.Rotation = Rotation;
        record.Rotate2Z = 2 * Rotation.Z;

        if (!record.TrySetMotionPoint(motionPoint)) {
            return null;
        }

        Field.Broadcast(SkillPacket.Use(record));

        return record;
    }

    protected abstract void OnDeath();
}
