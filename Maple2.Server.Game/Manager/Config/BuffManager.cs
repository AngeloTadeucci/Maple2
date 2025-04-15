using System.Collections.Concurrent;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Util;
using Maple2.Server.World.Service;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager.Config;

public class BuffManager : IUpdatable {
    #region ObjectId
    private int idCounter;

    /// <summary>
    /// Generates an ObjectId unique to this specific actor instance.
    /// </summary>
    /// <returns>Returns a local ObjectId</returns>
    private int NextLocalId() => Interlocked.Increment(ref idCounter);
    #endregion
    public IActor Actor { get; private set; }
    public ConcurrentDictionary<int, List<Buff>> Buffs { get; } = new();
    public IDictionary<InvokeEffectType, IDictionary<int, InvokeRecord>> Invokes { get; init; }
    public IDictionary<CompulsionEventType, IDictionary<int, AdditionalEffectMetadataStatus.CompulsionEvent>> Compulsions { get; init; }
    private Dictionary<BasicAttribute, float> Resistances { get; } = new();
    public ConcurrentDictionary<int, long> CooldownTimes { get; } = new(); // TODO: Cache this
    public ReflectRecord? Reflect;
    private readonly ILogger logger = Log.ForContext<BuffManager>();

    public BuffManager(IActor actor) {
        Actor = actor;
        Invokes = new ConcurrentDictionary<InvokeEffectType, IDictionary<int, InvokeRecord>>();
        Compulsions = new ConcurrentDictionary<CompulsionEventType, IDictionary<int, AdditionalEffectMetadataStatus.CompulsionEvent>>();
    }

    public void Initialize() {
        // Load buffs that are not broadcasted to the field
        if (Actor is FieldPlayer player) {
            player.Session.Config.Skill.UpdatePassiveBuffs(false);
            player.Stats.Refresh();
        }
    }

    public void ResetActor(IActor actor) {
        Actor = actor;
        foreach ((int id, List<Buff> buffDict) in Buffs) {
            foreach (Buff buff in buffDict) {
                buff.ResetActor(actor);
            }
        }
    }

    public void LoadFieldBuffs() {
        // Lapenshards
        // Game Events
        // Prestige
        EnterField();
        if (Actor is FieldPlayer player) {
            player.Session.Config.RefreshPremiumClubBuffs();
        }
    }

    public void AddBuff(IActor caster, IActor owner, int id, short level, long startTick, int durationMs = -1, bool notifyField = true) {
        if (!owner.Field.SkillMetadata.TryGetEffect(id, level, out AdditionalEffectMetadata? additionalEffect)) {
            logger.Error("Invalid buff: {SkillId},{Level}", id, level);
            return;
        }

        if (CooldownTimes.TryGetValue(id, out long cooldownTick) && startTick < cooldownTick) {
            return;
        }

        if (durationMs < 0) {
            durationMs = additionalEffect.Property.DurationTick;
        }

        // Check if immune to any present buffs
        if (!CheckImmunity(id, additionalEffect.Property.Category)) {
            return;
        }

        CooldownTimes[id] = startTick + additionalEffect.Property.CooldownTime;
        Buff? existing = GetBuff(id, additionalEffect.Property.CasterIndividualBuff ? caster.ObjectId : 0);
        long endTick = startTick + durationMs;

        switch (additionalEffect.Property.ResetCondition) {
            case BuffResetCondition.ResetEndTick:
            case BuffResetCondition.Reset2: // This isn't correct, but it seems to behave VERY close to ResetEndTick
                // endTick = startTick + durationMs;
                break;
            case BuffResetCondition.PersistEndTick:
                if (existing != null) {
                    endTick = existing.EndTick;
                }
                break;
            case BuffResetCondition.Replace:
                if (existing != null) {
                    Remove(existing.Id, existing.Caster.ObjectId);
                    existing = null;
                }
                //endTick = startTick + durationMs;
                break;
        }
        if (existing != null) {
            if ((existing.UpdateEndTime(endTick) || existing.Stack()) && notifyField) {
                owner.Field.Broadcast(BuffPacket.Update(existing));
            }
            return;
        }

        // Remove existing buff if it's in the same group
        if (additionalEffect.Property.Group > 0) {
            List<Buff> buffs = EnumerateBuffs().Where(b => b.Metadata.Property.Group == additionalEffect.Property.Group).ToList();
            foreach (Buff existingBuff in buffs) {
                existingBuff.Disable(); // Disable?
                owner.Field.Broadcast(BuffPacket.Remove(existingBuff));
            }
        }

        var buff = new Buff(additionalEffect, NextLocalId(), caster, owner, startTick, endTick);

        if (!TryAdd(buff)) {
            logger.Error("Could not add buff {Id} to {Object}", buff.Id, Actor.ObjectId);
            return;
        }

        // Set Reflect if applicable
        SetReflect(buff);
        SetInvoke(buff);
        SetCompulsionEvent(buff);
        SetShield(buff);
        SetUpdates(buff);
        ModifyBuffStackCount(buff);

        // refresh stats if needed
        if (buff.Metadata.Status.Values.Any() || buff.Metadata.Status.Rates.Any() || buff.Metadata.Status.SpecialValues.Any() || buff.Metadata.Status.SpecialRates.Any()) {
            Actor.Stats.Refresh();
        }

        // Add resistances
        foreach ((BasicAttribute attribute, float value) in additionalEffect.Status.Resistances) {
            Resistances.TryGetValue(attribute, out float existingValue);
            Resistances[attribute] = existingValue + value;
        }

        if (!additionalEffect.Condition.Check(caster, owner, Actor)) {
            buff.Disable();
        }

        logger.Information("{Id} AddBuff to {ObjectId}: {SkillId},{Level} for {Tick}ms", buff.ObjectId, owner.ObjectId, id, level, buff.EndTick - buff.StartTick);
        if (owner is FieldPlayer player) {
            if (additionalEffect.Property.Exp > 0) { // Assuming this doesnt proc s
                player.Session.Exp.AddStaticExp(additionalEffect.Property.Exp);
            }

            player.Session.ConditionUpdate(ConditionType.buff, codeLong: buff.Id);
            player.Session.Dungeon.UpdateDungeonEnterLimit();
        }
        if (notifyField) {
            owner.Field.Broadcast(BuffPacket.Add(buff));
        }
        SetMount(buff);
    }

