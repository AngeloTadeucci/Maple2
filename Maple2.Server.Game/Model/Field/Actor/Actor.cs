using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;
using Maple2.Tools.Collision;
using Serilog;

namespace Maple2.Server.Game.Model;

/// <summary>
/// Actor is an entity that can engage in combat.
/// </summary>
/// <typeparam name="T">The type contained by this object</typeparam>
public abstract class Actor<T> : IActor<T>, IDisposable {

    protected readonly ILogger Logger = Log.ForContext<T>();

    public FieldManager Field { get; }
    public T Value { get; }

    public virtual Stats Stats { get; } = new(0, 0);

    protected readonly ConcurrentDictionary<int, long> DamageDealers = new();

    public int ObjectId { get; }
    public virtual Vector3 Position { get; set; }
    public virtual Vector3 Rotation { get; set; }

    public virtual bool IsDead { get; protected set; }
    public abstract IPrism Shape { get; }

    public BuffManager Buffs { get; }

    protected Actor(FieldManager field, int objectId, T value) {
        Field = field;
        ObjectId = objectId;
        Value = value;
        Buffs = new BuffManager(this);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) { }

    public virtual void ApplyEffect(IActor caster, IActor owner, SkillEffectMetadata effect, bool notifyField = true) {
        Debug.Assert(effect.Condition != null);

        foreach (SkillEffectMetadata.Skill skill in effect.Skills) {
            Buffs.AddBuff(caster, owner, skill.Id, skill.Level, notifyField);
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
            targetRecord.AddDamage(DamageType.Normal, -2000);
            damageAmount -= 2000;
        }

        if (damageAmount != 0) {
            long positiveDamage = damageAmount * -1;
            DamageDealers.AddOrUpdate(caster.ObjectId, positiveDamage, (_, current) => current + positiveDamage);
            Stats[BasicAttribute.Health].Add(damageAmount);
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
        target.Buffs.AddBuff(this, target, record.Metadata.EffectId, record.Metadata.EffectLevel);

        // TODO: Reflect should also amend the target's damage record from Reflect.ReflectValues and ReflectRates
    }

    public virtual void TargetAttack(SkillRecord record) {
        if (record.Targets.Count == 0) {
            return;
        }

        var damage = new DamageRecord {
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

        foreach (SkillEffectMetadata effect in record.Attack.Skills) {
            if (effect.Condition != null) {
                foreach (IActor actor in record.Targets) {
                    actor.ApplyEffect(record.Caster, this, effect);
                }
            } else if (effect.Splash != null) {
                // Handled by SplashAttack?
            }
        }
    }

    public virtual void Update(long tickCount) {
        if (IsDead) return;

        if (Stats[BasicAttribute.Health].Current <= 0) {
            IsDead = true;
            OnDeath();
            return;
        }

        Buffs.Update(tickCount);
    }

    protected abstract void OnDeath();
}
