using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Network;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Manager.Items;
using Maple2.Server.Game.Model.Skill;
using Maple2.Tools.VectorMath;
using Maple2.Server.Game.Packets;
using Maple2.Tools.Collision;
using Serilog;
using Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;
using Maple2.Database.Storage;
using Maple2.Server.Core.Formulas;
using Maple2.Server.Game.Manager;
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
            (DamageType, double) damageHit = GetDamage(caster, damage);
            targetRecord.AddDamage(damageHit.Item1, (long) damageHit.Item2);
            damageAmount -= (long) damageHit.Item2;
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
        target.Buffs.AddBuff(this, target, record.Metadata.EffectId, record.Metadata.EffectLevel);

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

        foreach (SkillEffectMetadata effect in record.Attack.Skills) {
            if (effect.Condition != null) {
                foreach (IActor actor in record.Targets) {
                    IActor caster = GetTarget(effect.Condition.Target, record.Caster, actor);
                    IActor owner = GetTarget(effect.Condition.Target, record.Caster, actor);
                    if (effect.Condition.Condition.Check(caster, owner, actor)) {
                        actor.ApplyEffect(caster, owner, effect);
                    }
                }
            } else if (effect.Splash != null) {
                Field.AddSkill(record.Caster, effect, [
                    record.Caster.Position,
                ], record.Caster.Rotation);
            }
        }
    }

    public virtual void ApplyEffect(IActor caster, IActor target, SkillEffectMetadata effect) {
        foreach (SkillEffectMetadata.Skill skill in effect.Skills) {
            Buffs.AddBuff(caster, target, skill.Id, skill.Level);
        }
    }

    public (DamageType, long) GetDamage(IActor caster, DamageRecord record) {
        // Check block
        float blockRate = Buffs.TotalCompulsionRate(CompulsionEventType.BlockChance, record.SkillId);
        if (blockRate >= 1 || (blockRate > 0 && Random.Shared.NextDouble() <= blockRate)) {
            return (DamageType.Block, 0);
        }

        // Check evade
        float evadeRate = Buffs.TotalCompulsionRate(CompulsionEventType.EvasionChanceOverride, record.SkillId);
        if (evadeRate > 0 && Random.Shared.NextDouble() <= evadeRate) {
            return (DamageType.Miss, 0);
        }

        // Get hit rate
        double hitRate = Damage.HitRate(this.Buffs.GetResistance(BasicAttribute.Accuracy), this.Stats.Values[BasicAttribute.Evasion].Total, this.Stats.Values[BasicAttribute.Dexterity].Total,
            caster.Stats.Values[BasicAttribute.Accuracy].Total, caster.Buffs.GetResistance(BasicAttribute.Evasion), caster.Stats.Values[BasicAttribute.Luck].Total);

        if (Random.Shared.NextDouble() > hitRate) {
            return (DamageType.Miss, 0);
        }

        var damageType = DamageType.Normal;
        (double, double) casterBonusAttackDamage = caster.Stats.GetBonusAttack(Buffs.GetResistance(BasicAttribute.BonusAtk), this.Buffs.GetResistance(BasicAttribute.MaxWeaponAtk));
        double attackDamage = casterBonusAttackDamage.Item1 + (casterBonusAttackDamage.Item2 - casterBonusAttackDamage.Item1) * Random.Shared.NextDouble();

        // change the NPCNormalDamage to be changed depending on target?
        double damageBonus = 1 + (caster.Stats.Values[SpecialAttribute.TotalDamage].Multiplier()) + (caster.Stats.Values[SpecialAttribute.NormalNpcDamage].Multiplier());
        if (this is FieldNpc npc && npc.Value.IsBoss) {
            damageBonus += caster.Stats.Values[SpecialAttribute.BossNpcDamage].Multiplier();
        }

        // Get elemental bonus
        switch (record.SkillMetadata.Property.Element) {
            case Element.Fire:
                damageBonus += caster.Stats.Values[SpecialAttribute.FireDamage].Multiplier();
                break;
            case Element.Ice:
                damageBonus += caster.Stats.Values[SpecialAttribute.IceDamage].Multiplier();
                break;
            case Element.Electric:
                damageBonus += caster.Stats.Values[SpecialAttribute.ElectricDamage].Multiplier();
                break;
            case Element.Holy:
                damageBonus += caster.Stats.Values[SpecialAttribute.HolyDamage].Multiplier();
                break;
            case Element.Dark:
                damageBonus += caster.Stats.Values[SpecialAttribute.DarkDamage].Multiplier();
                break;
            case Element.Poison:
                damageBonus += caster.Stats.Values[SpecialAttribute.PoisonDamage].Multiplier();
                break;
        }

        // Get range bonus
        switch (record.SkillMetadata.Property.RangeType) {
            case RangeType.None:
                break;
            case RangeType.Melee:
                damageBonus += caster.Stats.Values[SpecialAttribute.MeleeDamage].Multiplier();
                break;
            case RangeType.Range:
                damageBonus += caster.Stats.Values[SpecialAttribute.RangedDamage].Multiplier();
                break;
        }

        damageBonus += -this.Buffs.GetResistance(BasicAttribute.AttackSpeed) * caster.Stats.Values[BasicAttribute.AttackSpeed].Total;

        // Check for crit and get crit damage
        /* TODO: DotDamage can be be NOT crit. Player skills yes. Will need to implement this */
        if (record.AttackMetadata.CompulsionTypes.Contains(CompulsionType.Critical)) {
            damageType = DamageType.Critical;
        }

        if (damageType != DamageType.Critical) {
            damageType = caster.Stats.GetCriticalRate(this.Stats.Values[BasicAttribute.CriticalEvasion].Total, caster.Buffs.TotalCompulsionRate(CompulsionEventType.CritChanceOverride, record.SkillId));
        }

        damageBonus *= damageType == DamageType.Critical ? caster.Stats.GetCriticalDamage(this.Buffs.GetResistance(BasicAttribute.CriticalDamage)) : 1;

        // Get invoked buff values
        // TODO: Need to make this flexible to get invoke values from skills or buffs. If buff -> InvokeEffectType.IncreaseDotDamage
        (int, float) invokeValues = caster.Buffs.GetInvokeValues(InvokeEffectType.IncreaseSkillDamage, record.SkillId, record.SkillMetadata.Property.SkillGroup);

        double damageMultiplier = damageBonus * (1 + invokeValues.Item2) * (record.AttackMetadata.Damage.Rate + invokeValues.Item1);

        double defensePierce = 1 - Math.Min(0.3, (1 / (1 + this.Buffs.GetResistance(BasicAttribute.Piercing)) * (caster.Stats.Values[BasicAttribute.Piercing].Multiplier() - 1)));
        damageMultiplier *= 1 / (Math.Max(this.Stats.Values[BasicAttribute.Defense].Total, 1) * defensePierce);

        // Check resistances
        double attackTypeAmount = 0;
        double resistance = 0;
        double finalDamage = 0;
        switch (record.SkillMetadata.Property.AttackType) {
            case AttackType.Physical:
                resistance = Damage.CalculateResistance(this.Stats.Values[BasicAttribute.PhysicalRes].Total, caster.Stats.Values[SpecialAttribute.PhysicalPiercing].Multiplier());
                attackTypeAmount = caster.Stats.Values[BasicAttribute.PhysicalAtk].Total;
                finalDamage = caster.Stats.Values[SpecialAttribute.OffensivePhysicalDamage].Multiplier();
                break;
            case AttackType.Magic:
                resistance = Damage.CalculateResistance(this.Stats.Values[BasicAttribute.MagicalRes].Total, caster.Stats.Values[SpecialAttribute.MagicalPiercing].Multiplier());
                attackTypeAmount = caster.Stats.Values[BasicAttribute.MagicalAtk].Total;
                finalDamage = caster.Stats.Values[SpecialAttribute.OffensiveMagicalDamage].Multiplier();
                break;
            // If all, calculate the higher of the two
            case AttackType.All:
                BasicAttribute attackTypeAttribute = caster.Stats.Values[BasicAttribute.PhysicalAtk].Total >= caster.Stats.Values[BasicAttribute.MagicalAtk].Total
                    ? BasicAttribute.PhysicalAtk
                    : BasicAttribute.MagicalAtk;
                SpecialAttribute piercingAttribute = attackTypeAttribute == BasicAttribute.PhysicalAtk
                    ? SpecialAttribute.PhysicalPiercing
                    : SpecialAttribute.MagicalPiercing;
                SpecialAttribute finalDamageAttribute = attackTypeAttribute == BasicAttribute.PhysicalAtk
                    ? SpecialAttribute.OffensivePhysicalDamage
                    : SpecialAttribute.OffensiveMagicalDamage;
                attackTypeAmount = Math.Max(caster.Stats.Values[BasicAttribute.PhysicalAtk].Total, caster.Stats.Values[BasicAttribute.MagicalAtk].Total) * 0.5f;
                resistance = Damage.CalculateResistance(this.Stats.Values[attackTypeAttribute].Total, caster.Stats.Values[piercingAttribute].Multiplier());
                finalDamage = caster.Stats.Values[finalDamageAttribute].Multiplier();
                break;
        }

        damageMultiplier *= attackTypeAmount * resistance * (finalDamage == 0 ? 1 : finalDamage);
        attackDamage *= damageMultiplier * Constant.AttackDamageFactor + record.AttackMetadata.Damage.Value;

        // Apply any shields
        foreach (Buff buff in Buffs.Buffs.Values.Where(buff => buff.Metadata.Shield != null)) {
            if (buff.ShieldHealth >= attackDamage) {
                buff.ShieldHealth -= (long) attackDamage;
                attackDamage = 0;
                Field.Broadcast(BuffPacket.Update(buff, BuffFlag.UpdateBuff));
                return (DamageType.Block, (long) attackDamage);
            }

            attackDamage -= buff.ShieldHealth;
            Buffs.Remove(buff.Id);
        }

        return (damageType, (long) attackDamage);
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
