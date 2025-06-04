using Maple2.Model.Enum;

namespace Maple2.Server.Game.Trigger.Helpers;

public partial class Trigger {
    public class BonusGameReward(int boxId, int type) : ICondition {
        public string Name => "bonus_game_reward";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.BonusGameReward(boxId, type);
    }

    public class CheckAnyUserAdditionalEffect(int boxId, int additionalEffectId, int level, bool negate) : ICondition {
        public string Name => "check_any_user_additional_effect";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.CheckAnyUserAdditionalEffect(boxId, additionalEffectId, level, negate);
    }

    public class CheckDungeonLobbyUserCount(bool negate) : ICondition {
        public string Name => "check_dungeon_lobby_user_count";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.CheckDungeonLobbyUserCount(negate);
    }

    public class CheckNpcAdditionalEffect(int spawnId, int additionalEffectId, int level, bool negate) : ICondition {
        public string Name => "check_npc_additional_effect";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.CheckNpcAdditionalEffect(spawnId, additionalEffectId, level, negate);
    }

    public class NpcDamage(int spawnId, float damageRate, OperatorType operatorType) : ICondition {
        public string Name => "npc_damage";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.NpcDamage(spawnId, damageRate, operatorType);
    }

    public class NpcExtraData(int spawnPointId, string extraDataKey, int extraDataValue, OperatorType operatorType) : ICondition {
        public string Name => "npc_extra_data";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.NpcExtraData(spawnPointId, extraDataKey, extraDataValue, operatorType);
    }

    public class NpcHp(int spawnId, bool isRelative, int value, CompareType compareType) : ICondition {
        public string Name => "npc_hp";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.NpcHp(spawnId, isRelative, value, compareType);
    }

    public class CheckSameUserTag(int boxId, bool negate) : ICondition {
        public string Name => "check_same_user_tag";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.CheckSameUserTag(boxId, negate);
    }

    public class CheckUser(bool negate) : ICondition {
        public string Name => "check_user";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.CheckUser(negate);
    }

    public class UserCount(int count) : ICondition {
        public string Name => "user_count";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.UserCount(count);
    }

    public class CountUsers(int boxId, int userTagId, int minUsers, OperatorType operatorType, bool negate) : ICondition {
        public string Name => "count_users";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.CountUsers(boxId, userTagId, minUsers, operatorType, negate);
    }

    public class DayOfWeek(int[] dayOfWeeks, string desc, bool negate) : ICondition {
        public string Name => "day_of_week";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DayOfWeek(dayOfWeeks, desc, negate);
    }

    public class DetectLiftableObject(int[] boxIds, int itemId, bool negate) : ICondition {
        public string Name => "detect_liftable_object";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DetectLiftableObject(boxIds, itemId, negate);
    }

    public class DungeonPlayTime(int playSeconds) : ICondition {
        public string Name => "dungeon_play_time";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DungeonPlayTime(playSeconds);
    }

    public class DungeonState(string checkState) : ICondition {
        public string Name => "dungeon_state";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DungeonState(checkState);
    }

    public class DungeonFirstUserMissionScore(int score, OperatorType operatorType) : ICondition {
        public string Name => "dungeon_first_user_mission_score";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DungeonFirstUserMissionScore(score, operatorType);
    }

    public class DungeonId(int dungeonId) : ICondition {
        public string Name => "dungeon_id";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DungeonId(dungeonId);
    }

    public class DungeonLevel(int level) : ICondition {
        public string Name => "dungeon_level";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DungeonLevel(level);
    }

    public class DungeonMaxUserCount(int value) : ICondition {
        public string Name => "dungeon_max_user_count";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DungeonMaxUserCount(value);
    }

    public class DungeonRound(int round) : ICondition {
        public string Name => "dungeon_round";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DungeonRound(round);
    }

    public class DungeonTimeout : ICondition {
        public string Name => "dungeon_timeout";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DungeonTimeout();
    }

    public class DungeonVariable(int varId, int value) : ICondition {
        public string Name => "dungeon_variable";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.DungeonVariable(varId, value);
    }

    public class GuildVsGameScoredTeam(int teamId) : ICondition {
        public string Name => "guild_vs_game_scored_team";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.GuildVsGameScoredTeam(teamId);
    }

    public class GuildVsGameWinnerTeam(int teamId) : ICondition {
        public string Name => "guild_vs_game_winner_team";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.GuildVsGameWinnerTeam(teamId);
    }

    public class IsDungeonRoom(bool negate) : ICondition {
        public string Name => "is_dungeon_room";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.IsDungeonRoom(negate);
    }

    public class IsPlayingMapleSurvival(bool negate) : ICondition {
        public string Name => "is_playing_maple_survival";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.IsPlayingMapleSurvival(negate);
    }

