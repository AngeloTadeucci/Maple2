﻿using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Formulas;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Manager;

public class StatsManager {
    public IActor Actor { get; private set; }
    private readonly IReadOnlyDictionary<short, IReadOnlyDictionary<BasicAttribute, long>> levelStats;

    public Stats Values { get; init; }

    public StatsManager(IActor actor, UserStatTable userStatsMetadata) {
        this.Actor = actor;
        levelStats = new Dictionary<short, IReadOnlyDictionary<BasicAttribute, long>>();

        if (actor is not FieldPlayer player) {
            Values = new Stats(new Dictionary<BasicAttribute, long>(), 0);
            return;
        }
        if (!userStatsMetadata.JobStats.TryGetValue(player.Value.Character.Job.Code(), out IReadOnlyDictionary<short, IReadOnlyDictionary<BasicAttribute, long>>? stats)) {
            throw new Exception("Failed to initialize StatsManager, could not find metadata for job " + player.Value.Character.Job.Code() + ".");
        }

        levelStats = stats;
        if (!stats.TryGetValue(player.Value.Character.Level, out IReadOnlyDictionary<BasicAttribute, long>? metadata)) {
            throw new Exception("Failed to initialize StatsManager, could not find metadata for level " + player.Value.Character.Level + ".");
        }
        Values = new Stats(metadata, player.Value.Character.Job.Code());
    }

    public StatsManager(IActor actor) {
        this.Actor = actor;
        levelStats = new Dictionary<short, IReadOnlyDictionary<BasicAttribute, long>>();

        if (actor is FieldNpc npc) {
            Values = new Stats(npc.Value.Metadata.Stat);
            return;
        }

        Values = new Stats(new Dictionary<BasicAttribute, long>(), 0);
    }

    /// <summary>
    /// Get the actor's bonus attack min and max.
    /// </summary>
    /// <param name="targetBonusAttackResistance">Target's total Bonus Attack resistance</param>
    /// <param name="targetMaxWeaponAttackResistance">Target's total Max Weapon Attack resistance</param>
    /// <returns>Tuple. Item1 = Min Damage. Item2 = Max Damage.</returns>
    public (double, double) GetBonusAttack(float targetBonusAttackResistance, float targetMaxWeaponAttackResistance) {
        switch (Actor) {
            case FieldPlayer player:
                double bonusAttack = Values[BasicAttribute.BonusAtk].Total + Constant.PetAttackMultiplier * Values[BasicAttribute.PetBonusAtk].Total;
                double bonusAttackResistance = 1 / (1 + targetBonusAttackResistance);
                double weaponAttackWeakness = 1 / (1 + targetMaxWeaponAttackResistance);
                double bonusAttackCoefficient = bonusAttackResistance * BonusAttackCoefficient(player);
                double minDamage = weaponAttackWeakness * Values[BasicAttribute.MinWeaponAtk].Total + bonusAttackCoefficient * bonusAttack;
                double maxDamage = weaponAttackWeakness * Values[BasicAttribute.MaxWeaponAtk].Total + bonusAttackCoefficient * bonusAttack;
                return (minDamage, maxDamage);
            case FieldNpc npc:
                return (1, 1);
        }
        return (1, 1);

        double BonusAttackCoefficient(FieldPlayer player) {
            int leftHandRarity = player.Session.Item.Equips.Get(EquipSlot.RH)?.Rarity ?? 0;
            int rightHandRarity = player.Session.Item.Equips.Get(EquipSlot.LH)?.Rarity ?? 0;
            return BonusAttack.Coefficient(rightHandRarity, leftHandRarity, player.Value.Character.Job.Code());
        }
    }

    /// <summary>
    /// Get actor's critical damage result. Takes into consideration the target's critical damage resistance.
    /// </summary>
    /// <param name="targetCriticalDamageResistance">Target's critical damage resistance</param>
    /// <param name="mode">Unknown. Used for Lua script. If 0, the cap of damage is 250%. If 14, it's 300%</param>
    /// <returns>Critical damage</returns>
    public float GetCriticalDamage(float targetCriticalDamageResistance, int mode = 0) {
        float criticalDamage = Actor.Field.Lua.CalcCritDamage(Values[BasicAttribute.CriticalDamage].Total, mode);
        //TODO: Apply target's resistance. Need to figure out formula for this.
        return criticalDamage;
    }

