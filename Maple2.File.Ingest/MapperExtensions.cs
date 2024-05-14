using System.Diagnostics;
using Maple2.File.Ingest.Utils;
using Maple2.File.Parser.Xml;
using Maple2.File.Parser.Xml.Common;
using Maple2.File.Parser.Xml.Skill;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using BeginCondition = Maple2.Model.Metadata.BeginCondition;
using ItemOption = Maple2.Model.Metadata.ItemOption;

namespace Maple2.File.Ingest;

public static class MapperExtensions {
    public static Dictionary<BasicAttribute, long> ToDictionary(this StatValue values) {
        var result = new Dictionary<BasicAttribute, long>();
        foreach (BasicAttribute attribute in Enum.GetValues<BasicAttribute>()) {
            long value = values[(byte) attribute];
            if (value != default) {
                result[attribute] = value;
            }
        }

        return result;
    }

    public static Dictionary<BasicAttribute, float> ToDictionary(this StatRate rates) {
        var result = new Dictionary<BasicAttribute, float>();
        foreach (BasicAttribute attribute in Enum.GetValues<BasicAttribute>()) {
            float rate = rates[(byte) attribute];
            if (rate != default) {
                result[attribute] = rate;
            }
        }

        return result;
    }

    public static byte OptionIndex(this SpecialAttribute attribute) {
        return attribute switch {
            SpecialAttribute.Experience => 0,
            SpecialAttribute.Meso => 1,
            SpecialAttribute.SwimSpeed => 2,
            SpecialAttribute.DashDistance => 3,
            // MoveSpeed = 4
            // "sid" = 5?
            SpecialAttribute.TotalDamage => 6,
            SpecialAttribute.CriticalDamage => 7,
            SpecialAttribute.NormalNpcDamage => 8,
            SpecialAttribute.LeaderNpcDamage => 9,
            SpecialAttribute.EliteNpcDamage => 10,
            SpecialAttribute.BossNpcDamage => 11,
            SpecialAttribute.HpOnKill => 12,
            SpecialAttribute.SpiritOnKill => 13,
            SpecialAttribute.StaminaOnKill => 14,
            SpecialAttribute.RecoveryBonus => 15,
            SpecialAttribute.BonusRecoveryFromAlly => 16,
            SpecialAttribute.IceDamage => 17,
            SpecialAttribute.FireDamage => 18,
            SpecialAttribute.DarkDamage => 19,
            SpecialAttribute.HolyDamage => 20,
            SpecialAttribute.PoisonDamage => 21,
            SpecialAttribute.ElectricDamage => 22,
            SpecialAttribute.MeleeDamage => 23,
            SpecialAttribute.RangedDamage => 24,
            SpecialAttribute.PhysicalPiercing => 25,
            SpecialAttribute.MagicalPiercing => 26,
            SpecialAttribute.ReduceIceDamage => 27,
            SpecialAttribute.ReduceFireDamage => 28,
            SpecialAttribute.ReduceDarkDamage => 29,
            SpecialAttribute.ReduceHolyDamage => 30,
            SpecialAttribute.ReducePoisonDamage => 31,
            SpecialAttribute.ReduceElectricDamage => 32,
            SpecialAttribute.ReduceStun => 33,
            SpecialAttribute.ReduceDebuff => 34,
            SpecialAttribute.ReduceCooldown => 35,
            SpecialAttribute.ReduceMeleeDamage => 36,
            SpecialAttribute.ReduceRangedDamage => 37,
            SpecialAttribute.ReduceKnockBack => 38,
            SpecialAttribute.MeleeStun => 39,
            SpecialAttribute.RangedStun => 40,
            SpecialAttribute.MeleeKnockBack => 41,
            SpecialAttribute.RangedKnockBack => 42,
            SpecialAttribute.MeleeImmobilize => 43,
            SpecialAttribute.RangedImmobilize => 44,
            SpecialAttribute.MeleeSplashDamage => 45,
            SpecialAttribute.RangedSplashDamage => 46,
            SpecialAttribute.DropRate => 47,
            SpecialAttribute.QuestExp => 48,
            SpecialAttribute.QuestMeso => 49,
            SpecialAttribute.FishingExp => 50,
            SpecialAttribute.ArcadeExp => 51,
            SpecialAttribute.PlayInstrumentExp => 52,
            SpecialAttribute.InvokeEffect1 => 53,
            SpecialAttribute.InvokeEffect2 => 54,
            SpecialAttribute.InvokeEffect3 => 55,
            SpecialAttribute.PvpDamage => 56,
            SpecialAttribute.ReducePvpDamage => 57,
            SpecialAttribute.GuildExp => 58,
            SpecialAttribute.GuildCoin => 59,
            SpecialAttribute.MassiveEventExpBall => 60,
            SpecialAttribute.ReduceMesoTradeFee => 61,
            SpecialAttribute.ReduceEnchantMaterialFee => 62,
            SpecialAttribute.ReduceMeretRevivalFee => 63,
            SpecialAttribute.MiningRewardItem => 64,
            SpecialAttribute.BreedingRewardItem => 65,
            SpecialAttribute.SmithingRewardMastery => 66,
            SpecialAttribute.EngravingRewardMastery => 67,
            SpecialAttribute.GatheringRewardItem => 68,
            SpecialAttribute.FarmingRewardItem => 69,
            SpecialAttribute.AlchemistRewardMastery => 70,
            SpecialAttribute.CookingRewardMastery => 71,
            SpecialAttribute.AcquireGatheringExp => 72,
            SpecialAttribute.SkillLevelUpTier1 => 73,
            SpecialAttribute.SkillLevelUpTier2 => 74,
            SpecialAttribute.SkillLevelUpTier3 => 75,
            SpecialAttribute.SkillLevelUpTier4 => 76,
            SpecialAttribute.SkillLevelUpTier5 => 77,
            SpecialAttribute.SkillLevelUpTier6 => 78,
            SpecialAttribute.SkillLevelUpTier7 => 79,
            SpecialAttribute.SkillLevelUpTier8 => 80,
            SpecialAttribute.SkillLevelUpTier9 => 81,
            SpecialAttribute.SkillLevelUpTier10 => 82,
            SpecialAttribute.SkillLevelUpTier11 => 83,
            SpecialAttribute.SkillLevelUpTier12 => 84,
            SpecialAttribute.SkillLevelUpTier13 => 85,
            SpecialAttribute.SkillLevelUpTier14 => 86,
            SpecialAttribute.MassiveOxExp => 87,
            SpecialAttribute.MassiveTrapMasterExp => 88,
            SpecialAttribute.MassiveFinalSurvivalExp => 89,
            SpecialAttribute.MassiveCrazyRunnerExp => 90,
            SpecialAttribute.MassiveShCrazyRunnerExp => 91,
            SpecialAttribute.MassiveEscapeExp => 92,
            SpecialAttribute.MassiveSpringBeachExp => 93,
            SpecialAttribute.MassiveDanceDanceExp => 94,
            SpecialAttribute.MassiveOxSpeed => 95,
            SpecialAttribute.MassiveTrapMasterSpeed => 96,
            SpecialAttribute.MassiveFinalSurvivalSpeed => 97,
            SpecialAttribute.MassiveCrazyRunnerSpeed => 98,
            SpecialAttribute.MassiveShCrazyRunnerSpeed => 99,
            SpecialAttribute.MassiveEscapeSpeed => 100,
            SpecialAttribute.MassiveSpringBeachSpeed => 101,
            SpecialAttribute.MassiveDanceDanceSpeed => 102,
            SpecialAttribute.NpcHitRewardSpBall => 103,
            SpecialAttribute.NpcHitRewardEpBall => 104,
            SpecialAttribute.HonorToken => 105,
            SpecialAttribute.PvpExp => 106,
            SpecialAttribute.DarkStreamDamage => 107,
            SpecialAttribute.ReduceDarkStreamReceiveDamage => 108,
            SpecialAttribute.DarkStreamEvp => 109,
            SpecialAttribute.FishingDoubleMastery => 110,
            SpecialAttribute.PlayInstrumentDoubleMastery => 111,
            SpecialAttribute.CompleteFieldMissionSpeed => 112,
            SpecialAttribute.GlideVerticalVelocity => 113,
            SpecialAttribute.AdditionalEffect95000018 => 114,
            SpecialAttribute.AdditionalEffect95000012 => 115,
            SpecialAttribute.AdditionalEffect95000014 => 116,
            SpecialAttribute.AdditionalEffect95000020 => 117,
            SpecialAttribute.AdditionalEffect95000021 => 118,
            SpecialAttribute.AdditionalEffect95000022 => 119,
            SpecialAttribute.AdditionalEffect95000023 => 120,
            SpecialAttribute.AdditionalEffect95000024 => 121,
            SpecialAttribute.AdditionalEffect95000025 => 122,
            SpecialAttribute.AdditionalEffect95000026 => 123,
            SpecialAttribute.AdditionalEffect95000027 => 124,
            SpecialAttribute.AdditionalEffect95000028 => 125,
            SpecialAttribute.AdditionalEffect95000029 => 126,
            SpecialAttribute.ReduceRecoveryEpInv => 127,
            SpecialAttribute.MaxWeaponAttack => 128,
            SpecialAttribute.MiningDoubleReward => 129,
            SpecialAttribute.BreedingDoubleReward => 130,
            SpecialAttribute.GatheringDoubleReward => 131,
            SpecialAttribute.FarmingDoubleReward => 132,
            SpecialAttribute.SmithingDoubleReward => 133,
            SpecialAttribute.EngravingDoubleReward => 134,
            SpecialAttribute.AlchemistDoubleReward => 135,
            SpecialAttribute.CookingDoubleReward => 136,
            SpecialAttribute.MiningDoubleMastery => 137,
            SpecialAttribute.BreedingDoubleMastery => 138,
            SpecialAttribute.GatheringDoubleMastery => 139,
            SpecialAttribute.FarmingDoubleMastery => 140,
            SpecialAttribute.SmithingDoubleMastery => 141,
            SpecialAttribute.EngravingDoubleMastery => 142,
            SpecialAttribute.AlchemistDoubleMastery => 143,
            SpecialAttribute.CookingDoubleMastery => 144,
            SpecialAttribute.ChaosRaidAttack => 145,
            SpecialAttribute.ChaosRaidAttackSpeed => 146,
            SpecialAttribute.ChaosRaidAccuracy => 147,
            SpecialAttribute.ChaosRaidHp => 148,
            SpecialAttribute.RecoveryBall => 149,
            SpecialAttribute.FieldBossExp => 150,
            SpecialAttribute.FieldBossDropRate => 151,
            SpecialAttribute.ReduceFieldBossReceiveDamage => 152,
            SpecialAttribute.AdditionalEffect95000016 => 153,
            SpecialAttribute.PetTrapReward => 154,
            SpecialAttribute.MiningEfficiency => 155,
            SpecialAttribute.BreedingEfficiency => 156,
            SpecialAttribute.GatheringEfficiency => 157,
            SpecialAttribute.FarmingEfficiency => 158,
            SpecialAttribute.ReduceDamageByTargetMaxHp => 159,
            SpecialAttribute.ReduceMesoRevivalFee => 160,
            SpecialAttribute.RidingRunSpeed => 161,
            SpecialAttribute.DungeonRewardMeso => 162,
            SpecialAttribute.ShopBuyingMeso => 163,
            SpecialAttribute.ItemBoxRewardMeso => 164,
            SpecialAttribute.ReduceRemakeOptionFee => 165,
            SpecialAttribute.ReduceAirTaxiFee => 166,
            SpecialAttribute.SocketUnlockProbability => 167,
            SpecialAttribute.ReduceGemstoneUpgradeFee => 168,
            SpecialAttribute.ReducePetRemakeOptionFee => 169,
            SpecialAttribute.RidingSpeed => 170,
            // SurvivalKillExp = 171
            // SurvivalTimeExp = 172
            // PhysicalDamage = 173
            // MagicalDamage = 174
            SpecialAttribute.ReduceGameItemSocketUnlockFee => 175,

            // Not mappable
            // SpecialAttribute.TonicDropRate => 4,
            // SpecialAttribute.GearDropRate => 5,
            // SpecialAttribute.MaidExp => 62,
            // SpecialAttribute.ReduceMaidRecipe => 63,
            // SpecialAttribute.AcquireManufacturingExp => 76,
            _ => byte.MaxValue,
        };
    }