    private Buff? GetBuff(int buffId, int casterObjectId = 0) {
        if (casterObjectId > 0) {
            if (Buffs.TryGetValue(buffId, out List<Buff>? buffs)) {
                return buffs.FirstOrDefault(buff => buff.Caster.ObjectId == casterObjectId);
            }
        } else {
            if (Buffs.TryGetValue(buffId, out List<Buff>? buffs)) {
                return buffs.FirstOrDefault();
            }
        }
        return null;
    }

    private bool TryAdd(Buff buff) {
        if (Buffs.TryGetValue(buff.Id, out List<Buff>? buffs)) {
            Buff? casterBuff = buffs.FirstOrDefault(b => b.Caster.ObjectId == buff.Caster.ObjectId);
            if (casterBuff != null) {
                return false;
            }
            buffs.Add(buff);
            return true;
        }

        return Buffs.TryAdd(buff.Id, [buff]);
    }

    public List<Buff> EnumerateBuffs() => Buffs.Values.SelectMany(list => list).ToList();
    public List<Buff> EnumerateBuffs(int buffId) => Buffs.TryGetValue(buffId, out List<Buff>? buffs) ? buffs : [];

    public bool HasBuff(int effectId, short effectLevel = 1, int stacks = 0) {
        // Should we be validating all possible buffs with the same ID to see if level/stack match?
        Buff? buff = GetBuff(effectId);
        if (buff == null) {
            return false;
        }

        if (stacks != 0 && buff.Stacks < stacks) {
            return false;
        }

        return effectLevel == 0 || buff.Level >= effectLevel;
    }

    public bool HasBuff(BuffEventType eventType) {
        return EnumerateBuffs().Any(buff => buff.Metadata.Property.EventType == eventType);
    }

    private void SetReflect(Buff buff) {
        if (buff.Metadata.Reflect.EffectId == 0 || !Actor.Field.SkillMetadata.TryGetEffect(buff.Metadata.Reflect.EffectId, buff.Metadata.Reflect.EffectLevel,
                out AdditionalEffectMetadata? _)) {
            return;
        }

        // Does this get overwritten if a new reflect is applied?
        var record = new ReflectRecord(buff.Id, buff.Metadata.Reflect);
        Reflect = record;
    }