    /// <summary>
    /// Gets the critical rate of the actor - uses a different formula for players and NPCs.
    /// </summary>
    /// <param name="targetCriticalEvasion"></param>
    /// <param name="casterCriticalOverride"></param>
    /// <returns>DamageType. If successful crit, returns DamageType.Critical. else returns DamageType.Normal</returns>
    public DamageType GetCriticalRate(long targetCriticalEvasion, double casterCriticalOverride) {
        float criticalChance = Actor switch {
            FieldPlayer player => Actor.Field.Lua.CalcPlayerCritRate((int) player.Value.Character.Job.Code(), player.Stats.Values[BasicAttribute.Luck].Total, player.Stats.Values[BasicAttribute.CriticalRate].Total, targetCriticalEvasion, 0, 0),
            FieldNpc npc => Actor.Field.Lua.CalcNpcCritRate(npc.Stats.Values[BasicAttribute.Luck].Total, npc.Stats.Values[BasicAttribute.CriticalRate].Total, targetCriticalEvasion),
            _ => 0
        };

        return Random.Shared.NextDouble() < Math.Max(criticalChance, casterCriticalOverride) ? DamageType.Critical : DamageType.Normal;
    }

    public void Refresh() {
        if (Actor is not FieldPlayer player) {
            return;
        }

        Character character = player.Value.Character;
        if (levelStats.TryGetValue(character.Level, out IReadOnlyDictionary<BasicAttribute, long>? metadata)) {
            Values.Reset(metadata, character.Job.Code());
        } else {
            Log.Logger.Error("Failed to refresh stats for {Job} level {Level}.", character.Job.Code(), character.Level);
            Values.Reset(character.Job.Code(), character.Level);
        }

        AddEquips(player);
        AddBuffs(player);
        Values.Total();
        StatConversion(player);
        Actor.Field.Broadcast(StatsPacket.Init(player));
        Actor.Field.Broadcast(StatsPacket.Update(player), player.Session);
    }

    public void ResetActor(IActor actor) {
        Actor = actor;
    }

    private void AddEquips(FieldPlayer player) {
        Values.GearScore = 0;
        foreach (Item item in player.Session.Item.Equips.Gear.Values) {
            if (item.Stats != null) {
                AddItemStats(item.Stats);
            }

            Log.Logger.Debug("Calculating Gearscore. Item ID: {id} - Gearscore: {gearscore} - Rarity: {rarity}, Enchant Level: {enchantLevel}, Limit Break Level: {limitBreakLevel}", item.Metadata.Id, item.Metadata.Property.GearScore, item.Rarity, item.Enchant?.Enchants ?? 0, item.LimitBreak?.Level ?? 0);
            Values.GearScore += player.Field.Lua.CalcItemLevel(item.Metadata.Property.GearScore, item.Rarity, item.Type.Type, item.Enchant?.Enchants ?? 0, item.LimitBreak?.Level ?? 0).Item1;

            if (item.Socket != null) {
                for (int index = 0; index < item.Socket.UnlockSlots; index++) {
                    ItemGemstone? gem = item.Socket.Sockets[index];
                    if (gem != null && gem.Stats != null) {
                        AddItemStats(gem.Stats);
                    }
                }
            }

            player.Buffs.AddItemBuffs(item);
        }
    }

    private void AddBuffs(FieldPlayer player) {
        foreach (Buff buff in player.Buffs.Buffs.Values) {
            foreach ((BasicAttribute valueBasicAttribute, long value) in buff.Metadata.Status.Values) {
                Values[valueBasicAttribute].AddTotal(value);
            }
            foreach ((BasicAttribute ratespecialAttribute, float rate) in buff.Metadata.Status.Rates) {
                Values[ratespecialAttribute].AddRate(rate);
            }
            foreach ((SpecialAttribute valueSpecialAttribute, float value) in buff.Metadata.Status.SpecialValues) {
                Values[valueSpecialAttribute].AddTotal((long) value);
            }
            foreach ((SpecialAttribute rateSpecialAttribute, float rate) in buff.Metadata.Status.SpecialRates) {
                Values[rateSpecialAttribute].AddRate(rate);
            }
        }
    }

    private void StatConversion(FieldPlayer player) {
        foreach (Buff buff in player.Buffs.Buffs.Values) {
            if (buff.Metadata.Status.Conversion != null) {
                float sum = Values[buff.Metadata.Status.Conversion.BaseAttribute].Total;
                sum *= buff.Metadata.Status.Conversion.Rate;
                Values[buff.Metadata.Status.Conversion.ResultAttribute].AddTotal((long) sum);
            }
        }
    }

    private void AddItemStats(ItemStats stats) {
        for (int type = 0; type < ItemStats.TYPE_COUNT; type++) {
            foreach ((BasicAttribute attribute, BasicOption option) in stats[(ItemStats.Type) type].Basic) {
                Values[attribute].AddTotal(option);
            }
            foreach ((SpecialAttribute attribute, SpecialOption option) in stats[(ItemStats.Type) type].Special) {
                Values[attribute].AddTotal(option);
            }
        }
    }
}