    public static SkillEffectMetadata Convert(this TriggerSkill trigger) {
        SkillEffectMetadataCondition? condition = null;
        SkillEffectMetadataSplash? splash = null;
        if (trigger.splash) {
            splash = new SkillEffectMetadataSplash(
                Interval: trigger.interval,
                Delay: trigger.delay > int.MaxValue ? int.MaxValue : (int) trigger.delay,
                RemoveDelay: trigger.removeDelay,
                UseDirection: trigger.useDirection,
                ImmediateActive: trigger.immediateActive,
                NonTargetActive: trigger.nonTargetActive,
                OnlySensingActive: trigger.onlySensingActive,
                DependOnCasterState: trigger.dependOnCasterState,
                Independent: trigger.independent,
                Chain: trigger.chain ? new SkillEffectMetadataChain(trigger.chainDistance) : null);
        } else {
            var owner = SkillEntity.Target;
            if (trigger.skillOwner > 0 && Enum.IsDefined<SkillEntity>((SkillEntity) trigger.skillOwner)) {
                owner = (SkillEntity) trigger.skillOwner;
            }
            condition = new SkillEffectMetadataCondition(
                Condition: trigger.beginCondition.Convert(),
                Owner: owner,
                Target: (SkillEntity) trigger.skillTarget,
                OverlapCount: trigger.overlapCount,
                RandomCast: trigger.randomCast,
                ActiveByIntervalTick: trigger.activeByIntervalTick,
                DependOnDamageCount: trigger.dependOnDamageCount);
        }

        SkillEffectMetadata.Skill[] skills;
        if (trigger.linkSkillID.Length > 0) {
            skills = trigger.skillID
                .Zip(trigger.level, (skillId, level) => new { skillId, level })
                .Zip(trigger.linkSkillID, (skill, linkSkillId) => new SkillEffectMetadata.Skill(skill.skillId, skill.level, linkSkillId))
                .ToArray();
        } else {
            skills = trigger.skillID
                .Zip(trigger.level, (skillId, level) => new SkillEffectMetadata.Skill(skillId, level))
                .ToArray();
        }

        return new SkillEffectMetadata(
            FireCount: trigger.fireCount,
            Skills: skills,
            Condition: condition,
            Splash: splash);
    }

