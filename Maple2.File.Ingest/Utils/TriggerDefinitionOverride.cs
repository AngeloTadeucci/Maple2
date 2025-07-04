using System.Diagnostics;

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
    public Dictionary<string, string?> Types { get; init; } = null!;

    // Comparison Operation (Only for Conditions)
    public (string Field, string Op, string Default) Compare { get; init; }

    public string? FunctionSplitter { get; init; }
    public Dictionary<string, TriggerDefinitionOverride> FunctionLookup { get; init; } = null!;

    private TriggerDefinitionOverride(string name, string? splitter = null) {
        Name = name;
        FunctionSplitter = splitter;
    }

    public static readonly Dictionary<string, TriggerDefinitionOverride> ActionOverride = new Dictionary<string, TriggerDefinitionOverride>();
    public static readonly Dictionary<string, TriggerDefinitionOverride> ConditionOverride = new Dictionary<string, TriggerDefinitionOverride>();

    static TriggerDefinitionOverride() {
        // Action Override
        ActionOverride["add_balloon_talk"] = new TriggerDefinitionOverride("add_balloon_talk") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", null), ("duration", null), ("delayTick", null), ("npcID", null)),
        };
        ActionOverride["add_buff"] = new TriggerDefinitionOverride("add_buff") {
            Names = BuildNameOverride(("arg1", "boxIds"), ("arg2", "skillId"), ("arg3", "level"), ("arg4", "ignorePlayer"), ("arg5", "isSkillSet")),
            Types = BuildTypeOverride(("boxIds", Required), ("skillId", Required), ("level", Required), ("ignorePlayer", "True"), ("isSkillSet", "True")),
        };
        ActionOverride["add_cinematic_talk"] = new TriggerDefinitionOverride("add_cinematic_talk") {
            Names = BuildNameOverride(("npcID", "npcId"), ("illustID", "illustId"), ("illust", "illustId"), ("delay", "delayTick")),
            Types = BuildTypeOverride(("npcId", Required), ("duration", null), ("align", null), ("delayTick", null)),
        };
        ActionOverride["add_effect_nif"] = new TriggerDefinitionOverride("add_effect_nif") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Required), ("isOutline", null), ("scale", null), ("rotateZ", null)),
        };
        ActionOverride["add_user_value"] = new TriggerDefinitionOverride("add_user_value") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("value", Required)),
        };
        ActionOverride["allocate_battlefield_points"] = new TriggerDefinitionOverride("allocate_battlefield_points") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "points")),
            Types = BuildTypeOverride(("boxId", Required), ("points", Required)),
        };
        ActionOverride["announce"] = new TriggerDefinitionOverride("announce") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "content")),
            Types = BuildTypeOverride(("type", null), ("content", Required), ("arg3", null)),
        };
        ActionOverride["arcade_boom_boom_ocean"] = new TriggerDefinitionOverride(string.Empty) {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new TriggerDefinitionOverride("arcade_boom_boom_ocean_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("lifeCount", Required)),
                },
                ["EndGame"] = new TriggerDefinitionOverride("arcade_boom_boom_ocean_end_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
                ["SetSkillScore"] = new TriggerDefinitionOverride("arcade_boom_boom_ocean_set_skill_score", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("type", Required), ("id", Required), ("score", Required)),
                },
                ["StartRound"] = new TriggerDefinitionOverride("arcade_boom_boom_ocean_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("type", Required), ("round", Required), ("roundDuration", Required), ("timeScoreRate", Required)),
                },
                ["ClearRound"] = new TriggerDefinitionOverride("arcade_boom_boom_ocean_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("type", Required), ("round", Required)),
                },
            },
        };
        ActionOverride["arcade_spring_farm"] = new TriggerDefinitionOverride("") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new TriggerDefinitionOverride("arcade_spring_farm_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("lifeCount", Required)),
                },
                ["EndGame"] = new TriggerDefinitionOverride("arcade_spring_farm_end_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
                ["SetInteractScore"] = new TriggerDefinitionOverride("arcade_spring_farm_set_interact_score", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("id", Required), ("score", Required)),
                },
                ["SpawnMonster"] = new TriggerDefinitionOverride("arcade_spring_farm_spawn_monster", splitter: "type") {
                    Names = BuildNameOverride(("spawnID", "spawnIds")),
                    Types = BuildTypeOverride(("spawnIds", Required), ("score", Required)),
                },
                ["StartRound"] = new TriggerDefinitionOverride("arcade_spring_farm_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("uiDuration", Required), ("round", Required), ("roundDuration", Required), ("timeScoreType", Required), ("timeScoreRate", Required)),
                },
                ["ClearRound"] = new TriggerDefinitionOverride("arcade_spring_farm_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Required)),
                },
            },
        };
        ActionOverride["arcade_three_two_one"] = new TriggerDefinitionOverride("") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new TriggerDefinitionOverride("arcade_three_two_one_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("lifeCount", Required), ("initScore", Required)),
                },
                ["EndGame"] = new TriggerDefinitionOverride("arcade_three_two_one_end_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
                ["StartRound"] = new TriggerDefinitionOverride("arcade_three_two_one_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("uiDuration", Required), ("round", Required)),
                },
                ["ResultRound"] = new TriggerDefinitionOverride("arcade_three_two_one_result_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("resultDirection", Required)),
                },
                ["ResultRound2"] = new TriggerDefinitionOverride("arcade_three_two_one_result_round2", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Required)),
                },
                ["ClearRound"] = new TriggerDefinitionOverride("arcade_three_two_one_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Required)),
                },
            },
        };
        ActionOverride["arcade_three_two_one2"] = new TriggerDefinitionOverride("") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new TriggerDefinitionOverride("arcade_three_two_one2_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("lifeCount", Required), ("initScore", Required)),
                },
                ["EndGame"] = new TriggerDefinitionOverride("arcade_three_two_one2_end_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
                ["StartRound"] = new TriggerDefinitionOverride("arcade_three_two_one2_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("uiDuration", Required), ("round", Required)),
                },
                ["ResultRound"] = new TriggerDefinitionOverride("arcade_three_two_one2_result_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("resultDirection", Required)),
                },
                ["ResultRound2"] = new TriggerDefinitionOverride("arcade_three_two_one2_result_round2", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Required)),
                },
                ["ClearRound"] = new TriggerDefinitionOverride("arcade_three_two_one2_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Required)),
                },
            },
        };
        ActionOverride["arcade_three_two_one3"] = new TriggerDefinitionOverride("") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new TriggerDefinitionOverride("arcade_three_two_one3_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("lifeCount", Required), ("initScore", Required)),
                },
                ["EndGame"] = new TriggerDefinitionOverride("arcade_three_two_one3_end_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
                ["StartRound"] = new TriggerDefinitionOverride("arcade_three_two_one3_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("uiDuration", Required), ("round", Required)),
                },
                ["ResultRound"] = new TriggerDefinitionOverride("arcade_three_two_one3_result_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("resultDirection", Required)),
                },
                ["ResultRound2"] = new TriggerDefinitionOverride("arcade_three_two_one3_result_round2", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Required)),
                },
                ["ClearRound"] = new TriggerDefinitionOverride("arcade_three_two_one3_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Required)),
                },
            },
        };
        ActionOverride["change_background"] = new TriggerDefinitionOverride("change_background") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("dds", Required)),
        };
        ActionOverride["change_monster"] = new TriggerDefinitionOverride("change_monster") {
            Names = BuildNameOverride(("arg1", "fromSpawnId"), ("arg2", "toSpawnId")),
            Types = BuildTypeOverride(("fromSpawnId", Required), ("toSpawnId", Required)),
        };
        ActionOverride["close_cinematic"] = new TriggerDefinitionOverride("close_cinematic") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["create_field_game"] = new TriggerDefinitionOverride("create_field_game") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("type", Required), ("reset", null)),
        };
        ActionOverride["create_item"] = new TriggerDefinitionOverride("create_item") {
            Names = BuildNameOverride(("arg1", "spawnIds"), ("arg2", "triggerId"), ("arg3", "itemId")),
            Types = BuildTypeOverride(("spawnIds", Required), ("triggerId", null), ("itemId", null), ("arg5", null)),
        };
        ActionOverride["spawn_monster"] = new TriggerDefinitionOverride("spawn_monster") {
            Names = BuildNameOverride(("arg1", "spawnIds"), ("arg2", "autoTarget"), ("agr2", "autoTarget"), ("arg", "autoTarget"), ("arg3", "delay")),
            Types = BuildTypeOverride(("spawnIds", Required), ("autoTarget", "True"), ("delay", null)),
        };
        ActionOverride["create_widget"] = new TriggerDefinitionOverride("create_widget") {
            Names = BuildNameOverride(("arg1", "type")),
            Types = BuildTypeOverride(("type", Required)),
        };
        ActionOverride["dark_stream"] = new TriggerDefinitionOverride("dark_stream") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["StartGame"] = new TriggerDefinitionOverride("dark_stream_start_game", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Required)),
                },
                ["SpawnMonster"] = new TriggerDefinitionOverride("dark_stream_spawn_monster", splitter: "type") {
                    Names = BuildNameOverride(("spawnID", "spawnIds")),
                    Types = BuildTypeOverride(("spawnIds", Required), ("score", Required)),
                },
                ["StartRound"] = new TriggerDefinitionOverride("dark_stream_start_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("uiDuration", Required), ("round", Required), ("damagePenalty", Required)),
                },
                ["ClearRound"] = new TriggerDefinitionOverride("dark_stream_clear_round", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("round", Required)),
                },
            },
        };
        ActionOverride["debug_string"] = new TriggerDefinitionOverride("debug_string") {
            Names = BuildNameOverride(("arg1", "value"), ("string", "value")),
            Types = BuildTypeOverride(("value", Required)),
        };
        ActionOverride["destroy_monster"] = new TriggerDefinitionOverride("destroy_monster") {
            Names = BuildNameOverride(("arg1", "spawnIds"), ("agr2", "arg2")),
            Types = BuildTypeOverride(("spawnIds", Required), ("arg2", "True")),
        };
        ActionOverride["dungeon_clear"] = new TriggerDefinitionOverride("dungeon_clear") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("uiType", null)),
        };
        ActionOverride["dungeon_clear_round"] = new TriggerDefinitionOverride("dungeon_clear_round") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("round", Required)),
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
            Types = BuildTypeOverride(("isEnable", null)),
        };

        ActionOverride["dungeon_fail"] = new TriggerDefinitionOverride("dungeon_fail") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };

        ActionOverride["dungeon_mission_complete"] = new TriggerDefinitionOverride("dungeon_mission_complete") {
            Names = BuildNameOverride(("missionID", "missionId")),
            Types = BuildTypeOverride(("missionId", Required)),
        };

        ActionOverride["dungeon_move_lap_time_to_now"] = new TriggerDefinitionOverride("dungeon_move_lap_time_to_now") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("id", Required)),
        };

        ActionOverride["dungeon_reset_time"] = new TriggerDefinitionOverride("dungeon_reset_time") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("seconds", Required)),
        };

        ActionOverride["dungeon_set_end_time"] = new TriggerDefinitionOverride("dungeon_set_end_time") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };

        ActionOverride["dungeon_set_lap_time"] = new TriggerDefinitionOverride("dungeon_set_lap_time") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("id", Required), ("lapTime", null)),
        };

        ActionOverride["dungeon_stop_timer"] = new TriggerDefinitionOverride("dungeon_stop_timer") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["dungeon_variable"] = new TriggerDefinitionOverride("set_dungeon_variable") {
            Names = BuildNameOverride(("varID", "varId")),
            Types = BuildTypeOverride(("varId", Required), ("value", Required)),
        };
        ActionOverride["enable_local_camera"] = new TriggerDefinitionOverride("enable_local_camera") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isEnable", null)),
        };
        ActionOverride["enable_spawn_point_pc"] = new TriggerDefinitionOverride("enable_spawn_point_pc") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Required), ("isEnable", null)),
        };
        ActionOverride["end_mini_game"] = new TriggerDefinitionOverride("end_mini_game") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("winnerBoxId", null), ("isEnable", null), ("isOnlyWinner", null)),
        };
        ActionOverride["end_mini_game_round"] = new TriggerDefinitionOverride("end_mini_game_round") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("winnerBoxId", Required), ("expRate", null), ("meso", null), ("isOnlyWinner", null), ("isGainLoserBonus", null)),
        };
        ActionOverride["face_emotion"] = new TriggerDefinitionOverride("face_emotion") {
            Names = BuildNameOverride(("spawnPointID", "spawnId"), ("spwnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", null)),
        };
        ActionOverride["field_game_constant"] = new TriggerDefinitionOverride("field_game_constant") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("key", Required), ("value", Required), ("locale", null)),
        };
        ActionOverride["field_game_message"] = new TriggerDefinitionOverride("field_game_message") {
            Names = BuildNameOverride(("arg2", "script"), ("arg3", "duration")),
            Types = BuildTypeOverride(("custom", null), ("type", Required), ("duration", null), ("arg1", null), ("script", Required)),
        };
        ActionOverride["field_war_end"] = new TriggerDefinitionOverride("field_war_end") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isClear", null)),
        };
        ActionOverride["give_exp"] = new TriggerDefinitionOverride("give_exp") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "rate")),
            Types = BuildTypeOverride(("boxId", Required), ("rate", "1.0"), ("arg3", null)),
        };
        ActionOverride["give_guild_exp"] = new TriggerDefinitionOverride("give_guild_exp") {
            Names = BuildNameOverride(("boxID", "boxId")),
            Types = BuildTypeOverride(("boxId", null), ("type", Required)),
        };
        ActionOverride["give_reward_content"] = new TriggerDefinitionOverride("give_reward_content") {
            Names = BuildNameOverride(("rewardID", "rewardId")),
            Types = BuildTypeOverride(("rewardId", Required)),
        };
        ActionOverride["guide_event"] = new TriggerDefinitionOverride("guide_event") {
            Names = BuildNameOverride(("eventID", "eventId")),
            Types = BuildTypeOverride(("eventId", Required)),
        };
        ActionOverride["guild_vs_game_end_game"] = new TriggerDefinitionOverride("guild_vs_game_end_game") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["guild_vs_game_give_contribution"] = new TriggerDefinitionOverride("guild_vs_game_give_contribution") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Required), ("isWin", null)),
        };
        ActionOverride["guild_vs_game_give_reward"] = new TriggerDefinitionOverride("guild_vs_game_give_reward") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Required), ("isWin", null)),
        };
        ActionOverride["guild_vs_game_log_result"] = new TriggerDefinitionOverride("guild_vs_game_log_result") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["guild_vs_game_log_won_by_default"] = new TriggerDefinitionOverride("guild_vs_game_log_won_by_default") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Required)),
        };
        ActionOverride["guild_vs_game_result"] = new TriggerDefinitionOverride("guild_vs_game_result") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["guild_vs_game_score_by_user"] = new TriggerDefinitionOverride("guild_vs_game_score_by_user") {
            Names = BuildNameOverride(("triggerBoxID", "boxId")),
            Types = BuildTypeOverride(("boxId", Required), ("score", Required)),
        };
        ActionOverride["hide_guide_summary"] = new TriggerDefinitionOverride("hide_guide_summary") {
            Names = BuildNameOverride(("entityID", "entityId"), ("textID", "textId")),
            Types = BuildTypeOverride(("entityId", Required), ("textId", null)),
        };
        ActionOverride["init_npc_rotation"] = new TriggerDefinitionOverride("init_npc_rotation") {
            Names = BuildNameOverride(("arg1", "spawnIds")),
            Types = BuildTypeOverride(("spawnIds", Required)),
        };
        ActionOverride["kick_music_audience"] = new TriggerDefinitionOverride("kick_music_audience") {
            Names = BuildNameOverride(("targetBoxID", "boxId"), ("targetPortalID", "portalId")),
            Types = BuildTypeOverride(("boxId", Required), ("portalId", Required)),
        };
        ActionOverride["limit_spawn_npc_count"] = new TriggerDefinitionOverride("limit_spawn_npc_count") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("limitCount", Required)),
        };
        ActionOverride["lock_my_pc"] = new TriggerDefinitionOverride("lock_my_pc") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isLock", null)),
        };
        ActionOverride["mini_game_camera_direction"] = new TriggerDefinitionOverride("mini_game_camera_direction") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("boxId", Required), ("cameraId", Required)),
        };
        ActionOverride["mini_game_give_exp"] = new TriggerDefinitionOverride("mini_game_give_exp") {
            Names = BuildNameOverride(("isOutSide", "isOutside")),
            Types = BuildTypeOverride(("boxId", Required), ("expRate", "1.0"), ("isOutside", null)),
        };
        ActionOverride["mini_game_give_reward"] = new TriggerDefinitionOverride("mini_game_give_reward") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("winnerBoxId", Required), ("contentType", Required)),
        };
        ActionOverride["move_npc"] = new TriggerDefinitionOverride("move_npc") {
            Names = BuildNameOverride(("arg1", "spawnId"), ("arg2", "patrolName")),
            Types = BuildTypeOverride(("spawnId", Required), ("patrolName", Required)),
        };
        ActionOverride["move_npc_to_pos"] = new TriggerDefinitionOverride("move_npc_to_pos") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Required), ("pos", Required), ("rot", Required)),
        };
        ActionOverride["move_random_user"] = new TriggerDefinitionOverride("move_random_user") {
            Names = BuildNameOverride(("arg1", "mapId"), ("arg2", "portalId"), ("arg3", "boxId"), ("arg4", "count")),
            Types = BuildTypeOverride(("mapId", Required), ("portalId", Required), ("boxId", Required), ("count", Required)),
        };
        ActionOverride["move_to_portal"] = new TriggerDefinitionOverride("move_to_portal") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("userTagId", null), ("portalId", null), ("boxId", null)),
        };
        ActionOverride["move_user"] = new TriggerDefinitionOverride("move_user") {
            Names = BuildNameOverride(("arg1", "mapId"), ("arg2", "portalId"), ("arg3", "boxId")),
            Types = BuildTypeOverride(("mapId", null), ("portalId", null), ("boxId", null)),
        };
        ActionOverride["move_user_path"] = new TriggerDefinitionOverride("move_user_path") {
            Names = BuildNameOverride(("arg1", "patrolName")),
            Types = BuildTypeOverride(("patrolName", Required)),
        };
        ActionOverride["move_user_to_box"] = new TriggerDefinitionOverride("move_user_to_box") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("boxId", Required), ("portalId", Required)),
        };
        ActionOverride["move_user_to_pos"] = new TriggerDefinitionOverride("move_user_to_pos") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("pos", Required), ("rot", null)),
        };
        ActionOverride["notice"] = new TriggerDefinitionOverride("notice") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "script")),
            Types = BuildTypeOverride(("type", null), ("script", Required), ("arg3", null)),
        };
        ActionOverride["npc_remove_additional_effect"] = new TriggerDefinitionOverride("npc_remove_additional_effect") {
            Names = BuildNameOverride(("spawnPointID", "spawnId"), ("additionalEffectID", "additionalEffectId")),
            Types = BuildTypeOverride(("spawnId", Required), ("additionalEffectId", Required)),
        };
        ActionOverride["npc_to_patrol_in_box"] = new TriggerDefinitionOverride("npc_to_patrol_in_box") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("boxId", Required), ("npcId", Required)),
        };
        ActionOverride["patrol_condition_user"] = new TriggerDefinitionOverride("patrol_condition_user") {
            Names = BuildNameOverride(("additionalEffectID", "additionalEffectId")),
            Types = BuildTypeOverride(("patrolIndex", Required), ("additionalEffectId", Required)),
        };
        ActionOverride["play_scene_movie"] = new TriggerDefinitionOverride("play_scene_movie") {
            Names = BuildNameOverride(("movieID", "movieId")),
            Types = BuildTypeOverride(("movieId", null)),
        };
        ActionOverride["play_system_sound_by_user_tag"] = new TriggerDefinitionOverride("play_system_sound_by_user_tag") {
            Names = BuildNameOverride(("userTagID", "userTagId")),
            Types = BuildTypeOverride(("userTagId", Required), ("soundKey", Required)),
        };
        ActionOverride["play_system_sound_in_box"] = new TriggerDefinitionOverride("play_system_sound_in_box") {
            Names = BuildNameOverride(("arg1", "boxIds"), ("arg2", "sound")),
            Types = BuildTypeOverride(("boxIds", null), ("sound", Required)),
        };
        ActionOverride["random_additional_effect"] = new TriggerDefinitionOverride("random_additional_effect") {
            Names = BuildNameOverride(("Target", "target"), ("triggerBoxID", "boxId"), ("spawnPointID", "spawnId"), ("arg1", "boxIds"), ("additionalEffectID", "additionalEffectId")),
            Types = BuildTypeOverride(("boxId", null), ("spawnId", null), ("targetCount", null), ("tick", null), ("waitTick", null), ("additionalEffectId", null)),
        };
        ActionOverride["remove_balloon_talk"] = new TriggerDefinitionOverride("remove_balloon_talk") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", null)),
        };
        ActionOverride["remove_buff"] = new TriggerDefinitionOverride("remove_buff") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "skillId"), ("arg3", "isPlayer")),
            Types = BuildTypeOverride(("boxId", Required), ("skillId", Required), ("isPlayer", null)),
        };
        ActionOverride["remove_cinematic_talk"] = new TriggerDefinitionOverride("remove_cinematic_talk") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["remove_effect_nif"] = new TriggerDefinitionOverride("remove_effect_nif") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Required)),
        };
        ActionOverride["reset_camera"] = new TriggerDefinitionOverride("reset_camera") {
            Names = BuildNameOverride(("arg1", "interpolationTime"), ("arg2", "interpolationTime")),
            Types = BuildTypeOverride(("interpolationTime", null)),
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
            Types = BuildTypeOverride(("type", null), ("title", null), ("maxScore", null)),
        };
        ActionOverride["score_board_remove"] = new TriggerDefinitionOverride("score_board_remove") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["score_board_set_score"] = new TriggerDefinitionOverride("score_board_set_score") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("score", Required)),
        };
        ActionOverride["select_camera"] = new TriggerDefinitionOverride("select_camera") {
            Names = BuildNameOverride(("arg1", "triggerId"), ("arg2", "enable")),
            Types = BuildTypeOverride(("triggerId", Required), ("enable", "True")),
        };
        ActionOverride["select_camera_path"] = new TriggerDefinitionOverride("select_camera_path") {
            Names = BuildNameOverride(("arg1", "pathIds"), ("arg2", "returnView")),
            Types = BuildTypeOverride(("pathIds", Required), ("returnView", "True")),
        };
        ActionOverride["set_achievement"] = new TriggerDefinitionOverride("set_achievement") {
            Names = BuildNameOverride(("arg1", "triggerId"), ("arg2", "type"), ("arg3", "achieve")),
            Types = BuildTypeOverride(("triggerId", null)),
        };
        ActionOverride["set_actor"] = new TriggerDefinitionOverride("set_actor") {
            Names = BuildNameOverride(("arg1", "triggerId"), ("arg2", "visible"), ("arg3", "initialSequence")),
            Types = BuildTypeOverride(("triggerId", Required), ("visible", null), ("arg4", null), ("arg5", null)),
        };
        ActionOverride["set_agent"] = new TriggerDefinitionOverride("set_agent") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible")),
            Types = BuildTypeOverride(("triggerIds", Required), ("visible", null)),
        };
        ActionOverride["set_ai_extra_data"] = new TriggerDefinitionOverride("set_ai_extra_data") {
            Names = BuildNameOverride(("boxID", "boxId")),
            Types = BuildTypeOverride(("key", Required), ("value", Required), ("isModify", null), ("boxId", null)),
        };
        ActionOverride["set_ambient_light"] = new TriggerDefinitionOverride("set_ambient_light") {
            Names = BuildNameOverride(("arg1", "primary"), ("arg2", "secondary"), ("arg3", "tertiary")),
            Types = BuildTypeOverride(("primary", Required), ("secondary", null), ("tertiary", null)),
        };
        ActionOverride["set_breakable"] = new TriggerDefinitionOverride("set_breakable") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "enable")),
            Types = BuildTypeOverride(("triggerIds", Required), ("enable", null)),
        };
        ActionOverride["set_cinematic_intro"] = new TriggerDefinitionOverride("set_cinematic_intro") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["set_cinematic_ui"] = new TriggerDefinitionOverride("set_cinematic_ui") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "script")),
            Types = BuildTypeOverride(("type", Required), ("script", null), ("arg3", null)),
        };
        ActionOverride["set_dialogue"] = new TriggerDefinitionOverride("set_dialogue") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "spawnId"), ("arg3", "script"), ("arg4", "time")),
            Types = BuildTypeOverride(("type", Required), ("spawnId", null), ("script", Required), ("time", null), ("arg5", null), ("align", null)),
        };
        ActionOverride["set_cube"] = new TriggerDefinitionOverride("set_cube") {
            Names = BuildNameOverride(("IDs", "triggerIds"), ("arg1", "triggerIds"), ("arg2", "isVisible")),
            Types = BuildTypeOverride(("triggerIds", Required), ("isVisible", null), ("randomCount", null)),
        };
        ActionOverride["set_directional_light"] = new TriggerDefinitionOverride("set_directional_light") {
            Names = BuildNameOverride(("arg1", "diffuseColor"), ("arg2", "specularColor")),
            Types = BuildTypeOverride(("diffuseColor", Required), ("specularColor", null)),
        };
        ActionOverride["set_effect"] = new TriggerDefinitionOverride("set_effect") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible"), ("arg3", "startDelay"), ("arg4", "interval")),
            Types = BuildTypeOverride(("triggerIds", null), ("visible", null), ("startDelay", null), ("interval", null)),
        };
        ActionOverride["set_event_ui"] = new TriggerDefinitionOverride(string.Empty) {
            FunctionSplitter = "arg1",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["0"] = new TriggerDefinitionOverride("set_event_ui_round", splitter: "arg1") {
                    Names = BuildNameOverride(("arg2", "rounds"), ("arg4", "vOffset")),
                    Types = BuildTypeOverride(("rounds", Required), ("arg3", null), ("vOffset", null)),
                },
                ["1"] = new TriggerDefinitionOverride("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", Required), ("script", null), ("duration", Required), ("boxIds", null)),
                },
                ["2"] = new TriggerDefinitionOverride("set_event_ui_countdown", splitter: "arg1") {
                    Names = BuildNameOverride(("arg2", "script"), ("arg3", "roundCountdown"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("script", null), ("roundCountdown", Required), ("boxIds", null)),
                },
                ["3"] = new TriggerDefinitionOverride("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", Required), ("script", null), ("duration", Required), ("boxIds", null)),
                },
                ["4"] = new TriggerDefinitionOverride("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", Required), ("script", null), ("duration", Required), ("boxIds", null)),
                },
                ["5"] = new TriggerDefinitionOverride("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", Required), ("script", null), ("duration", Required), ("boxIds", null)),
                },
                ["6"] = new TriggerDefinitionOverride("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", Required), ("script", null), ("duration", Required), ("boxIds", null)),
                },
                ["7"] = new TriggerDefinitionOverride("set_event_ui_script", splitter: "arg1") {
                    Names = BuildNameOverride(("arg1", "type"), ("arg2", "script"), ("arg3", "duration"), ("arg4", "boxIds")),
                    Types = BuildTypeOverride(("type", Required), ("script", null), ("duration", Required), ("boxIds", null)),
                },
            },
        };
        ActionOverride["set_gravity"] = new TriggerDefinitionOverride("set_gravity") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("gravity", Required)),
        };
        ActionOverride["set_interact_object"] = new TriggerDefinitionOverride("set_interact_object") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "state")),
            Types = BuildTypeOverride(("triggerIds", Required), ("state", Required), ("arg4", null), ("arg3", null)),
        };
        ActionOverride["set_ladder"] = new TriggerDefinitionOverride("set_ladder") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible"), ("arg3", "enable"), ("arg4", "fade")),
            Types = BuildTypeOverride(("triggerIds", Required), ("visible", null), ("enable", null), ("fade", null)),
        };
        ActionOverride["set_local_camera"] = new TriggerDefinitionOverride("set_local_camera") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("cameraId", Required), ("enable", null)),
        };
        ActionOverride["set_mesh"] = new TriggerDefinitionOverride("set_mesh") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible"), ("arg3", "startDelay"), ("arg4", "interval"), ("arg5", "fade")),
            Types = BuildTypeOverride(("triggerIds", Required), ("visible", null), ("startDelay", null), ("interval", null), ("fade", null)),
        };
        ActionOverride["set_mesh_animation"] = new TriggerDefinitionOverride("set_mesh_animation") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible"), ("arg3", "startDelay"), ("arg4", "interval")),
            Types = BuildTypeOverride(("triggerIds", Required), ("visible", null), ("startDelay", null), ("interval", null)),
        };
        ActionOverride["set_mini_game_area_for_hack"] = new TriggerDefinitionOverride("set_mini_game_area_for_hack") {
            Names = BuildNameOverride(("boxID", "boxId")),
            Types = BuildTypeOverride(("boxId", Required)),
        };
        ActionOverride["set_npc_duel_hp_bar"] = new TriggerDefinitionOverride("set_npc_duel_hp_bar") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("isOpen", null), ("spawnId", Required), ("durationTick", null), ("npcHpStep", null)),
        };
        ActionOverride["set_npc_emotion_loop"] = new TriggerDefinitionOverride("set_npc_emotion_loop") {
            Names = BuildNameOverride(("arg1", "spawnId"), ("arg2", "sequenceName"), ("arg3", "duration"), ("arg", "duration")),
            Types = BuildTypeOverride(("spawnId", Required), ("duration", null)),
        };
        ActionOverride["set_npc_emotion_sequence"] = new TriggerDefinitionOverride("set_npc_emotion_sequence") {
            Names = BuildNameOverride(("arg1", "spawnId"), ("arg2", "sequenceName"), ("arg3", "durationTick")),
            Types = BuildTypeOverride(("spawnId", Required), ("sequenceName", Required), ("durationTick", null)),
        };
        ActionOverride["set_npc_rotation"] = new TriggerDefinitionOverride("set_npc_rotation") {
            Names = BuildNameOverride(("arg1", "spawnId"), ("arg2", "rotation")),
            Types = BuildTypeOverride(("spawnId", Required), ("rotation", Required)),
        };
        ActionOverride["set_onetime_effect"] = new TriggerDefinitionOverride("set_onetime_effect") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("id", null), ("enable", null)),
        };
        ActionOverride["set_pc_emotion_loop"] = new TriggerDefinitionOverride("set_pc_emotion_loop") {
            Names = BuildNameOverride(("arg1", "sequenceName"), ("arg2", "duration"), ("arg3", "loop")),
            Types = BuildTypeOverride(("sequenceName", Required), ("duration", null), ("loop", null)),
        };
        ActionOverride["set_pc_emotion_sequence"] = new TriggerDefinitionOverride("set_pc_emotion_sequence") {
            Names = BuildNameOverride(("arg1", "sequenceNames")),
            Types = BuildTypeOverride(("sequenceNames", Required)),
        };
        ActionOverride["set_pc_rotation"] = new TriggerDefinitionOverride("set_pc_rotation") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("rotation", Required)),
        };
        ActionOverride["set_photo_studio"] = new TriggerDefinitionOverride("set_photo_studio") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isEnable", null)),
        };
        ActionOverride["set_portal"] = new TriggerDefinitionOverride("set_portal") {
            Names = BuildNameOverride(("arg1", "portalId"), ("arg2", "visible"), ("arg3", "enable"), ("arg4", "minimapVisible"), ("arg", "minimapVisible")),
            Types = BuildTypeOverride(("portalId", Required), ("visible", null), ("enable", null), ("minimapVisible", null), ("arg5", null)),
        };
        ActionOverride["set_pvp_zone"] = new TriggerDefinitionOverride("set_pvp_zone") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "prepareTime"), ("arg3", "matchTime"), ("arg4", "additionalEffectId"), ("arg5", "type"), ("arg6", "boxIds")),
            Types = BuildTypeOverride(("boxId", Required), ("prepareTime", Required), ("matchTime", Required), ("additionalEffectId", null), ("type", null), ("boxIds", null)),
        };
        ActionOverride["set_quest_accept"] = new TriggerDefinitionOverride("set_quest_accept") {
            Names = BuildNameOverride(("questID", "questId"), ("arg1", "questId")),
            Types = BuildTypeOverride(("questId", Required)),
        };
        ActionOverride["set_quest_complete"] = new TriggerDefinitionOverride("set_quest_complete") {
            Names = BuildNameOverride(("questID", "questId")),
            Types = BuildTypeOverride(("questId", Required)),
        };
        ActionOverride["set_random_mesh"] = new TriggerDefinitionOverride("set_random_mesh") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible"), ("arg3", "startDelay"), ("arg4", "interval"), ("arg5", "fade")),
            Types = BuildTypeOverride(("triggerIds", Required), ("visible", null), ("startDelay", null), ("interval", null), ("fade", null)),
        };
        ActionOverride["set_rope"] = new TriggerDefinitionOverride("set_rope") {
            Names = BuildNameOverride(("arg1", "triggerId"), ("arg2", "visible"), ("arg3", "enable"), ("arg4", "fade")),
            Types = BuildTypeOverride(("triggerId", Required), ("visible", null), ("enable", null), ("fade", null)),
        };
        ActionOverride["set_scene_skip"] = new TriggerDefinitionOverride("set_scene_skip") {
            Names = BuildNameOverride(("arg1", "state"), ("arg2", "action")),
            Types = BuildTypeOverride(("state", null)),
        };
        ActionOverride["set_skill"] = new TriggerDefinitionOverride("set_skill") {
            Names = BuildNameOverride(("objectIDs", "triggerIds"), ("arg1", "triggerIds"), ("arg2", "enable"), ("isEnable", "enable")),
            Types = BuildTypeOverride(("triggerIds", Required), ("enable", null)),
        };
        ActionOverride["set_skip"] = new TriggerDefinitionOverride("set_skip") {
            Names = BuildNameOverride(("arg1", "state")),
            Types = BuildTypeOverride(("state", null)),
        };
        ActionOverride["set_sound"] = new TriggerDefinitionOverride("set_sound") {
            Names = BuildNameOverride(("arg1", "triggerId"), ("arg2", "enable")),
            Types = BuildTypeOverride(("triggerId", Required), ("enable", null)),
        };
        ActionOverride["set_state"] = new TriggerDefinitionOverride("set_state") {
            Names = BuildNameOverride(("arg1", "id"), ("arg2", "states"), ("arg3", "randomize")),
            Types = BuildTypeOverride(("id", Required), ("states", Required), ("randomize", null)),
        };
        ActionOverride["set_time_scale"] = new TriggerDefinitionOverride("set_time_scale") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("enable", null), ("startScale", null), ("endScale", null), ("duration", null), ("interpolator", null)),
        };
        ActionOverride["set_timer"] = new TriggerDefinitionOverride("set_timer") {
            Names = BuildNameOverride(("arg1", "timerId"), ("arg2", "seconds"), ("arg3", "autoRemove"), ("ara3", "autoRemove"), ("arg4", "display"), ("arg5", "vOffset"), ("arg6", "type")),
            Types = BuildTypeOverride(("seconds", null), ("autoRemove", null), ("display", null), ("vOffset", null)),
        };
        ActionOverride["set_user_value"] = new TriggerDefinitionOverride("set_user_value") {
            Names = BuildNameOverride(("triggerID", "triggerId")),
            Types = BuildTypeOverride(("triggerId", null), ("key", Required), ("value", Required)),
        };
        ActionOverride["set_user_value_from_dungeon_reward_count"] = new TriggerDefinitionOverride("set_user_value_from_dungeon_reward_count") {
            Names = BuildNameOverride(("dungeonRewardID", "dungeonRewardId")),
            Types = BuildTypeOverride(("dungeonRewardId", Required)),
        };
        ActionOverride["set_user_value_from_guild_vs_game_score"] = new TriggerDefinitionOverride("set_user_value_from_guild_vs_game_score") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Required)),
        };
        ActionOverride["set_user_value_from_user_count"] = new TriggerDefinitionOverride("set_user_value_from_user_count") {
            Names = BuildNameOverride(("triggerBoxID", "triggerBoxId"), ("userTagID", "userTagId")),
            Types = BuildTypeOverride(("triggerBoxId", Required), ("key", Required), ("userTagId", Required)),
        };
        ActionOverride["set_visible_breakable_object"] = new TriggerDefinitionOverride("set_visible_breakable_object") {
            Names = BuildNameOverride(("arg1", "triggerIds"), ("arg2", "visible")),
            Types = BuildTypeOverride(("triggerIds", Required), ("visible", null)),
        };
        ActionOverride["set_visible_ui"] = new TriggerDefinitionOverride("set_visible_ui") {
            Names = BuildNameOverride(("uiName", "uiNames")),
            Types = BuildTypeOverride(("uiNames", Required), ("visible", null)),
        };
        ActionOverride["shadow_expedition"] = new TriggerDefinitionOverride("") {
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["OpenBossGauge"] = new TriggerDefinitionOverride("shadow_expedition_open_boss_gauge", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("maxGaugePoint", Required)),
                },
                ["CloseBossGauge"] = new TriggerDefinitionOverride("shadow_expedition_close_boss_gauge", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(),
                },
            },
        };
        ActionOverride["show_caption"] = new TriggerDefinitionOverride("show_caption") {
            Names = BuildNameOverride(("offestRateX", "offsetRateX")),
            Types = BuildTypeOverride(("type", Required), ("title", Required), ("align", null), ("offsetRateX", null), ("offsetRateY", null), ("duration", null), ("scale", null)),
        };
        ActionOverride["show_count_ui"] = new TriggerDefinitionOverride("show_count_ui") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("text", Required), ("stage", null), ("count", Required), ("soundType", "1")),
        };
        ActionOverride["show_event_result"] = new TriggerDefinitionOverride("show_event_result") {
            Names = BuildNameOverride(("userTagID", "userTagId"), ("triggerBoxID", "triggerBoxId"), ("isOutSide", "isOutside")),
            Types = BuildTypeOverride(("type", Required), ("text", Required), ("duration", null), ("userTagId", null), ("triggerBoxId", null), ("isOutside", null)),
        };
        ActionOverride["show_guide_summary"] = new TriggerDefinitionOverride("show_guide_summary") {
            Names = BuildNameOverride(("entityID", "entityId"), ("textID", "textId"), ("durationTime", "duration")),
            Types = BuildTypeOverride(("entityId", Required), ("textId", null), ("duration", null)),
        };
        ActionOverride["show_round_ui"] = new TriggerDefinitionOverride("show_round_ui") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("round", Required), ("duration", null), ("isFinalRound", null)),
        };
        ActionOverride["side_npc_talk"] = new TriggerDefinitionOverride("") {
            Types = BuildTypeOverride(("type", "talk")),
            FunctionSplitter = "type",
            FunctionLookup = new Dictionary<string, TriggerDefinitionOverride> {
                ["talk"] = new TriggerDefinitionOverride("side_npc_talk", splitter: "type") {
                    Names = BuildNameOverride(("npcID", "npcId")),
                    Types = BuildTypeOverride(("npcId", Required), ("illust", Required), ("duration", Required), ("script", Required)),
                },
                ["talkbottom"] = new TriggerDefinitionOverride("side_npc_talk_bottom", splitter: "type") {
                    Names = BuildNameOverride(("npcID", "npcId")),
                    Types = BuildTypeOverride(("npcId", Required), ("illust", Required), ("duration", Required), ("script", Required)),
                },
                ["movie"] = new TriggerDefinitionOverride("side_npc_movie", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("usm", Required), ("duration", Required)),
                },
                ["cutin"] = new TriggerDefinitionOverride("side_npc_cutin", splitter: "type") {
                    Names = BuildNameOverride(),
                    Types = BuildTypeOverride(("illust", Required), ("duration", Required)),
                },
            },
        };
        ActionOverride["sight_range"] = new TriggerDefinitionOverride("sight_range") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("enable", null), ("range", Required), ("rangeZ", null), ("border", null)),
        };
        ActionOverride["spawn_item_range"] = new TriggerDefinitionOverride("spawn_item_range") {
            Names = BuildNameOverride(("rangeID", "rangeIds")),
            Types = BuildTypeOverride(("rangeIds", Required), ("randomPickCount", Required)),
        };
        ActionOverride["spawn_npc_range"] = new TriggerDefinitionOverride("spawn_npc_range") {
            Names = BuildNameOverride(("rangeID", "rangeIds")),
            Types = BuildTypeOverride(("rangeIds", Required), ("isAutoTargeting", null), ("randomPickCount", null), ("score", null)),
        };
        ActionOverride["start_combine_spawn"] = new TriggerDefinitionOverride("start_combine_spawn") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("groupId", Required), ("isStart", null)),
        };
        ActionOverride["start_mini_game"] = new TriggerDefinitionOverride("start_mini_game") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("boxId", Required), ("round", Required), ("gameName", Required), ("isShowResultUI", "True")),
        };
        ActionOverride["start_mini_game_round"] = new TriggerDefinitionOverride("start_mini_game_round") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("boxId", Required), ("round", Required)),
        };
        ActionOverride["start_tutorial"] = new TriggerDefinitionOverride("start_tutorial") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["talk_npc"] = new TriggerDefinitionOverride("talk_npc") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Required)),
        };
        ActionOverride["unset_mini_game_area_for_hack"] = new TriggerDefinitionOverride("unset_mini_game_area_for_hack") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["use_state"] = new TriggerDefinitionOverride("use_state") {
            Names = BuildNameOverride(("arg1", "id"), ("arg2", "randomize")),
            Types = BuildTypeOverride(("id", null), ("randomize", null)),
        };
        ActionOverride["user_tag_symbol"] = new TriggerDefinitionOverride("user_tag_symbol") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("symbol1", Required), ("symbol2", Required)),
        };
        ActionOverride["user_value_to_number_mesh"] = new TriggerDefinitionOverride("user_value_to_number_mesh") {
            Names = BuildNameOverride(("startMeshID", "startMeshId")),
            Types = BuildTypeOverride(("key", Required), ("startMeshId", Required), ("digitCount", Required)),
        };
        ActionOverride["visible_my_pc"] = new TriggerDefinitionOverride("visible_my_pc") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("isVisible", Required)),
        };
        ActionOverride["weather"] = new TriggerDefinitionOverride("weather") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("weatherType", Required)),
        };
        ActionOverride["wedding_broken"] = new TriggerDefinitionOverride("wedding_broken") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["wedding_move_user"] = new TriggerDefinitionOverride("wedding_move_user") {
            Names = BuildNameOverride(("arg1", "mapId"), ("arg2", "portalIds"), ("arg3", "boxId")),
            Types = BuildTypeOverride(("entryType", Required), ("mapId", Required), ("portalIds", Required), ("boxId", Required)),
        };
        ActionOverride["wedding_mutual_agree"] = new TriggerDefinitionOverride("wedding_mutual_agree") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("agreeType", Required)),
        };
        ActionOverride["wedding_mutual_cancel"] = new TriggerDefinitionOverride("wedding_mutual_cancel") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("agreeType", Required)),
        };
        ActionOverride["wedding_set_user_emotion"] = new TriggerDefinitionOverride("wedding_set_user_emotion") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("entryType", Required), ("id", Required)),
        };
        ActionOverride["wedding_set_user_look_at"] = new TriggerDefinitionOverride("wedding_set_user_look_at") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("entryType", Required), ("lookAtEntryType", Required), ("immediate", null)),
        };
        ActionOverride["wedding_set_user_rotation"] = new TriggerDefinitionOverride("wedding_set_user_rotation") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("entryType", Required), ("rotation", Required), ("immediate", null)),
        };
        ActionOverride["wedding_user_to_patrol"] = new TriggerDefinitionOverride("wedding_user_to_patrol") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("entryType", Required), ("patrolIndex", null)),
        };
        ActionOverride["wedding_vow_complete"] = new TriggerDefinitionOverride("wedding_vow_complete") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ActionOverride["widget_action"] = new TriggerDefinitionOverride("widget_action") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "func"), ("arg3", "widgetArg")),
            Types = BuildTypeOverride(("type", Required), ("func", Required), ("widgetArgNum", null)),
        };
        ActionOverride["write_log"] = new TriggerDefinitionOverride("write_log") {
            Names = BuildNameOverride(("arg1", "logName"), ("arg2", "triggerId"), ("arg3", "event"), ("arg4", "level"), ("arg5", "subEvent")),
            Types = BuildTypeOverride(("logName", Required), ("triggerId", null), ("level", null)),
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
            Types = BuildTypeOverride(("arg1", "True")),
        };
        ConditionOverride["bonus_game_reward_detected"] = new TriggerDefinitionOverride("bonus_game_reward") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "type")),
            Types = BuildTypeOverride(("boxId", Required), ("type", Required)),
            Compare = BuildCompareOverride("type", "<none>"),
        };
        ConditionOverride["check_any_user_additional_effect"] = new TriggerDefinitionOverride("check_any_user_additional_effect") {
            Names = BuildNameOverride(("triggerBoxID", "boxId"), ("additionalEffectID", "additionalEffectId")),
            Types = BuildTypeOverride(("boxId", Required), ("additionalEffectId", Required), ("level", Required)),
        };
        ConditionOverride["check_dungeon_lobby_user_count"] = new TriggerDefinitionOverride("check_dungeon_lobby_user_count") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["check_npc_additional_effect"] = new TriggerDefinitionOverride("check_npc_additional_effect") {
            Names = BuildNameOverride(("spawnPointID", "spawnId"), ("additionalEffectID", "additionalEffectId")),
            Types = BuildTypeOverride(("spawnId", Required), ("additionalEffectId", Required), ("level", Required)),
        };
        ConditionOverride["check_npc_damage"] = new TriggerDefinitionOverride("npc_damage") {
            Names = BuildNameOverride(("spawnPointID", "spawnId")),
            Types = BuildTypeOverride(("spawnId", Required), ("damageRate", Required), ("operator", "GreaterEqual")),
            Compare = BuildCompareOverride("damageRate", "operator", "GreaterEqual"),
        };
        ConditionOverride["check_npc_extra_data"] = new TriggerDefinitionOverride("npc_extra_data") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("spawnPointId", Required), ("extraDataKey", Required), ("extraDataValue", Required)),
            Compare = BuildCompareOverride("extraDataValue", "operator", Required),
        };
        ConditionOverride["check_npc_hp"] = new TriggerDefinitionOverride("npc_hp") {
            Names = BuildNameOverride(("spawnPointId", "spawnId")),
            Types = BuildTypeOverride(("value", Required), ("spawnId", Required), ("isRelative", Required)),
            Compare = BuildCompareOverride("value", "compare", Required),
        };
        ConditionOverride["npc_is_dead_by_string_id"] = new TriggerDefinitionOverride("npc_is_dead_by_string_id") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("stringId", Required)),
        };
        ConditionOverride["check_same_user_tag"] = new TriggerDefinitionOverride("check_same_user_tag") {
            Names = BuildNameOverride(("triggerBoxID", "boxId")),
            Types = BuildTypeOverride(("boxId", Required)),
        };
        ConditionOverride["check_user"] = new TriggerDefinitionOverride("check_user") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["check_user_count"] = new TriggerDefinitionOverride("user_count") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("checkCount", null)),
            Compare = BuildCompareOverride("checkCount", "<none>"),
        };
        ConditionOverride["count_users"] = new TriggerDefinitionOverride("count_users") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "minUsers"), ("arg3", "operator"), ("userTagID", "userTagId")),
            Types = BuildTypeOverride(("boxId", Required), ("minUsers", Required), ("operator", "GreaterEqual"), ("userTagId", null)),
            Compare = BuildCompareOverride("minUsers", "operator", "GreaterEqual"),
        };
        ConditionOverride["day_of_week"] = new TriggerDefinitionOverride("day_of_week") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("dayOfWeeks", Required)),
            Compare = BuildCompareOverride("dayOfWeeks", "<none>", "in"),
        };
        ConditionOverride["detect_liftable_object"] = new TriggerDefinitionOverride("detect_liftable_object") {
            Names = BuildNameOverride(("triggerBoxIDs", "boxIds"), ("itemID", "itemId")),
            Types = BuildTypeOverride(("boxIds", Required), ("itemId", Required)),
        };
        ConditionOverride["dungeon_check_play_time"] = new TriggerDefinitionOverride("dungeon_play_time") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("playSeconds", Required), ("operator", "GreaterEqual")),
            Compare = BuildCompareOverride("playSeconds", "operator", "GreaterEqual"),
        };
        ConditionOverride["dungeon_check_state"] = new TriggerDefinitionOverride("dungeon_state") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
            Compare = BuildCompareOverride("checkState", "<none>"),
        };
        ConditionOverride["dungeon_first_user_mission_score"] = new TriggerDefinitionOverride("dungeon_first_user_mission_score") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("score", Required), ("operator", "GreaterEqual")),
            Compare = BuildCompareOverride("score", "operator", "GreaterEqual"),
        };
        ConditionOverride["dungeon_id"] = new TriggerDefinitionOverride("dungeon_id") {
            Names = BuildNameOverride(("dungeonID", "dungeonId")),
            Types = BuildTypeOverride(("dungeonId", Required)),
            Compare = BuildCompareOverride("dungeonId", "<none>"),
        };
        ConditionOverride["dungeon_level"] = new TriggerDefinitionOverride("dungeon_level") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("level", Required)),
            Compare = BuildCompareOverride("level", "<none>"),
        };
        ConditionOverride["dungeon_max_user_count"] = new TriggerDefinitionOverride("dungeon_max_user_count") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("value", Required)),
            Compare = BuildCompareOverride("value", "<none>"),
        };
        ConditionOverride["dungeon_round_require"] = new TriggerDefinitionOverride("dungeon_round") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("round", Required)),
            Compare = BuildCompareOverride("round", "<none>"),
        };
        ConditionOverride["dungeon_time_out"] = new TriggerDefinitionOverride("dungeon_timeout") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(),
        };
        ConditionOverride["dungeon_variable"] = new TriggerDefinitionOverride("dungeon_variable") {
            Names = BuildNameOverride(("varID", "varId")),
            Types = BuildTypeOverride(("varId", Required), ("value", Required)),
            Compare = BuildCompareOverride("value", "<none>"),
        };
        ConditionOverride["guild_vs_game_scored_team"] = new TriggerDefinitionOverride("guild_vs_game_scored_team") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Required)),
        };
        ConditionOverride["guild_vs_game_winner_team"] = new TriggerDefinitionOverride("guild_vs_game_winner_team") {
            Names = BuildNameOverride(("teamID", "teamId")),
            Types = BuildTypeOverride(("teamId", Required)),
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
            Types = BuildTypeOverride(("spawnIds", Required), ("autoTarget", "True")),
        };
        ConditionOverride["monster_in_combat"] = new TriggerDefinitionOverride("monster_in_combat") {
            Names = BuildNameOverride(("arg1", "spawnIds")),
            Types = BuildTypeOverride(("spawnIds", Required)),
        };
        ConditionOverride["npc_detected"] = new TriggerDefinitionOverride("npc_detected") {
            Names = BuildNameOverride(("arg1", "boxId"), ("arg2", "spawnIds")),
            Types = BuildTypeOverride(("boxId", Required), ("spawnIds", Required)),
        };
        ConditionOverride["object_interacted"] = new TriggerDefinitionOverride("object_interacted") {
            Names = BuildNameOverride(("arg1", "interactIds"), ("arg2", "state"), ("ar2", "state")),
            Types = BuildTypeOverride(("interactIds", Required), ("state", "0")),
        };
        ConditionOverride["pvp_zone_ended"] = new TriggerDefinitionOverride("pvp_zone_ended") {
            Names = BuildNameOverride(("arg1", "boxId")),
            Types = BuildTypeOverride(("boxId", Required)),
        };
        ConditionOverride["quest_user_detected"] = new TriggerDefinitionOverride("quest_user_detected") {
            Names = BuildNameOverride(("arg1", "boxIds"), ("arg2", "questIds"), ("arg3", "questStates"), ("arg4", "jobCode")),
            Types = BuildTypeOverride(("boxIds", Required), ("questIds", Required), ("questStates", Required), ("jobCode", null)),
        };
        ConditionOverride["random_condition"] = new TriggerDefinitionOverride("random_condition") {
            Names = BuildNameOverride(("arg1", "weight")),
            Types = BuildTypeOverride(("weight", Required)),
        };
        ConditionOverride["score_board_compare"] = new TriggerDefinitionOverride("score_board_score") {
            Names = BuildNameOverride(("compareOp", "operator")),
            Types = BuildTypeOverride(("operator", "GreaterEqual"), ("score", Required)),
            Compare = BuildCompareOverride("score", "operator", "GreaterEqual"),
        };
        ConditionOverride["shadow_expedition_reach_point"] = new TriggerDefinitionOverride("shadow_expedition_points") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("point", Required)),
            Compare = BuildCompareOverride("point", "<none>", "GreaterEqual"),
        };
        ConditionOverride["time_expired"] = new TriggerDefinitionOverride("time_expired") {
            Names = BuildNameOverride(("arg1", "timerId")),
            Types = BuildTypeOverride(("timerId", Required)),
        };
        ConditionOverride["user_detected"] = new TriggerDefinitionOverride("user_detected") {
            Names = BuildNameOverride(("arg1", "boxIds"), ("arg2", "jobCode")),
            Types = BuildTypeOverride(("boxIds", Required), ("jobCode", null)),
        };
        ConditionOverride["user_value"] = new TriggerDefinitionOverride("user_value") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("key", Required), ("value", Required), ("operator", "Equal")),
            Compare = BuildCompareOverride("value", "operator"),
        };
        ConditionOverride["wait_and_reset_tick"] = new TriggerDefinitionOverride("wait_and_reset_tick") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("waitTick", Required)),
        };
        ConditionOverride["wait_seconds_user_value"] = new TriggerDefinitionOverride("wait_seconds_user_value") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("key", Required)),
        };
        ConditionOverride["wait_tick"] = new TriggerDefinitionOverride("wait_tick") {
            Names = BuildNameOverride(("arg1", "waitTick")),
            Types = BuildTypeOverride(("waitTick", Required)),
        };
        ConditionOverride["wedding_entry_in_field"] = new TriggerDefinitionOverride("wedding_entry_in_field") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("entryType", Required), ("isInField", Required)),
        };
        ConditionOverride["wedding_hall_state"] = new TriggerDefinitionOverride("wedding_hall_state") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("success", null)),
            Compare = BuildCompareOverride("hall_state", "<none>"),
        };
        ConditionOverride["wedding_mutual_agree_result"] = new TriggerDefinitionOverride("wedding_mutual_agree_result") {
            Names = BuildNameOverride(),
            Types = BuildTypeOverride(("agreeType", Required), ("success", "True")),
            Compare = BuildCompareOverride("success", "<none>"),
        };
        ConditionOverride["widget_condition"] = new TriggerDefinitionOverride("widget_value") {
            Names = BuildNameOverride(("arg1", "type"), ("arg2", "widgetName"), ("arg3", "condition")),
            Types = BuildTypeOverride(("type", Required), ("widgetName", Required)),
            Compare = BuildCompareOverride("condition", "condition", "<placeholder>"),
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

    private static Dictionary<string, string?> BuildTypeOverride(params (string, string?)[] overrides) {
        Dictionary<string, string?> mapping = [];
        foreach ((string name, string? defaultValue) in overrides) {
            string argName = TriggerTranslate.ToSnakeCase(name);
            Debug.Assert(!mapping.ContainsKey(argName), $"Duplicate override key: {argName}");
            mapping.Add(argName, defaultValue);
        }
        return mapping;
    }

    // Passing an invalid string as @default
    private static (string, string, string) BuildCompareOverride(string field, string op, string @default = "Equal") {
        return (TriggerTranslate.ToSnakeCase(field), TriggerTranslate.ToSnakeCase(op), @default);
    }
}
