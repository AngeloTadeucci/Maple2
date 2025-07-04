using System.ComponentModel;

namespace Maple2.Model.Enum;

public enum SpecialAttribute : byte {
    None = 0,
    [Description("s_item_opt_sa_improve_acquire_exp: Experience from Enemies")]
    Experience = 1,
    [Description("s_item_opt_sa_improve_acquire_meso: Mesos from Monsters")]
    Meso = 2,
    [Description("s_item_opt_sa_improve_speed_swim: Swim Speed")]
    SwimSpeed = 3,
    [Description("s_item_opt_sa_improve_speed_dash: Dash Distance")]
    DashDistance = 4,
    [Description("s_item_opt_sa_improve_acquire_potion: Tonic Drop Rate")]
    TonicDropRate = 5,
    [Description("s_item_opt_sa_improve_acquire_equipment: Gear Drop Rate")]
    GearDropRate = 6,
    [Description("s_item_opt_sa_improve_damage_final: Total Damage Bonus")]
    TotalDamage = 7,
    [Description("s_item_opt_sa_improve_damage_critical: Critical Damage")]
    CriticalDamage = 8,
    [Description("s_item_opt_sa_improve_damage_normalNpc: Common Monster Damage")]
    NormalNpcDamage = 9,
    [Description("s_item_opt_sa_improve_damage_leaderNpc: Leader Monster Damage")]
    LeaderNpcDamage = 10,
    [Description("s_item_opt_sa_improve_damage_namedNpc: Elite Monster Damage")]
    EliteNpcDamage = 11,
    [Description("s_item_opt_sa_improve_damage_bossNpc: Bonus Boss Damage")]
    BossNpcDamage = 12,
    [Description("s_item_opt_sa_improve_recovery_hp_dokill: Heal from Beating Enemies")]
    HpOnKill = 13,
    [Description("s_item_opt_sa_improve_recovery_sp_dokill: Spirit from Beating Enemies")]
    SpiritOnKill = 14,
    [Description("s_item_opt_sa_improve_recovery_ep_dokill: Stamina from Beating Enemies")]
    StaminaOnKill = 15,
    [Description("s_item_opt_sa_improve_recovery_regen_doheal: Recovery Bonus")]
    RecoveryBonus = 16,
    [Description("s_item_opt_sa_improve_recovery_regen_receiveheal: Bonus Recovery from Allies")]
    BonusRecoveryFromAlly = 17,
    [Description("s_item_opt_sa_improve_elements_ice: Ice Damage Bonus")]
    IceDamage = 18,
    [Description("s_item_opt_sa_improve_elements_fire: Fire Damage Bonus")]
    FireDamage = 19,
    [Description("s_item_opt_sa_improve_elements_dark: Dark Damage Bonus")]
    DarkDamage = 20,
    [Description("s_item_opt_sa_improve_elements_light: Holy Damage Bonus")]
    HolyDamage = 21,
    [Description("s_item_opt_sa_improve_elements_poison: Poison Damage Bonus")]
    PoisonDamage = 22,
    [Description("s_item_opt_sa_improve_elements_thunder: Electric Damage Bonus")]
    ElectricDamage = 23,
    [Description("s_item_opt_sa_improve_damage_nearrange: Melee Damage Bonus")]
    MeleeDamage = 24,
    [Description("s_item_opt_sa_improve_damage_longrange: Ranged Damage Bonus")]
    RangedDamage = 25,
    [Description("s_item_opt_sa_improve_piercing_par: Physical Piercing")]
    PhysicalPiercing = 26,
    [Description("s_item_opt_sa_improve_piercing_mar: Magic Piercing")]
    MagicalPiercing = 27,
    [Description("s_item_opt_sa_reduce_elements_ice: Ice Damage Reduction")]
    ReduceIceDamage = 28,
    [Description("s_item_opt_sa_reduce_elements_fire: Fire Damage Reduction")]
    ReduceFireDamage = 29,
    [Description("s_item_opt_sa_reduce_elements_dark: Dark Damage Reduction")]
    ReduceDarkDamage = 30,
    [Description("s_item_opt_sa_reduce_elements_light: Holy Damage Reduction")]
    ReduceHolyDamage = 31,
    [Description("s_item_opt_sa_reduce_elements_poison: Poison Damage Reduction")]
    ReducePoisonDamage = 32,
    [Description("s_item_opt_sa_reduce_elements_thunder: Electric Damage Reduction")]
    ReduceElectricDamage = 33,
    [Description("s_item_opt_sa_reduce_time_stun: Stun Duration Reduction")]
    ReduceStun = 34,
    [Description("s_item_opt_sa_reduce_time_cooldown: Skill Cooldown Reduction")]
    ReduceCooldown = 35,
    [Description("s_item_opt_sa_reduce_time_condition: Debuff Duration Reduction")]
    ReduceDebuff = 36,
    [Description("s_item_opt_sa_reduce_damage_nearrange: Melee Reduction")]
    ReduceMeleeDamage = 37,
    [Description("s_item_opt_sa_reduce_damage_longrange: Ranged Damage Reduction")]
    ReduceRangedDamage = 38,
    [Description("s_item_opt_sa_reduce_distance_knockBack: Knockback Distance Reduction")]
    ReduceKnockBack = 39,
    [Description("s_item_opt_sa_probability_stun_nearrange: Melee Attacks Stun")]
    MeleeStun = 40,
    [Description("s_item_opt_sa_probability_stun_longrange: Ranged Attacks Stun")]
    RangedStun = 41,
    [Description("s_item_opt_sa_probability_knockback_nearrange: Melee Attacks Gain Knockback")]
    MeleeKnockBack = 42,
    [Description("s_item_opt_sa_probability_knockback_longrange: Ranged Attacks Gain Knockback")]
    RangedKnockBack = 43,
    [Description("s_item_opt_sa_probability_cannotmove_nearrange: Melee Attacks Immobilize Target")]
    MeleeImmobilize = 44,
    [Description("s_item_opt_sa_probability_cannotmove_longrange: Ranged Attacks Immobilize Target")]
    RangedImmobilize = 45,
    [Description("s_item_opt_sa_probability_splashdamage_nearrange: Melee Attacks Damage in Area")]
    MeleeSplashDamage = 46,
    [Description("s_item_opt_sa_probability_splashdamage_longrange: Ranged Attacks Damage in Area")]
    RangedSplashDamage = 47,
    [Description("s_item_opt_sa_improve_npckill_dropitem_incrate: Enemy Item Drop Chance")]
    DropRate = 48,
    [Description("s_item_opt_sa_improve_acquire_questreward_exp: Experience from Quests")]
    QuestExp = 49,
    [Description("s_item_opt_sa_improve_acquire_questreward_meso: Mesos from Quests")]
    QuestMeso = 50,
    [Description("")]
    InvokeEffect1 = 51,
    [Description("")]
    InvokeEffect2 = 52,
    [Description("")]
    InvokeEffect3 = 53,
    [Description("s_item_opt_sa_improve_damage_pvp: PvP Damage")]
    PvpDamage = 54,
    [Description("s_item_opt_sa_reduce_damage_pvp: PvP Defense")]
    ReducePvpDamage = 55,
    [Description("s_item_opt_sa_improve_guild_exp: Guild Experience")]
    GuildExp = 56,
    [Description("s_item_opt_sa_improve_guild_coin: Guild Coins")]
    GuildCoin = 57,
    [Description("s_item_opt_sa_improve_massive_event_expball: Experience Orb Drop Rate")]
    MassiveEventExpBall = 58,
    [Description("s_item_opt_sa_improve_acquire_fishing_exp: Experience from Fishing")]
    FishingExp = 59,
    [Description("s_item_opt_sa_improve_acquire_arcade_exp: Arcade Clear Experience")]
    ArcadeExp = 60,
    [Description("s_item_opt_sa_improve_acquire_playinstrument_exp: Experience from Performance")]
    PlayInstrumentExp = 61,
    [Description("s_item_opt_sa_improve_maid_mood: Assistant Experience")]
    MaidExp = 62,
    [Description("s_item_opt_sa_reduce_maid_recipe: Assistant Craft Material Discount")]
    ReduceMaidRecipe = 63,
    [Description("s_item_opt_sa_reduce_meso_trade_fee: Meso Handling Fee Discount")]
    ReduceMesoTradeFee = 64,
    [Description("s_item_opt_sa_reduce_enchant_material_fee: Enchant Material Discount")]
    ReduceEnchantMaterialFee = 65,
    [Description("s_item_opt_sa_reduce_merat_revival_fee: Meret Revive Discount")]
    ReduceMeretRevivalFee = 66,
    [Description("s_item_opt_sa_improve_mining_reward_item: Mining Quantity Earned")]
    MiningRewardItem = 67,
    [Description("s_item_opt_sa_improve_breeding_reward_item: Ranching Quantity Earned")]
    BreedingRewardItem = 68,
    [Description("s_item_opt_sa_improve_blacksmithing_reward_mastery: Smithing Experience Earned")]
    SmithingRewardMastery = 69,
    [Description("s_item_opt_sa_improve_engraving_reward_mastery: Handicraft Mastery Earned")]
    EngravingRewardMastery = 70,
    [Description("s_item_opt_sa_improve_gathering_reward_item: Foraging Quantity Earned")]
    GatheringRewardItem = 71,
    [Description("s_item_opt_sa_improve_farming_reward_item: Farming Quantity Earned")]
    FarmingRewardItem = 72,
    [Description("s_item_opt_sa_improve_alchemist_reward_mastery: Alchemy Mastery Earned")]
    AlchemistRewardMastery = 73,
    [Description("s_item_opt_sa_improve_cooking_reward_mastery: Cooking Mastery Earned")]
    CookingRewardMastery = 74,
    [Description("s_item_opt_sa_improve_acquire_gathering_exp: Experience from Foraging")]
    AcquireGatheringExp = 75,
    [Description("s_item_opt_sa_improve_acquire_manufacturing_exp: Experience from Crafting")]
    AcquireManufacturingExp = 76,
    [Description("s_item_opt_sa_skill_levelup_tier_1: Lv. 1 (1) Acquired Skill Level")]
    SkillLevelUpTier1 = 77,
    [Description("s_item_opt_sa_skill_levelup_tier_2: Lv. 1 (2) Acquired Skill Level")]
    SkillLevelUpTier2 = 78,
    [Description("s_item_opt_sa_skill_levelup_tier_3: Lv. 10 Acquired Skill Level")]
    SkillLevelUpTier3 = 79,
    [Description("s_item_opt_sa_skill_levelup_tier_4: Lv. 13 Acquired Skill Level")]
    SkillLevelUpTier4 = 80,
    [Description("s_item_opt_sa_skill_levelup_tier_5: Lv. 16 Acquired Skill Level")]
    SkillLevelUpTier5 = 81,
    [Description("s_item_opt_sa_skill_levelup_tier_6: Lv. 19 Acquired Skill Level")]
    SkillLevelUpTier6 = 82,
    [Description("s_item_opt_sa_skill_levelup_tier_7: Lv. 22 Acquired Skill Level")]
    SkillLevelUpTier7 = 83,
    [Description("s_item_opt_sa_skill_levelup_tier_8: Lv. 25 Acquired Skill Level")]
    SkillLevelUpTier8 = 84,
    [Description("s_item_opt_sa_skill_levelup_tier_9: Lv. 28 Acquired Skill Level")]
    SkillLevelUpTier9 = 85,
    [Description("s_item_opt_sa_skill_levelup_tier_10: Lv. 31 Acquired Skill Level")]
    SkillLevelUpTier10 = 86,
    [Description("s_item_opt_sa_skill_levelup_tier_11: Lv. 34 Acquired Skill Level")]
    SkillLevelUpTier11 = 87,
    [Description("s_item_opt_sa_skill_levelup_tier_12: Lv. 37 Acquired Skill Level")]
    SkillLevelUpTier12 = 88,
    [Description("s_item_opt_sa_skill_levelup_tier_13: Lv. 40 Acquired Skill Level")]
    SkillLevelUpTier13 = 89,
    [Description("s_item_opt_sa_skill_levelup_tier_14: Lv. 43 Acquired Skill Level")]
    SkillLevelUpTier14 = 90,
    [Description("s_item_opt_sa_improve_massive_ox_exp: OX Quiz Experience")]
    MassiveOxExp = 91,
    [Description("s_item_opt_sa_improve_massive_trapmaster_exp: Trap Master Experience")]
    MassiveTrapMasterExp = 92,
    [Description("s_item_opt_sa_improve_massive_finalsurvival_exp: Sole Survivor Experience")]
    MassiveFinalSurvivalExp = 93,
    [Description("s_item_opt_sa_improve_massive_crazyrunner_exp: Crazy Runners Experience")]
    MassiveCrazyRunnerExp = 94,
    [Description("s_item_opt_sa_improve_massive_escape_exp: Ludibrium Escape Experience")]
    MassiveEscapeExp = 95,
    [Description("s_item_opt_sa_improve_massive_springbeach_exp: Spring Beach Experience")]
    MassiveSpringBeachExp = 96,
    [Description("s_item_opt_sa_improve_massive_dancedance_exp: Dance Dance Stop Experience")]
    MassiveDanceDanceExp = 97,
    [Description("s_item_opt_sa_improve_massive_ox_msp: OX Quiz Movement Speed")]
    MassiveOxSpeed = 98,
    [Description("s_item_opt_sa_improve_massive_trapmaster_msp: Trap Master Movement Speed")]
    MassiveTrapMasterSpeed = 99,
    [Description("s_item_opt_sa_improve_massive_finalsurvival_msp: Sole Survivor Movement Speed")]
    MassiveFinalSurvivalSpeed = 100,
    [Description("s_item_opt_sa_improve_massive_crazyrunner_msp: Crazy Runners Movement Speed")]
    MassiveCrazyRunnerSpeed = 101,
    [Description("s_item_opt_sa_improve_massive_escape_msp: Ludibrium Escape Movement Speed")]
    MassiveEscapeSpeed = 102,
    [Description("s_item_opt_sa_improve_massive_springbeach_msp: Spring Beach Movement Speed")]
    MassiveSpringBeachSpeed = 103,
    [Description("s_item_opt_sa_improve_massive_dancedance_msp: Dance Dance Stop Movement Speed")]
    MassiveDanceDanceSpeed = 104,
    [Description("s_item_opt_sa_npc_hit_reward_sp_ball: Chance to Generate Spirit Orbs on Attack")]
    NpcHitRewardSpBall = 105,
    [Description("s_item_opt_sa_npc_hit_reward_ep_ball: Chance to Generate Stamina Orbs on Attack")]
    NpcHitRewardEpBall = 106,
    [Description("s_item_opt_sa_improve_honor_token: Valor Tokens")]
    HonorToken = 107,
    [Description("s_item_opt_sa_improve_pvp_exp: PvP Experience")]
    PvpExp = 108,
    [Description("s_item_opt_sa_improve_darkstream_damage: Dark Descent Damage Bonus")]
    DarkStreamDamage = 109,
    [Description("s_item_opt_sa_reduce_darkstream_recive_damage: Decreases Damage in the Dark Descent")]
    ReduceDarkStreamReceiveDamage = 110,
    [Description("s_item_opt_sa_improve_darkstream_evp: Dark Descent Evasion")]
    DarkStreamEvp = 111,
    [Description("s_item_opt_sa_fishing_double_mastery: Chance for 2x Fishing Mastery")]
    FishingDoubleMastery = 112,
    [Description("s_item_opt_sa_playinstrument_double_mastery: Chance for 2x Performance Mastery")]
    PlayInstrumentDoubleMastery = 113,
    [Description("s_item_opt_sa_complete_fieldmission_msp: Movement Speed in Explored Areas")]
    CompleteFieldMissionSpeed = 114,
    [Description("s_item_opt_sa_improve_glide_vertical_velocity: Air Mount Ascent Speed")]
    GlideVerticalVelocity = 115,
    [Description("s_item_opt_sa_additionaleffect_95000018: Being fixed")]
    AdditionalEffect95000018 = 116,
    [Description("s_item_opt_sa_additionaleffect_95000012: Enemy Defense on Hit")]
    AdditionalEffect95000012 = 117,
    [Description("s_item_opt_sa_additionaleffect_95000014: Enemy Attack on Hit")]
    AdditionalEffect95000014 = 118,
    [Description("s_item_opt_sa_additionaleffect_95000020: Total Damage when Enemy within 5m")]
    AdditionalEffect95000020 = 119,
    [Description("s_item_opt_sa_additionaleffect_95000021: Total Damage when 3 Enemies within 5m")]
    AdditionalEffect95000021 = 120,
    [Description("s_item_opt_sa_additionaleffect_95000022: Total Damage when Spirit Is 80 or More")]
    AdditionalEffect95000022 = 121,
    [Description("s_item_opt_sa_additionaleffect_95000023: Total Damage when Stamina Full")]
    AdditionalEffect95000023 = 122,
    [Description("s_item_opt_sa_additionaleffect_95000024: Total Damage when Herb Effects Active")]
    AdditionalEffect95000024 = 123,
    [Description("s_item_opt_sa_additionaleffect_95000025: World Boss Damage")]
    AdditionalEffect95000025 = 124,
    [Description("s_item_opt_sa_additionaleffect_95000026: 95000026")]
    AdditionalEffect95000026 = 125,
    [Description("s_item_opt_sa_additionaleffect_95000027: 95000027")]
    AdditionalEffect95000027 = 126,
    [Description("s_item_opt_sa_additionaleffect_95000028: 95000028")]
    AdditionalEffect95000028 = 127,
    [Description("s_item_opt_sa_additionaleffect_95000029: 95000029")]
    AdditionalEffect95000029 = 128,
    [Description("s_item_opt_sa_reduce_recovery_ep_inv: Stamina Recovery Speed")]
    ReduceRecoveryEpInv = 129,
    [Description("s_item_opt_sa_improve_stat_wap_u: Maximum Weapon Attack")]
    MaxWeaponAttack = 130,
    [Description("s_item_opt_sa_mining_double_reward: Chance for 2x Mining Production")]
    MiningDoubleReward = 131,
    [Description("s_item_opt_sa_breeding_double_reward: Chance for 2x Ranching Production")]
    BreedingDoubleReward = 132,
    [Description("s_item_opt_sa_gathering_double_reward: Chance for 2x Foraging Production")]
    GatheringDoubleReward = 133,
    [Description("s_item_opt_sa_farming_double_reward: Chance for 2x Farming Production")]
    FarmingDoubleReward = 134,
    [Description("s_item_opt_sa_blacksmithing_double_reward: Chance for 2x Smithing Production")]
    SmithingDoubleReward = 135,
    [Description("s_item_opt_sa_engraving_double_reward: Chance for 2x Handicraft Production")]
    EngravingDoubleReward = 136,
    [Description("s_item_opt_sa_alchemist_double_reward: Chance for 2x Alchemy Production")]
    AlchemistDoubleReward = 137,
    [Description("s_item_opt_sa_cooking_double_reward: Chance for 2x Cooking Production")]
    CookingDoubleReward = 138,
    [Description("s_item_opt_sa_mining_double_mastery: Chance for 2x Mining Mastery")]
    MiningDoubleMastery = 139,
    [Description("s_item_opt_sa_breeding_double_mastery: Chance for 2x Ranching Mastery")]
    BreedingDoubleMastery = 140,
    [Description("s_item_opt_sa_gathering_double_mastery: Chance for 2x Foraging Mastery")]
    GatheringDoubleMastery = 141,
    [Description("s_item_opt_sa_farming_double_mastery: Chance for 2x Farming Mastery")]
    FarmingDoubleMastery = 142,
    [Description("s_item_opt_sa_blacksmithing_double_mastery: Chance for 2x Smithing Mastery")]
    SmithingDoubleMastery = 143,
    [Description("s_item_opt_sa_engraving_double_mastery: Chance for 2x Handicraft Mastery")]
    EngravingDoubleMastery = 144,
    [Description("s_item_opt_sa_alchemist_double_mastery: Chance for 2x Alchemy Mastery")]
    AlchemistDoubleMastery = 145,
    [Description("s_item_opt_sa_cooking_double_mastery: Chance for 2x Cooking Mastery")]
    CookingDoubleMastery = 146,
    [Description("s_item_opt_sa_improve_chaosraid_wap: Weapon Attack in Chaos Raids")]
    ChaosRaidAttack = 147,
    [Description("s_item_opt_sa_improve_chaosraid_asp: Attack Speed in Chaos Raids")]
    ChaosRaidAttackSpeed = 148,
    [Description("s_item_opt_sa_improve_chaosraid_atp: Accuracy in Chaos Raids")]
    ChaosRaidAccuracy = 149,
    [Description("s_item_opt_sa_improve_chaosraid_hp: Health in Chaos Raids")]
    ChaosRaidHp = 150,
    [Description("s_item_opt_sa_improve_recovery_ball: Stamina and Spirit from Orbs")]
    RecoveryBall = 151,
    [Description("s_item_opt_sa_improve_fieldboss_kill_exp: World Boss Experience")]
    FieldBossExp = 152,
    [Description("s_item_opt_sa_improve_fieldboss_kill_drop: World Boss Item Drop Rate")]
    FieldBossDropRate = 153,
    [Description("s_item_opt_sa_reduce_fieldboss_recive_damage: Reduced Damage from World Bosses")]
    ReduceFieldBossReceiveDamage = 154,
    [Description("s_item_opt_sa_additionaleffect_95000016: 95000016")]
    AdditionalEffect95000016 = 155,
    [Description("s_item_opt_sa_improve_pettrap_reward: Max Pet Capture Reward Count")]
    PetTrapReward = 156,
    [Description("s_item_opt_sa_ming_multiaction: Mining Efficiency")]
    MiningEfficiency = 157,
    [Description("s_item_opt_sa_breeding_multiaction: Ranching Efficiency")]
    BreedingEfficiency = 158,
    [Description("s_item_opt_sa_gathering_multiaction: Foraging Efficiency")]
    GatheringEfficiency = 159,
    [Description("s_item_opt_sa_farming_multiaction: Farming Efficiency")]
    FarmingEfficiency = 160,
    [Description("s_item_opt_sa_improve_massive_sh_crazyrunner_exp: Shanghai Crazy Runners Experience")]
    MassiveShCrazyRunnerExp = 161,
    [Description("s_item_opt_sa_improve_massive_sh_crazyrunner_msp: Shanghai Crazy Runners Movement Speed")]
    MassiveShCrazyRunnerSpeed = 162,
    [Description("s_item_opt_sa_reduce_damage_by_targetmaxhp: Health-Based Damage Reduction")]
    ReduceDamageByTargetMaxHp = 163,
    [Description("s_item_opt_sa_reduce_meso_revival_fee")]
    ReduceMesoRevivalFee = 164,
    [Description("s_item_opt_sa_improve_riding_run_speed")]
    RidingRunSpeed = 165,
    [Description("s_item_opt_sa_improve_dungeon_reward_meso")]
    DungeonRewardMeso = 166,
    [Description("s_item_opt_sa_improve_shop_buying_meso")]
    ShopBuyingMeso = 167,
    [Description("s_item_opt_sa_improve_itembox_reward_meso")]
    ItemBoxRewardMeso = 168,
    [Description("s_item_opt_sa_reduce_remakeoption_fee")]
    ReduceRemakeOptionFee = 169,
    [Description("s_item_opt_sa_reduce_airtaxi_fee")]
    ReduceAirTaxiFee = 170,
    [Description("s_item_opt_sa_improve_socket_unlock_probability")]
    SocketUnlockProbability = 171,
    [Description("s_item_opt_sa_reduce_gemstone_upgrade_fee")]
    ReduceGemstoneUpgradeFee = 172,
    [Description("s_item_opt_sa_reduce_pet_remakeoption_fee")]
    ReducePetRemakeOptionFee = 173,
    [Description("s_item_opt_sa_improve_riding_speed")]
    RidingSpeed = 174,
    [Description("s_item_opt_sa_improve_survival_kill_exp")]
    ImproveSurvivalKillExp = 175,
    [Description("s_item_opt_sa_improve_survival_time_exp")]
    ImproveSurvivalTimeExp = 176,
    [Description("s_item_opt_sa_offensive_physicaldamage")]
    OffensivePhysicalDamage = 177,
    [Description("s_item_opt_sa_offensive_magicaldamage")]
    OffensiveMagicalDamage = 178,
    [Description("s_item_opt_sa_reduce_gameitem_socket_unlock_fee")]
    ReduceGameItemSocketUnlockFee = 179,
}