    public static SkillMetadataChange Convert(this ChangeSkill change) {
        return new SkillMetadataChange(
            Origin: new SkillMetadataChange.Skill(
                Id: change.originSkillID,
                Level: change.originSkillLevel),
            Effects: change.changeSkillCheckEffectID
                .Zip(change.changeSkillCheckEffectLevel, (effectId, effectLevel) => new {
                    effectId,
                    effectLevel
                })
                .Zip(change.changeSkillCheckEffectOverlapCount, (effect, overlapCount) => new SkillMetadataChange.Effect(effect.effectId, effect.effectLevel, overlapCount))
                .ToArray(),
            Skills: change.changeSkillID
                .Zip(change.changeSkillLevel, (skillId, level) => new SkillMetadataChange.Skill(skillId, level))
                .ToArray()
        );
    }

    public static BeginCondition Convert(this Maple2.File.Parser.Xml.Skill.BeginCondition beginCondition) {
        return new BeginCondition(
            Level: beginCondition.level,
            Gender: (Gender) beginCondition.gender,
            Mesos: beginCondition.money,
            Stat: beginCondition.stat.ToDictionary(),
            JobCode: beginCondition.job.Select(job => (JobCode) job.code).ToArray(),
            Probability: beginCondition.probability,
            CooldownTime: beginCondition.cooldownTime,
            OnlyShadowWorld: beginCondition.onlyShadowWorld || beginCondition.isShadowWorld,
            OnlyFlyableMap: beginCondition.onlyFlyableMap,
            Weapon: beginCondition.weapon.Select(weapon => new BeginConditionWeapon(
                new ItemType(1, (byte) weapon.lh),
                new ItemType(1, (byte) weapon.rh))).ToArray(),
            Target: Convert(beginCondition.skillTarget),
            Owner: Convert(beginCondition.skillOwner),
            Caster: Convert(beginCondition.skillCaster));
    }

