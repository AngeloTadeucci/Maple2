// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Enum;

public enum ActorState : byte {
    [Description("gosNone")]
    None = 0,
    [Description("gosIdle")]
    Idle = 1,
    [Description("gosWalk")]
    Walk = 2,
    [Description("gosCrawl")]
    Crawl = 3,
    [Description("gosLand")]
    Land = 4,
    [Description("gosFall")]
    Fall = 5,
    [Description("gosJump")]
    Jump = 6,
    [Description("gosJumpTo")]
    JumpTo = 7,
    [Description("gosLadder")]
    Ladder = 8,
    [Description("gosRope")]
    Rope = 9,
    [Description("gosSit")]
    Sit = 10,
    [Description("gosEmotion")]
    Emotion = 11,
    [Description("gosDead")]
    Dead = 12,
    [Description("gosHit")]
    Hit = 13,
    [Description("gosWaittingSelect")]
    WaitingSelect = 14,
    [Description("gosEmotionIdle")]
    EmotionIdle = 15,
    [Description("gosPcSkill")]
    PcSkill = 16,
    [Description("gosSpawn")]
    Spawn = 17,
    [Description("gosStun")]
    Stun = 18,
    [Description("gosDash")]
    Dash = 20,
    [Description("gosPush")]
    Push = 21,
    [Description("gosTalk")]
    Talk = 22,
    [Description("gosRegen")]
    Regen = 23,
    [Description("gosRevival")]
    Revival = 24,
    [Description("gosInteraction")]
    Interaction = 25,
    [Description("gosInteractionNpc")]
    InteractionNpc = 26,
    [Description("gosSwim")]
    Swim = 27,
    [Description("gosSwimDash")]
    SwimDash = 28,
    [Description("gosClimb")]
    Climb = 29,
    [Description("gosGlide")]
    Glide = 30,
    [Description("gosFallDamage")]
    FallDamage = 31,
    [Description("gosHold")]
    Hold = 32,
    [Description("gosRide")]
    Ride = 33,
    [Description("gosTransform")]
    Transform = 34,
    [Description("gosPuppet")]
    Puppet = 35,
    [Description("gosFloat")]
    Float = 36,
    [Description("gosTaxiCall")]
    TaxiCall = 37,
    [Description("gosSummon")]
    Summon = 38,
    [Description("gosGotoHome")]
    GotoHome = 39,
    [Description("gosPvPWinLose")]
    PvPWinLose = 40,
    [Description("gosUseFurniture")]
    UseFurniture = 41,
    [Description("gosCashCall")]
    CashCall = 42,
    [Description("gosGrabTarget")]
    GrabTarget = 43,
    [Description("gosRecall")]
    Recall = 44,
    [Description("gosFishing")]
    Fishing = 45,
    [Description("gosFishingFloat")]
    FishingFloat = 46,
    [Description("gosPlayInstrument")]
    PlayInstrument = 47,
    [Description("gosSummonRevive")]
    SummonRevive = 48,
    [Description("gosSummonExpire")]
    SummonExpire = 49,
    [Description("gosSummonItemPick")]
    SummonItemPick = 50,
    [Description("gosWarp")]
    Warp = 51,
    [Description("gosTrace")]
    Trace = 52,
    [Description("gosTriggerEmotion")]
    TriggerEmotion = 53,
    [Description("gosHomeConvenient")]
    HomeConvenient = 54,
    [Description("gosHomemade")]
    Homemade = 55,
    [Description("gosSummonPortal")]
    SummonPortal = 56,
    [Description("gosAutoInteraction")]
    AutoInteraction = 57,
    [Description("gosCoupleEmotion")]
    CoupleEmotion = 58,
    [Description("gosBanner")]
    Banner = 59,
    [Description("gosMicroGameRps")]
    MicroGameRps = 60,
    [Description("gosReact")]
    React = 61,
    [Description("gosTreeWatering")]
    TreeWatering = 62,
    [Description("gosObserver")]
    Observer = 63,
    [Description("gosNurturing")]
    Nurturing = 64,
    [Description("gosSkillMagicControl")]
    SkillMagicControl = 65,
    [Description("gosGroggy")]
    Groggy = 66,
    [Description("gosRescuee")]
    Rescuee = 67,
    [Description("gosMicroGameCoupleDance")]
    MicroGameCoupleDance = 69,
    [Description("gosTriggerFollowNpc")]
    TriggerFollowNpc = 70,
    [Description("gosWeddingEmotion")]
    WeddingEmotion = 71,
    [Description("gosMAX")]
    Max = 72,
}