    public class MonsterDead(int[] spawnIds, bool autoTarget) : ICondition {
        public string Name => "monster_dead";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.MonsterDead(spawnIds, autoTarget);
    }

    public class MonsterInCombat(int[] spawnIds, bool negate) : ICondition {
        public string Name => "monster_in_combat";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.MonsterInCombat(spawnIds, negate);
    }

    public class NpcDetected(int boxId, int[] spawnIds, bool negate) : ICondition {
        public string Name => "npc_detected";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.NpcDetected(boxId, spawnIds, negate);
    }

    public class NpcIsDeadByStringId(string stringId) : ICondition {
        public string Name => "npc_is_dead_by_string_id";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.NpcIsDeadByStringId(stringId);
    }

    public class ObjectInteracted(int[] interactIds, int state) : ICondition {
        public string Name => "object_interacted";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.ObjectInteracted(interactIds, state);
    }

    public class PvpZoneEnded(int boxId) : ICondition {
        public string Name => "pvp_zone_ended";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.PvpZoneEnded(boxId);
    }

    public class QuestUserDetected(int[] boxIds, int[] questIds, int[] questStates, int jobCode, bool negate) : ICondition {
        public string Name => "quest_user_detected";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.QuestUserDetected(boxIds, questIds, questStates, jobCode, negate);
    }

    public class RandomCondition(float weight, string desc) : ICondition {
        public string Name => "random_condition";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.RandomCondition(weight, desc);
    }

    public class ScoreBoardScore(int score, OperatorType operatorType) : ICondition {
        public string Name => "score_board_score";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.ScoreBoardScore(score, operatorType);
    }

    public class ShadowExpeditionPoints(int score) : ICondition {
        public string Name => "shadow_expedition_points";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.ShadowExpeditionPoints(score);
    }

    public class TimeExpired(string timerId) : ICondition {
        public string Name => "time_expired";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.TimeExpired(timerId);
    }

    public class UserDetected(int[] boxIds, JobCode jobCode, bool negate) : ICondition {
        public string Name => "user_detected";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.UserDetected(boxIds, jobCode, negate);
    }

    public class UserValue(string key, int value, bool negate) : ICondition {
        public string Name => "user_value";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.UserValue(key, value, negate);
    }

    public class WaitAndResetTick(int waitTick) : ICondition {
        public string Name => "wait_and_reset_tick";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.WaitAndResetTick(waitTick);
    }

    public class WaitSecondsUserValue(string key, string desc) : ICondition {
        public string Name => "wait_seconds_user_value";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.WaitSecondsUserValue(key, desc);
    }

    public class WaitTick(int waitTick) : ICondition {
        public string Name => "wait_tick";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.WaitTick(waitTick);
    }

    public class WeddingEntryInField(string entryType, bool isInField) : ICondition {
        public string Name => "wedding_entry_in_field";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.WeddingEntryInField(entryType, isInField);
    }

    public class WeddingHallState(string state, bool success) : ICondition {
        public string Name => "wedding_hall_state";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.WeddingHallState(state, success);
    }

    public class WeddingMutualAgreeResult(string agreeType) : ICondition {
        public string Name => "wedding_mutual_agree_result";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.WeddingMutualAgreeResult(agreeType);
    }

    public class WidgetValue(string type, string widgetName, string widgeArg, bool negate, string desc) : ICondition {
        public string Name => "widget_value";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];
        public bool Evaluate(TriggerContext context) => context.WidgetValue(type, widgetName, widgeArg, negate, desc);
    }

    public class GroupAnyOne : IGroupCondition {
        public string Name => "any_one";
        public string? NextState { get; set; }
        public LinkedList<ICondition> Conditions { get; set; } = [];
        public LinkedList<IAction> Actions { get; set; } = [];

        public bool Evaluate(TriggerContext context) {
            foreach (ICondition condition in Conditions) {
                if (condition.Evaluate(context)) return true;
            }
            return false;
        }
    }

    public class GroupAllOf : IGroupCondition {
        public string Name => "all_of";
        public string? NextState { get; set; }
        public LinkedList<ICondition> Conditions { get; set; } = [];
        public LinkedList<IAction> Actions { get; set; } = [];

        public bool Evaluate(TriggerContext context) {
            foreach (ICondition condition in Conditions) {
                if (!condition.Evaluate(context)) return false;
            }
            return true;
        }
    }

    public class GroupTrue : ICondition {
        public string Name => "true";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];

        public bool Evaluate(TriggerContext context) => true;
    }

    public class GroupAlways : ICondition {
        public string Name => "always";
        public string? NextState { get; set; }
        public LinkedList<IAction> Actions { get; set; } = [];

        public bool Evaluate(TriggerContext context) => true;
    }

}
