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

    public virtual void ApplyEffect(IActor caster, IActor owner, SkillEffectMetadata effect, long startTick, EventConditionType type = EventConditionType.Activate, int skillId = 0, int buffId = 0, bool notifyField = true) { }
    public virtual void ApplyDamage(IActor caster, DamageRecord damage, SkillMetadataAttack attack) { }

    public virtual void TargetAttack(SkillRecord record) { }

    public virtual SkillRecord? CastSkill(int id, short level, long uid = 0, byte motionPoint = 0) { return null; }
    public virtual void KeyframeEvent(string keyName) { }
}

public interface IActor<out T> : IActor {
    public T Value { get; }
}