    // We use this default to avoid writing useless checks
    private static readonly BeginConditionTarget DefaultBeginConditionTarget = new(Array.Empty<BeginConditionTarget.HasBuff>(), null);
    private static BeginConditionTarget? Convert(SubConditionTarget? target) {
        if (target == null) {
            return null;
        }

        var result = new BeginConditionTarget(
            Buff: ParseBuffs(target),
            Event: ParseEvent(target));

        return DefaultBeginConditionTarget.Equals(result) ? null : result;

        BeginConditionTarget.HasBuff[] ParseBuffs(SubConditionTarget data) {
            if (data.hasBuffID.Length == 0 || data.hasBuffID[0] == 0) {
                return Array.Empty<BeginConditionTarget.HasBuff>();
            }

            var hasBuff = new BeginConditionTarget.HasBuff[data.hasBuffID.Length];
            for (int i = 0; i < hasBuff.Length; i++) {
                hasBuff[i] = new BeginConditionTarget.HasBuff(
                    Id: data.hasBuffID[i],
                    Level: data.hasBuffLevel.Length > i ? data.hasBuffLevel[i] : (short) 0,
                    Owned: data.hasBuffOwner.Length > i && data.hasBuffOwner[i],
                    Count: data.hasBuffCount.Length > i ? data.hasBuffCount[i] : 0,
                    Compare: data.hasBuffCountCompare.Length > i ? Enum.Parse<CompareType>(data.hasBuffCountCompare[i]) : CompareType.Equals);
            }

            return hasBuff;
        }

        // Seems to only be used for test skills.
        // BeginConditionTarget.HasSkill? ParseSkill(SubConditionTarget data) {
        //     return data.hasSkillID > 0 ? new BeginConditionTarget.HasSkill(data.hasSkillID, data.hasSkillLevel) : null;
        // }

        BeginConditionTarget.EventCondition? ParseEvent(SubConditionTarget data) {
            if (data.eventCondition == 0) {
                return null;
            }

            return new BeginConditionTarget.EventCondition(
                Type: (EventConditionType) data.eventCondition,
                IgnoreOwner: data.ignoreOwnerEvent != 0,
                SkillIds: data.eventSkillID,
                BuffIds: data.eventEffectID);
        }
    }

