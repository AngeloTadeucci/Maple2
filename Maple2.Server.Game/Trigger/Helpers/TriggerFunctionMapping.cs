﻿using System.Numerics;
using System.Xml;

namespace Maple2.Server.Game.Trigger.Helpers;

public static class TriggerFunctionMapping {
    public static readonly Dictionary<string, Action<ITriggerContext, XmlAttributeCollection?>> ActionMap = new Dictionary<string, Action<ITriggerContext, XmlAttributeCollection?>> {
        { "add_balloon_talk", (ctx, attrs) => ctx.AddBalloonTalk(ParseInt(attrs?["spawn_id"]?.Value), attrs?["msg"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), ParseInt(attrs?["delay_tick"]?.Value), ParseInt(attrs?["npc_id"]?.Value)) },
        { "add_buff", (ctx, attrs) => ctx.AddBuff(ParseIntArray(attrs?["box_ids"]?.Value), ParseInt(attrs?["skill_id"]?.Value), ParseInt(attrs?["level"]?.Value), ParseBool(attrs?["ignore_player"]?.Value), ParseBool(attrs?["is_skill_set"]?.Value), attrs?["feature"]?.Value ?? string.Empty) },
        { "add_cinematic_talk", (ctx, attrs) => ctx.AddCinematicTalk(ParseInt(attrs?["npc_id"]?.Value), attrs?["illust_id"]?.Value ?? string.Empty, attrs?["msg"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), ParseAlign(attrs?["align"]?.Value), ParseInt(attrs?["delay_tick"]?.Value)) },
        { "add_effect_nif", (ctx, attrs) => ctx.AddEffectNif(ParseInt(attrs?["spawn_id"]?.Value), attrs?["nif_path"]?.Value ?? string.Empty, ParseBool(attrs?["is_outline"]?.Value), ParseFloat(attrs?["scale"]?.Value), ParseInt(attrs?["rotate_z"]?.Value)) },
        { "add_user_value", (ctx, attrs) => ctx.AddUserValue(attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["value"]?.Value)) },
        { "allocate_battlefield_points", (ctx, attrs) => ctx.AllocateBattlefieldPoints(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["points"]?.Value)) },
        { "announce", (ctx, attrs) => ctx.Announce(ParseInt(attrs?["type"]?.Value), attrs?["content"]?.Value ?? string.Empty, ParseBool(attrs?["arg3"]?.Value)) },
        { "arcade_boom_boom_ocean_clear_round", (ctx, attrs) => ctx.ArcadeBoomBoomOceanClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_boom_boom_ocean_end_game", (ctx, _) => ctx.ArcadeBoomBoomOceanEndGame() },
        { "arcade_boom_boom_ocean_set_skill_score", (ctx, attrs) => ctx.ArcadeBoomBoomOceanSetSkillScore(ParseInt(attrs?["id"]?.Value), ParseInt(attrs?["score"]?.Value)) },
        { "arcade_boom_boom_ocean_start_game", (ctx, attrs) => ctx.ArcadeBoomBoomOceanStartGame(ParseInt(attrs?["life_count"]?.Value)) },
        { "arcade_boom_boom_ocean_start_round", (ctx, attrs) => ctx.ArcadeBoomBoomOceanStartRound(ParseInt(attrs?["round"]?.Value), ParseInt(attrs?["round_duration"]?.Value), ParseInt(attrs?["time_score_rate"]?.Value)) },
        { "arcade_spring_farm_clear_round", (ctx, attrs) => ctx.ArcadeSpringFarmClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_spring_farm_end_game", (ctx, _) => ctx.ArcadeSpringFarmEndGame() },
        { "arcade_spring_farm_set_interact_score", (ctx, attrs) => ctx.ArcadeSpringFarmSetInteractScore(ParseInt(attrs?["id"]?.Value), ParseInt(attrs?["score"]?.Value)) },
        { "arcade_spring_farm_spawn_monster", (ctx, attrs) => ctx.ArcadeSpringFarmSpawnMonster(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseInt(attrs?["score"]?.Value)) },
        { "arcade_spring_farm_start_game", (ctx, attrs) => ctx.ArcadeSpringFarmStartGame(ParseInt(attrs?["life_count"]?.Value)) },
        { "arcade_spring_farm_start_round", (ctx, attrs) => ctx.ArcadeSpringFarmStartRound(ParseInt(attrs?["ui_duration"]?.Value), ParseInt(attrs?["round"]?.Value), attrs?["time_score_type"]?.Value ?? string.Empty, ParseInt(attrs?["time_score_rate"]?.Value), ParseInt(attrs?["round_duration"]?.Value)) },
        { "arcade_three_two_one_clear_round", (ctx, attrs) => ctx.ArcadeThreeTwoOneClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one_end_game", (ctx, _) => ctx.ArcadeThreeTwoOneEndGame() },
        { "arcade_three_two_one_result_round", (ctx, attrs) => ctx.ArcadeThreeTwoOneResultRound(ParseInt(attrs?["result_direction"]?.Value)) },
        { "arcade_three_two_one_result_round2", (ctx, attrs) => ctx.ArcadeThreeTwoOneResultRound2(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one_start_game", (ctx, attrs) => ctx.ArcadeThreeTwoOneStartGame(ParseInt(attrs?["life_count"]?.Value), ParseInt(attrs?["init_score"]?.Value)) },
        { "arcade_three_two_one_start_round", (ctx, attrs) => ctx.ArcadeThreeTwoOneStartRound(ParseInt(attrs?["ui_duration"]?.Value), ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one2_clear_round", (ctx, attrs) => ctx.ArcadeThreeTwoOne2ClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one2_end_game", (ctx, _) => ctx.ArcadeThreeTwoOne2EndGame() },
        { "arcade_three_two_one2_result_round", (ctx, attrs) => ctx.ArcadeThreeTwoOne2ResultRound(ParseInt(attrs?["result_direction"]?.Value)) },
        { "arcade_three_two_one2_result_round2", (ctx, attrs) => ctx.ArcadeThreeTwoOne2ResultRound2(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one2_start_game", (ctx, attrs) => ctx.ArcadeThreeTwoOne2StartGame(ParseInt(attrs?["life_count"]?.Value), ParseInt(attrs?["init_score"]?.Value)) },
        { "arcade_three_two_one2_start_round", (ctx, attrs) => ctx.ArcadeThreeTwoOne2StartRound(ParseInt(attrs?["ui_duration"]?.Value), ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one3_clear_round", (ctx, attrs) => ctx.ArcadeThreeTwoOne3ClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one3_end_game", (ctx, _) => ctx.ArcadeThreeTwoOne3EndGame() },
        { "arcade_three_two_one3_result_round", (ctx, attrs) => ctx.ArcadeThreeTwoOne3ResultRound(ParseInt(attrs?["result_direction"]?.Value)) },
        { "arcade_three_two_one3_result_round2", (ctx, attrs) => ctx.ArcadeThreeTwoOne3ResultRound2(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one3_start_game", (ctx, attrs) => ctx.ArcadeThreeTwoOne3StartGame(ParseInt(attrs?["life_count"]?.Value), ParseInt(attrs?["init_score"]?.Value)) },
        { "arcade_three_two_one3_start_round", (ctx, attrs) => ctx.ArcadeThreeTwoOne3StartRound(ParseInt(attrs?["ui_duration"]?.Value), ParseInt(attrs?["round"]?.Value)) },
        { "change_background", (ctx, attrs) => ctx.ChangeBackground(attrs?["dds"]?.Value ?? string.Empty) },
        { "change_monster", (ctx, attrs) => ctx.ChangeMonster(ParseInt(attrs?["from_spawn_id"]?.Value), ParseInt(attrs?["to_spawn_id"]?.Value)) },
        { "close_cinematic", (ctx, _) => ctx.CloseCinematic() },
        { "create_field_game", (ctx, attrs) => ctx.CreateFieldGame(ParseFieldGame(attrs?["type"]?.Value), ParseBool(attrs?["reset"]?.Value)) },
        { "create_item", (ctx, attrs) => ctx.CreateItem(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseInt(attrs?["trigger_id"]?.Value), ParseInt(attrs?["item_id"]?.Value), ParseInt(attrs?["arg5"]?.Value)) },
        { "create_widget", (ctx, attrs) => ctx.CreateWidget(attrs?["type"]?.Value ?? string.Empty) },
        { "dark_stream_clear_round", (ctx, attrs) => ctx.DarkStreamClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "dark_stream_spawn_monster", (ctx, attrs) => ctx.DarkStreamSpawnMonster(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseInt(attrs?["score"]?.Value)) },
        { "dark_stream_start_game", (ctx, attrs) => ctx.DarkStreamStartGame(ParseInt(attrs?["round"]?.Value)) },
        { "dark_stream_start_round", (ctx, attrs) => ctx.DarkStreamStartRound(ParseInt(attrs?["round"]?.Value), ParseInt(attrs?["ui_duration"]?.Value), ParseInt(attrs?["damage_penalty"]?.Value)) },
        { "debug_string", (ctx, attrs) => ctx.DebugString(attrs?["value"]?.Value ?? string.Empty, attrs?["feature"]?.Value ?? string.Empty) },
        { "destroy_monster", (ctx, attrs) => ctx.DestroyMonster(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseBool(attrs?["arg2"]?.Value)) },
        { "dungeon_clear", (ctx, attrs) => ctx.DungeonClear(attrs?["ui_type"]?.Value ?? string.Empty) },
        { "dungeon_clear_round", (ctx, attrs) => ctx.DungeonClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "dungeon_close_timer", (ctx, _) => ctx.DungeonCloseTimer() },
        { "dungeon_disable_ranking", (ctx, _) => ctx.DungeonDisableRanking() },
        { "dungeon_enable_give_up", (ctx, attrs) => ctx.DungeonEnableGiveUp(ParseBool(attrs?["is_enable"]?.Value)) },
        { "dungeon_fail", (ctx, _) => ctx.DungeonFail() },
        { "dungeon_mission_complete", (ctx, attrs) => ctx.DungeonMissionComplete(attrs?["feature"]?.Value ?? string.Empty, ParseInt(attrs?["mission_id"]?.Value)) },
        { "dungeon_move_lap_time_to_now", (ctx, attrs) => ctx.DungeonMoveLapTimeToNow(ParseInt(attrs?["id"]?.Value)) },
        { "dungeon_reset_time", (ctx, attrs) => ctx.DungeonResetTime(ParseInt(attrs?["seconds"]?.Value)) },
        { "dungeon_set_end_time", (ctx, _) => ctx.DungeonSetEndTime() },
        { "dungeon_set_lap_time", (ctx, attrs) => ctx.DungeonSetLapTime(ParseInt(attrs?["id"]?.Value), ParseInt(attrs?["lap_time"]?.Value)) },
        { "dungeon_stop_timer", (ctx, _) => ctx.DungeonStopTimer() },
        { "set_dungeon_variable", (ctx, attrs) => ctx.SetDungeonVariable(ParseInt(attrs?["var_id"]?.Value), ParseInt(attrs?["value"]?.Value)) },
        { "enable_local_camera", (ctx, attrs) => ctx.EnableLocalCamera(ParseBool(attrs?["is_enable"]?.Value)) },
        { "enable_spawn_point_pc", (ctx, attrs) => ctx.EnableSpawnPointPc(ParseInt(attrs?["spawn_id"]?.Value), ParseBool(attrs?["is_enable"]?.Value)) },
        { "end_mini_game", (ctx, attrs) => ctx.EndMiniGame(ParseInt(attrs?["winner_box_id"]?.Value), attrs?["game_name"]?.Value ?? string.Empty, ParseBool(attrs?["is_only_winner"]?.Value)) },
        { "end_mini_game_round", (ctx, attrs) => ctx.EndMiniGameRound(ParseInt(attrs?["winner_box_id"]?.Value), ParseFloat(attrs?["exp_rate"]?.Value), ParseFloat(attrs?["meso"]?.Value), ParseBool(attrs?["is_only_winner"]?.Value), ParseBool(attrs?["is_gain_loser_bonus"]?.Value), attrs?["game_name"]?.Value ?? string.Empty) },
        { "face_emotion", (ctx, attrs) => ctx.FaceEmotion(ParseInt(attrs?["spawn_id"]?.Value), attrs?["emotion_name"]?.Value ?? string.Empty) },
        { "field_game_constant", (ctx, attrs) => ctx.FieldGameConstant(attrs?["key"]?.Value ?? string.Empty, attrs?["value"]?.Value ?? string.Empty, attrs?["feature"]?.Value ?? string.Empty, ParseLocale(attrs?["locale"]?.Value)) },
        { "field_game_message", (ctx, attrs) => ctx.FieldGameMessage(ParseInt(attrs?["custom"]?.Value), attrs?["type"]?.Value ?? string.Empty, ParseBool(attrs?["arg1"]?.Value), attrs?["script"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value)) },
        { "field_war_end", (ctx, attrs) => ctx.FieldWarEnd(ParseBool(attrs?["is_clear"]?.Value)) },
        { "give_exp", (ctx, attrs) => ctx.GiveExp(ParseInt(attrs?["box_id"]?.Value), ParseFloat(attrs?["rate"]?.Value), ParseBool(attrs?["arg3"]?.Value)) },
        { "give_guild_exp", (ctx, attrs) => ctx.GiveGuildExp(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["type"]?.Value)) },
        { "give_reward_content", (ctx, attrs) => ctx.GiveRewardContent(ParseInt(attrs?["reward_id"]?.Value)) },
        { "guide_event", (ctx, attrs) => ctx.GuideEvent(ParseInt(attrs?["event_id"]?.Value)) },
        { "guild_vs_game_end_game", (ctx, _) => ctx.GuildVsGameEndGame() },
        { "guild_vs_game_give_contribution", (ctx, attrs) => ctx.GuildVsGameGiveContribution(ParseInt(attrs?["team_id"]?.Value), ParseBool(attrs?["is_win"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "guild_vs_game_give_reward", (ctx, attrs) => ctx.GuildVsGameGiveReward(attrs?["type"]?.Value ?? string.Empty, ParseInt(attrs?["team_id"]?.Value), ParseBool(attrs?["is_win"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "guild_vs_game_log_result", (ctx, attrs) => ctx.GuildVsGameLogResult(attrs?["desc"]?.Value ?? string.Empty) },
        { "guild_vs_game_log_won_by_default", (ctx, attrs) => ctx.GuildVsGameLogWonByDefault(ParseInt(attrs?["team_id"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "guild_vs_game_result", (ctx, attrs) => ctx.GuildVsGameResult(attrs?["desc"]?.Value ?? string.Empty) },
        { "guild_vs_game_score_by_user", (ctx, attrs) => ctx.GuildVsGameScoreByUser(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["score"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "hide_guide_summary", (ctx, attrs) => ctx.HideGuideSummary(ParseInt(attrs?["entity_id"]?.Value), ParseInt(attrs?["text_id"]?.Value)) },
        { "init_npc_rotation", (ctx, attrs) => ctx.InitNpcRotation(ParseIntArray(attrs?["spawn_ids"]?.Value)) },
        { "kick_music_audience", (ctx, attrs) => ctx.KickMusicAudience(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["portal_id"]?.Value)) },
        { "limit_spawn_npc_count", (ctx, attrs) => ctx.LimitSpawnNpcCount(ParseInt(attrs?["limit_count"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "lock_my_pc", (ctx, attrs) => ctx.LockMyPc(ParseBool(attrs?["is_lock"]?.Value)) },
        { "mini_game_camera_direction", (ctx, attrs) => ctx.MiniGameCameraDirection(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["camera_id"]?.Value)) },
        { "mini_game_give_exp", (ctx, attrs) => ctx.MiniGameGiveExp(ParseInt(attrs?["box_id"]?.Value), ParseFloat(attrs?["exp_rate"]?.Value), ParseBool(attrs?["is_outside"]?.Value)) },
        { "mini_game_give_reward", (ctx, attrs) => ctx.MiniGameGiveReward(ParseInt(attrs?["winner_box_id"]?.Value), attrs?["content_type"]?.Value ?? string.Empty, attrs?["game_name"]?.Value ?? string.Empty) },
        { "move_npc", (ctx, attrs) => ctx.MoveNpc(ParseInt(attrs?["spawn_id"]?.Value), attrs?["patrol_name"]?.Value ?? string.Empty) },
        { "move_npc_to_pos", (ctx, attrs) => ctx.MoveNpcToPos(ParseInt(attrs?["spawn_id"]?.Value), ParseVector3(attrs?["pos"]?.Value), ParseVector3(attrs?["rot"]?.Value)) },
        { "move_random_user", (ctx, attrs) => ctx.MoveRandomUser(ParseInt(attrs?["map_id"]?.Value), ParseInt(attrs?["portal_id"]?.Value), ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["count"]?.Value)) },
        { "move_to_portal", (ctx, attrs) => ctx.MoveToPortal(ParseInt(attrs?["user_tag_id"]?.Value), ParseInt(attrs?["portal_id"]?.Value), ParseInt(attrs?["box_id"]?.Value)) },
        { "move_user", (ctx, attrs) => ctx.MoveUser(ParseInt(attrs?["map_id"]?.Value), ParseInt(attrs?["portal_id"]?.Value), ParseInt(attrs?["box_id"]?.Value)) },
        { "move_user_path", (ctx, attrs) => ctx.MoveUserPath(attrs?["patrol_name"]?.Value ?? string.Empty) },
        { "move_user_to_box", (ctx, attrs) => ctx.MoveUserToBox(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["portal_id"]?.Value)) },
        { "move_user_to_pos", (ctx, attrs) => ctx.MoveUserToPos(ParseVector3(attrs?["pos"]?.Value), ParseVector3(attrs?["rot"]?.Value)) },
        { "notice", (ctx, attrs) => ctx.Notice(ParseInt(attrs?["type"]?.Value), attrs?["script"]?.Value ?? string.Empty, ParseBool(attrs?["arg3"]?.Value)) },
        { "npc_remove_additional_effect", (ctx, attrs) => ctx.NpcRemoveAdditionalEffect(ParseInt(attrs?["spawn_id"]?.Value), ParseInt(attrs?["additional_effect_id"]?.Value)) },
        { "npc_to_patrol_in_box", (ctx, attrs) => ctx.NpcToPatrolInBox(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["npc_id"]?.Value), attrs?["spawn_id"]?.Value ?? string.Empty, attrs?["patrol_name"]?.Value ?? string.Empty) },
        { "patrol_condition_user", (ctx, attrs) => ctx.PatrolConditionUser(attrs?["patrol_name"]?.Value ?? string.Empty, ParseInt(attrs?["patrol_index"]?.Value), ParseInt(attrs?["additional_effect_id"]?.Value)) },
        { "play_scene_movie", (ctx, attrs) => ctx.PlaySceneMovie(attrs?["file_name"]?.Value ?? string.Empty, ParseInt(attrs?["movie_id"]?.Value), attrs?["skip_type"]?.Value ?? string.Empty) },
        { "play_system_sound_by_user_tag", (ctx, attrs) => ctx.PlaySystemSoundByUserTag(ParseInt(attrs?["user_tag_id"]?.Value), attrs?["sound_key"]?.Value ?? string.Empty) },
        { "play_system_sound_in_box", (ctx, attrs) => ctx.PlaySystemSoundInBox(attrs?["sound"]?.Value ?? string.Empty, ParseIntArray(attrs?["box_ids"]?.Value)) }, {
            "random_additional_effect", (ctx, attrs) =>
                ctx.RandomAdditionalEffect(attrs?["target"]?.Value ?? string.Empty, ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["spawn_id"]?.Value),
                    ParseInt(attrs?["target_count"]?.Value), ParseInt(attrs?["tick"]?.Value), ParseInt(attrs?["wait_tick"]?.Value), attrs?["target_effect"]?.Value ?? string.Empty, ParseInt(attrs?["additional_effect_id"]?.Value))
        },
        { "remove_balloon_talk", (ctx, attrs) => ctx.RemoveBalloonTalk(ParseInt(attrs?["spawn_id"]?.Value)) },
        { "remove_buff", (ctx, attrs) => ctx.RemoveBuff(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["skill_id"]?.Value), ParseBool(attrs?["is_player"]?.Value)) },
        { "remove_cinematic_talk", (ctx, _) => ctx.RemoveCinematicTalk() },
        { "remove_effect_nif", (ctx, attrs) => ctx.RemoveEffectNif(ParseInt(attrs?["spawn_id"]?.Value)) },
        { "reset_camera", (ctx, attrs) => ctx.ResetCamera(ParseFloat(attrs?["interpolation_time"]?.Value)) },
        { "reset_timer", (ctx, attrs) => ctx.ResetTimer(attrs?["timer_id"]?.Value ?? string.Empty) },
        { "room_expire", (ctx, _) => ctx.RoomExpire() },
        { "score_board_create", (ctx, attrs) => ctx.ScoreBoardCreate(attrs?["type"]?.Value ?? string.Empty, attrs?["title"]?.Value ?? string.Empty, ParseInt(attrs?["max_score"]?.Value)) },
        { "score_board_remove", (ctx, _) => ctx.ScoreBoardRemove() },
        { "score_board_set_score", (ctx, attrs) => ctx.ScoreBoardSetScore(ParseInt(attrs?["score"]?.Value)) },
        { "select_camera", (ctx, attrs) => ctx.SelectCamera(ParseInt(attrs?["trigger_id"]?.Value), ParseBool(attrs?["enable"]?.Value)) },
        { "select_camera_path", (ctx, attrs) => ctx.SelectCameraPath(ParseIntArray(attrs?["path_ids"]?.Value), ParseBool(attrs?["return_view"]?.Value)) },
        { "set_achievement", (ctx, attrs) => ctx.SetAchievement(ParseInt(attrs?["trigger_id"]?.Value), attrs?["type"]?.Value ?? string.Empty, attrs?["achieve"]?.Value ?? string.Empty) },
        { "set_actor", (ctx, attrs) => ctx.SetActor(ParseInt(attrs?["trigger_id"]?.Value), ParseBool(attrs?["visible"]?.Value), attrs?["initial_sequence"]?.Value ?? string.Empty, ParseBool(attrs?["arg4"]?.Value), ParseBool(attrs?["arg5"]?.Value)) },
        { "set_agent", (ctx, attrs) => ctx.SetAgent(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value)) },
        { "set_ai_extra_data", (ctx, attrs) => ctx.SetAiExtraData(attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["value"]?.Value), ParseBool(attrs?["is_modify"]?.Value), ParseInt(attrs?["box_id"]?.Value)) },
        { "set_ambient_light", (ctx, attrs) => ctx.SetAmbientLight(ParseVector3(attrs?["primary"]?.Value), ParseVector3(attrs?["secondary"]?.Value), ParseVector3(attrs?["tertiary"]?.Value)) },
        { "set_breakable", (ctx, attrs) => ctx.SetBreakable(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["enable"]?.Value)) },
        { "set_cinematic_intro", (ctx, attrs) => ctx.SetCinematicIntro(attrs?["text"]?.Value ?? string.Empty) },
        { "set_cinematic_ui", (ctx, attrs) => ctx.SetCinematicUi(ParseInt(attrs?["type"]?.Value), attrs?["script"]?.Value ?? string.Empty, ParseBool(attrs?["arg3"]?.Value)) },
        { "set_cube", (ctx, attrs) => ctx.SetCube(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["is_visible"]?.Value), ParseInt(attrs?["random_count"]?.Value)) },
        { "set_dialogue", (ctx, attrs) => ctx.SetDialogue(ParseInt(attrs?["type"]?.Value), ParseInt(attrs?["spawn_id"]?.Value), attrs?["script"]?.Value ?? string.Empty, ParseInt(attrs?["time"]?.Value), ParseInt(attrs?["arg5"]?.Value), ParseAlign(attrs?["align"]?.Value)) },
        { "set_directional_light", (ctx, attrs) => ctx.SetDirectionalLight(ParseVector3(attrs?["diffuse_color"]?.Value), ParseVector3(attrs?["specular_color"]?.Value)) },
        { "set_effect", (ctx, attrs) => ctx.SetEffect(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseInt(attrs?["start_delay"]?.Value), ParseInt(attrs?["interval"]?.Value)) },
        { "set_event_ui_countdown", (ctx, attrs) => ctx.SetEventUiCountdown(attrs?["script"]?.Value ?? string.Empty, ParseIntArray(attrs?["round_countdown"]?.Value), attrs?["box_ids"]?.Value.Split(',') ?? []) },
        { "set_event_ui_round", (ctx, attrs) => ctx.SetEventUiRound(ParseIntArray(attrs?["rounds"]?.Value), ParseInt(attrs?["v_offset"]?.Value), ParseInt(attrs?["arg3"]?.Value)) },
        { "set_event_ui_script", (ctx, attrs) => ctx.SetEventUiScript(ParseBannerType(attrs?["type"]?.Value), attrs?["script"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), attrs?["box_ids"]?.Value.Split(',') ?? []) },
        { "set_gravity", (ctx, attrs) => ctx.SetGravity(ParseFloat(attrs?["gravity"]?.Value)) },
        { "set_interact_object", (ctx, attrs) => ctx.SetInteractObject(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseInt(attrs?["state"]?.Value), ParseBool(attrs?["arg4"]?.Value), ParseBool(attrs?["arg3"]?.Value)) },
        { "set_ladder", (ctx, attrs) => ctx.SetLadder(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseBool(attrs?["enable"]?.Value), ParseInt(attrs?["fade"]?.Value)) },
        { "set_local_camera", (ctx, attrs) => ctx.SetLocalCamera(ParseInt(attrs?["camera_id"]?.Value), ParseBool(attrs?["enable"]?.Value)) },
        { "set_mesh", (ctx, attrs) => ctx.SetMesh(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseInt(attrs?["start_delay"]?.Value), ParseInt(attrs?["interval"]?.Value), ParseFloat(attrs?["fade"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "set_mesh_animation", (ctx, attrs) => ctx.SetMeshAnimation(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseInt(attrs?["start_delay"]?.Value), ParseInt(attrs?["interval"]?.Value)) },
        { "set_mini_game_area_for_hack", (ctx, attrs) => ctx.SetMiniGameAreaForHack(ParseInt(attrs?["box_id"]?.Value)) },
        { "set_npc_duel_hp_bar", (ctx, attrs) => ctx.SetNpcDuelHpBar(ParseBool(attrs?["is_open"]?.Value), ParseInt(attrs?["spawn_id"]?.Value), ParseInt(attrs?["duration_tick"]?.Value), ParseInt(attrs?["npc_hp_step"]?.Value)) },
        { "set_npc_emotion_loop", (ctx, attrs) => ctx.SetNpcEmotionLoop(ParseInt(attrs?["spawn_id"]?.Value), attrs?["sequence_name"]?.Value ?? string.Empty, ParseFloat(attrs?["duration"]?.Value)) },
        { "set_npc_emotion_sequence", (ctx, attrs) => ctx.SetNpcEmotionSequence(ParseInt(attrs?["spawn_id"]?.Value), attrs?["sequence_name"]?.Value ?? string.Empty, ParseInt(attrs?["duration_tick"]?.Value)) },
        { "set_npc_rotation", (ctx, attrs) => ctx.SetNpcRotation(ParseInt(attrs?["spawn_id"]?.Value), ParseFloat(attrs?["rotation"]?.Value)) },
        { "set_onetime_effect", (ctx, attrs) => ctx.SetOnetimeEffect(ParseInt(attrs?["id"]?.Value), ParseBool(attrs?["enable"]?.Value), attrs?["path"]?.Value ?? string.Empty) },
        { "set_pc_emotion_loop", (ctx, attrs) => ctx.SetPcEmotionLoop(attrs?["sequence_name"]?.Value ?? string.Empty, ParseFloat(attrs?["duration"]?.Value), ParseBool(attrs?["loop"]?.Value)) },
        { "set_pc_emotion_sequence", (ctx, attrs) => ctx.SetPcEmotionSequence(attrs?["sequence_names"]?.Value.Split(',') ?? []) },
        { "set_pc_rotation", (ctx, attrs) => ctx.SetPcRotation(ParseVector3(attrs?["rotation"]?.Value)) },
        { "set_photo_studio", (ctx, attrs) => ctx.SetPhotoStudio(ParseBool(attrs?["is_enable"]?.Value)) },
        { "set_portal", (ctx, attrs) => ctx.SetPortal(ParseInt(attrs?["portal_id"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseBool(attrs?["enable"]?.Value), ParseBool(attrs?["minimap_visible"]?.Value), ParseBool(attrs?["arg5"]?.Value)) },
        { "set_pvp_zone", (ctx, attrs) => ctx.SetPvpZone(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["prepare_time"]?.Value), ParseInt(attrs?["match_time"]?.Value), ParseInt(attrs?["additional_effect_id"]?.Value), ParseInt(attrs?["type"]?.Value), ParseIntArray(attrs?["box_ids"]?.Value)) },
        { "set_quest_accept", (ctx, attrs) => ctx.SetQuestAccept(ParseInt(attrs?["quest_id"]?.Value)) },
        { "set_quest_complete", (ctx, attrs) => ctx.SetQuestComplete(ParseInt(attrs?["quest_id"]?.Value)) },
        { "set_random_mesh", (ctx, attrs) => ctx.SetRandomMesh(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseInt(attrs?["start_delay"]?.Value), ParseInt(attrs?["interval"]?.Value), ParseInt(attrs?["fade"]?.Value)) },
        { "set_rope", (ctx, attrs) => ctx.SetRope(ParseInt(attrs?["trigger_id"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseBool(attrs?["enable"]?.Value), ParseInt(attrs?["fade"]?.Value)) },
        { "set_scene_skip", (ctx, attrs) => ctx.SetSceneSkip(attrs?["state"]?.Value ?? string.Empty, attrs?["action"]?.Value ?? string.Empty) },
        { "set_skill", (ctx, attrs) => ctx.SetSkill(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["enable"]?.Value)) },
        { "set_skip", (ctx, attrs) => ctx.SetSkip(attrs?["state"]?.Value ?? string.Empty) },
        { "set_sound", (ctx, attrs) => ctx.SetSound(ParseInt(attrs?["trigger_id"]?.Value), ParseBool(attrs?["enable"]?.Value)) },
        { "set_state", (ctx, attrs) => ctx.SetState(ParseInt(attrs?["id"]?.Value), attrs?["states"]?.Value.Split(',') ?? [], ParseBool(attrs?["randomize"]?.Value)) },
        { "set_time_scale", (ctx, attrs) => ctx.SetTimeScale(ParseBool(attrs?["enable"]?.Value), ParseFloat(attrs?["start_scale"]?.Value), ParseFloat(attrs?["end_scale"]?.Value), ParseFloat(attrs?["duration"]?.Value), ParseInt(attrs?["interpolator"]?.Value)) },
        { "set_timer", (ctx, attrs) => ctx.SetTimer(attrs?["timer_id"]?.Value ?? string.Empty, ParseInt(attrs?["seconds"]?.Value), ParseBool(attrs?["auto_remove"]?.Value), ParseBool(attrs?["display"]?.Value), ParseInt(attrs?["v_offset"]?.Value), attrs?["type"]?.Value ?? string.Empty, attrs?["desc"]?.Value ?? string.Empty) },
        { "set_user_value", (ctx, attrs) => ctx.SetUserValue(ParseInt(attrs?["trigger_id"]?.Value), attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["value"]?.Value)) },
        { "set_user_value_from_dungeon_reward_count", (ctx, attrs) => ctx.SetUserValueFromDungeonRewardCount(attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["dungeon_reward_id"]?.Value)) },
        { "set_user_value_from_guild_vs_game_score", (ctx, attrs) => ctx.SetUserValueFromGuildVsGameScore(ParseInt(attrs?["team_id"]?.Value), attrs?["key"]?.Value ?? string.Empty) },
        { "set_user_value_from_user_count", (ctx, attrs) => ctx.SetUserValueFromUserCount(ParseInt(attrs?["trigger_box_id"]?.Value), attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["user_tag_id"]?.Value)) },
        { "set_visible_breakable_object", (ctx, attrs) => ctx.SetVisibleBreakableObject(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value)) },
        { "set_visible_ui", (ctx, attrs) => ctx.SetVisibleUi(attrs?["ui_names"]?.Value.Split(',') ?? [], ParseBool(attrs?["visible"]?.Value)) },
        { "shadow_expedition_close_boss_gauge", (ctx, _) => ctx.ShadowExpeditionCloseBossGauge() },
        { "shadow_expedition_open_boss_gauge", (ctx, attrs) => ctx.ShadowExpeditionOpenBossGauge(ParseInt(attrs?["max_gauge_point"]?.Value), attrs?["title"]?.Value ?? string.Empty) }, {
            "show_caption", (ctx, attrs) => ctx.ShowCaption(attrs?["type"]?.Value ?? string.Empty, attrs?["title"]?.Value ?? string.Empty, attrs?["desc"]?.Value ?? string.Empty,
                ParseAlign(attrs?["align"]?.Value), ParseFloat(attrs?["offset_rate_x"]?.Value), ParseFloat(attrs?["offset_rate_y"]?.Value), ParseInt(attrs?["duration"]?.Value), ParseFloat(attrs?["scale"]?.Value))
        },
        { "show_count_ui", (ctx, attrs) => ctx.ShowCountUi(attrs?["text"]?.Value ?? string.Empty, ParseInt(attrs?["stage"]?.Value), ParseInt(attrs?["count"]?.Value), ParseInt(attrs?["sound_type"]?.Value)) },
        { "show_event_result", (ctx, attrs) => ctx.ShowEventResult(attrs?["type"]?.Value ?? string.Empty, attrs?["text"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), ParseInt(attrs?["user_tag_id"]?.Value), ParseInt(attrs?["trigger_box_id"]?.Value), ParseBool(attrs?["is_outside"]?.Value)) },
        { "show_guide_summary", (ctx, attrs) => ctx.ShowGuideSummary(ParseInt(attrs?["entity_id"]?.Value), ParseInt(attrs?["text_id"]?.Value), ParseInt(attrs?["duration"]?.Value)) },
        { "show_round_ui", (ctx, attrs) => ctx.ShowRoundUi(ParseInt(attrs?["round"]?.Value), ParseInt(attrs?["duration"]?.Value), ParseBool(attrs?["is_final_round"]?.Value)) },
        { "side_npc_cutin", (ctx, attrs) => ctx.SideNpcCutin(attrs?["illust"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value)) },
        { "side_npc_movie", (ctx, attrs) => ctx.SideNpcMovie(attrs?["usm"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value)) },
        { "side_npc_talk", (ctx, attrs) => ctx.SideNpcTalk(ParseInt(attrs?["npc_id"]?.Value), attrs?["illust"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), attrs?["script"]?.Value ?? string.Empty, attrs?["voice"]?.Value ?? string.Empty) },
        { "side_npc_talk_bottom", (ctx, attrs) => ctx.SideNpcTalkBottom(ParseInt(attrs?["npc_id"]?.Value), attrs?["illust"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), attrs?["script"]?.Value ?? string.Empty) },
        { "sight_range", (ctx, attrs) => ctx.SightRange(ParseBool(attrs?["enable"]?.Value), ParseInt(attrs?["range"]?.Value), ParseInt(attrs?["range_z"]?.Value), ParseInt(attrs?["border"]?.Value)) },
        { "spawn_item_range", (ctx, attrs) => ctx.SpawnItemRange(ParseIntArray(attrs?["range_ids"]?.Value), ParseInt(attrs?["random_pick_count"]?.Value)) },
        { "spawn_monster", (ctx, attrs) => ctx.SpawnMonster(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseBool(attrs?["auto_target"]?.Value), ParseInt(attrs?["delay"]?.Value)) },
        { "spawn_npc_range", (ctx, attrs) => ctx.SpawnNpcRange(ParseIntArray(attrs?["range_ids"]?.Value), ParseBool(attrs?["is_auto_targeting"]?.Value), ParseInt(attrs?["random_pick_count"]?.Value), ParseInt(attrs?["score"]?.Value)) },
        { "start_combine_spawn", (ctx, attrs) => ctx.StartCombineSpawn(ParseIntArray(attrs?["group_id"]?.Value), ParseBool(attrs?["is_start"]?.Value)) },
        { "start_mini_game", (ctx, attrs) => ctx.StartMiniGame(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["round"]?.Value), attrs?["game_name"]?.Value ?? string.Empty, ParseBool(attrs?["is_show_result_ui"]?.Value)) },
        { "start_mini_game_round", (ctx, attrs) => ctx.StartMiniGameRound(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["round"]?.Value)) },
        { "start_tutorial", (ctx, _) => ctx.StartTutorial() },
        { "talk_npc", (ctx, attrs) => ctx.TalkNpc(ParseInt(attrs?["spawn_id"]?.Value)) },
        { "unset_mini_game_area_for_hack", (ctx, _) => ctx.UnsetMiniGameAreaForHack() },
        { "use_state", (ctx, attrs) => ctx.UseState(ParseInt(attrs?["id"]?.Value), ParseBool(attrs?["randomize"]?.Value)) },
        { "user_tag_symbol", (ctx, attrs) => ctx.UserTagSymbol(attrs?["symbol1"]?.Value ?? string.Empty, attrs?["symbol2"]?.Value ?? string.Empty) },
        { "user_value_to_number_mesh", (ctx, attrs) => ctx.UserValueToNumberMesh(attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["start_mesh_id"]?.Value), ParseInt(attrs?["digit_count"]?.Value)) },
        { "visible_my_pc", (ctx, attrs) => ctx.VisibleMyPc(ParseBool(attrs?["is_visible"]?.Value)) },
        { "weather", (ctx, attrs) => ctx.Weather(ParseWeather(attrs?["weather_type"]?.Value)) },
        { "wedding_broken", (ctx, _) => ctx.WeddingBroken() },
        { "wedding_move_user", (ctx, attrs) => ctx.WeddingMoveUser(attrs?["entry_type"]?.Value ?? string.Empty, ParseInt(attrs?["map_id"]?.Value), ParseIntArray(attrs?["portal_ids"]?.Value), ParseInt(attrs?["box_id"]?.Value)) },
        { "wedding_mutual_agree", (ctx, attrs) => ctx.WeddingMutualAgree(attrs?["agree_type"]?.Value ?? string.Empty) },
        { "wedding_mutual_cancel", (ctx, attrs) => ctx.WeddingMutualCancel(attrs?["agree_type"]?.Value ?? string.Empty) },
        { "wedding_set_user_emotion", (ctx, attrs) => ctx.WeddingSetUserEmotion(attrs?["entry_type"]?.Value ?? string.Empty, ParseInt(attrs?["id"]?.Value)) },
        { "wedding_set_user_look_at", (ctx, attrs) => ctx.WeddingSetUserLookAt(attrs?["entry_type"]?.Value ?? string.Empty, attrs?["look_at_entry_type"]?.Value ?? string.Empty, ParseBool(attrs?["immediate"]?.Value)) },
        { "wedding_set_user_rotation", (ctx, attrs) => ctx.WeddingSetUserRotation(attrs?["entry_type"]?.Value ?? string.Empty, ParseVector3(attrs?["rotation"]?.Value), ParseBool(attrs?["immediate"]?.Value)) },
        { "wedding_user_to_patrol", (ctx, attrs) => ctx.WeddingUserToPatrol(attrs?["patrol_name"]?.Value ?? string.Empty, attrs?["entry_type"]?.Value ?? string.Empty, ParseInt(attrs?["patrol_index"]?.Value)) },
        { "wedding_vow_complete", (ctx, _) => ctx.WeddingVowComplete() },
        { "widget_action", (ctx, attrs) => ctx.WidgetAction(attrs?["type"]?.Value ?? string.Empty, attrs?["func"]?.Value ?? string.Empty, attrs?["widget_arg"]?.Value ?? string.Empty, attrs?["desc"]?.Value ?? string.Empty, ParseInt(attrs?["widget_arg_num"]?.Value)) },
        { "write_log", (ctx, attrs) => ctx.WriteLog(attrs?["log_name"]?.Value ?? string.Empty, attrs?["event"]?.Value ?? string.Empty, ParseInt(attrs?["trigger_id"]?.Value), attrs?["sub_event"]?.Value ?? string.Empty, ParseInt(attrs?["level"]?.Value)) },

    };

    public static readonly Dictionary<string, Func<ITriggerContext, XmlAttributeCollection?, bool>> ConditionMap = new Dictionary<string, Func<ITriggerContext, XmlAttributeCollection?, bool>> {
        { "bonus_game_reward", (ctx, attrs) => ctx.BonusGameReward(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["type"]?.Value)) },
        { "check_any_user_additional_effect", (ctx, attrs) => ctx.CheckAnyUserAdditionalEffect(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["additional_effect_id"]?.Value), ParseInt(attrs?["level"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "check_dungeon_lobby_user_count", (ctx, attrs) => ctx.CheckDungeonLobbyUserCount(ParseBool(attrs?["negate"]?.Value)) },
        { "check_npc_additional_effect", (ctx, attrs) => ctx.CheckNpcAdditionalEffect(ParseInt(attrs?["spawn_id"]?.Value), ParseInt(attrs?["additional_effect_id"]?.Value), ParseInt(attrs?["level"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "npc_damage", (ctx, attrs) => ctx.NpcDamage(ParseInt(attrs?["spawn_id"]?.Value), ParseFloat(attrs?["damageRate"]?.Value), ParseOperatorType(attrs?["operator"]?.Value)) },
        { "npc_extra_data", (ctx, attrs) => ctx.NpcExtraData(ParseInt(attrs?["spawn_point_id"]?.Value), attrs?["extra_data_key"]?.Value ?? string.Empty, ParseInt(attrs?["extra_data_value"]?.Value), ParseOperatorType(attrs?["operator"]?.Value)) },
        { "npc_hp", (ctx, attrs) => ctx.NpcHp(ParseInt(attrs?["spawn_id"]?.Value), ParseBool(attrs?["is_relative"]?.Value), ParseInt(attrs?["value"]?.Value), ParseCompareType(attrs?["compare_type"]?.Value)) },
        { "check_same_user_tag", (ctx, attrs) => ctx.CheckSameUserTag(ParseInt(attrs?["box_id"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "check_user", (ctx, attrs) => ctx.CheckUser(ParseBool(attrs?["negate"]?.Value)) },
        { "user_count", (ctx, attrs) => ctx.UserCount(ParseInt(attrs?["check_count"]?.Value)) },
        { "count_users", (ctx, attrs) => ctx.CountUsers(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["user_tag_id"]?.Value), ParseInt(attrs?["min_users"]?.Value), ParseOperatorType(attrs?["operator"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "day_of_week", (ctx, attrs) => ctx.DayOfWeek(ParseIntArray(attrs?["day_of_weeks"]?.Value), attrs?["desc"]?.Value ?? string.Empty, ParseBool(attrs?["negate"]?.Value)) },
        { "detect_liftable_object", (ctx, attrs) => ctx.DetectLiftableObject(ParseIntArray(attrs?["box_ids"]?.Value), ParseInt(attrs?["item_id"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "dungeon_play_time", (ctx, attrs) => ctx.DungeonPlayTime(ParseInt(attrs?["play_seconds"]?.Value)) },
        { "dungeon_state", (ctx, attrs) => ctx.DungeonState(attrs?["check_state"]?.Value ?? string.Empty) },
        { "dungeon_first_user_mission_score", (ctx, attrs) => ctx.DungeonFirstUserMissionScore(ParseInt(attrs?["score"]?.Value), ParseOperatorType(attrs?["operator"]?.Value)) },
        { "dungeon_id", (ctx, attrs) => ctx.DungeonId(ParseInt(attrs?["dungeon_id"]?.Value)) },
        { "dungeon_level", (ctx, attrs) => ctx.DungeonLevel(ParseInt(attrs?["level"]?.Value)) },
        { "dungeon_max_user_count", (ctx, attrs) => ctx.DungeonMaxUserCount(ParseInt(attrs?["level"]?.Value)) },
        { "dungeon_round", (ctx, attrs) => ctx.DungeonRound(ParseInt(attrs?["round"]?.Value)) },
        { "dungeon_timeout", (ctx, _) => ctx.DungeonTimeout() },
        { "dungeon_variable", (ctx, attrs) => ctx.DungeonVariable(ParseInt(attrs?["var_id"]?.Value), ParseInt(attrs?["value"]?.Value)) },
        { "guild_vs_game_scored_team", (ctx, attrs) => ctx.GuildVsGameScoredTeam(ParseInt(attrs?["team_id"]?.Value)) },
        { "guild_vs_game_winner_team", (ctx, attrs) => ctx.GuildVsGameWinnerTeam(ParseInt(attrs?["team_id"]?.Value)) },
        { "is_dungeon_room", (ctx, attrs) => ctx.IsDungeonRoom(ParseBool(attrs?["negate"]?.Value)) },
        { "is_playing_maple_survival", (ctx, attrs) => ctx.IsPlayingMapleSurvival(ParseBool(attrs?["negate"]?.Value)) },
        { "monster_dead", (ctx, attrs) => ctx.MonsterDead(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseBool(attrs?["auto_target"]?.Value)) },
        { "monster_in_combat", (ctx, attrs) => ctx.MonsterInCombat(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "npc_detected", (ctx, attrs) => ctx.NpcDetected(ParseInt(attrs?["box_id"]?.Value), ParseIntArray(attrs?["spawn_ids"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "npc_is_dead_by_string_id", (ctx, attrs) => ctx.NpcIsDeadByStringId(attrs?["string_id"]?.Value ?? string.Empty) },
        { "object_interacted", (ctx, attrs) => ctx.ObjectInteracted(ParseIntArray(attrs?["interact_ids"]?.Value), ParseInt(attrs?["state"]?.Value)) },
        { "pvp_zone_ended", (ctx, attrs) => ctx.PvpZoneEnded(ParseInt(attrs?["box_id"]?.Value)) },
        { "quest_user_detected", (ctx, attrs) => ctx.QuestUserDetected(ParseIntArray(attrs?["box_ids"]?.Value), ParseIntArray(attrs?["quest_ids"]?.Value), ParseIntArray(attrs?["quest_states"]?.Value), ParseInt(attrs?["job_code"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "random_condition", (ctx, attrs) => ctx.RandomCondition(ParseFloat(attrs?["weight"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "score_board_score", (ctx, attrs) => ctx.ScoreBoardScore(ParseInt(attrs?["score"]?.Value), ParseOperatorType(attrs?["compare_op"]?.Value)) },
        { "shadow_expedition_points", (ctx, attrs) => ctx.ShadowExpeditionPoints(ParseInt(attrs?["score"]?.Value)) },
        { "time_expired", (ctx, attrs) => ctx.TimeExpired(attrs?["timer_id"]?.Value ?? string.Empty) },
        { "user_detected", (ctx, attrs) => ctx.UserDetected(ParseIntArray(attrs?["box_ids"]?.Value), ParseInt(attrs?["job_code"]?.Value)) },
        { "user_value", (ctx, attrs) => ctx.UserValue(attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["value"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "wait_and_reset_tick", (ctx, attrs) => ctx.WaitAndResetTick(ParseInt(attrs?["wait_tick"]?.Value)) },
        { "wait_seconds_user_value", (ctx, attrs) => ctx.WaitSecondsUserValue(attrs?["key"]?.Value ?? string.Empty, attrs?["desc"]?.Value ?? string.Empty) },
        { "wait_tick", (ctx, attrs) => ctx.WaitTick(ParseInt(attrs?["wait_tick"]?.Value)) },
        { "wedding_entry_in_field", (ctx, attrs) => ctx.WeddingEntryInField(attrs?["entry_type"]?.Value ?? string.Empty, ParseBool(attrs?["is_in_field"]?.Value)) },
        { "wedding_hall_state", (ctx, attrs) => ctx.WeddingHallState(attrs?["hallState"]?.Value ?? string.Empty, ParseBool(attrs?["success"]?.Value)) },
        { "wedding_mutual_agree_result", (ctx, attrs) => ctx.WeddingMutualAgreeResult(attrs?["agree_type"]?.Value ?? string.Empty) },
        { "widget_value", (ctx, attrs) => ctx.WidgetValue(attrs?["type"]?.Value ?? string.Empty, attrs?["widget_name"]?.Value ?? string.Empty, ParseInt(attrs?["condition"]?.Value), ParseBool(attrs?["negate"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
    };

    private static Weather ParseWeather(string? value) {
        if (string.IsNullOrEmpty(value)) return Weather.Clear;
        return (Weather) Enum.Parse(typeof(Weather), value, true);
    }

    private static BannerType ParseBannerType(string? value) {
        if (string.IsNullOrEmpty(value)) return BannerType.Lose;
        return (BannerType) Enum.Parse(typeof(BannerType), value, true);
    }

    private static Locale ParseLocale(string? value) {
        if (string.IsNullOrEmpty(value)) return Locale.ALL;
        return (Locale) Enum.Parse(typeof(Locale), value, true);
    }

    private static FieldGame ParseFieldGame(string? value) {
        if (string.IsNullOrEmpty(value)) return FieldGame.Unknown;
        return (FieldGame) Enum.Parse(typeof(FieldGame), value, true);
    }

    private static Align ParseAlign(string? align) {
        if (string.IsNullOrEmpty(align)) return Align.center;
        return (Align) Enum.Parse(typeof(Align), align, true);
    }

    private static OperatorType ParseOperatorType(string? operatorType) {
        if (string.IsNullOrEmpty(operatorType)) return OperatorType.GreaterEqual;
        return (OperatorType) Enum.Parse(typeof(OperatorType), operatorType, true);
    }

    private static CompareType ParseCompareType(string? compareType) {
        if (string.IsNullOrEmpty(compareType)) return CompareType.higher;
        return (CompareType) Enum.Parse(typeof(CompareType), compareType, true);
    }

    public static int[] ParseIntArray(string? value) {
        if (string.IsNullOrEmpty(value)) return [];
        if (value is "all") return [-1]; // Special case for "all" to indicate all IDs.
        // Handles ranges and comma-separated values and mixed usage.
        if (value.Contains(',')) {
            var result = new List<int>();
            foreach (string part in value.Split(',')) {
                if (part.Contains('-')) {
                    string[] rangeParts = part.Split('-');
                    if (rangeParts.Length == 2 && int.TryParse(rangeParts[0], out int start) && int.TryParse(rangeParts[1], out int end)) {
                        result.AddRange(Enumerable.Range(start, end - start + 1));
                    }
                } else if (int.TryParse(part, out int singleValue)) {
                    result.Add(singleValue);
                }
            }
            return result.ToArray();
        }
        if (value.Contains('-')) {
            string[] parts = value.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end)) {
                return Enumerable.Range(start, end - start + 1).ToArray();
            }
        }
        return value.Split(',').Select(int.Parse).ToArray();
    }

    public static int ParseInt(string? value) => int.TryParse(value, out int v) ? v : 0;

    public static float ParseFloat(string? value) => float.TryParse(value, out float v) ? v : 0f;

    public static bool ParseBool(string? value) {
        if (string.IsNullOrEmpty(value)) return false;
        return value == "1" || value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
    }

    public static Vector3 ParseVector3(string? value) {
        if (string.IsNullOrEmpty(value)) return Vector3.Zero;

        string[] parts = value.Split(',');
        if (parts.Length != 3 || !float.TryParse(parts[0], out float x) || !float.TryParse(parts[1], out float y) || !float.TryParse(parts[2], out float z)) {
            return Vector3.Zero;
        }

        return new Vector3(x, y, z);
    }
}