    private void SetInvoke(Buff buff) {
        if (buff.Metadata.InvokeEffect == null) {
            return;
        }

        for (int i = 0; i < buff.Metadata.InvokeEffect.Types.Length; i++) {
            var record = new InvokeRecord(buff.Id, buff.Metadata.InvokeEffect) {
                Value = buff.Metadata.InvokeEffect.Values[i],
                Rate = buff.Metadata.InvokeEffect.Rates[i],
            };

            // if exists, replace the record to support differentiating buff levels.
            if (Invokes.TryGetValue(buff.Metadata.InvokeEffect.Types[i], out IDictionary<int, InvokeRecord>? nestedInvokeDic)) {
                Invokes.RemoveAll(buff.Id);

                if (!nestedInvokeDic.TryAdd(buff.Id, record)) {
                    logger.Error("Could not add invoke record from {Id} to {Object}", buff.Id, Actor.ObjectId);
                }
                continue;
            }

            Invokes.Add(buff.Metadata.InvokeEffect.Types[i], new ConcurrentDictionary<int, InvokeRecord> {
                [buff.Id] = record,
            });
        }
    }

    private void SetCompulsionEvent(Buff buff) {
        if (buff.Metadata.Status.Compulsion == null) {
            return;
        }

        CompulsionEventType eventType = buff.Metadata.Status.Compulsion.Type;
        if (Compulsions.TryGetValue(eventType, out IDictionary<int, AdditionalEffectMetadataStatus.CompulsionEvent>? nestedCompulsionDic)) {
            Compulsions.RemoveAll(buff.Id);

            if (!nestedCompulsionDic.TryAdd(buff.Id, buff.Metadata.Status.Compulsion)) {
                logger.Error("Could not add compulsion event from {Id} to {Object}", buff.Id, Actor.ObjectId);
            }
            return;
        }

        Compulsions.Add(eventType, new ConcurrentDictionary<int, AdditionalEffectMetadataStatus.CompulsionEvent> {
            [buff.Id] = buff.Metadata.Status.Compulsion
        });
    }

    public float TotalCompulsionRate(CompulsionEventType type, int skillId = 0) {
        if (!Compulsions.TryGetValue(type, out IDictionary<int, AdditionalEffectMetadataStatus.CompulsionEvent>? nestedCompulsionDic)) {
            return 0;
        }

        return skillId == 0 ? nestedCompulsionDic.Values.Sum(compulsion => compulsion.Rate) :
            nestedCompulsionDic.Values.Where(compulsion => compulsion.SkillIds.Contains(skillId)).Sum(compulsion => compulsion.Rate);
    }

    public float GetResistance(BasicAttribute attribute) {
        if (Resistances.TryGetValue(attribute, out float value)) {
            return value;
        }

        return 0;
    }

    public (int, float) GetInvokeValues(InvokeEffectType invokeType, int skillId, params int[] skillGroup) {
        float value = 0;
        float rate = 0f;
        if (Invokes.TryGetValue(invokeType, out IDictionary<int, InvokeRecord>? nestedInvokeDic)) {
            foreach (InvokeRecord invoke in nestedInvokeDic.Values.Where(record => record.Metadata.SkillId == skillId || skillGroup.Contains(record.Metadata.SkillGroupId))) {
                value += invoke.Value;
                rate += invoke.Rate;
            }
        }

        return ((int, float)) (value, rate);
    }

    private void SetShield(Buff buff) {
        if (buff.Metadata.Shield == null) {
            return;
        }

        if (buff.Metadata.Shield.HpValue > 0) {
            buff.ShieldHealth = buff.Metadata.Shield.HpValue;
        } else if (buff.Metadata.Shield.HpByTargetMaxHp > 0f) {
            buff.ShieldHealth = (long) (Actor.Stats.Values[BasicAttribute.Health].Total * buff.Metadata.Shield.HpByTargetMaxHp);
        }
    }

    private void ModifyBuffStackCount(Buff buff) {
        if (buff.Metadata.ModifyOverlapCount.Length <= 0) {
            return;
        }
        foreach (AdditionalEffectMetadataModifyOverlapCount modifyOverlapCount in buff.Metadata.ModifyOverlapCount) {
            List<Buff> buffs = EnumerateBuffs(modifyOverlapCount.Id);
            foreach (Buff buffResult in buffs) {
                if (buffResult.Stack(modifyOverlapCount.OffsetCount)) {
                    Actor.Field.Broadcast(BuffPacket.Update(buffResult));
                }
            }
        }
    }

    private void SetMount(Buff buff) {
        if (buff.Metadata.Property.RideId == 0 || Actor is not FieldPlayer player) {
            return;
        }

        if (!player.Session.Ride.Mount(buff.Metadata)) {
            logger.Error("Failed to mount {Id} on {Object}", buff.Id, Actor.ObjectId);
            Remove(buff.Id, Actor.ObjectId);
        }
    }

