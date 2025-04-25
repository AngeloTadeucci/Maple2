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
using Maple2.Server.Core.Constants;
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
    public AnimationManager Animation { get; init; }
    public SkillState SkillState { get; init; }

    public virtual bool IsDead { get; protected set; }
    public abstract IPrism Shape { get; }
    public SkillQueue ActiveSkills { get; init; }

    public virtual BuffManager Buffs { get; }
    public Lua.Lua Lua { get; init; }

    /// <summary>
    /// Tick duration of actor in the same position.
    /// </summary>
    public (Vector3 Position, long LastTick, long Duration) PositionTick { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Actor{T}"/> class with the specified field, object ID, value, and NPC metadata.
    /// </summary>
    /// <param name="field">The field manager responsible for this actor's environment.</param>
    /// <param name="objectId">The unique identifier for the actor.</param>
    /// <param name="value">The value or entity represented by this actor.</param>
    /// <param name="npcMetadata">Metadata storage for NPC-related data.</param>
    protected Actor(FieldManager field, int objectId, T value, NpcMetadataStorage npcMetadata) {
        Field = field;
        ObjectId = objectId;
        Value = value;
        Buffs = new BuffManager(this);
        Transform = new Transform();
        NpcMetadata = npcMetadata;
        Animation = new AnimationManager(this);
        SkillState = new SkillState(this);
        Stats = new StatsManager(this);
        PositionTick = new ValueTuple<Vector3, long, long>(Vector3.Zero, 0, 0);
        Lua = new Lua.Lua(Target.LOCALE);
        ActiveSkills = new SkillQueue();
    }

    /// <summary>
    /// Releases resources used by the actor.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) { }

    /// <summary>
    /// Applies a skill effect from a caster to an owner, adding one or more buffs based on the effect's conditions.
    /// </summary>
    /// <param name="caster">The actor applying the effect.</param>
    /// <param name="owner">The actor receiving the effect.</param>
    /// <param name="effect">The skill effect metadata describing the buffs to apply.</param>
    /// <param name="startTick">The game tick when the effect starts.</param>
    /// <param name="type">The event condition type triggering the effect.</param>
    /// <param name="skillId">The skill ID associated with the effect.</param>
    /// <param name="buffId">The buff ID associated with the effect.</param>
    /// <param name="notifyField">Whether to notify the field of the buff application.</param>
    public virtual void ApplyEffect(IActor caster, IActor owner, SkillEffectMetadata effect, long startTick, EventConditionType type = EventConditionType.Activate, int skillId = 0, int buffId = 0, bool notifyField = true) {
        Debug.Assert(effect.Condition != null);

        if (effect.Condition.RandomCast) {
            int index = Random.Shared.Next(effect.Skills.Length);
            owner.Buffs.AddBuff(caster, owner, effect.Skills[index].Id, effect.Skills[index].Level, startTick, stacks: effect.Condition.OverlapCount, notifyField: notifyField, type: type);
        } else {
            foreach (SkillEffectMetadata.Skill skill in effect.Skills) {
                owner.Buffs.AddBuff(caster, owner, skill.Id, skill.Level, startTick, stacks: effect.Condition.OverlapCount, notifyField: notifyField, type: type);
            }
        }
    }

    /// <summary>
    /// Applies damage from a caster to this actor using the provided damage record and attack metadata, updating health, tracking damage sources, and triggering relevant buff events based on damage type.
    /// </summary>
    public virtual void ApplyDamage(IActor caster, DamageRecord damage, SkillMetadataAttack attack) {
        var targetRecord = new DamageRecordTarget(this) {
            Position = caster.Position,
            Direction = caster.Rotation, // Idk why this is wrong
        };
        damage.Targets.TryAdd(ObjectId, targetRecord);

        if (attack.Damage.Count <= 0) {
            return;
        }

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
                record = new DamageRecordTarget(this);
                DamageDealers.TryAdd(caster.ObjectId, record);
            }
            record.AddDamage(DamageType.Normal, positiveDamage);
            Stats.Values[BasicAttribute.Health].Add(damageAmount);
            Field.Broadcast(StatsPacket.Update(this, BasicAttribute.Health));
        }

        foreach ((DamageType damageType, long amount) in targetRecord.Damage) {
            switch (damageType) {
                case DamageType.Critical:
                    caster.Buffs.TriggerEvent(caster, caster, this, EventConditionType.OnOwnerAttackHit, skillId: damage.SkillId);
                    caster.Buffs.TriggerEvent(caster, caster, this, EventConditionType.OnOwnerAttackCrit, skillId: damage.SkillId);
                    this.Buffs.TriggerEvent(this, this, this, EventConditionType.OnAttacked, skillId: damage.SkillId);
                    break;
                case DamageType.Normal:
                    caster.Buffs.TriggerEvent(caster, caster, this, EventConditionType.OnOwnerAttackHit, skillId: damage.SkillId);
                    break;
                case DamageType.Block:
                    this.Buffs.TriggerEvent(this, this, this, EventConditionType.OnBlock, skillId: damage.SkillId);
                    break;
                case DamageType.Miss:
                    caster.Buffs.TriggerEvent(caster, caster, this, EventConditionType.OnAttackMiss, skillId: damage.SkillId);
                    this.Buffs.TriggerEvent(this, this, this, EventConditionType.OnEvade, skillId: damage.SkillId);
                    break;
            }
        }
    }

    /// <summary>
    /// Applies a reflect effect to the target actor if the reflect buff is active and within its usage limits.
    /// </summary>
    /// <param name="target">The actor to receive the reflected effect.</param>
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
            Buffs.Remove(record.SourceBuffId, ObjectId);
        }
        target.Buffs.AddBuff(this, target, record.Metadata.EffectId, record.Metadata.EffectLevel, Field.FieldTick);

        // TODO: Reflect should also amend the target's damage record from Reflect.ReflectValues and ReflectRates
    }

    /// <summary>
    /// Executes an attack on multiple targets, applying damage, broadcasting the results, and triggering associated skill effects.
    /// </summary>
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

        foreach (IActor target in record.Targets.Values) {
            target.ApplyDamage(this, damage, record.Attack);
        }

        Field.Broadcast(SkillDamagePacket.Damage(damage));


        ApplyEffects(record.Attack.Skills, record.Caster, this, skillId: record.SkillId, targets: record.Targets.Values.ToArray());
        ApplyEffects(record.Attack.SkillsOnDamage, record.Caster, damage, record.Targets.Values.ToArray());
        /*foreach (SkillEffectMetadata effect in record.Attack.Skills.Where(e => e.Splash != null)) {
            // This should not be sent on init skill use from PLAYER because a Splash skill packet is sent from client to server.
            // Field.AddSkill(record.Caster, effect, [record.Caster.Position], record.Caster.Rotation);
        }*/
    }

    /// <summary>
    /// Resolves and returns the appropriate actor based on the specified skill target type.
    /// </summary>
    /// <param name="targetType">The type of target to resolve (e.g., owner, caster, target, etc.).</param>
    /// <param name="caster">The actor casting the skill.</param>
    /// <param name="target">The primary target actor.</param>
    /// <param name="owner">The owner actor, if applicable.</param>
    /// <returns>The actor corresponding to the given target type.</returns>
    public virtual IActor GetTarget(SkillTargetType targetType, IActor caster, IActor target, IActor owner) {
        return targetType switch {
            SkillTargetType.Owner => owner,
            SkillTargetType.Target => target,
            SkillTargetType.Caster => caster,
            SkillTargetType.None => this, // Should be on self/inherit
            SkillTargetType.Attacker => target,
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Resolves and returns the owner actor based on the specified skill target type.
    /// </summary>
    /// <param name="targetType">The type of skill target to resolve.</param>
    /// <param name="caster">The actor casting the skill.</param>
    /// <param name="target">The primary target actor.</param>
    /// <param name="owner">The owner actor, if applicable.</param>
    /// <returns>The resolved owner actor according to the target type.</returns>
    public IActor GetOwner(SkillTargetType targetType, IActor caster, IActor target, IActor owner) {
        return targetType switch {
            SkillTargetType.Owner => owner,
            SkillTargetType.Target => owner,
            SkillTargetType.Caster => caster,
            SkillTargetType.None => this, // Should be on self/inherit
            SkillTargetType.Attacker => target,
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Filter effects before applying. This is needed instead of applying as iterating to ensure the current state of the actor is used.
    /// <summary>
    /// Applies skill effects to target actors based on specified conditions and event type.
    /// </summary>
    /// <param name="effects">Array of skill effect metadata to evaluate and apply.</param>
    /// <param name="caster">The actor casting the skill.</param>
    /// <param name="owner">The owner actor for effect resolution.</param>
    /// <param name="type">The event condition type that triggers effect application.</param>
    /// <param name="skillId">The skill ID associated with the effect, if applicable.</param>
    /// <param name="buffId">The buff ID associated with the effect, if applicable.</param>
    /// <param name="targets">Target actors to evaluate for effect application.</param>
    public virtual void ApplyEffects(SkillEffectMetadata[] effects, IActor caster, IActor owner, EventConditionType type = EventConditionType.Activate, int skillId = 0, int buffId = 0, params IActor[] targets) {
        var appliedEffects = new List<(IActor Owner, IActor Caster, SkillEffectMetadata Effect)>();
        long startTick = Field.FieldTick;

        foreach (SkillEffectMetadata effect in effects) {
            if (effect.Condition != null) {
                foreach (IActor target in targets) {
                    IActor resultOwner = GetTarget(effect.Condition!.Target, caster, target, owner);
                    IActor resultCaster = GetOwner(effect.Condition.Owner, caster, target, owner);
                    if (effect.Condition.Condition.Check(resultCaster, resultOwner, target, type, skillId, buffId)) {
                        appliedEffects.Add((resultOwner, resultCaster, effect));
                    }
                }
            }
        }

        foreach ((IActor effectOwner, IActor effectCaster, SkillEffectMetadata effect) in appliedEffects) {
            ApplyEffect(effectCaster, effectOwner, effect, startTick, type, skillId, buffId);
        }
    }

    /// <summary>
    /// Used for On damage
    /// </summary>
    /// <param name="effects"></param>
    /// <param name="caster"></param>
    /// <param name="record"></param>
    /// <summary>
    /// Applies skill effects to targets based on damage dealt, ensuring effects are only applied if the target received damage from the skill.
    /// </summary>
    /// <param name="effects">The skill effects to apply.</param>
    /// <param name="caster">The actor casting the skill.</param>
    /// <param name="record">The damage record containing attack and target information.</param>
    /// <param name="targets">The actors to potentially receive effects.</param>
    public virtual void ApplyEffects(SkillEffectMetadata[] effects, IActor caster, DamageRecord record, params IActor[] targets) {
        // Skill does no damage, apply effects regardless
        if (record.AttackMetadata.Damage.Count == 0) {
            ApplyEffects(effects, caster, caster, skillId: record.SkillId, targets: targets);
            return;
        }

        var appliedEffects = new List<(IActor Owner, IActor Caster, SkillEffectMetadata Effect)>();
        long startTick = Field.FieldTick;
        foreach (SkillEffectMetadata effect in effects) {
            if (effect.Condition != null) {
                foreach (IActor target in targets) {
                    IActor resultOwner = GetTarget(effect.Condition!.Target, caster, target, caster);
                    IActor resultCaster = GetOwner(effect.Condition.Owner, caster, target, caster);
                    if (effect.Condition.Condition.Check(resultCaster, resultOwner, target, eventSkillId: record.SkillId) &&
                        DealtDamage(target)) {
                        appliedEffects.Add((resultOwner, resultCaster, effect));
                    }
                }
            }
        }

        foreach ((IActor effectOwner, IActor effectCaster, SkillEffectMetadata effect) in appliedEffects) {
            ApplyEffect(effectCaster, effectOwner, effect, startTick, skillId: record.SkillId);
        }
        return;

        bool DealtDamage(IActor target) {
            return record.Targets.TryGetValue(target.ObjectId, out DamageRecordTarget? targetRecord) && targetRecord.Damage.Any(x => x.Amount > 0);
        }
    }

    /// <summary>
    /// Updates the actor's state for the current tick, handling death, position tracking, animation, and buffs.
    /// </summary>
    /// <param name="tickCount">The current server tick count.</param>
    public virtual void Update(long tickCount) {
        if (IsDead) return;

        if (Stats.Values[BasicAttribute.Health].Current <= 0) {
            IsDead = true;
            OnDeath();
            return;
        }

        if (PositionTick.Position != Position) {
            PositionTick = new ValueTuple<Vector3, long, long>(Position, tickCount, 0);
        } else {
            PositionTick = new ValueTuple<Vector3, long, long>(Position, PositionTick.LastTick, tickCount - PositionTick.LastTick);
        }

        Animation.Update(tickCount);
        Buffs.Update(tickCount);
    }

    public virtual void KeyframeEvent(string keyName) { }

    /// <summary>
    /// Attempts to cast a skill by its ID and level, creating and broadcasting a skill record if valid.
    /// </summary>
    /// <param name="id">The skill's unique identifier.</param>
    /// <param name="level">The level of the skill to cast.</param>
    /// <param name="uid">Optional unique identifier for the skill instance.</param>
    /// <param name="motionPoint">Optional motion point for the skill animation.</param>
    /// <returns>The created <see cref="SkillRecord"/> if the skill is valid and motion point is set; otherwise, null.</returns>
    public virtual SkillRecord? CastSkill(int id, short level, long uid = 0, byte motionPoint = 0) {
        if (!Field.SkillMetadata.TryGet(id, level, out SkillMetadata? metadata)) {
            Logger.Error("Invalid skill use: {SkillId},{Level}", id, level);
            return null;
        }

        var record = new SkillRecord(metadata, uid, this) {
            Position = Position,
            Rotation = Rotation,
            Rotate2Z = 2 * Rotation.Z,
        };

        if (!record.TrySetMotionPoint(motionPoint)) {
            return null;
        }

        Field.Broadcast(SkillPacket.Use(record));

        return record;
    }

    /// <summary>
    /// Invoked when the actor dies, triggering all buffs' OnDeath events for this actor.
    /// </summary>
    protected virtual void OnDeath() {
        Buffs.TriggerEvent(this, this, this, EventConditionType.OnDeath);
    }

    /// <summary>
    /// Consumes needed stats to cast skill.
    /// <summary>
    /// Determines whether the actor can consume the required resources to cast the specified skill.
    /// </summary>
    /// <param name="record">The skill record representing the skill to be cast.</param>
    /// <returns>True if the skill can be cast; otherwise, false.</returns>
    public virtual bool SkillCastConsume(SkillRecord record) {
        return true;
    }
}