public enum ActorSubState : byte {
    [Description("SubState_None")]
    None = 0,

    [Description("StateIdle_Idle")]
    Idle_Idle = 1,
    [Description("StateIdle_Bore_A")]
    Idle_Bore_A = 2,
    [Description("StateIdle_Bore_B")]
    Idle_Bore_B = 3,
    [Description("StateIdle_Bore_C")]
    Idle_Bore_C = 4,
    [Description("StateIdle_Bore_D")]
    Idle_Bore_D = 5,
    [Description("StateIdle_Talk")]
    Idle_Talk = 6,
    [Description("StateIdle_Water")]
    Idle_Water = 7,
    [Description("StateIdle_QuestCraft")]
    Idle_QuestCraft = 8,
    [Description("StateIdle_UGCCraft")]
    Idle_UGCCraft = 9,
    [Description("StateIdle_Fitting")]
    Idle_Fitting = 10,
    [Description("StateIdle_Fitting_Basic")]
    Idle_Fitting_Basic = 11,
    [Description("StateIdle_Stunt")]
    Idle_Stunt = 12,
    [Description("StateIdle_CreateCharComplete")]
    Idle_CreateCharComplete = 13,
    [Description("StateIdle_CharacterList")]
    Idle_CharacterList = 14,
    [Description("StateIdle_Happy")]
    Idle_Happy = 15,
    [Description("StateIdle_UseSkill")]
    Idle_UseSkill = 16,
    [Description("StateIdle_AutoRevive")]
    Idle_AutoRevive = 17,
    [Description("StateIdle_NotUseSkill")]
    Idle_NotUseSkill = 18,
    [Description("StateIdle_Nutrient")]
    Idle_Nutrient = 19,
    [Description("StateIdle_CharacterSelect_Bore")]
    Idle_CharacterSelect_Bore = 20,
    [Description("StateIdle_CharacterSelect_Bore_Idle")]
    Idle_CharacterSelect_Bore_Idle = 21,
    [Description("StateIdle_ListenMusic")]
    Idle_ListenMusic = 22,

    [Description("StateWalk_Stunt")]
    Walk_Stunt = 23,
    [Description("StateWalk_Running")]
    Walk_Running = 24,
    [Description("StateWalk_Walking")]
    Walk_Walking = 25,
    [Description("StateWalk_Booster")]
    Walk_Booster = 26,

    [Description("StateCrawl_Idle")]
    Crawl_Idle = 27,
    [Description("StateCrawl_Crawling")]
    Crawl_Crawling = 28,

    [Description("StateDash_ForwardDash")]
    Dash_ForwardDash = 29,
    [Description("StateDash_Falling")]
    Dash_Falling = 30,
    [Description("StateDash_Landing")]
    Dash_Landing = 31,
    [Description("StateDash_Hit")]
    Dash_Hit = 32,

    [Description("StateJump_Jump")]
    Jump_Jump = 33,
    [Description("StateJump_JumpSpecial")]
    Jump_JumpSpecial = 34,
    [Description("StateJump_Jump2")]
    Jump_Jump2 = 35,
    [Description("StateJump_JumpSpecial2")]
    Jump_JumpSpecial2 = 36,
    [Description("StateJump_JumpRandom")]
    Jump_JumpRandom = 37,
    [Description("StateJump_Hit")]
    Jump_Hit = 38,
    [Description("StateJump_Stunt")]
    Jump_Stunt = 39,

    [Description("StateJumpTo_Idle")]
    JumpTo_Idle = 40,
    [Description("StateJumpTo_Done")]
    JumpTo_Done = 41,

    [Description("StateLadder_Idle")]
    Ladder_Idle = 42,
    [Description("StateLadder_Up")]
    Ladder_Up = 43,
    [Description("StateLadder_Down")]
    Ladder_Down = 44,
    [Description("StateLadder_UpLand")]
    Ladder_UpLand = 45,
    [Description("StateLadder_DownLand")]
    Ladder_DownLand = 46,
    [Description("StateLadder_UpTake")]
    Ladder_UpTake = 47,
    [Description("StateLadder_DownTake")]
    Ladder_DownTake = 48,
    [Description("StateLadder_MiddleTake")]
    Ladder_MiddleTake = 49,
    [Description("StateLadder_Fall")]
    Ladder_Fall = 50,