    private void SetUpdates(Buff buff) {
        if (buff.Metadata.Update.Cancel != null) {
            CancelBuffs(buff, buff.Metadata.Update.Cancel);
        }

        // Reset skill cooldowns
        if (buff.Metadata.Update.ResetCooldown.Length > 0 && Actor is FieldPlayer player) {
            foreach (int skillId in buff.Metadata.Update.ResetCooldown) {
                player.Session.Config.SetSkillCooldown(skillId);
            }
        }
    }

    public void TriggerEvent(IActor caster, IActor owner, IActor target, EventConditionType type, int effectSkillId = 0, int buffSkillId = 0) {
        foreach (Buff buff in EnumerateBuffs()) {
            if (!buff.Enabled) {
                continue;
            }
            buff.ApplySkills(caster, owner, target, type, effectSkillId, buffSkillId);
        }
    }

    private void CancelBuffs(Buff buff, AdditionalEffectMetadataUpdate.CancelEffect cancel) {
        List<(int buffId, int casterId)> buffsToRemove = [];
        foreach (int cancelId in cancel.Ids) {
            foreach (Buff buffResult in EnumerateBuffs(cancelId)) {
                if (cancel.CheckSameCaster) {
                    if (buffResult.Caster != buff.Caster) {
                        continue;
                    }
                }
                buffsToRemove.Add((buffResult.Id, Actor.ObjectId));
            }
        }

        foreach (BuffCategory category in cancel.Categories) {
            foreach (Buff cancelCategoryBuff in EnumerateBuffs()) {
                if (cancelCategoryBuff.Metadata.Property.Category == category) {
                    buffsToRemove.Add((cancelCategoryBuff.Id, Actor.ObjectId));
                }
            }
        }
        Remove(buffsToRemove.ToArray());
    }

    public void ModifyDuration(int buffId, int modifyValue) {
        List<Buff> buffs = EnumerateBuffs(buffId);
        foreach (Buff buff in buffs) {
            buff.UpdateEndTime(modifyValue);
        }
    }

    public virtual void Update(long tickCount) {
        foreach (Buff buff in EnumerateBuffs()) {
            buff.Update(tickCount);
        }
    }

    public void LeaveField() {
        if (Actor.Field == null) {
            return;
        }
        List<(int id, int casterId)> buffsToRemove = [];
        foreach (MapEntranceBuff buff in Actor.Field.Metadata.EntranceBuffs) {
            buffsToRemove.Add((buff.Id, Actor.ObjectId));
        }
        foreach (Buff buff in EnumerateBuffs()) {
            if (buff.Metadata.Property.RemoveOnLeaveField) {
                buffsToRemove.Add((buff.Id, Actor.ObjectId));
            }
        }
        Remove(buffsToRemove.ToArray());
    }

    private void EnterField() {
        foreach (MapEntranceBuff buff in Actor.Field.Metadata.EntranceBuffs) {
            AddBuff(Actor, Actor, buff.Id, buff.Level, Actor.Field.FieldTick);
        }

        if (Actor.Field.Metadata.Property.Type == MapType.Pvp) {
            List<(int id, int casterId)> buffsToRemove = [];
            foreach (Buff buff in EnumerateBuffs()) {
                if (buff.Metadata.Property.RemoveOnPvpZone) {
                    buffsToRemove.Add((buff.Id, Actor.ObjectId));
                }

                if (!buff.Metadata.Property.KeepOnEnterPvpZone) {
                    buffsToRemove.Add((buff.Id, Actor.ObjectId));
                }
            }
            Remove(buffsToRemove.ToArray());
        }

        if (Actor.Field.Metadata.Property.Region == MapRegion.ShadowWorld) {
            AddBuff(Actor, Actor, Constant.shadowWorldBuffHpUp, 1, Actor.Field.FieldTick);
            AddBuff(Actor, Actor, Constant.shadowWorldBuffMoveProtect, 1, Actor.Field.FieldTick);
        }
    }

    public void AddItemBuffs(Item item) {
        foreach (ItemMetadataAdditionalEffect buff in item.Metadata.AdditionalEffects) {
            AddBuff(Actor, Actor, buff.Id, buff.Level, Actor.Field.FieldTick);
        }

        if (item.Socket != null) {
            foreach (ItemGemstone? gem in item.Socket.Sockets) {
                if (gem != null && Actor.Field.ItemMetadata.TryGet(gem.ItemId, out ItemMetadata? metadata)) {
                    foreach (ItemMetadataAdditionalEffect buff in metadata.AdditionalEffects) {
                        AddBuff(Actor, Actor, buff.Id, buff.Level, Actor.Field.FieldTick);
                    }
                }
            }
        }
    }

