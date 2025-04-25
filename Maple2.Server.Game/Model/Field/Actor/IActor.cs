using System.Collections.Concurrent;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Model.ActorStateComponent;
using Maple2.Server.Game.Model.Skill;
using Maple2.Tools.Collision;

namespace Maple2.Server.Game.Model;

public interface IActor : IFieldEntity {
    protected static readonly ConcurrentDictionary<int, Buff> NoBuffs = new();
    public NpcMetadataStorage? NpcMetadata { get; init; }
    public BuffManager Buffs { get; }

    public StatsManager Stats { get; }
    public AnimationManager Animation { get; }
    public SkillState SkillState { get; init; }

    public bool IsDead { get; }
    public IPrism Shape { get; }
    public SkillQueue ActiveSkills { get; init; }
    /// <summary>
/// Applies multiple skill effects to this actor, with optional context for caster, owner, event condition, skill and buff identifiers, and target actors.
/// </summary>
/// <param name="effects">The array of skill effects to apply.</param>
/// <param name="caster">The actor casting the effects.</param>
/// <param name="owner">The owner actor associated with the effects.</param>
/// <param name="type">The event condition type triggering the effects.</param>
/// <param name="skillId">The skill identifier associated with the effects.</param>
/// <param name="buffId">The buff identifier associated with the effects.</param>
/// <param name="targets">Optional target actors to which the effects are applied.</param>
public virtual void ApplyEffects(SkillEffectMetadata[] effects, IActor caster, IActor owner, EventConditionType type = EventConditionType.Activate, int skillId = 0, int buffId = 0, params IActor[] targets) { }
    /// <summary>
/// Applies multiple skill effects to the actor using the provided damage record and optional target actors.
/// </summary>
/// <param name="effects">An array of skill effect metadata to apply.</param>
/// <param name="caster">The actor casting the effects.</param>
/// <param name="record">The damage record associated with the effects.</param>
/// <param name="targets">Optional target actors to receive the effects.</param>
public virtual void ApplyEffects(SkillEffectMetadata[] effects, IActor caster, DamageRecord record, params IActor[] targets) { }
    /// <summary>
/// Applies a single skill effect to the actor, specifying the caster, owner, effect metadata, timing, and related identifiers.
/// </summary>
/// <param name="caster">The actor applying the effect.</param>
/// <param name="owner">The owner of the effect, if different from the caster.</param>
/// <param name="effect">Metadata describing the skill effect to apply.</param>
/// <param name="startTick">The game tick when the effect starts.</param>
/// <param name="type">The event condition type triggering the effect.</param>
/// <param name="skillId">The ID of the skill associated with the effect.</param>
/// <param name="buffId">The ID of the buff associated with the effect.</param>
/// <param name="notifyField">Whether to notify the field of the effect application.</param>
public virtual void ApplyEffect(IActor caster, IActor owner, SkillEffectMetadata effect, long startTick, EventConditionType type = EventConditionType.Activate, int skillId = 0, int buffId = 0, bool notifyField = true) { }
    /// <summary>
/// Applies damage to the actor based on the provided damage record and attack metadata.
/// </summary>
/// <param name="caster">The actor inflicting the damage.</param>
/// <param name="damage">Details of the damage to apply.</param>
/// <param name="attack">Metadata describing the attack causing the damage.</param>
public virtual void ApplyDamage(IActor caster, DamageRecord damage, SkillMetadataAttack attack) { }
    /// <summary>
/// Resolves and returns the target actor for a skill based on the specified target type and context.
/// </summary>
/// <param name="targetType">The type of target selection for the skill.</param>
/// <param name="caster">The actor casting the skill.</param>
/// <param name="target">The initially intended target actor.</param>
/// <param name="owner">The owner actor, if applicable.</param>
/// <returns>The resolved target actor. Default implementation returns the current actor instance.</returns>
public virtual IActor GetTarget(SkillTargetType targetType, IActor caster, IActor target, IActor owner) { return this; }
    /// <summary>
/// Resolves and returns the owner actor for a skill action based on the specified target type and context.
/// </summary>
/// <param name="targetType">The type of skill target to consider when determining the owner.</param>
/// <param name="caster">The actor casting the skill.</param>
/// <param name="target">The intended target actor.</param>
/// <param name="owner">The actor designated as the owner in the skill context.</param>
/// <returns>The resolved owner actor for the skill action. By default, returns the current actor instance.</returns>
public virtual IActor GetOwner(SkillTargetType targetType, IActor caster, IActor target, IActor owner) { return this; }

    /// <summary>
/// Processes an attack on a target using the specified skill record.
/// </summary>
public virtual void TargetAttack(SkillRecord record) { }

    public virtual SkillRecord? CastSkill(int id, short level, long uid = 0, byte motionPoint = 0) { return null; }
    public virtual void KeyframeEvent(string keyName) { }
}

public interface IActor<out T> : IActor {
    public T Value { get; }
}