    [Description("StateRope_Idle")]
    Rope_Idle = 51,
    [Description("StateRope_Up")]
    Rope_Up = 52,
    [Description("StateRope_Down")]
    Rope_Down = 53,
    [Description("StateRope_Middle")]
    Rope_Middle = 54,
    [Description("StateRope_Turn")]
    Rope_Turn = 55,
    [Description("StateRope_Take")]
    Rope_Take = 56,
    [Description("StateRope_Fall")]
    Rope_Fall = 57,

    [Description("StateSkill_Default")]
    Skill_Default = 58,
    [Description("StateSkill_HoldAttack")]
    Skill_HoldAttack = 59,
    [Description("StateSkill_MagicControl")]
    Skill_MagicControl = 60,

    [Description("StateStun_LieStart")]
    Stun_LieStart = 61,
    [Description("StateStun_LieKeep")]
    Stun_LieKeep = 62,
    [Description("StateStun_LieStop")]
    Stun_LieStop = 63,
    [Description("StateStun_Standing")]
    Stun_Standing = 64,
    [Description("StateStun_Freezing")]
    Stun_Freezing = 65,
    [Description("StateStun_Snare")]
    Stun_Snare = 66,
    [Description("StateStun_Vomit")]
    Stun_Vomit = 67,
    [Description("StateStun_Frozen")]
    Stun_Frozen = 68,
    [Description("StateStun_Stuck")]
    Stun_Stuck = 69,
    [Description("StateStun_Custom")]
    Stun_Custom = 70,

    [Description("StateTalk_Start")]
    Talk_Start = 71,
    [Description("StateTalk_Loop")]
    Talk_Loop = 72,
    [Description("StateTalk_Idle")]
    Talk_Idle = 73,
    [Description("StateTalk_EnchantSuccess")]
    Talk_EnchantSuccess = 74,
    [Description("StateTalk_EnchantFail")]
    Talk_EnchantFail = 75,

    [Description("StateWaitingSelect_CHANGE_CAP")]
    WaitingSelect_ChangeCap = 76,
    [Description("StateWaitingSelect_CHANGE_IDLE")]
    WaitingSelect_ChangeIdle = 77,
    [Description("StateWaitingSelect_CHANGE_BODY")]
    WaitingSelect_ChangeBody = 78,
    [Description("StateWaitingSelect_CHANGE_HEAD")]
    WaitingSelect_ChangeHead = 79,
    [Description("StateWaitingSelect_CHANGE_HAIR")]
    WaitingSelect_ChangeHair = 80,
    [Description("StateWaitingSelect_CHANGE_GLOVE")]
    WaitingSelect_ChangeGlove = 81,
    [Description("StateWaitingSelect_CHANGE_MANTLE")]
    WaitingSelect_ChangeMantle = 82,
    [Description("StateWaitingSelect_CHANGE_SHOES")]
    WaitingSelect_ChangeShoes = 83,
    [Description("StateWaitingSelect_CHANGE_WEAPON")]
    WaitingSelect_ChangeWeapon = 84,
    [Description("StateWaitingSelect_CHANGE_WEAPON_IDLE")]
    WaitingSelect_ChangeWeaponIdle = 85,

    [Description("StateClimb_Idle")]
    Climb_Idle = 86,
    [Description("StateClimb_Up")]
    Climb_Up = 87,
    [Description("StateClimb_UpLeft")]
    Climb_UpLeft = 88,
    [Description("StateClimb_UpRight")]
    Climb_UpRight = 89,
    [Description("StateClimb_Down")]
    Climb_Down = 90,
    [Description("StateClimb_DownLeft")]
    Climb_DownLeft = 91,
    [Description("StateClimb_DownRight")]
    Climb_DownRight = 92,
    [Description("StateClimb_Left")]
    Climb_Left = 93,
    [Description("StateClimb_Right")]
    Climb_Right = 94,
    [Description("StateClimb_UpLand")]
    Climb_UpLand = 95,
    [Description("StateClimb_UpTake")]
    Climb_UpTake = 96,
    [Description("StateClimb_DownTake")]
    Climb_DownTake = 97,
    [Description("StateClimb_DownLand")]
    Climb_DownLand = 98,