    public static Dictionary<int, IReadOnlyDictionary<int, ItemOption>> ToDictionary(this IEnumerable<ItemOptionData> entries) {
        var results = new Dictionary<int, IReadOnlyDictionary<int, ItemOption>>();
        foreach (ItemOptionData entry in entries) {
            var optionEntries = new List<ItemOption.Entry>();
            foreach (BasicAttribute attribute in Enum.GetValues<BasicAttribute>()) {
                int[] value = entry.StatValue((byte) attribute);
                if (value.Length > 0) {
                    Debug.Assert(value.Length is 1 or 2);
                    var valueRange = new ItemOption.Range<int>(value[0], value.Length > 1 ? value[1] : value[0]);
                    optionEntries.Add(new ItemOption.Entry(BasicAttribute: attribute, Values: valueRange));
                }
                float[] rate = entry.StatRate((byte) attribute);
                if (rate.Length > 0) {
                    Debug.Assert(rate.Length is 1 or 2);
                    var rateRange = new ItemOption.Range<float>(rate[0], rate.Length > 1 ? rate[1] : rate[0]);
                    optionEntries.Add(new ItemOption.Entry(BasicAttribute: attribute, Rates: rateRange));
                }
            }

            foreach (SpecialAttribute attribute in Enum.GetValues<SpecialAttribute>()) {
                byte index = attribute.OptionIndex();
                if (index == byte.MaxValue) continue;

                SpecialAttribute fixAttribute = attribute.SgiTarget(entry.sgi_target);
                int[] value = entry.SpecialValue(index);
                if (value.Length > 0) {
                    Debug.Assert(value.Length is 1 or 2);
                    var valueRange = new ItemOption.Range<int>(value[0], value.Length > 1 ? value[1] : value[0]);
                    optionEntries.Add(new ItemOption.Entry(SpecialAttribute: fixAttribute, Values: valueRange));
                }
                float[] rate = entry.SpecialRate(index);
                if (rate.Length > 0) {
                    Debug.Assert(rate.Length is 1 or 2);
                    var rateRange = new ItemOption.Range<float>(rate[0], rate.Length > 1 ? rate[1] : rate[0]);
                    optionEntries.Add(new ItemOption.Entry(SpecialAttribute: fixAttribute, Rates: rateRange));
                }
            }

            if (!results.ContainsKey(entry.code)) {
                results[entry.code] = new Dictionary<int, ItemOption>();
            }

            // these entries are useless because they cannot be used.
            if (entry.optionNumPick.Length == 0 || (entry.optionNumPick[0] == 0 && entry.optionNumPick[1] == 0)) {
                continue;
            }

            var option = new ItemOption(
                MultiplyFactor: entry.multiply_factor,
                NumPick: new ItemOption.Range<int>(entry.optionNumPick[0], entry.optionNumPick[1]),
                Entries: optionEntries.ToArray());
            if (results[entry.code].ContainsKey(entry.grade)) {
                Console.WriteLine($"{entry.code} already has grade {entry.grade}");
            }

            (results[entry.code] as Dictionary<int, ItemOption>)!.Add(entry.grade, option);
        }

        return results;
    }

    public static ConditionMetadata.Parameters? ConvertCodes(this string[] codes) {
        if (codes.Length == 0) {
            return null;
        }

        if (codes.Length > 1) {
            var integers = new List<int>();
            var strings = new List<string>();
            foreach (string code in codes) {
                if (int.TryParse(code, out int intCode)) {
                    integers.Add(intCode);
                } else {
                    strings.Add(code);
                }
            }

            return new ConditionMetadata.Parameters(
                Strings: strings.Count == 0 ? null : strings.ToArray(),
                Integers: integers.Count == 0 ? null : integers.ToArray());
        }

        string[] split = codes[0].Split('-');
        if (split.Length > 1) {
            return new ConditionMetadata.Parameters(
                Range: new ConditionMetadata.Range<int>(int.Parse(split[0]), int.Parse(split[1])));
        }

        if (int.TryParse(codes[0], out int integerResult)) {
            return new ConditionMetadata.Parameters(Integers: [integerResult]);
        }
        return new ConditionMetadata.Parameters(Strings: [codes[0]]);
    }
}
