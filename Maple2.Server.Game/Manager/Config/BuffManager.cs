using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Util;
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
    private readonly IActor actor;
    // TODO: Change this to support multiple buffs of the same id, different casters. Possibly also different levels?
    public ConcurrentDictionary<int, Buff> Buffs { get; } = new();
    public IDictionary<InvokeEffectType, IDictionary<int, InvokeRecord>> Invokes { get; init; }
    public IDictionary<CompulsionEventType, IDictionary<int, AdditionalEffectMetadataStatus.CompulsionEvent>> Compulsions { get; init; }
    private Dictionary<BasicAttribute, float> Resistances { get; } = new();
    public ReflectRecord? Reflect;
    private readonly ILogger logger = Log.ForContext<BuffManager>();

    public BuffManager(IActor actor) {
        this.actor = actor;
        Invokes = new ConcurrentDictionary<InvokeEffectType, IDictionary<int, InvokeRecord>>();
        Compulsions = new ConcurrentDictionary<CompulsionEventType, IDictionary<int, AdditionalEffectMetadataStatus.CompulsionEvent>>();
    }

    public void Initialize() {
        // Load buffs that are not broadcasted to the field
        if (actor is FieldPlayer player) {
            player.Session.Config.Skill.UpdatePassiveBuffs(false);
            foreach (Item item in player.Session.Item.Equips.Gear.Values) {
                foreach (ItemMetadataAdditionalEffect buff in item.Metadata.AdditionalEffects) {
                    AddBuff(actor, actor, buff.Id, buff.Level, false);
                }

                // Check for buffs on gemstones
                if (item.Socket?.Sockets is not null && item.Socket.Sockets.Length > 0) {
                    foreach (ItemGemstone? gemstone in item.Socket?.Sockets!) {
                        if (gemstone == null || !player.Session.ItemMetadata.TryGet(gemstone.ItemId, out ItemMetadata? metadata)) {
                            continue;
                        }

                        foreach (ItemMetadataAdditionalEffect buff in metadata.AdditionalEffects) {
                            AddBuff(actor, actor, buff.Id, buff.Level, false);
                        }
                    }
                }
            }

            foreach (Item item in player.Session.Item.Equips.Badge.Values) {
                foreach (ItemMetadataAdditionalEffect buff in item.Metadata.AdditionalEffects) {
                    AddBuff(actor, actor, buff.Id, buff.Level, false);
                }
            }
        }
    }

    public void LoadFieldBuffs() {
        // Lapenshards
        // Game Events
        // Prestige
        AddMapBuffs();
        if (actor is FieldPlayer player) {
            player.Session.Config.RefreshPremiumClubBuffs();
        }
    }

    public void AddBuff(IActor caster, IActor owner, int id, short level, bool notifyField = true) {
        if (!owner.Field.SkillMetadata.TryGetEffect(id, level, out AdditionalEffectMetadata? additionalEffect)) {
            logger.Error("Invalid buff: {SkillId},{Level}", id, level);
            return;
        }

        // Check if immune to any present buffs
        if (!CheckImmunity(id, additionalEffect.Property.Category)) {
            return;
        }

        // TODO: Implement AdditionalEffectMetadata.CasterIndividualBuff.
        // If true, each caster will have their own buff added to the same Actor.
        if (Buffs.TryGetValue(id, out Buff? existing)) {
            if (level > existing.Level) {

            }
            if (!existing.Stack()) {
                return;
            }
            if (notifyField) {
                owner.Field.Broadcast(BuffPacket.Update(existing));
            }
            return;
        }

        // Remove existing buff if it's in the same group
        if (additionalEffect.Property.Group > 0) {
            existing = Buffs.Values.FirstOrDefault(buff => buff.Metadata.Property.Group == additionalEffect.Property.Group);
            if (existing != null) {
                existing.Disable();
                owner.Field.Broadcast(BuffPacket.Remove(existing));
            }
        }


        var buff = new Buff(owner.Field, additionalEffect, NextLocalId(), caster, owner);
        if (!Buffs.TryAdd(id, buff)) {
            Buffs[id].Stack();
            owner.Field.Broadcast(BuffPacket.Update(buff));
            return;
        }

        // Set Reflect if applicable
        SetReflect(buff);
        SetInvoke(buff);
        SetCompulsionEvent(buff);
        SetShield(buff);
        SetUpdates(buff);

        // TODO: Uncomment this when stats PR is merged.
        // refresh stats if needed
        //if (buff.Metadata.Status.Values.Any() || buff.Metadata.Status.Rates.Any() || buff.Metadata.Status.SpecialValues.Any() || buff.Metadata.Status.SpecialRates.Any()) {
        //        actor.Stats.Refresh();
        //}

        // Add resistances
        foreach ((BasicAttribute attribute, float value) in additionalEffect.Status.Resistances) {
            Resistances.TryGetValue(attribute, out float existingValue);
            Resistances[attribute] = existingValue + value;
        }

        if (!additionalEffect.Condition.Check(caster, owner, actor)) {
            buff.Disable();
        }

        logger.Information("{Id} AddBuff to {ObjectId}: {SkillId},{Level} for {Tick}ms", buff.ObjectId, owner.ObjectId, id, level, buff.EndTick - buff.StartTick);
        // Logger.Information("> {Data}", additionalEffect.Property);
        if (owner is FieldPlayer player) {
            player.Session.ConditionUpdate(ConditionType.buff, codeLong: buff.Id);
        }
        if (notifyField) {
            owner.Field.Broadcast(BuffPacket.Add(buff));
        }
    }

    private void SetReflect(Buff buff) {
        if (buff.Metadata.Reflect.EffectId == 0 || !actor.Field.SkillMetadata.TryGetEffect(buff.Metadata.Reflect.EffectId, buff.Metadata.Reflect.EffectLevel,
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
                    logger.Error("Could not add invoke record from {Id} to {Object}", buff.Id, actor.ObjectId);
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
                logger.Error("Could not add compulsion event from {Id} to {Object}", buff.Id, actor.ObjectId);
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
            buff.ShieldHealth = (long) (actor.Stats[BasicAttribute.Health].Total * buff.Metadata.Shield.HpByTargetMaxHp);
        }
    }

    private void SetUpdates(Buff buff) {
        if (buff.Metadata.Update.Cancel != null) {
            CancelBuffs(buff, buff.Metadata.Update.Cancel);
        }

        // Reset skill cooldowns
        if (buff.Metadata.Update.ResetCooldown.Length > 0 && actor is FieldPlayer player) {
            foreach (int skillId in buff.Metadata.Update.ResetCooldown) {
                // TODO: Uncomment this when skill cooldown PR is merged
                //player.Session.Config.SetSkillCooldown(skillId);
            }
        }
    }

    private void CancelBuffs(Buff buff, AdditionalEffectMetadataUpdate.CancelEffect cancel) {
        foreach (int cancelId in cancel.Ids) {
            if (Buffs.TryGetValue(cancelId, out Buff? cancelBuff)) {
                if (cancel.CheckSameCaster) {
                    if (cancelBuff.Caster != buff.Caster) {
                        continue;
                    }
                }

                Remove(cancelId);
            }
        }

        foreach (BuffCategory category in cancel.Categories) {
            foreach (Buff cancelCategoryBuff in Buffs.Values) {
                if (cancelCategoryBuff.Metadata.Property.Category == category) {
                    Remove(cancelCategoryBuff.Id);
                }
            }
        }
    }

    public virtual void Update(long tickCount) {
        foreach (Buff buff in Buffs.Values) {
            buff.Update(tickCount);
        }
    }

    public void LeaveField() {
        foreach (MapEntranceBuff buff in actor.Field.Metadata.EntranceBuffs) {
            Remove(buff.Id);
        }
        foreach (Buff buff in Buffs.Values) {
            if (buff.Metadata.Property.RemoveOnLeaveField) {
                Remove(buff.Id);
            }
        }
    }

    private void AddMapBuffs() {
        foreach (MapEntranceBuff buff in actor.Field.Metadata.EntranceBuffs) {
            AddBuff(actor, actor, buff.Id, buff.Level);
        }

        if (actor.Field.Metadata.Property.Type == MapType.Pvp) {
            foreach (Buff buff in Buffs.Values) {
                if (buff.Metadata.Property.RemoveOnPvpZone) {
                    Remove(buff.Id);
                }

                if (!buff.Metadata.Property.KeepOnEnterPvpZone) {
                    Remove(buff.Id);
                }
            }
        }

        if (actor.Field.Metadata.Property.Region == MapRegion.ShadowWorld) {
            AddBuff(actor, actor, Constant.shadowWorldBuffHpUp, 1);
            AddBuff(actor, actor, Constant.shadowWorldBuffMoveProtect, 1);
        }
    }

    public void AddItemBuffs(Item item) {
        foreach (ItemMetadataAdditionalEffect buff in item.Metadata.AdditionalEffects) {
            AddBuff(actor, actor, buff.Id, buff.Level);
        }
    }

    public void RemoveItemBuffs(Item item) {
        foreach (ItemMetadataAdditionalEffect buff in item.Metadata.AdditionalEffects) {
            Remove(buff.Id);
        }
    }

    public bool Remove(int id) {
        //TODO: Check if buff is removable/should be removed
        if (!Buffs.Remove(id, out Buff? buff)) {
            return false;
        }

        if (Reflect?.SourceBuffId == id) {
            Reflect = null;
        }

        foreach ((BasicAttribute attribute, float value) in buff.Metadata.Status.Resistances) {
            Resistances[attribute] = Math.Min(0, Resistances[attribute] - value);
        }

        Invokes.RemoveAll(id);
        Compulsions.RemoveAll(id);

        if (buff.Metadata.Status.Values.Count > 0 || buff.Metadata.Status.Rates.Count > 0 || buff.Metadata.Status.SpecialValues.Count > 0 || buff.Metadata.Status.SpecialRates.Count > 0) {
            if (actor is FieldPlayer player) {
                player.Session.Stats.Refresh();
            }
        }

        actor.Field.Broadcast(BuffPacket.Remove(buff));
        return true;
    }

    public void OnDeath() {
        foreach (Buff buff in Buffs.Values) {
            if (!buff.Metadata.Property.KeepOnDeath) {
                Remove(buff.Id);
            }
        }
    }

    private bool CheckImmunity(int newBuffId, BuffCategory category) {
        foreach ((int id, Buff buff) in Buffs) {
            if (buff.Metadata.Update.ImmuneIds.Contains(newBuffId) || buff.Metadata.Update.ImmuneCategories.Contains(category)) {
                return false;
            }
        }

        return true;
    }
}