    public void RemoveItemBuffs(Item item) {
        List<(int id, int casterId)> buffsToRemove = [];
        foreach (ItemMetadataAdditionalEffect buff in item.Metadata.AdditionalEffects) {
            buffsToRemove.Add((buff.Id, Actor.ObjectId));
        }

        if (item.Socket != null) {
            foreach (ItemGemstone? gem in item.Socket.Sockets) {
                if (gem != null && Actor.Field.ItemMetadata.TryGet(gem.ItemId, out ItemMetadata? metadata)) {
                    foreach (ItemMetadataAdditionalEffect buff in metadata.AdditionalEffects) {
                        buffsToRemove.Add((buff.Id, Actor.ObjectId));
                    }
                }
            }
        }

        Remove(buffsToRemove.ToArray());
    }

    public void SetCacheBuffs(IList<BuffInfo> buffs, long currentTick) {
        if (Actor is not FieldPlayer player) {
            return;
        }

        foreach (BuffInfo info in buffs) {
            if (!player.Field.SkillMetadata.TryGetEffect(info.Id, (short) info.Level, out AdditionalEffectMetadata? additionalEffect)) {
                logger.Error("Invalid buff: {SkillId},{Level}", info.Id, info.Level);
                continue;
            }

            if (additionalEffect.Property.UseInGameTime || info.MsRemaining > 0) {
                AddBuff(Actor, Actor, info.Id, (short) info.Level, currentTick, info.MsRemaining);
            }
        }
    }

    public void Remove(params (int id, int casterId)[] buffIds) {
        foreach ((int id, int casterId) in buffIds) {
            Remove(id, casterId);
        }
    }

    public bool Remove(int id, int casterId) {
        //TODO: Check if buff is removable/should be removed
        bool refreshStats = false;
        List<Buff> buffsToRemove = [];
        foreach (Buff buff in EnumerateBuffs(id)) {
            if (buff.Metadata.Property.CasterIndividualBuff && buff.Caster.ObjectId != casterId) {
                continue;
            }

            buffsToRemove.Add(buff);
            foreach ((BasicAttribute attribute, float value) in buff.Metadata.Status.Resistances) {
                Resistances[attribute] = Math.Min(0, Resistances[attribute] - value);
            }

            if (buff.Metadata.Status.Values.Count > 0 || buff.Metadata.Status.Rates.Count > 0 || buff.Metadata.Status.SpecialValues.Count > 0 || buff.Metadata.Status.SpecialRates.Count > 0) {
                if (Actor is FieldPlayer player) {
                    refreshStats = true;
                }
            }
        }

        if (Reflect?.SourceBuffId == id) {
            Reflect = null;
        }


        Invokes.RemoveAll(id);
        Compulsions.RemoveAll(id);
        foreach (Buff buff in buffsToRemove) {
            RemoveInternal(buff);
            Actor.Field.Broadcast(BuffPacket.Remove(buff));
            if (buff.Metadata.Property.RideId > 0) {
                refreshStats = true;
                if (Actor is FieldPlayer player && player.Session.Ride.Ride?.Action is RideOnActionBattle battle && battle.SkillId == buff.Id) {
                    player.Session.Ride.Dismount(RideOffType.AdditionalEffect);
                }
            }
        }
        if (refreshStats) {
            Actor.Stats.Refresh();
        }

        return true;

        void RemoveInternal(Buff buffToRemove) {
            if (Buffs.TryGetValue(buffToRemove.Id, out List<Buff>? buffList)) {
                if (!buffList.Remove(buffToRemove)) {
                    logger.Error("Failed to remove buff {Id} from {Object}", buffToRemove.Id, Actor.ObjectId);
                }
            }
        }
    }

    public void OnDeath() {
        foreach (Buff buff in EnumerateBuffs()) {
            if (!buff.Metadata.Property.KeepOnDeath) {
                Remove(buff.Id, Actor.ObjectId);
                continue;
            }
            buff.UpdateEnabled();
        }
    }

    private bool CheckImmunity(int newBuffId, BuffCategory category) {
        foreach (Buff buff in EnumerateBuffs()) {
            if (buff.Metadata.Update.ImmuneIds.Contains(newBuffId) || buff.Metadata.Update.ImmuneCategories.Contains(category)) {
                return false;
            }
        }

        return true;
    }

    public List<Buff> GetSaveCacheBuffs() {
        return EnumerateBuffs()
            .Where(buff => !buff.Metadata.Property.RemoveOnLogout)
            .ToList();
    }

    public void UpdateEnabled() {
        foreach (Buff buff in EnumerateBuffs()) {
            buff.UpdateEnabled();
        }
    }
}
