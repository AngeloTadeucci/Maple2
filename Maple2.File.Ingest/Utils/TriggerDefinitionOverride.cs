using System.Diagnostics;
using static Maple2.File.Ingest.Utils.ScriptType;

namespace Maple2.File.Ingest.Utils;

internal class TriggerDefinitionOverride {
    private const string Required = "<required>";

    // Function Name
    public readonly string Name;
    // Docstring description
    public readonly string Description = string.Empty;

    // Parameter Names
    public Dictionary<string, string> Names { get; init; } = null!;

    // Parameter Types
    public Dictionary<string, (ScriptType, string? Default)> Types { get; init; } = null!;

    // Comparison Operation (Only for Conditions)
    public (ScriptType Type, string Field, string Op, string Default) Compare { get; init; }

    public string? FunctionSplitter { get; init; }
    public Dictionary<string, TriggerDefinitionOverride> FunctionLookup { get; init; } = null!;

    private TriggerDefinitionOverride(string name, string? splitter = null) {
        Name = name;
        FunctionSplitter = splitter;
    }

    public static readonly Dictionary<string, TriggerDefinitionOverride> ActionOverride = new();
    public static readonly Dictionary<string, TriggerDefinitionOverride> ConditionOverride = new();

    static TriggerDefinitionOverride() {
        // Action Override
        ActionOverride["add_balloon_talk"] = new TriggerDefinitionOverride("add_balloon_talk") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Int, null), ("duration", Int, null), ("delayTick", Int, null), ("npcID", Int, null)),
        };
        ActionOverride["add_buff"] = new TriggerDefinitionOverride("add_buff") {
            Names = BuildNameOverride(("arg1", "boxIds"), ("arg2", "skillId"), ("arg3", "level"), ("arg4", "ignorePlayer"), ("arg5", "isSkillSet")),
            Types = BuildTypeOverride(("boxIds", IntList, Required), ("skillId", Int, Required), ("level", Int, Required), ("ignorePlayer", Bool, "True"), ("isSkillSet", Bool, "True")),
        };
        ActionOverride["add_cinematic_talk"] = new TriggerDefinitionOverride("add_cinematic_talk") {
            Names = BuildNameOverride(("npcID", "npcId"), ("illustID", "illustId"), ("illust", "illustId"), ("delay", "delayTick")),
            Types = BuildTypeOverride(("npcId", Int, Required), ("duration", Int, null), ("align", EnumAlign, null), ("delayTick", Int, null)),
        };
        ActionOverride["add_effect_nif"] = new TriggerDefinitionOverride("add_effect_nif") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Int, Required), ("isOutline", Bool, null), ("scale", Float, null), ("rotateZ", Int, null)),
        };
        ActionOverride["add_user_value"] = new TriggerDefinitionOverride("add_user_value") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("value", Int, Required)),
        };
        ActionOverride["allocate_battlefield_points"] = new TriggerDefinitionOverride("allocate_battlefield_points") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "points")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("points", Int, Required)),
        };
        ActionOverride["announce"] = new TriggerDefinitionOverride("announce") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "content")),
            Types = BuildTypeOverride(("type", Int, null), ("content", Str, Required), ("arg3", Bool, null)),
        };
        ActionOverride["arcade_boom_boom_ocean"] = new TriggerDefinitionOverride(string.Empty) {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new("arcade_boom_boom_ocean_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("lifeCount", Int, Required)),
                },
                ["EndGame"] = new("arcade_boom_boom_ocean_end_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
                ["SetSkillScore"] = new("arcade_boom_boom_ocean_set_skill_score", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("type", Str, Required), ("id", Int, Required), ("score", Int, Required)),
                },
                ["StartRound"] = new("arcade_boom_boom_ocean_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("type", Str, Required), ("round", Int, Required), ("roundDuration", Int, Required), ("timeScoreRate", Int, Required)),
                },
                ["ClearRound"] = new("arcade_boom_boom_ocean_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("type", Str, Required), ("round", Int, Required)),
                },
            },
        };
        ActionOverride["arcade_spring_farm"] = new TriggerDefinitionOverride("") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new("arcade_spring_farm_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("lifeCount", Int, Required)),
                },
                ["EndGame"] = new("arcade_spring_farm_end_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
                ["SetInteractScore"] = new("arcade_spring_farm_set_interact_score", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("id", Int, Required), ("score", Int, Required)),
                },
                ["SpawnMonster"] = new("arcade_spring_farm_spawn_monster", splitter: "type") {
                    Names = BuildNameOverride(("spawnID", "spawnIds")),
                    Types = BuildTypeOverride(("spawnIds", IntList, Required), ("score", Int, Required)),
                },
                ["StartRound"] = new("arcade_spring_farm_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("uiDuration", Int, Required), ("round", Int, Required), ("roundDuration", Int, Required), ("timeScoreType", Str, Required), ("timeScoreRate", Int, Required)),
                },
                ["ClearRound"] = new("arcade_spring_farm_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Int, Required)),
                },
            },
        };
        ActionOverride["arcade_three_two_one"] = new TriggerDefinitionOverride("") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new("arcade_three_two_one_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("lifeCount", Int, Required), ("initScore", Int, Required)),
                },
                ["EndGame"] = new("arcade_three_two_one_end_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
                ["StartRound"] = new("arcade_three_two_one_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("uiDuration", Int, Required), ("round", Int, Required)),
                },
                ["ResultRound"] = new("arcade_three_two_one_result_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("resultDirection", Int, Required)),
                },
                ["ResultRound2"] = new("arcade_three_two_one_result_round2", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Int, Required)),
                },
                ["ClearRound"] = new("arcade_three_two_one_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Int, Required)),
                },
            },
        };
        ActionOverride["arcade_three_two_one2"] = new TriggerDefinitionOverride("") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new("arcade_three_two_one2_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("lifeCount", Int, Required), ("initScore", Int, Required)),
                },
                ["EndGame"] = new("arcade_three_two_one2_end_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
                ["StartRound"] = new("arcade_three_two_one2_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("uiDuration", Int, Required), ("round", Int, Required)),
                },
                ["ResultRound"] = new("arcade_three_two_one2_result_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("resultDirection", Int, Required)),
                },
                ["ResultRound2"] = new("arcade_three_two_one2_result_round2", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Int, Required)),
                },
                ["ClearRound"] = new("arcade_three_two_one2_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Int, Required)),
                },
            },
        };
        ActionOverride["arcade_three_two_one3"] = new TriggerDefinitionOverride("") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new("arcade_three_two_one3_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("lifeCount", Int, Required), ("initScore", Int, Required)),
                },
                ["EndGame"] = new("arcade_three_two_one3_end_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
                ["StartRound"] = new("arcade_three_two_one3_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("uiDuration", Int, Required), ("round", Int, Required)),
                },
                ["ResultRound"] = new("arcade_three_two_one3_result_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("resultDirection", Int, Required)),
                },
                ["ResultRound2"] = new("arcade_three_two_one3_result_round2", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Int, Required)),
                },
                ["ClearRound"] = new("arcade_three_two_one3_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Int, Required)),
                },
            },
        };
        ActionOverride["change_background"] = new TriggerDefinitionOverride("change_background") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("dds", Str, Required)),
        };
        ActionOverride["change_monster"] = new TriggerDefinitionOverride("change_monster") {
            Names = BuildNameOverride(("arg1", "fromSpawnId"), ("arg2", "toSpawnId")),
            Types = BuildTypeOverride(("fromSpawnId", Int, Required), ("toSpawnId", Int, Required)),
        };
        ActionOverride["close_cinematic"] = new TriggerDefinitionOverride("close_cinematic") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["create_field_game"] = new TriggerDefinitionOverride("create_field_game") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("type", EnumFieldGame, Required), ("reset", Bool, null)),
        };
        ActionOverride["create_item"] = new TriggerDefinitionOverride("create_item") {
            Names = BuildNameOverride(("arg1", "spawnIds"), ("arg2", "triggerId"), ("arg3", "itemId")),
            Types = BuildTypeOverride(("spawnIds", IntList, Required), ("triggerId", Int, null), ("itemId", Int, null), ("arg5", Int, null)),
        };
        ActionOverride["spawn_monster"] = new TriggerDefinitionOverride("spawn_monster") {
            Names = BuildNameOverride(("arg1", "spawnIds"), ("arg2", "autoTarget"), ("agr2", "autoTarget"), ("arg", "autoTarget"), ("arg3", "delay")),
            Types = BuildTypeOverride(("spawnIds", IntList, Required), ("autoTarget", Bool, "True"), ("delay", Int, null)),
        };
        ActionOverride["create_widget"] = new TriggerDefinitionOverride("create_widget") {
            Names = BuildNameOverride(("arg1", "type")),
            Types = BuildTypeOverride(("type", Str, Required)),
        };
        ActionOverride["dark_stream"] = new TriggerDefinitionOverride("dark_stream") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new("dark_stream_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Int, Required)),
                },
                ["SpawnMonster"] = new("dark_stream_spawn_monster", splitter: "type") {
                    Names = BuildNameOverride(("spawnID", "spawnIds")),
                    Types = BuildTypeOverride(("spawnIds", IntList, Required), ("score", Int, Required)),
                },
                ["StartRound"] = new("dark_stream_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("uiDuration", Int, Required), ("round", Int, Required), ("damagePenalty", Int, Required)),
                },
                ["ClearRound"] = new("dark_stream_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Int, Required)),
                },
            },
        };
        ActionOverride["debug_string"] = new TriggerDefinitionOverride("debug_string") {
            Names = BuildNameOverride(("arg1", "value"), ("string", "value")),
            Types = BuildTypeOverride(("value", Str, Required)),
        };
        ActionOverride["destroy_monster"] = new TriggerDefinitionOverride("destroy_monster") {
            Names = BuildNameOverride(("arg1", "spawnIds"), ("agr2", "arg2")),
            Types = BuildTypeOverride(("spawnIds", IntList, Required), ("arg2", Bool, "True")),
        };
        ActionOverride["dungeon_clear"] = new TriggerDefinitionOverride("dungeon_clear") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("uiType", Str, null)),
        };
        ActionOverride["dungeon_clear_round"] = new TriggerDefinitionOverride("dungeon_clear_round") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("round", Int, Required)),
        };

        ActionOverride["dungeon_close_timer"] = new TriggerDefinitionOverride("dungeon_close_timer") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };

        ActionOverride["dungeon_disable_ranking"] = new TriggerDefinitionOverride("dungeon_disable_ranking") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };

        ActionOverride["dungeon_enable_give_up"] = new TriggerDefinitionOverride("dungeon_enable_give_up") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isEnable", Bool, null)),
        };

        ActionOverride["dungeon_fail"] = new TriggerDefinitionOverride("dungeon_fail") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };

        ActionOverride["dungeon_mission_complete"] = new TriggerDefinitionOverride("dungeon_mission_complete") {
            Names = BuildNameOverride(("missionID", "missionId")),
            Types = BuildTypeOverride(("missionId", Int, Required)),
        };

        ActionOverride["dungeon_move_lap_time_to_now"] = new TriggerDefinitionOverride("dungeon_move_lap_time_to_now") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("id", Int, Required)),
        };

        ActionOverride["dungeon_reset_time"] = new TriggerDefinitionOverride("dungeon_reset_time") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("seconds", Int, Required)),
        };

        ActionOverride["dungeon_set_end_time"] = new TriggerDefinitionOverride("dungeon_set_end_time") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };

        ActionOverride["dungeon_set_lap_time"] = new TriggerDefinitionOverride("dungeon_set_lap_time") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("id", Int, Required), ("lapTime", Int, null)),
        };

        ActionOverride["dungeon_stop_timer"] = new TriggerDefinitionOverride("dungeon_stop_timer") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["dungeon_variable"] = new TriggerDefinitionOverride("set_dungeon_variable") {
            Names = BuildNameOverride(("varID", "varId")),
            Types = BuildTypeOverride(("varId", Int, Required), ("value", Int, Required)),
        };
        ActionOverride["enable_local_camera"] = new TriggerDefinitionOverride("enable_local_camera") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isEnable", Bool, null)),
        };
        ActionOverride["enable_spawn_point_pc"] = new TriggerDefinitionOverride("enable_spawn_point_pc") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Int, Required), ("isEnable", Bool, null)),
        };
        ActionOverride["end_mini_game"] = new TriggerDefinitionOverride("end_mini_game") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("winnerBoxId", Int, null), ("isEnable", Bool, null), ("isOnlyWinner", Bool, null)),
        };
        ActionOverride["end_mini_game_round"] = new TriggerDefinitionOverride("end_mini_game_round") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("winnerBoxId", Int, Required), ("expRate", Float, null), ("meso", Float, null), ("isOnlyWinner", Bool, null), ("isGainLoserBonus", Bool, null)),
        };
        ActionOverride["face_emotion"] = new TriggerDefinitionOverride("face_emotion") {
            Names = BuildNameOverride(("spawnPointID", "spawnId"), ("spwnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Int, null)),
        };
        ActionOverride["field_game_constant"] = new TriggerDefinitionOverride("field_game_constant") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("key", Str, Required), ("value", Str, Required), ("locale", EnumLocale, null)),
        };
        ActionOverride["field_game_message"] = new TriggerDefinitionOverride("field_game_message") {
            Names = BuildNameOverride(("arg2", "script"), ("arg3", "duration")),
            Types = BuildTypeOverride(("custom", Int, null), ("type", Str, Required), ("duration", Int, null), ("arg1", Bool, null), ("script", Str, Required)),
        };
        ActionOverride["field_war_end"] = new TriggerDefinitionOverride("field_war_end") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isClear", Bool, null)),
        };
        ActionOverride["give_exp"] = new TriggerDefinitionOverride("give_exp") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "rate")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("rate", Float, "1.0"), ("arg3", Bool, null)),
        };
        ActionOverride["give_guild_exp"] = new TriggerDefinitionOverride("give_guild_exp") {
            Names = BuildNameOverride(("boxID", "boxId")),
            Types = BuildTypeOverride(("boxId", Int, null), ("type", Int, Required)),
        };
        ActionOverride["give_reward_content"] = new TriggerDefinitionOverride("give_reward_content") {
            Names = BuildNameOverride(("rewardID", "rewardId")),
            Types = BuildTypeOverride(("rewardId", Int, Required)),
        };
        ActionOverride["guide_event"] = new TriggerDefinitionOverride("guide_event") {
            Names = BuildNameOverride(("eventID", "eventId")),
            Types = BuildTypeOverride(("eventId", Int, Required)),
        };
        ActionOverride["guild_vs_game_end_game"] = new TriggerDefinitionOverride("guild_vs_game_end_game") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["guild_vs_game_give_contribution"] = new TriggerDefinitionOverride("guild_vs_game_give_contribution") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Int, Required), ("isWin", Bool, null)),
        };
        ActionOverride["guild_vs_game_give_reward"] = new TriggerDefinitionOverride("guild_vs_game_give_reward") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Int, Required), ("isWin", Bool, null)),
        };
        ActionOverride["guild_vs_game_log_result"] = new TriggerDefinitionOverride("guild_vs_game_log_result") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["guild_vs_game_log_won_by_default"] = new TriggerDefinitionOverride("guild_vs_game_log_won_by_default") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Int, Required)),
        };
        ActionOverride["guild_vs_game_result"] = new TriggerDefinitionOverride("guild_vs_game_result") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["guild_vs_game_score_by_user"] = new TriggerDefinitionOverride("guild_vs_game_score_by_user") {
            Names = BuildNameOverride(("triggerBoxID", "boxId")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("score", Int, Required)),
        };
        ActionOverride["hide_guide_summary"] = new TriggerDefinitionOverride("hide_guide_summary") {
            Names = BuildNameOverride(("entityID", "entityId"), ("textID", "textId")),
            Types = BuildTypeOverride(("entityId", Int, Required), ("textId", Int, null)),
        };
        ActionOverride["init_npc_rotation"] = new TriggerDefinitionOverride("init_npc_rotation") {
            Names = BuildNameOverride(("arg1", "spawnIds")),
            Types = BuildTypeOverride(("spawnIds", IntList, Required)),
        };
        ActionOverride["kick_music_audience"] = new TriggerDefinitionOverride("kick_music_audience") {
            Names = BuildNameOverride(("targetBoxID", "boxId"), ("targetPortalID", "portalId")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("portalId", Int, Required)),
        };
        ActionOverride["limit_spawn_npc_count"] = new TriggerDefinitionOverride("limit_spawn_npc_count") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("limitCount", Int, Required)),
        };
        ActionOverride["lock_my_pc"] = new TriggerDefinitionOverride("lock_my_pc") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isLock", Bool, null)),
        };
        ActionOverride["mini_game_camera_direction"] = new TriggerDefinitionOverride("mini_game_camera_direction") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("boxId", Int, Required), ("cameraId", Int, Required)),
        };
        ActionOverride["mini_game_give_exp"] = new TriggerDefinitionOverride("mini_game_give_exp") {
            Names = BuildNameOverride(("isOutSide", "isOutside")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("expRate", Float, "1.0"), ("isOutside", Bool, null)),
        };
        ActionOverride["mini_game_give_reward"] = new TriggerDefinitionOverride("mini_game_give_reward") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("winnerBoxId", Int, Required), ("contentType", Str, Required)),
        };
        ActionOverride["move_npc"] = new TriggerDefinitionOverride("move_npc") {
            Names = BuildNameOverride(("arg1", "spawnId"), ("arg2", "patrolName")),
            Types = BuildTypeOverride(("spawnId", Int, Required), ("patrolName", Str, Required)),
        };
        ActionOverride["move_npc_to_pos"] = new TriggerDefinitionOverride("move_npc_to_pos") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Int, Required), ("pos", Vector3, Required), ("rot", Vector3, Required)),
        };
        ActionOverride["move_random_user"] = new TriggerDefinitionOverride("move_random_user") {
            Names = BuildNameOverride(("arg1", "mapId"), ("arg2", "portalId"), ("arg3", "boxId"), ("arg4", "count")),
            Types = BuildTypeOverride(("mapId", Int, Required), ("portalId", Int, Required), ("boxId", Int, Required), ("count", Int, Required)),
        };
        ActionOverride["move_to_portal"] = new TriggerDefinitionOverride("move_to_portal") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("userTagId", Int, null), ("portalId", Int, null), ("boxId", Int, null)),
        };
        ActionOverride["move_user"] = new TriggerDefinitionOverride("move_user") {
            Names = BuildNameOverride(("arg1", "mapId"), ("arg2", "portalId"), ("arg3", "boxId")),
            Types = BuildTypeOverride(("mapId", Int, null), ("portalId", Int, null), ("boxId", Int, null)),
        };
        ActionOverride["move_user_path"] = new TriggerDefinitionOverride("move_user_path") {
            Names = BuildNameOverride(("arg1", "patrolName")),
            Types = BuildTypeOverride(("patrolName", Str, Required)),
        };
        ActionOverride["move_user_to_box"] = new TriggerDefinitionOverride("move_user_to_box") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("boxId", Int, Required), ("portalId", Int, Required)),
        };
        ActionOverride["move_user_to_pos"] = new TriggerDefinitionOverride("move_user_to_pos") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("pos", Vector3, Required), ("rot", Vector3, null)),
        };
        ActionOverride["notice"] = new TriggerDefinitionOverride("notice") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "script")),
            Types = BuildTypeOverride(("type", Int, null), ("script", Str, Required), ("arg3", Bool, null)),
        };
        ActionOverride["npc_remove_additional_effect"] = new TriggerDefinitionOverride("npc_remove_additional_effect") {
            Names = BuildNameOverride(("spawnPointID", "spawnId"), ("additionalEffectID", "additionalEffectId")),
            Types = BuildTypeOverride(("spawnId", Int, Required), ("additionalEffectId", Int, Required)),
        };
        ActionOverride["npc_to_patrol_in_box"] = new TriggerDefinitionOverride("npc_to_patrol_in_box") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("boxId", Int, Required), ("npcId", Int, Required)),
        };
        ActionOverride["patrol_condition_user"] = new TriggerDefinitionOverride("patrol_condition_user") {
            Names = BuildNameOverride(("additionalEffectID", "additionalEffectId")),
            Types = BuildTypeOverride(("patrolIndex", Int, Required), ("additionalEffectId", Int, Required)),
        };
        ActionOverride["play_scene_movie"] = new TriggerDefinitionOverride("play_scene_movie") {
            Names = BuildNameOverride(("movieID", "movieId")),
            Types = BuildTypeOverride(("movieId", Int, null)),
        };
        ActionOverride["play_system_sound_by_user_tag"] = new TriggerDefinitionOverride("play_system_sound_by_user_tag") {
            Names = BuildNameOverride(("userTagID", "userTagId")),
            Types = BuildTypeOverride(("userTagId", Int, Required), ("soundKey", Str, Required)),
        };
        ActionOverride["play_system_sound_in_box"] = new TriggerDefinitionOverride("play_system_sound_in_box") {
            Names = BuildNameOverride(("arg1", "boxIds"), ("arg2", "sound")),
            Types = BuildTypeOverride(("boxIds", IntList, null), ("sound", Str, Required)),
        };
        ActionOverride["random_additional_effect"] = new TriggerDefinitionOverride("random_additional_effect") {
            Names = BuildNameOverride(("Target", "target"), ("triggerBoxID", "boxId"), ("spawnPointID", "spawnId"), ("arg1", "boxIds"), ("additionalEffectID", "additionalEffectId")),
            Types = BuildTypeOverride(("boxId", Int, null), ("spawnId", Int, null), ("targetCount", Int, null), ("tick", Int, null), ("waitTick", Int, null), ("additionalEffectId", Int, null)),
        };
        ActionOverride["remove_balloon_talk"] = new TriggerDefinitionOverride("remove_balloon_talk") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Int, null)),
        };
        ActionOverride["remove_buff"] = new TriggerDefinitionOverride("remove_buff") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "skillId"), ("arg3", "isPlayer")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("skillId", Int, Required), ("isPlayer", Bool, null)),
        };
        ActionOverride["remove_cinematic_talk"] = new TriggerDefinitionOverride("remove_cinematic_talk") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["remove_effect_nif"] = new TriggerDefinitionOverride("remove_effect_nif") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Int, Required)),
        };
        ActionOverride["reset_camera"] = new TriggerDefinitionOverride("reset_camera") {
            Names = BuildNameOverride(("arg1", "interpolationTime"), ("arg2", "interpolationTime")),
            Types = BuildTypeOverride(("interpolationTime", Float, null)),
        };
        ActionOverride["reset_timer"] = new TriggerDefinitionOverride("reset_timer") {
            Names = BuildNameOverride(("arg1", "timerId")),
            Types = BuildTypeOverride(),
        };
        ActionOverride["room_expire"] = new TriggerDefinitionOverride("room_expire") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["score_board_create"] = new TriggerDefinitionOverride("score_board_create") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("type", Str, null), ("title", Str, null), ("maxScore", Int, null)),
        };
        ActionOverride["score_board_remove"] = new TriggerDefinitionOverride("score_board_remove") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["score_board_set_score"] = new TriggerDefinitionOverride("score_board_set_score") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("score", Int, Required)),
        };
        ActionOverride["select_camera"] = new TriggerDefinitionOverride("select_camera") {
            Names = BuildNameOverride(("arg1", "triggerId"), ("arg2", "enable")),
            Types = BuildTypeOverride(("triggerId", Int, Required), ("enable", Bool, "True")),
        };
        ActionOverride["select_camera_path"] = new TriggerDefinitionOverride("select_camera_path") {
            Names = BuildNameOverride(("arg1", "pathIds"), ("arg2", "returnView")),
            Types = BuildTypeOverride(("pathIds", IntList, Required), ("returnView", Bool, "True")),
        };
        ActionOverride["set_achievement"] = new TriggerDefinitionOverride("set_achievement") {
            Names = BuildNameOverride(("arg1", "triggerId"), ("arg2", "type"), ("arg3", "achieve")),
            Types = BuildTypeOverride(("triggerId", Int, null)),
        };
        ActionOverride["set_actor"] = new TriggerDefinitionOverride("set_actor") {
            Names = BuildNameOverride(("arg1", "triggerId"), ("arg2", "visible"), ("arg3", "initialSequence")),
            Types = BuildTypeOverride(("triggerId", Int, Required), ("visible", Bool, null), ("arg4", Bool, null), ("arg5", Bool, null)),
        };
        ActionOverride["set_agent"] = new TriggerDefinitionOverride("set_agent") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible")),
            Types = BuildTypeOverride(("triggerIds", IntList, Required), ("visible", Bool, null)),
        };
        ActionOverride["set_ai_extra_data"] = new TriggerDefinitionOverride("set_ai_extra_data") {
            Names = BuildNameOverride(("boxID", "boxId")),
            Types = BuildTypeOverride(("key", Str, Required), ("value", Int, Required), ("isModify", Bool, null), ("boxId", Int, null)),
        };
        ActionOverride["set_ambient_light"] = new TriggerDefinitionOverride("set_ambient_light") {
            Names = BuildNameOverride(("arg1", "primary"), ("arg2", "secondary"), ("arg3", "tertiary")),
            Types = BuildTypeOverride(("primary", Vector3, Required), ("secondary", Vector3, null), ("tertiary", Vector3, null)),
        };
        ActionOverride["set_breakable"] = new TriggerDefinitionOverride("set_breakable") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "enable")),
            Types = BuildTypeOverride(("triggerIds", IntList, Required), ("enable", Bool, null)),
        };
        ActionOverride["set_cinematic_intro"] = new TriggerDefinitionOverride("set_cinematic_intro") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["set_cinematic_ui"] = new TriggerDefinitionOverride("set_cinematic_ui") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "script")),
            Types = BuildTypeOverride(("type", Int, Required), ("script", Str, null), ("arg3", Bool, null)),
        };
        ActionOverride["set_dialogue"] = new TriggerDefinitionOverride("set_dialogue") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "spawnId"), ("arg3", "script"), ("arg4", "time")),
            Types = BuildTypeOverride(("type", Int, Required), ("spawnId", Int, null), ("script", Str, Required), ("time", Int, null), ("arg5", Int, null), ("align", EnumAlign, null)),
        };
        ActionOverride["set_cube"] = new TriggerDefinitionOverride("set_cube") {
            Names = BuildNameOverride(("IDs", "triggerIds"), ("arg1", "triggerIds"), ("arg2", "isVisible")),
            Types = BuildTypeOverride(("triggerIds", IntList, Required), ("isVisible", Bool, null), ("randomCount", Int, null)),
        };
        ActionOverride["set_directional_light"] = new TriggerDefinitionOverride("set_directional_light") {
            Names = BuildNameOverride(("arg1", "diffuseColor"), ("arg2", "specularColor")),
            Types = BuildTypeOverride(("diffuseColor", Vector3, Required), ("specularColor", Vector3, null)),
        };
        ActionOverride["set_effect"] = new TriggerDefinitionOverride("set_effect") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible"), ("arg3", "startDelay"), ("arg4", "interval")),
            Types = BuildTypeOverride(("triggerIds", IntList, null), ("visible", Bool, null), ("startDelay", Int, null), ("interval", Int, null)),
        };
        ActionOverride["set_event_ui"] = new TriggerDefinitionOverride(string.Empty) {
            FunctionSplitter = "arg1",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["0"] = new("set_event_ui_round", splitter: "arg1") {
                    Names = BuildNameOverride(("arg2", "rounds"), ("arg4", "vOffset")),
                    Types = BuildTypeOverride(("rounds", IntList, Required), ("arg3", Int, null), ("vOffset", Int, null)),
                },
                ["1"] = new("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", EnumBannerType, Required), ("script", Str, null), ("duration", Int, Required), ("boxIds", StrList, null)),
                },
                ["2"] = new("set_event_ui_countdown", splitter: "arg1") {
                    Names = BuildNameOverride(("arg2", "script"), ("arg3", "roundCountdown"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("script", Str, null), ("roundCountdown", IntList, Required), ("boxIds", StrList, null)),
                },
                ["3"] = new("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", EnumBannerType, Required), ("script", Str, null), ("duration", Int, Required), ("boxIds", StrList, null)),
                },
                ["4"] = new("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", EnumBannerType, Required), ("script", Str, null), ("duration", Int, Required), ("boxIds", StrList, null)),
                },
                ["5"] = new("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", EnumBannerType, Required), ("script", Str, null), ("duration", Int, Required), ("boxIds", StrList, null)),
                },
                ["6"] = new("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", EnumBannerType, Required), ("script", Str, null), ("duration", Int, Required), ("boxIds", StrList, null)),
                },
                ["7"] = new("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", EnumBannerType, Required), ("script", Str, null), ("duration", Int, Required), ("boxIds", StrList, null)),
                },
            },
        };
        ActionOverride["set_gravity"] = new TriggerDefinitionOverride("set_gravity") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("gravity", Float, Required)),
        };
        ActionOverride["set_interact_object"] = new TriggerDefinitionOverride("set_interact_object") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "state")),
            Types = BuildTypeOverride(("triggerIds", IntList, Required), ("state", Int, Required), ("arg4", Bool, null), ("arg3", Bool, null)),
        };
        ActionOverride["set_ladder"] = new TriggerDefinitionOverride("set_ladder") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible"), ("arg3", "enable"), ("arg4", "fade")),
            Types = BuildTypeOverride(("triggerIds", IntList, Required), ("visible", Bool, null), ("enable", Bool, null), ("fade", Int, null)),
        };
        ActionOverride["set_local_camera"] = new TriggerDefinitionOverride("set_local_camera") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("cameraId", Int, Required), ("enable", Bool, null)),
        };
        ActionOverride["set_mesh"] = new TriggerDefinitionOverride("set_mesh") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible"), ("arg3", "startDelay"), ("arg4", "interval"), ("arg5", "fade")),
            Types = BuildTypeOverride(("triggerIds", IntList, Required), ("visible", Bool, null), ("startDelay", Int, null), ("interval", Int, null), ("fade", Float, null)),
        };
        ActionOverride["set_mesh_animation"] = new TriggerDefinitionOverride("set_mesh_animation") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible"), ("arg3", "startDelay"), ("arg4", "interval")),
            Types = BuildTypeOverride(("triggerIds", IntList, Required), ("visible", Bool, null), ("startDelay", Int, null), ("interval", Int, null)),
        };
        ActionOverride["set_mini_game_area_for_hack"] = new TriggerDefinitionOverride("set_mini_game_area_for_hack") {
            Names = BuildNameOverride(("boxID", "boxId")),
            Types = BuildTypeOverride(("boxId", Int, Required)),
        };
        ActionOverride["set_npc_duel_hp_bar"] = new TriggerDefinitionOverride("set_npc_duel_hp_bar") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("isOpen", Bool, null), ("spawnId", Int, Required), ("durationTick", Int, null), ("npcHpStep", Int, null)),
        };
        ActionOverride["set_npc_emotion_loop"] = new TriggerDefinitionOverride("set_npc_emotion_loop") {
            Names = BuildNameOverride(("arg1", "spawnId"), ("arg2", "sequenceName"), ("arg3", "duration"), ("arg", "duration")),
            Types = BuildTypeOverride(("spawnId", Int, Required), ("duration", Float, null)),
        };
        ActionOverride["set_npc_emotion_sequence"] = new TriggerDefinitionOverride("set_npc_emotion_sequence") {
            Names = BuildNameOverride(("arg1", "spawnId"), ("arg2", "sequenceName"), ("arg3", "durationTick")),
            Types = BuildTypeOverride(("spawnId", Int, Required), ("sequenceName", Str, Required), ("durationTick", Int, null)),
        };
        ActionOverride["set_npc_rotation"] = new TriggerDefinitionOverride("set_npc_rotation") {
            Names = BuildNameOverride(("arg1", "spawnId"), ("arg2", "rotation")),
            Types = BuildTypeOverride(("spawnId", Int, Required), ("rotation", Float, Required)),
        };
        ActionOverride["set_onetime_effect"] = new TriggerDefinitionOverride("set_onetime_effect") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("id", Int, null), ("enable", Bool, null)),
        };
        ActionOverride["set_pc_emotion_loop"] = new TriggerDefinitionOverride("set_pc_emotion_loop") {
            Names = BuildNameOverride(("arg1", "sequenceName"), ("arg2", "duration"), ("arg3", "loop")),
            Types = BuildTypeOverride(("sequenceName", Str, Required), ("duration", Float, null), ("loop", Bool, null)),
        };
        ActionOverride["set_pc_emotion_sequence"] = new TriggerDefinitionOverride("set_pc_emotion_sequence") {
            Names = BuildNameOverride(("arg1", "sequenceNames")),
            Types = BuildTypeOverride(("sequenceNames", StrList, Required)),
        };
        ActionOverride["set_pc_rotation"] = new TriggerDefinitionOverride("set_pc_rotation") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("rotation", Vector3, Required)),
        };
        ActionOverride["set_photo_studio"] = new TriggerDefinitionOverride("set_photo_studio") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isEnable", Bool, null)),
        };
        ActionOverride["set_portal"] = new TriggerDefinitionOverride("set_portal") {
            Names = BuildNameOverride(("arg1", "portalId"), ("arg2", "visible"), ("arg3", "enable"), ("arg4", "minimapVisible"), ("arg", "minimapVisible")),
            Types = BuildTypeOverride(("portalId", Int, Required), ("visible", Bool, null), ("enable", Bool, null), ("minimapVisible", Bool, null), ("arg5", Bool, null)),
        };
        ActionOverride["set_pvp_zone"] = new TriggerDefinitionOverride("set_pvp_zone") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "prepareTime"), ("arg3", "matchTime"), ("arg4", "additionalEffectId"), ("arg5", "type"), ("arg6", "boxIds")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("prepareTime", Int, Required), ("matchTime", Int, Required), ("additionalEffectId", Int, null), ("type", Int, null), ("boxIds", IntList, null)),
        };
        ActionOverride["set_quest_accept"] = new TriggerDefinitionOverride("set_quest_accept") {
            Names = BuildNameOverride(("questID", "questId"), ("arg1", "questId")),
            Types = BuildTypeOverride(("questId", Int, Required)),
        };
        ActionOverride["set_quest_complete"] = new TriggerDefinitionOverride("set_quest_complete") {
            Names = BuildNameOverride(("questID", "questId")),
            Types = BuildTypeOverride(("questId", Int, Required)),
        };
        ActionOverride["set_random_mesh"] = new TriggerDefinitionOverride("set_random_mesh") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible"), ("arg3", "startDelay"), ("arg4", "interval"), ("arg5", "fade")),
            Types = BuildTypeOverride(("triggerIds", IntList, Required), ("visible", Bool, null), ("startDelay", Int, null), ("interval", Int, null), ("fade", Int, null)),
        };
        ActionOverride["set_rope"] = new TriggerDefinitionOverride("set_rope") {
            Names = BuildNameOverride(("arg1", "triggerId"), ("arg2", "visible"), ("arg3", "enable"), ("arg4", "fade")),
            Types = BuildTypeOverride(("triggerId", Int, Required), ("visible", Bool, null), ("enable", Bool, null), ("fade", Int, null)),
        };
        ActionOverride["set_scene_skip"] = new TriggerDefinitionOverride("set_scene_skip") {
            Names = BuildNameOverride(("arg1", "state"), ("arg2", "action")),
            Types = BuildTypeOverride(("state", State, null)),
        };
        ActionOverride["set_skill"] = new TriggerDefinitionOverride("set_skill") {
            Names = BuildNameOverride(("objectIDs", "triggerIds"), ("arg1", "triggerIds"), ("arg2", "enable"), ("isEnable", "enable")),
            Types = BuildTypeOverride(("triggerIds", IntList, Required), ("enable", Bool, null)),
        };
        ActionOverride["set_skip"] = new TriggerDefinitionOverride("set_skip") {
            Names = BuildNameOverride(("arg1", "state")),
            Types = BuildTypeOverride(("state", State, null)),
        };
        ActionOverride["set_sound"] = new TriggerDefinitionOverride("set_sound") {
            Names = BuildNameOverride(("arg1", "triggerId"), ("arg2", "enable")),
            Types = BuildTypeOverride(("triggerId", Int, Required), ("enable", Bool, null)),
        };
        ActionOverride["set_state"] = new TriggerDefinitionOverride("set_state") {
            Names = BuildNameOverride(("arg1", "id"), ("arg2", "states"), ("arg3", "randomize")),
            Types = BuildTypeOverride(("id", Int, Required), ("states", StateList, Required), ("randomize", Bool, null)),
        };
        ActionOverride["set_time_scale"] = new TriggerDefinitionOverride("set_time_scale") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("enable", Bool, null), ("startScale", Float, null), ("endScale", Float, null), ("duration", Float, null), ("interpolator", Int, null)),
        };
        ActionOverride["set_timer"] = new TriggerDefinitionOverride("set_timer") {
            Names = BuildNameOverride(("arg1", "timerId"), ("arg2", "seconds"), ("arg3", "autoRemove"), ("ara3", "autoRemove"), ("arg4", "display"), ("arg5", "vOffset"), ("arg6", "type")),
            Types = BuildTypeOverride(("seconds", Int, null), ("autoRemove", Bool, null), ("display", Bool, null), ("vOffset", Int, null)),
        };
        ActionOverride["set_user_value"] = new TriggerDefinitionOverride("set_user_value") {
            Names = BuildNameOverride(("triggerID", "triggerId")),
            Types = BuildTypeOverride(("triggerId", Int, null), ("key", Str, Required), ("value", Int, Required)),
        };
        ActionOverride["set_user_value_from_dungeon_reward_count"] = new TriggerDefinitionOverride("set_user_value_from_dungeon_reward_count") {
            Names = BuildNameOverride(("dungeonRewardID", "dungeonRewardId")),
            Types = BuildTypeOverride(("dungeonRewardId", Int, Required)),
        };
        ActionOverride["set_user_value_from_guild_vs_game_score"] = new TriggerDefinitionOverride("set_user_value_from_guild_vs_game_score") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Int, Required)),
        };
        ActionOverride["set_user_value_from_user_count"] = new TriggerDefinitionOverride("set_user_value_from_user_count") {
            Names = BuildNameOverride(("triggerBoxID", "triggerBoxId"), ("userTagID", "userTagId")),
            Types = BuildTypeOverride(("triggerBoxId", Int, Required), ("key", Str, Required), ("userTagId", Int, Required)),
        };
        ActionOverride["set_visible_breakable_object"] = new TriggerDefinitionOverride("set_visible_breakable_object") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible")),
            Types = BuildTypeOverride(("triggerIds", IntList, Required), ("visible", Bool, null)),
        };
        ActionOverride["set_visible_ui"] = new TriggerDefinitionOverride("set_visible_ui") {
            Names = BuildNameOverride(("uiName", "uiNames")),
            Types = BuildTypeOverride(("uiNames", StrList, Required), ("visible", Bool, null)),
        };
        ActionOverride["shadow_expedition"] = new TriggerDefinitionOverride("") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["OpenBossGauge"] = new("shadow_expedition_open_boss_gauge", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("maxGaugePoint", Int, Required)),
                },
                ["CloseBossGauge"] = new("shadow_expedition_close_boss_gauge", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
            },
        };
        ActionOverride["show_caption"] = new TriggerDefinitionOverride("show_caption") {
            Names = BuildNameOverride(("offestRateX", "offsetRateX")),
            Types = BuildTypeOverride(("type", Str, Required), ("title", Str, Required), ("align", EnumAlign, null), ("offsetRateX", Float, null), ("offsetRateY", Float, null), ("duration", Int, null), ("scale", Float, null)),
        };
        ActionOverride["show_count_ui"] = new TriggerDefinitionOverride("show_count_ui") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("text", Str, Required), ("stage", Int, null), ("count", Int, Required), ("soundType", Int, "1")),
        };
        ActionOverride["show_event_result"] = new TriggerDefinitionOverride("show_event_result") {
            Names = BuildNameOverride(("userTagID", "userTagId"), ("triggerBoxID", "triggerBoxId"), ("isOutSide", "isOutside")),
            Types = BuildTypeOverride(("type", Str, Required), ("text", Str, Required), ("duration", Int, null), ("userTagId", Int, null), ("triggerBoxId", Int, null), ("isOutside", Bool, null)),
        };
        ActionOverride["show_guide_summary"] = new TriggerDefinitionOverride("show_guide_summary") {
            Names = BuildNameOverride(("entityID", "entityId"), ("textID", "textId"), ("durationTime", "duration")),
            Types = BuildTypeOverride(("entityId", Int, Required), ("textId", Int, null), ("duration", Int, null)),
        };
        ActionOverride["show_round_ui"] = new TriggerDefinitionOverride("show_round_ui") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("round", Int, Required), ("duration", Int, null), ("isFinalRound", Bool, null)),
        };
        ActionOverride["side_npc_talk"] = new TriggerDefinitionOverride("") {
            Types = BuildTypeOverride(("type", Str, "talk")),
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["talk"] = new("side_npc_talk", splitter: "type") {
                    Names = BuildNameOverride(("npcID", "npcId")),
                    Types = BuildTypeOverride(("npcId", Int, Required), ("illust", Str, Required), ("duration", Int, Required), ("script", Str, Required)),
                },
                ["talkbottom"] = new("side_npc_talk_bottom", splitter: "type") {
                    Names = BuildNameOverride(("npcID", "npcId")),
                    Types = BuildTypeOverride(("npcId", Int, Required), ("illust", Str, Required), ("duration", Int, Required), ("script", Str, Required)),
                },
                ["movie"] = new("side_npc_movie", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("usm", Str, Required), ("duration", Int, Required)),
                },
                ["cutin"] = new("side_npc_cutin", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("illust", Str, Required), ("duration", Int, Required)),
                },
            },
        };
        ActionOverride["sight_range"] = new TriggerDefinitionOverride("sight_range") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("enable", Bool, null), ("range", Int, Required), ("rangeZ", Int, null), ("border", Int, null)),
        };
        ActionOverride["spawn_item_range"] = new TriggerDefinitionOverride("spawn_item_range") {
            Names = BuildNameOverride(("rangeID", "rangeIds")),
            Types = BuildTypeOverride(("rangeIds", IntList, Required), ("randomPickCount", Int, Required)),
        };
        ActionOverride["spawn_npc_range"] = new TriggerDefinitionOverride("spawn_npc_range") {
            Names = BuildNameOverride(("rangeID", "rangeIds")),
            Types = BuildTypeOverride(("rangeIds", IntList, Required), ("isAutoTargeting", Bool, null), ("randomPickCount", Int, null), ("score", Int, null)),
        };
        ActionOverride["start_combine_spawn"] = new TriggerDefinitionOverride("start_combine_spawn") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("groupId", IntList, Required), ("isStart", Bool, null)),
        };
        ActionOverride["start_mini_game"] = new TriggerDefinitionOverride("start_mini_game") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("boxId", Int, Required), ("round", Int, Required), ("gameName", Str, Required), ("isShowResultUI", Bool, "True")),
        };
        ActionOverride["start_mini_game_round"] = new TriggerDefinitionOverride("start_mini_game_round") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("boxId", Int, Required), ("round", Int, Required)),
        };
        ActionOverride["start_tutorial"] = new TriggerDefinitionOverride("start_tutorial") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["talk_npc"] = new TriggerDefinitionOverride("talk_npc") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Int, Required)),
        };
        ActionOverride["unset_mini_game_area_for_hack"] = new TriggerDefinitionOverride("unset_mini_game_area_for_hack") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["use_state"] = new TriggerDefinitionOverride("use_state") {
            Names = BuildNameOverride(("arg1", "id"), ("arg2", "randomize")),
            Types = BuildTypeOverride(("id", Int, null), ("randomize", Bool, null)),
        };
        ActionOverride["user_tag_symbol"] = new TriggerDefinitionOverride("user_tag_symbol") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("symbol1", Str, Required), ("symbol2", Str, Required)),
        };
        ActionOverride["user_value_to_number_mesh"] = new TriggerDefinitionOverride("user_value_to_number_mesh") {
            Names = BuildNameOverride(("startMeshID", "startMeshId")),
            Types = BuildTypeOverride(("key", Str, Required), ("startMeshId", Int, Required), ("digitCount", Int, Required)),
        };
        ActionOverride["visible_my_pc"] = new TriggerDefinitionOverride("visible_my_pc") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isVisible", Bool, Required)),
        };
        ActionOverride["weather"] = new TriggerDefinitionOverride("weather") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("weatherType", EnumWeather, Required)),
        };
        ActionOverride["wedding_broken"] = new TriggerDefinitionOverride("wedding_broken") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["wedding_move_user"] = new TriggerDefinitionOverride("wedding_move_user") {
            Names = BuildNameOverride(("arg1", "mapId"), ("arg2", "portalIds"), ("arg3", "boxId")),
            Types = BuildTypeOverride(("entryType", Str, Required), ("mapId", Int, Required), ("portalIds", IntList, Required), ("boxId", Int, Required)),
        };
        ActionOverride["wedding_mutual_agree"] = new TriggerDefinitionOverride("wedding_mutual_agree") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("agreeType", Str, Required)),
        };
        ActionOverride["wedding_mutual_cancel"] = new TriggerDefinitionOverride("wedding_mutual_cancel") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("agreeType", Str, Required)),
        };
        ActionOverride["wedding_set_user_emotion"] = new TriggerDefinitionOverride("wedding_set_user_emotion") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("entryType", Str, Required), ("id", Int, Required)),
        };
        ActionOverride["wedding_set_user_look_at"] = new TriggerDefinitionOverride("wedding_set_user_look_at") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("entryType", Str, Required), ("lookAtEntryType", Str, Required), ("immediate", Bool, null)),
        };
        ActionOverride["wedding_set_user_rotation"] = new TriggerDefinitionOverride("wedding_set_user_rotation") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("entryType", Str, Required), ("rotation", Vector3, Required), ("immediate", Bool, null)),
        };
        ActionOverride["wedding_user_to_patrol"] = new TriggerDefinitionOverride("wedding_user_to_patrol") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("entryType", Str, Required), ("patrolIndex", Int, null)),
        };
        ActionOverride["wedding_vow_complete"] = new TriggerDefinitionOverride("wedding_vow_complete") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["widget_action"] = new TriggerDefinitionOverride("widget_action") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "func"), ("arg3", "widgetArg")),
            Types = BuildTypeOverride(("type", Str, Required), ("func", Str, Required), ("widgetArgNum", Int, null)),
        };
        ActionOverride["write_log"] = new TriggerDefinitionOverride("write_log") {
            Names = BuildNameOverride(("arg1", "logName"), ("arg2", "triggerId"), ("arg3", "event"), ("arg4", "level"), ("arg5", "subEvent")),
            Types = BuildTypeOverride(("logName", Str, Required), ("triggerId", Int, null), ("level", Int, null)),
        };

        // Condition Override
        ConditionOverride["all_of"] = new TriggerDefinitionOverride("all_of") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["true"] = new TriggerDefinitionOverride("true") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["any_one"] = new TriggerDefinitionOverride("any_one") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["always"] = new TriggerDefinitionOverride("always") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("arg1", Bool, "True")),
        };
        ConditionOverride["bonus_game_reward_detected"] = new TriggerDefinitionOverride("bonus_game_reward") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "type")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("type", Int, Required)),
            Compare = BuildCompareOverride(Int, "type", "<none>"),
        };
        ConditionOverride["check_any_user_additional_effect"] = new TriggerDefinitionOverride("check_any_user_additional_effect") {
            Names = BuildNameOverride(("triggerBoxID", "boxId"), ("additionalEffectID", "additionalEffectId")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("additionalEffectId", Int, Required), ("level", Int, Required)),
        };
        ConditionOverride["check_dungeon_lobby_user_count"] = new TriggerDefinitionOverride("check_dungeon_lobby_user_count") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["check_npc_additional_effect"] = new TriggerDefinitionOverride("check_npc_additional_effect") {
            Names = BuildNameOverride(("spawnPointID", "spawnId"), ("additionalEffectID", "additionalEffectId")),
            Types = BuildTypeOverride(("spawnId", Int, Required), ("additionalEffectId", Int, Required), ("level", Int, Required)),
        };
        ConditionOverride["check_npc_damage"] = new TriggerDefinitionOverride("npc_damage") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Int, Required), ("damageRate", Float, Required), ("operator", Str, "GreaterEqual")),
            Compare = BuildCompareOverride(Float, "damageRate", "operator", "GreaterEqual"),
        };
        ConditionOverride["check_npc_extra_data"] = new TriggerDefinitionOverride("npc_extra_data") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("spawnPointId", Int, Required), ("extraDataKey", Str, Required), ("extraDataValue", Int, Required)),
            Compare = BuildCompareOverride(Int, "extraDataValue", "operator", Required),
        };
        ConditionOverride["check_npc_hp"] = new TriggerDefinitionOverride("npc_hp") {
            Names = BuildNameOverride(("spawnPointId", "spawnId")),
            Types = BuildTypeOverride(("value", Int, Required), ("spawnId", Int, Required), ("isRelative", Bool, Required)),
            Compare = BuildCompareOverride(Int, "value", "compare", Required),
        };
        ConditionOverride["npc_is_dead_by_string_id"] = new TriggerDefinitionOverride("npc_is_dead_by_string_id") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("stringId", Str, Required)),
        };
        ConditionOverride["check_same_user_tag"] = new TriggerDefinitionOverride("check_same_user_tag") {
            Names = BuildNameOverride(("triggerBoxID", "boxId")),
            Types = BuildTypeOverride(("boxId", Int, Required)),
        };
        ConditionOverride["check_user"] = new TriggerDefinitionOverride("check_user") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["check_user_count"] = new TriggerDefinitionOverride("user_count") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("checkCount", Int, null)),
            Compare = BuildCompareOverride(Int, "checkCount", "<none>"),
        };
        ConditionOverride["count_users"] = new TriggerDefinitionOverride("count_users") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "minUsers"), ("arg3", "operator"), ("userTagID", "userTagId")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("minUsers", Int, Required), ("operator", Str, "GreaterEqual"), ("userTagId", Int, null)),
            Compare = BuildCompareOverride(Int, "minUsers", "operator", "GreaterEqual"),
        };
        ConditionOverride["day_of_week"] = new TriggerDefinitionOverride("day_of_week") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("dayOfWeeks", IntList, Required)),
            Compare = BuildCompareOverride(Int, "dayOfWeeks", "<none>", "in"),
        };
        ConditionOverride["detect_liftable_object"] = new TriggerDefinitionOverride("detect_liftable_object") {
            Names = BuildNameOverride(("triggerBoxIDs", "boxIds"), ("itemID", "itemId")),
            Types = BuildTypeOverride(("boxIds", IntList, Required), ("itemId", Int, Required)),
        };
        ConditionOverride["dungeon_check_play_time"] = new TriggerDefinitionOverride("dungeon_play_time") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("playSeconds", Int, Required), ("operator", Str, "GreaterEqual")),
            Compare = BuildCompareOverride(Int, "playSeconds", "operator", "GreaterEqual"),
        };
        ConditionOverride["dungeon_check_state"] = new TriggerDefinitionOverride("dungeon_state") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
            Compare = BuildCompareOverride(Str, "checkState", "<none>"),
        };
        ConditionOverride["dungeon_first_user_mission_score"] = new TriggerDefinitionOverride("dungeon_first_user_mission_score") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("score", Int, Required), ("operator", Str, "GreaterEqual")),
            Compare = BuildCompareOverride(Int, "score", "operator", "GreaterEqual"),
        };
        ConditionOverride["dungeon_id"] = new TriggerDefinitionOverride("dungeon_id") {
            Names = BuildNameOverride(("dungeonID", "dungeonId")),
            Types = BuildTypeOverride(("dungeonId", Int, Required)),
            Compare = BuildCompareOverride(Int, "dungeonId", "<none>"),
        };
        ConditionOverride["dungeon_level"] = new TriggerDefinitionOverride("dungeon_level") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("level", Int, Required)),
            Compare = BuildCompareOverride(Int, "level", "<none>"),
        };
        ConditionOverride["dungeon_max_user_count"] = new TriggerDefinitionOverride("dungeon_max_user_count") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("value", Int, Required)),
            Compare = BuildCompareOverride(Int, "value", "<none>"),
        };
        ConditionOverride["dungeon_round_require"] = new TriggerDefinitionOverride("dungeon_round") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("round", Int, Required)),
            Compare = BuildCompareOverride(Int, "round", "<none>"),
        };
        ConditionOverride["dungeon_time_out"] = new TriggerDefinitionOverride("dungeon_timeout") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["dungeon_variable"] = new TriggerDefinitionOverride("dungeon_variable") {
            Names = BuildNameOverride(("varID", "varId")),
            Types = BuildTypeOverride(("varId", Int, Required), ("value", Int, Required)),
            Compare = BuildCompareOverride(Int, "value", "<none>"),
        };
        ConditionOverride["guild_vs_game_scored_team"] = new TriggerDefinitionOverride("guild_vs_game_scored_team") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Int, Required)),
        };
        ConditionOverride["guild_vs_game_winner_team"] = new TriggerDefinitionOverride("guild_vs_game_winner_team") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Int, Required)),
        };
        ConditionOverride["is_dungeon_room"] = new TriggerDefinitionOverride("is_dungeon_room") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["is_playing_maple_survival"] = new TriggerDefinitionOverride("is_playing_maple_survival") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["monster_dead"] = new TriggerDefinitionOverride("monster_dead") {
            Names = BuildNameOverride(("arg1", "spawnIds"), ("arg2", "autoTarget")),
            Types = BuildTypeOverride(("spawnIds", IntList, Required), ("autoTarget", Bool, "True")),
        };
        ConditionOverride["monster_in_combat"] = new TriggerDefinitionOverride("monster_in_combat") {
            Names = BuildNameOverride(("arg1", "spawnIds")),
            Types = BuildTypeOverride(("spawnIds", IntList, Required)),
        };
        ConditionOverride["npc_detected"] = new TriggerDefinitionOverride("npc_detected") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "spawnIds")),
            Types = BuildTypeOverride(("boxId", Int, Required), ("spawnIds", IntList, Required)),
        };
        ConditionOverride["object_interacted"] = new TriggerDefinitionOverride("object_interacted") {
            Names = BuildNameOverride(("arg1", "interactIds"), ("arg2", "state"), ("ar2", "state")),
            Types = BuildTypeOverride(("interactIds", IntList, Required), ("state", Int, "0")),
        };
        ConditionOverride["pvp_zone_ended"] = new TriggerDefinitionOverride("pvp_zone_ended") {
            Names = BuildNameOverride(("arg1", "boxId")),
            Types = BuildTypeOverride(("boxId", Int, Required)),
        };
        ConditionOverride["quest_user_detected"] = new TriggerDefinitionOverride("quest_user_detected") {
            Names = BuildNameOverride(("arg1", "boxIds"), ("arg2", "questIds"), ("arg3", "questStates"), ("arg4", "jobCode")),
            Types = BuildTypeOverride(("boxIds", IntList, Required), ("questIds", IntList, Required), ("questStates", IntList, Required), ("jobCode", Int, null)),
        };
        ConditionOverride["random_condition"] = new TriggerDefinitionOverride("random_condition") {
            Names = BuildNameOverride(("arg1", "weight")),
            Types = BuildTypeOverride(("weight", Float, Required)),
        };
        ConditionOverride["score_board_compare"] = new TriggerDefinitionOverride("score_board_score") {
            Names = BuildNameOverride(("compareOp", "operator")),
            Types = BuildTypeOverride(("operator", Str, "GreaterEqual"), ("score", Int, Required)),
            Compare = BuildCompareOverride(Int, "score", "operator", "GreaterEqual"),
        };
        ConditionOverride["shadow_expedition_reach_point"] = new TriggerDefinitionOverride("shadow_expedition_points") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("point", Int, Required)),
            Compare = BuildCompareOverride(Int, "point", "<none>", "GreaterEqual"),
        };
        ConditionOverride["time_expired"] = new TriggerDefinitionOverride("time_expired") {
            Names = BuildNameOverride(("arg1", "timerId")),
            Types = BuildTypeOverride(("timerId", Str, Required)),
        };
        ConditionOverride["user_detected"] = new TriggerDefinitionOverride("user_detected") {
            Names = BuildNameOverride(("arg1", "boxIds"), ("arg2", "jobCode")),
            Types = BuildTypeOverride(("boxIds", IntList, Required), ("jobCode", Int, null)),
        };
        ConditionOverride["user_value"] = new TriggerDefinitionOverride("user_value") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("key", Str, Required), ("value", Int, Required), ("operator", Str, "Equal")),
            Compare = BuildCompareOverride(Int, "value", "operator"),
        };
        ConditionOverride["wait_and_reset_tick"] = new TriggerDefinitionOverride("wait_and_reset_tick") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("waitTick", Int, Required)),
        };
        ConditionOverride["wait_seconds_user_value"] = new TriggerDefinitionOverride("wait_seconds_user_value") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("key", Str, Required)),
        };
        ConditionOverride["wait_tick"] = new TriggerDefinitionOverride("wait_tick") {
            Names = BuildNameOverride(("arg1", "waitTick")),
            Types = BuildTypeOverride(("waitTick", Int, Required)),
        };
        ConditionOverride["wedding_entry_in_field"] = new TriggerDefinitionOverride("wedding_entry_in_field") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("entryType", Str, Required), ("isInField", Bool, Required)),
        };
        ConditionOverride["wedding_hall_state"] = new TriggerDefinitionOverride("wedding_hall_state") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("success", Bool, null)),
            Compare = BuildCompareOverride(Str, "hall_state", "<none>"),
        };
        ConditionOverride["wedding_mutual_agree_result"] = new TriggerDefinitionOverride("wedding_mutual_agree_result") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("agreeType", Str, Required), ("success", Bool, "True")),
            Compare = BuildCompareOverride(Bool, "success", "<none>"),
        };
        ConditionOverride["widget_condition"] = new TriggerDefinitionOverride("widget_value") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "name"), ("arg3", "condition")),
            Types = BuildTypeOverride(("type", Str, Required), ("name", Str, Required)),
            Compare = BuildCompareOverride(Int, "condition", "condition", "<placeholder>"),
        };
    }

    private static Dictionary<string, string> BuildNameOverride(params (string, string)[] overrides) {
        Dictionary<string, string> mapping = [];
        foreach ((string Old, string New) entry in overrides) {
            string oldName = TriggerTranslate.ToSnakeCase(entry.Old);
            string newName = TriggerTranslate.ToSnakeCase(entry.New);
            Debug.Assert(!mapping.ContainsKey(oldName), $"Duplicate override key: {oldName}");
            mapping.Add(oldName, newName);
        }
        return mapping;
    }

    private static Dictionary<string, (ScriptType, string?)> BuildTypeOverride(params (string, ScriptType, string?)[] overrides) {
        Dictionary<string, (ScriptType, string?)> mapping = [];
        foreach ((string name, ScriptType argType, string? defaultValue) in overrides) {
            string argName = TriggerTranslate.ToSnakeCase(name);
            Debug.Assert(!mapping.ContainsKey(argName), $"Duplicate override key: {argName}");
            mapping.Add(argName, (argType, defaultValue));
        }
        return mapping;
    }

    // Passing an invalid string as @default
    private static (ScriptType, string, string, string) BuildCompareOverride(ScriptType type, string field, string op, string @default = "Equal") {
        return (type, TriggerTranslate.ToSnakeCase(field), TriggerTranslate.ToSnakeCase(op), @default);
    }
}