    [Description("StateGlide_Idle")]
    Glide_Idle = 99,
    [Description("StateGlide_Run")]
    Glide_Run = 100,

    [Description("StateTaxiCall_Call")]
    TaxiCall_Call = 101,
    [Description("StateTaxiCall_Arrive")]
    TaxiCall_Arrive = 102,
    [Description("StateTaxiCall_Wait1")]
    TaxiCall_Wait1 = 103,
    [Description("StateTaxiCall_Wait2")]
    TaxiCall_Wait2 = 104,
    [Description("StateTaxiCall_Leave")]
    TaxiCall_Leave = 105,
    [Description("StateTaxiCall_End")]
    TaxiCall_End = 106,

    [Description("StateCashCall_Call")]
    CashCall_Call = 107,
    [Description("StateCashCall_Arrive")]
    CashCall_Arrive = 108,
    [Description("StateCashCall_Leave")]
    CashCall_Leave = 109,
    [Description("StateCashCall_End")]
    CashCall_End = 110,

    [Description("StateEmotionIdle_Idle")]
    EmotionIdle_Idle = 111,
    [Description("StateEmotionIdle_Bore_A")]
    EmotionIdle_Bore_A = 112,
    [Description("StateEmotionIdle_Bore_B")]
    EmotionIdle_Bore_B = 113,
    [Description("StateEmotionIdle_Bore_C")]
    EmotionIdle_Bore_C = 114,

    [Description("StatePvPWinLose_Win")]
    PvPWinLose_Win = 115,
    [Description("StatePvPWinLose_Lose")]
    PvPWinLose_Lose = 116,

    [Description("StateFishing_Start")]
    Fishing_Start = 117,
    [Description("StateFishing_Idle")]
    Fishing_Idle = 118,
    [Description("StateFishing_Bore")]
    Fishing_Bore = 119,
    [Description("StateFishing_FishFighting")]
    Fishing_FishFighting = 120,
    [Description("StateFishing_Catch")]
    Fishing_Catch = 121,

    [Description("StateFishingFloat_Idle")]
    FishingFloat_Idle = 122,
    [Description("StateFishingFloat_Fishing")]
    FishingFloat_Fishing = 123,
    [Description("StateFishingFloat_Catch")]
    FishingFloat_Catch = 124,

    [Description("StatePlayInstrument_Ready")]
    PlayInstrument_Ready = 125,
    [Description("StatePlayInstrument_Playing_Direct")]
    PlayInstrument_Playing_Direct = 126,
    [Description("StatePlayInstrument_Playing_Score_Solo")]
    PlayInstrument_Playing_Score_Solo = 127,
    [Description("StatePlayInstrument_Ready_Score_Ensemble")]
    PlayInstrument_Ready_Score_Ensemble = 128,
    [Description("StatePlayInstrument_Playing_Score_Ensemble")]
    PlayInstrument_Playing_Score_Ensemble = 129,

    [Description("StateSummon_React")]
    Summon_React = 130,
    [Description("StateSummon_Pet")]
    Summon_Pet = 131,

    [Description("StateSwim_Swim")]
    Swim_Swim = 132,
    [Description("StateSwim_Stunt")]
    Swim_Stunt = 133,

    [Description("StateLand_Land")]
    Land_Land = 134,
    [Description("StateLand_Stunt")]
    Land_Stunt = 135,

    [Description("StateHomeConvenient_Call")]
    HomeConvenient_Call = 136,
    [Description("StateHomeConvenient_End")]
    HomeConvenient_End = 137,

    [Description("StateInteract_Intearct")]
    Interact_Interact = 138,
    [Description("StateInteract_Success")]
    Interact_Success = 139,
    [Description("StateInteract_Fail")]
    Interact_Fail = 140,

    [Description("StateHomemade_Try")]
    Homemade_Try = 142,
    [Description("StateHomemade_Harvest")]
    Homemade_Harvest = 143,
    [Description("StateHomemade_Success")]
    Homemade_Success = 144,
    [Description("StateHomemade_Fail")]
    Homemade_Fail = 145,

    [Description("SubState_Max")]
    Max = 179,
}
