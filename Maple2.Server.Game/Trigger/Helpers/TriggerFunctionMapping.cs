using System.Numerics;
using System.Xml;
using Maple2.Model.Enum;

namespace Maple2.Server.Game.Trigger.Helpers;

public static class TriggerFunctionMapping {
    public static readonly Dictionary<string, Func<XmlAttributeCollection?, IAction>> ActionMap = new Dictionary<string, Func<XmlAttributeCollection?, IAction>> {
        { "add_balloon_talk", attrs => new Trigger.AddBalloonTalk(ParseInt(attrs?["spawn_id"]?.Value), attrs?["msg"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), ParseInt(attrs?["delay_tick"]?.Value), ParseInt(attrs?["npc_id"]?.Value)) },
        { "add_buff", attrs => new Trigger.AddBuff(ParseIntArray(attrs?["box_ids"]?.Value), ParseInt(attrs?["skill_id"]?.Value), ParseInt(attrs?["level"]?.Value), ParseBool(attrs?["ignore_player"]?.Value), ParseBool(attrs?["is_skill_set"]?.Value), attrs?["feature"]?.Value ?? string.Empty) },
        { "add_cinematic_talk", attrs => new Trigger.AddCinematicTalk(ParseInt(attrs?["npc_id"]?.Value), attrs?["illust_id"]?.Value ?? string.Empty, attrs?["msg"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), ParseAlign(attrs?["align"]?.Value), ParseInt(attrs?["delay_tick"]?.Value)) },
        { "add_effect_nif", attrs => new Trigger.AddEffectNif(ParseInt(attrs?["spawn_id"]?.Value), attrs?["nif_path"]?.Value ?? string.Empty, ParseBool(attrs?["is_outline"]?.Value), ParseFloat(attrs?["scale"]?.Value), ParseInt(attrs?["rotate_z"]?.Value)) },
        { "add_user_value", attrs => new Trigger.AddUserValue(attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["value"]?.Value)) },
        { "allocate_battlefield_points", attrs => new Trigger.AllocateBattlefieldPoints(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["points"]?.Value)) },
        { "announce", attrs => new Trigger.Announce(ParseInt(attrs?["type"]?.Value), attrs?["content"]?.Value ?? string.Empty, ParseBool(attrs?["arg3"]?.Value)) },
        { "arcade_boom_boom_ocean_clear_round", attrs => new Trigger.ArcadeBoomBoomOceanClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_boom_boom_ocean_end_game", _ => new Trigger.ArcadeBoomBoomOceanEndGame() },
        { "arcade_boom_boom_ocean_set_skill_score", attrs => new Trigger.ArcadeBoomBoomOceanSetSkillScore(ParseInt(attrs?["id"]?.Value), ParseInt(attrs?["score"]?.Value)) },
        { "arcade_boom_boom_ocean_start_game", attrs => new Trigger.ArcadeBoomBoomOceanStartGame(ParseInt(attrs?["life_count"]?.Value)) },
        { "arcade_boom_boom_ocean_start_round", attrs => new Trigger.ArcadeBoomBoomOceanStartRound(ParseInt(attrs?["round"]?.Value), ParseInt(attrs?["round_duration"]?.Value), ParseInt(attrs?["time_score_rate"]?.Value)) },
        { "arcade_spring_farm_clear_round", attrs => new Trigger.ArcadeSpringFarmClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_spring_farm_end_game", _ => new Trigger.ArcadeSpringFarmEndGame() },
        { "arcade_spring_farm_set_interact_score", attrs => new Trigger.ArcadeSpringFarmSetInteractScore(ParseInt(attrs?["id"]?.Value), ParseInt(attrs?["score"]?.Value)) },
        { "arcade_spring_farm_spawn_monster", attrs => new Trigger.ArcadeSpringFarmSpawnMonster(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseInt(attrs?["score"]?.Value)) },
        { "arcade_spring_farm_start_game", attrs => new Trigger.ArcadeSpringFarmStartGame(ParseInt(attrs?["life_count"]?.Value)) },
        { "arcade_spring_farm_start_round", attrs => new Trigger.ArcadeSpringFarmStartRound(ParseInt(attrs?["ui_duration"]?.Value), ParseInt(attrs?["round"]?.Value), attrs?["time_score_type"]?.Value ?? string.Empty, ParseInt(attrs?["time_score_rate"]?.Value), ParseInt(attrs?["round_duration"]?.Value)) },
        { "arcade_three_two_one_clear_round", attrs => new Trigger.ArcadeThreeTwoOneClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one_end_game", _ => new Trigger.ArcadeThreeTwoOneEndGame() },
        { "arcade_three_two_one_result_round", attrs => new Trigger.ArcadeThreeTwoOneResultRound(ParseInt(attrs?["result_direction"]?.Value)) },
        { "arcade_three_two_one_result_round2", attrs => new Trigger.ArcadeThreeTwoOneResultRound2(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one_start_game", attrs => new Trigger.ArcadeThreeTwoOneStartGame(ParseInt(attrs?["life_count"]?.Value), ParseInt(attrs?["init_score"]?.Value)) },
        { "arcade_three_two_one_start_round", attrs => new Trigger.ArcadeThreeTwoOneStartRound(ParseInt(attrs?["ui_duration"]?.Value), ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one2_clear_round", attrs => new Trigger.ArcadeThreeTwoOne2ClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one2_end_game", _ => new Trigger.ArcadeThreeTwoOne2EndGame() },
        { "arcade_three_two_one2_result_round", attrs => new Trigger.ArcadeThreeTwoOne2ResultRound(ParseInt(attrs?["result_direction"]?.Value)) },
        { "arcade_three_two_one2_result_round2", attrs => new Trigger.ArcadeThreeTwoOne2ResultRound2(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one2_start_game", attrs => new Trigger.ArcadeThreeTwoOne2StartGame(ParseInt(attrs?["life_count"]?.Value), ParseInt(attrs?["init_score"]?.Value)) },
        { "arcade_three_two_one2_start_round", attrs => new Trigger.ArcadeThreeTwoOne2StartRound(ParseInt(attrs?["ui_duration"]?.Value), ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one3_clear_round", attrs => new Trigger.ArcadeThreeTwoOne3ClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one3_end_game", _ => new Trigger.ArcadeThreeTwoOne3EndGame() },
        { "arcade_three_two_one3_result_round", attrs => new Trigger.ArcadeThreeTwoOne3ResultRound(ParseInt(attrs?["result_direction"]?.Value)) },
        { "arcade_three_two_one3_result_round2", attrs => new Trigger.ArcadeThreeTwoOne3ResultRound2(ParseInt(attrs?["round"]?.Value)) },
        { "arcade_three_two_one3_start_game", attrs => new Trigger.ArcadeThreeTwoOne3StartGame(ParseInt(attrs?["life_count"]?.Value), ParseInt(attrs?["init_score"]?.Value)) },
        { "arcade_three_two_one3_start_round", attrs => new Trigger.ArcadeThreeTwoOne3StartRound(ParseInt(attrs?["ui_duration"]?.Value), ParseInt(attrs?["round"]?.Value)) },
        { "change_background", attrs => new Trigger.ChangeBackground(attrs?["dds"]?.Value ?? string.Empty) },
        { "change_monster", attrs => new Trigger.ChangeMonster(ParseInt(attrs?["from_spawn_id"]?.Value), ParseInt(attrs?["to_spawn_id"]?.Value)) },
        { "close_cinematic", _ => new Trigger.CloseCinematic() },
        { "create_field_game", attrs => new Trigger.CreateFieldGame(ParseFieldGame(attrs?["type"]?.Value), ParseBool(attrs?["reset"]?.Value)) },
        { "create_item", attrs => new Trigger.CreateItem(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseInt(attrs?["trigger_id"]?.Value), ParseInt(attrs?["item_id"]?.Value), ParseInt(attrs?["arg5"]?.Value)) },
        { "create_widget", attrs => new Trigger.CreateWidget(attrs?["type"]?.Value ?? string.Empty) },
        { "dark_stream_clear_round", attrs => new Trigger.DarkStreamClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "dark_stream_spawn_monster", attrs => new Trigger.DarkStreamSpawnMonster(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseInt(attrs?["score"]?.Value)) },
        { "dark_stream_start_game", attrs => new Trigger.DarkStreamStartGame(ParseInt(attrs?["round"]?.Value)) },
        { "dark_stream_start_round", attrs => new Trigger.DarkStreamStartRound(ParseInt(attrs?["round"]?.Value), ParseInt(attrs?["ui_duration"]?.Value), ParseInt(attrs?["damage_penalty"]?.Value)) },
        { "debug_string", attrs => new Trigger.DebugString(attrs?["value"]?.Value ?? string.Empty, attrs?["feature"]?.Value ?? string.Empty) },
        { "destroy_monster", attrs => new Trigger.DestroyMonster(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseBool(attrs?["arg2"]?.Value)) },
        { "dungeon_clear", attrs => new Trigger.DungeonClear(attrs?["ui_type"]?.Value ?? string.Empty) },
        { "dungeon_clear_round", attrs => new Trigger.DungeonClearRound(ParseInt(attrs?["round"]?.Value)) },
        { "dungeon_close_timer", _ => new Trigger.DungeonCloseTimer() },
        { "dungeon_disable_ranking", _ => new Trigger.DungeonDisableRanking() },
        { "dungeon_enable_give_up", attrs => new Trigger.DungeonEnableGiveUp(ParseBool(attrs?["is_enable"]?.Value)) },
        { "dungeon_fail", _ => new Trigger.DungeonFail() },
        { "dungeon_mission_complete", attrs => new Trigger.DungeonMissionComplete(attrs?["feature"]?.Value ?? string.Empty, ParseInt(attrs?["mission_id"]?.Value)) },
        { "dungeon_move_lap_time_to_now", attrs => new Trigger.DungeonMoveLapTimeToNow(ParseInt(attrs?["id"]?.Value)) },
        { "dungeon_reset_time", attrs => new Trigger.DungeonResetTime(ParseInt(attrs?["seconds"]?.Value)) },
        { "dungeon_set_end_time", _ => new Trigger.DungeonSetEndTime() },
        { "dungeon_set_lap_time", attrs => new Trigger.DungeonSetLapTime(ParseInt(attrs?["id"]?.Value), ParseInt(attrs?["lap_time"]?.Value)) },
        { "dungeon_stop_timer", _ => new Trigger.DungeonStopTimer() },
        { "set_dungeon_variable", attrs => new Trigger.SetDungeonVariable(ParseInt(attrs?["var_id"]?.Value), ParseInt(attrs?["value"]?.Value)) },
        { "enable_local_camera", attrs => new Trigger.EnableLocalCamera(ParseBool(attrs?["is_enable"]?.Value)) },
        { "enable_spawn_point_pc", attrs => new Trigger.EnableSpawnPointPc(ParseInt(attrs?["spawn_id"]?.Value), ParseBool(attrs?["is_enable"]?.Value)) },
        { "end_mini_game", attrs => new Trigger.EndMiniGame(ParseInt(attrs?["winner_box_id"]?.Value), attrs?["game_name"]?.Value ?? string.Empty, ParseBool(attrs?["is_only_winner"]?.Value)) },
        { "end_mini_game_round", attrs => new Trigger.EndMiniGameRound(ParseInt(attrs?["winner_box_id"]?.Value), ParseFloat(attrs?["exp_rate"]?.Value), ParseFloat(attrs?["meso"]?.Value), ParseBool(attrs?["is_only_winner"]?.Value), ParseBool(attrs?["is_gain_loser_bonus"]?.Value), attrs?["game_name"]?.Value ?? string.Empty) },
        { "face_emotion", attrs => new Trigger.FaceEmotion(ParseInt(attrs?["spawn_id"]?.Value), attrs?["emotion_name"]?.Value ?? string.Empty) },
        { "field_game_constant", attrs => new Trigger.FieldGameConstant(attrs?["key"]?.Value ?? string.Empty, attrs?["value"]?.Value ?? string.Empty, attrs?["feature"]?.Value ?? string.Empty, ParseLocale(attrs?["locale"]?.Value)) },
        { "field_game_message", attrs => new Trigger.FieldGameMessage(ParseInt(attrs?["custom"]?.Value), attrs?["type"]?.Value ?? string.Empty, ParseBool(attrs?["arg1"]?.Value), attrs?["script"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value)) },
        { "field_war_end", attrs => new Trigger.FieldWarEnd(ParseBool(attrs?["is_clear"]?.Value)) },
        { "give_exp", attrs => new Trigger.GiveExp(ParseInt(attrs?["box_id"]?.Value), ParseFloat(attrs?["rate"]?.Value), ParseBool(attrs?["arg3"]?.Value)) },
        { "give_guild_exp", attrs => new Trigger.GiveGuildExp(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["type"]?.Value)) },
        { "give_reward_content", attrs => new Trigger.GiveRewardContent(ParseInt(attrs?["reward_id"]?.Value)) },
        { "guide_event", attrs => new Trigger.GuideEvent(ParseInt(attrs?["event_id"]?.Value)) },
        { "guild_vs_game_end_game", _ => new Trigger.GuildVsGameEndGame() },
        { "guild_vs_game_give_contribution", attrs => new Trigger.GuildVsGameGiveContribution(ParseInt(attrs?["team_id"]?.Value), ParseBool(attrs?["is_win"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "guild_vs_game_give_reward", attrs => new Trigger.GuildVsGameGiveReward(attrs?["type"]?.Value ?? string.Empty, ParseInt(attrs?["team_id"]?.Value), ParseBool(attrs?["is_win"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "guild_vs_game_log_result", attrs => new Trigger.GuildVsGameLogResult(attrs?["desc"]?.Value ?? string.Empty) },
        { "guild_vs_game_log_won_by_default", attrs => new Trigger.GuildVsGameLogWonByDefault(ParseInt(attrs?["team_id"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "guild_vs_game_result", attrs => new Trigger.GuildVsGameResult(attrs?["desc"]?.Value ?? string.Empty) },
        { "guild_vs_game_score_by_user", attrs => new Trigger.GuildVsGameScoreByUser(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["score"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "hide_guide_summary", attrs => new Trigger.HideGuideSummary(ParseInt(attrs?["entity_id"]?.Value), ParseInt(attrs?["text_id"]?.Value)) },
        { "init_npc_rotation", attrs => new Trigger.InitNpcRotation(ParseIntArray(attrs?["spawn_ids"]?.Value)) },
        { "kick_music_audience", attrs => new Trigger.KickMusicAudience(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["portal_id"]?.Value)) },
        { "limit_spawn_npc_count", attrs => new Trigger.LimitSpawnNpcCount(ParseInt(attrs?["limit_count"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "lock_my_pc", attrs => new Trigger.LockMyPc(ParseBool(attrs?["is_lock"]?.Value)) },
        { "mini_game_camera_direction", attrs => new Trigger.MiniGameCameraDirection(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["camera_id"]?.Value)) },
        { "mini_game_give_exp", attrs => new Trigger.MiniGameGiveExp(ParseInt(attrs?["box_id"]?.Value), ParseFloat(attrs?["exp_rate"]?.Value), ParseBool(attrs?["is_outside"]?.Value)) },
        { "mini_game_give_reward", attrs => new Trigger.MiniGameGiveReward(ParseInt(attrs?["winner_box_id"]?.Value), attrs?["content_type"]?.Value ?? string.Empty, attrs?["game_name"]?.Value ?? string.Empty) },
        { "move_npc", attrs => new Trigger.MoveNpc(ParseInt(attrs?["spawn_id"]?.Value), attrs?["patrol_name"]?.Value ?? string.Empty) },
        { "move_npc_to_pos", attrs => new Trigger.MoveNpcToPos(ParseInt(attrs?["spawn_id"]?.Value), ParseVector3(attrs?["pos"]?.Value), ParseVector3(attrs?["rot"]?.Value)) },
        { "move_random_user", attrs => new Trigger.MoveRandomUser(ParseInt(attrs?["map_id"]?.Value), ParseInt(attrs?["portal_id"]?.Value), ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["count"]?.Value)) },
        { "move_to_portal", attrs => new Trigger.MoveToPortal(ParseInt(attrs?["user_tag_id"]?.Value), ParseInt(attrs?["portal_id"]?.Value), ParseInt(attrs?["box_id"]?.Value)) },
        { "move_user", attrs => new Trigger.MoveUser(ParseInt(attrs?["map_id"]?.Value), ParseInt(attrs?["portal_id"]?.Value), ParseInt(attrs?["box_id"]?.Value)) },
        { "move_user_path", attrs => new Trigger.MoveUserPath(attrs?["patrol_name"]?.Value ?? string.Empty) },
        { "move_user_to_box", attrs => new Trigger.MoveUserToBox(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["portal_id"]?.Value)) },
        { "move_user_to_pos", attrs => new Trigger.MoveUserToPos(ParseVector3(attrs?["pos"]?.Value), ParseVector3(attrs?["rot"]?.Value)) },
        { "notice", attrs => new Trigger.Notice(ParseInt(attrs?["type"]?.Value), attrs?["script"]?.Value ?? string.Empty, ParseBool(attrs?["arg3"]?.Value)) },
        { "npc_remove_additional_effect", attrs => new Trigger.NpcRemoveAdditionalEffect(ParseInt(attrs?["spawn_id"]?.Value), ParseInt(attrs?["additional_effect_id"]?.Value)) },
        { "npc_to_patrol_in_box", attrs => new Trigger.NpcToPatrolInBox(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["npc_id"]?.Value), attrs?["spawn_id"]?.Value ?? string.Empty, attrs?["patrol_name"]?.Value ?? string.Empty) },
        { "patrol_condition_user", attrs => new Trigger.PatrolConditionUser(attrs?["patrol_name"]?.Value ?? string.Empty, ParseInt(attrs?["patrol_index"]?.Value), ParseInt(attrs?["additional_effect_id"]?.Value)) },
        { "play_scene_movie", attrs => new Trigger.PlaySceneMovie(attrs?["file_name"]?.Value ?? string.Empty, ParseInt(attrs?["movie_id"]?.Value), attrs?["skip_type"]?.Value ?? string.Empty) },
        { "play_system_sound_by_user_tag", attrs => new Trigger.PlaySystemSoundByUserTag(ParseInt(attrs?["user_tag_id"]?.Value), attrs?["sound_key"]?.Value ?? string.Empty) },
        { "play_system_sound_in_box", attrs => new Trigger.PlaySystemSoundInBox(attrs?["sound"]?.Value ?? string.Empty, ParseIntArray(attrs?["box_ids"]?.Value)) }, {
            "random_additional_effect", attrs =>
                new Trigger.RandomAdditionalEffect(attrs?["target"]?.Value ?? string.Empty, ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["spawn_id"]?.Value),
                    ParseInt(attrs?["target_count"]?.Value), ParseInt(attrs?["tick"]?.Value), ParseInt(attrs?["wait_tick"]?.Value), attrs?["target_effect"]?.Value ?? string.Empty, ParseInt(attrs?["additional_effect_id"]?.Value))
        },
        { "remove_balloon_talk", attrs => new Trigger.RemoveBalloonTalk(ParseInt(attrs?["spawn_id"]?.Value)) },
        { "remove_buff", attrs => new Trigger.RemoveBuff(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["skill_id"]?.Value), ParseBool(attrs?["is_player"]?.Value)) },
        { "remove_cinematic_talk", _ => new Trigger.RemoveCinematicTalk() },
        { "remove_effect_nif", attrs => new Trigger.RemoveEffectNif(ParseInt(attrs?["spawn_id"]?.Value)) },
        { "reset_camera", attrs => new Trigger.ResetCamera(ParseFloat(attrs?["interpolation_time"]?.Value)) },
        { "reset_timer", attrs => new Trigger.ResetTimer(attrs?["timer_id"]?.Value ?? string.Empty) },
        { "room_expire", _ => new Trigger.RoomExpire() },
        { "score_board_create", attrs => new Trigger.ScoreBoardCreate(attrs?["type"]?.Value ?? string.Empty, attrs?["title"]?.Value ?? string.Empty, ParseInt(attrs?["max_score"]?.Value)) },
        { "score_board_remove", _ => new Trigger.ScoreBoardRemove() },
        { "score_board_set_score", attrs => new Trigger.ScoreBoardSetScore(ParseInt(attrs?["score"]?.Value)) },
        { "select_camera", attrs => new Trigger.SelectCamera(ParseInt(attrs?["trigger_id"]?.Value), ParseBool(attrs?["enable"]?.Value)) },
        { "select_camera_path", attrs => new Trigger.SelectCameraPath(ParseIntArray(attrs?["path_ids"]?.Value), ParseBool(attrs?["return_view"]?.Value)) },
        { "set_achievement", attrs => new Trigger.SetAchievement(ParseInt(attrs?["trigger_id"]?.Value), attrs?["type"]?.Value ?? string.Empty, attrs?["achieve"]?.Value ?? string.Empty) },
        { "set_actor", attrs => new Trigger.SetActor(ParseInt(attrs?["trigger_id"]?.Value), ParseBool(attrs?["visible"]?.Value), attrs?["initial_sequence"]?.Value ?? string.Empty, ParseBool(attrs?["arg4"]?.Value), ParseBool(attrs?["arg5"]?.Value)) },
        { "set_agent", attrs => new Trigger.SetAgent(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value)) },
        { "set_ai_extra_data", attrs => new Trigger.SetAiExtraData(attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["value"]?.Value), ParseBool(attrs?["is_modify"]?.Value), ParseInt(attrs?["box_id"]?.Value)) },
        { "set_ambient_light", attrs => new Trigger.SetAmbientLight(ParseVector3(attrs?["primary"]?.Value), ParseVector3(attrs?["secondary"]?.Value), ParseVector3(attrs?["tertiary"]?.Value)) },
        { "set_breakable", attrs => new Trigger.SetBreakable(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["enable"]?.Value)) },
        { "set_cinematic_intro", attrs => new Trigger.SetCinematicIntro(attrs?["text"]?.Value ?? string.Empty) },
        { "set_cinematic_ui", attrs => new Trigger.SetCinematicUi(ParseInt(attrs?["type"]?.Value), attrs?["script"]?.Value ?? string.Empty, ParseBool(attrs?["arg3"]?.Value)) },
        { "set_cube", attrs => new Trigger.SetCube(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["is_visible"]?.Value), ParseInt(attrs?["random_count"]?.Value)) },
        { "set_dialogue", attrs => new Trigger.SetDialogue(ParseInt(attrs?["type"]?.Value), ParseInt(attrs?["spawn_id"]?.Value), attrs?["script"]?.Value ?? string.Empty, ParseInt(attrs?["time"]?.Value), ParseInt(attrs?["arg5"]?.Value), ParseAlign(attrs?["align"]?.Value)) },
        { "set_directional_light", attrs => new Trigger.SetDirectionalLight(ParseVector3(attrs?["diffuse_color"]?.Value), ParseVector3(attrs?["specular_color"]?.Value)) },
        { "set_effect", attrs => new Trigger.SetEffect(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseInt(attrs?["start_delay"]?.Value), ParseInt(attrs?["interval"]?.Value)) },
        { "set_event_ui_countdown", attrs => new Trigger.SetEventUiCountdown(attrs?["script"]?.Value ?? string.Empty, ParseIntArray(attrs?["round_countdown"]?.Value), attrs?["box_ids"]?.Value.Split(',') ?? []) },
        { "set_event_ui_round", attrs => new Trigger.SetEventUiRound(ParseIntArray(attrs?["rounds"]?.Value), ParseInt(attrs?["v_offset"]?.Value), ParseInt(attrs?["arg3"]?.Value)) },
        { "set_event_ui_script", attrs => new Trigger.SetEventUiScript(ParseBannerType(attrs?["type"]?.Value), attrs?["script"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), attrs?["box_ids"]?.Value.Split(',') ?? []) },
        { "set_gravity", attrs => new Trigger.SetGravity(ParseFloat(attrs?["gravity"]?.Value)) },
        { "set_interact_object", attrs => new Trigger.SetInteractObject(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseInt(attrs?["state"]?.Value), ParseBool(attrs?["arg4"]?.Value), ParseBool(attrs?["arg3"]?.Value)) },
        { "set_ladder", attrs => new Trigger.SetLadder(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseBool(attrs?["enable"]?.Value), ParseInt(attrs?["fade"]?.Value)) },
        { "set_local_camera", attrs => new Trigger.SetLocalCamera(ParseInt(attrs?["camera_id"]?.Value), ParseBool(attrs?["enable"]?.Value)) },
        { "set_mesh", attrs => new Trigger.SetMesh(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseInt(attrs?["start_delay"]?.Value), ParseInt(attrs?["interval"]?.Value), ParseFloat(attrs?["fade"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "set_mesh_animation", attrs => new Trigger.SetMeshAnimation(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseInt(attrs?["start_delay"]?.Value), ParseInt(attrs?["interval"]?.Value)) },
        { "set_mini_game_area_for_hack", attrs => new Trigger.SetMiniGameAreaForHack(ParseInt(attrs?["box_id"]?.Value)) },
        { "set_npc_duel_hp_bar", attrs => new Trigger.SetNpcDuelHpBar(ParseBool(attrs?["is_open"]?.Value), ParseInt(attrs?["spawn_id"]?.Value), ParseInt(attrs?["duration_tick"]?.Value), ParseInt(attrs?["npc_hp_step"]?.Value)) },
        { "set_npc_emotion_loop", attrs => new Trigger.SetNpcEmotionLoop(ParseInt(attrs?["spawn_id"]?.Value), attrs?["sequence_name"]?.Value ?? string.Empty, ParseFloat(attrs?["duration"]?.Value)) },
        { "set_npc_emotion_sequence", attrs => new Trigger.SetNpcEmotionSequence(ParseInt(attrs?["spawn_id"]?.Value), attrs?["sequence_name"]?.Value ?? string.Empty, ParseInt(attrs?["duration_tick"]?.Value)) },
        { "set_npc_rotation", attrs => new Trigger.SetNpcRotation(ParseInt(attrs?["spawn_id"]?.Value), ParseFloat(attrs?["rotation"]?.Value)) },
        { "set_onetime_effect", attrs => new Trigger.SetOnetimeEffect(ParseInt(attrs?["id"]?.Value), ParseBool(attrs?["enable"]?.Value), attrs?["path"]?.Value ?? string.Empty) },
        { "set_pc_emotion_loop", attrs => new Trigger.SetPcEmotionLoop(attrs?["sequence_name"]?.Value ?? string.Empty, ParseFloat(attrs?["duration"]?.Value), ParseBool(attrs?["loop"]?.Value)) },
        { "set_pc_emotion_sequence", attrs => new Trigger.SetPcEmotionSequence(attrs?["sequence_names"]?.Value.Split(',') ?? []) },
        { "set_pc_rotation", attrs => new Trigger.SetPcRotation(ParseVector3(attrs?["rotation"]?.Value)) },
        { "set_photo_studio", attrs => new Trigger.SetPhotoStudio(ParseBool(attrs?["is_enable"]?.Value)) },
        { "set_portal", attrs => new Trigger.SetPortal(ParseInt(attrs?["portal_id"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseBool(attrs?["enable"]?.Value), ParseBool(attrs?["minimap_visible"]?.Value), ParseBool(attrs?["arg5"]?.Value)) },
        { "set_pvp_zone", attrs => new Trigger.SetPvpZone(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["prepare_time"]?.Value), ParseInt(attrs?["match_time"]?.Value), ParseInt(attrs?["additional_effect_id"]?.Value), ParseInt(attrs?["type"]?.Value), ParseIntArray(attrs?["box_ids"]?.Value)) },
        { "set_quest_accept", attrs => new Trigger.SetQuestAccept(ParseInt(attrs?["quest_id"]?.Value)) },
        { "set_quest_complete", attrs => new Trigger.SetQuestComplete(ParseInt(attrs?["quest_id"]?.Value)) },
        { "set_random_mesh", attrs => new Trigger.SetRandomMesh(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseInt(attrs?["start_delay"]?.Value), ParseInt(attrs?["interval"]?.Value), ParseInt(attrs?["fade"]?.Value)) },
        { "set_rope", attrs => new Trigger.SetRope(ParseInt(attrs?["trigger_id"]?.Value), ParseBool(attrs?["visible"]?.Value), ParseBool(attrs?["enable"]?.Value), ParseInt(attrs?["fade"]?.Value)) },
        { "set_scene_skip", attrs => new Trigger.SetSceneSkip(attrs?["state"]?.Value ?? string.Empty, attrs?["action"]?.Value ?? string.Empty) },
        { "set_skill", attrs => new Trigger.SetSkill(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["enable"]?.Value)) },
        { "set_skip", attrs => new Trigger.SetSkip(attrs?["state"]?.Value ?? string.Empty) },
        { "set_sound", attrs => new Trigger.SetSound(ParseInt(attrs?["trigger_id"]?.Value), ParseBool(attrs?["enable"]?.Value)) },
        { "set_state", attrs => new Trigger.SetState(ParseInt(attrs?["id"]?.Value), attrs?["states"]?.Value.Split(',') ?? [], ParseBool(attrs?["randomize"]?.Value)) },
        { "set_time_scale", attrs => new Trigger.SetTimeScale(ParseBool(attrs?["enable"]?.Value), ParseFloat(attrs?["start_scale"]?.Value), ParseFloat(attrs?["end_scale"]?.Value), ParseFloat(attrs?["duration"]?.Value), ParseInt(attrs?["interpolator"]?.Value)) },
        { "set_timer", attrs => new Trigger.SetTimer(attrs?["timer_id"]?.Value ?? string.Empty, ParseInt(attrs?["seconds"]?.Value), ParseBool(attrs?["auto_remove"]?.Value), ParseBool(attrs?["display"]?.Value), ParseInt(attrs?["v_offset"]?.Value), attrs?["type"]?.Value ?? string.Empty, attrs?["desc"]?.Value ?? string.Empty) },
        { "set_user_value", attrs => new Trigger.SetUserValue(ParseInt(attrs?["trigger_id"]?.Value), attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["value"]?.Value)) },
        { "set_user_value_from_dungeon_reward_count", attrs => new Trigger.SetUserValueFromDungeonRewardCount(attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["dungeon_reward_id"]?.Value)) },
        { "set_user_value_from_guild_vs_game_score", attrs => new Trigger.SetUserValueFromGuildVsGameScore(ParseInt(attrs?["team_id"]?.Value), attrs?["key"]?.Value ?? string.Empty) },
        { "set_user_value_from_user_count", attrs => new Trigger.SetUserValueFromUserCount(ParseInt(attrs?["trigger_box_id"]?.Value), attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["user_tag_id"]?.Value)) },
        { "set_visible_breakable_object", attrs => new Trigger.SetVisibleBreakableObject(ParseIntArray(attrs?["trigger_ids"]?.Value), ParseBool(attrs?["visible"]?.Value)) },
        { "set_visible_ui", attrs => new Trigger.SetVisibleUi(attrs?["ui_names"]?.Value.Split(',') ?? [], ParseBool(attrs?["visible"]?.Value)) },
        { "shadow_expedition_close_boss_gauge", _ => new Trigger.ShadowExpeditionCloseBossGauge() },
        { "shadow_expedition_open_boss_gauge", attrs => new Trigger.ShadowExpeditionOpenBossGauge(ParseInt(attrs?["max_gauge_point"]?.Value), attrs?["title"]?.Value ?? string.Empty) }, {
            "show_caption", attrs => new Trigger.ShowCaption(attrs?["type"]?.Value ?? string.Empty, attrs?["title"]?.Value ?? string.Empty, attrs?["desc"]?.Value ?? string.Empty,
                ParseAlign(attrs?["align"]?.Value), ParseFloat(attrs?["offset_rate_x"]?.Value), ParseFloat(attrs?["offset_rate_y"]?.Value), ParseInt(attrs?["duration"]?.Value), ParseFloat(attrs?["scale"]?.Value))
        },
        { "show_count_ui", attrs => new Trigger.ShowCountUi(attrs?["text"]?.Value ?? string.Empty, ParseInt(attrs?["stage"]?.Value), ParseInt(attrs?["count"]?.Value), ParseInt(attrs?["sound_type"]?.Value)) },
        { "show_event_result", attrs => new Trigger.ShowEventResult(attrs?["type"]?.Value ?? string.Empty, attrs?["text"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), ParseInt(attrs?["user_tag_id"]?.Value), ParseInt(attrs?["trigger_box_id"]?.Value), ParseBool(attrs?["is_outside"]?.Value)) },
        { "show_guide_summary", attrs => new Trigger.ShowGuideSummary(ParseInt(attrs?["entity_id"]?.Value), ParseInt(attrs?["text_id"]?.Value), ParseInt(attrs?["duration"]?.Value)) },
        { "show_round_ui", attrs => new Trigger.ShowRoundUi(ParseInt(attrs?["round"]?.Value), ParseInt(attrs?["duration"]?.Value), ParseBool(attrs?["is_final_round"]?.Value)) },
        { "side_npc_cutin", attrs => new Trigger.SideNpcCutin(attrs?["illust"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value)) },
        { "side_npc_movie", attrs => new Trigger.SideNpcMovie(attrs?["usm"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value)) },
        { "side_npc_talk", attrs => new Trigger.SideNpcTalk(ParseInt(attrs?["npc_id"]?.Value), attrs?["illust"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), attrs?["script"]?.Value ?? string.Empty, attrs?["voice"]?.Value ?? string.Empty) },
        { "side_npc_talk_bottom", attrs => new Trigger.SideNpcTalkBottom(ParseInt(attrs?["npc_id"]?.Value), attrs?["illust"]?.Value ?? string.Empty, ParseInt(attrs?["duration"]?.Value), attrs?["script"]?.Value ?? string.Empty) },
        { "sight_range", attrs => new Trigger.SightRange(ParseBool(attrs?["enable"]?.Value), ParseInt(attrs?["range"]?.Value), ParseInt(attrs?["range_z"]?.Value), ParseInt(attrs?["border"]?.Value)) },
        { "spawn_item_range", attrs => new Trigger.SpawnItemRange(ParseIntArray(attrs?["range_ids"]?.Value), ParseInt(attrs?["random_pick_count"]?.Value)) },
        { "spawn_monster", attrs => new Trigger.SpawnMonster(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseBool(attrs?["auto_target"]?.Value), ParseInt(attrs?["delay"]?.Value)) },
        { "spawn_npc_range", attrs => new Trigger.SpawnNpcRange(ParseIntArray(attrs?["range_ids"]?.Value), ParseBool(attrs?["is_auto_targeting"]?.Value), ParseInt(attrs?["random_pick_count"]?.Value), ParseInt(attrs?["score"]?.Value)) },
        { "start_combine_spawn", attrs => new Trigger.StartCombineSpawn(ParseIntArray(attrs?["group_id"]?.Value), ParseBool(attrs?["is_start"]?.Value)) },
        { "start_mini_game", attrs => new Trigger.StartMiniGame(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["round"]?.Value), attrs?["game_name"]?.Value ?? string.Empty, ParseBool(attrs?["is_show_result_ui"]?.Value)) },
        { "start_mini_game_round", attrs => new Trigger.StartMiniGameRound(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["round"]?.Value)) },
        { "start_tutorial", _ => new Trigger.StartTutorial() },
        { "talk_npc", attrs => new Trigger.TalkNpc(ParseInt(attrs?["spawn_id"]?.Value)) },
        { "unset_mini_game_area_for_hack", _ => new Trigger.UnsetMiniGameAreaForHack() },
        { "use_state", attrs => new Trigger.UseState(ParseInt(attrs?["id"]?.Value), ParseBool(attrs?["randomize"]?.Value)) },
        { "user_tag_symbol", attrs => new Trigger.UserTagSymbol(attrs?["symbol1"]?.Value ?? string.Empty, attrs?["symbol2"]?.Value ?? string.Empty) },
        { "user_value_to_number_mesh", attrs => new Trigger.UserValueToNumberMesh(attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["start_mesh_id"]?.Value), ParseInt(attrs?["digit_count"]?.Value)) },
        { "visible_my_pc", attrs => new Trigger.VisibleMyPc(ParseBool(attrs?["is_visible"]?.Value)) },
        { "weather", attrs => new Trigger.SetWeather(ParseWeather(attrs?["weather_type"]?.Value)) },
        { "wedding_broken", _ => new Trigger.WeddingBroken() },
        { "wedding_move_user", attrs => new Trigger.WeddingMoveUser(attrs?["entry_type"]?.Value ?? string.Empty, ParseInt(attrs?["map_id"]?.Value), ParseIntArray(attrs?["portal_ids"]?.Value), ParseInt(attrs?["box_id"]?.Value)) },
        { "wedding_mutual_agree", attrs => new Trigger.WeddingMutualAgree(attrs?["agree_type"]?.Value ?? string.Empty) },
        { "wedding_mutual_cancel", attrs => new Trigger.WeddingMutualCancel(attrs?["agree_type"]?.Value ?? string.Empty) },
        { "wedding_set_user_emotion", attrs => new Trigger.WeddingSetUserEmotion(attrs?["entry_type"]?.Value ?? string.Empty, ParseInt(attrs?["id"]?.Value)) },
        { "wedding_set_user_look_at", attrs => new Trigger.WeddingSetUserLookAt(attrs?["entry_type"]?.Value ?? string.Empty, attrs?["look_at_entry_type"]?.Value ?? string.Empty, ParseBool(attrs?["immediate"]?.Value)) },
        { "wedding_set_user_rotation", attrs => new Trigger.WeddingSetUserRotation(attrs?["entry_type"]?.Value ?? string.Empty, ParseVector3(attrs?["rotation"]?.Value), ParseBool(attrs?["immediate"]?.Value)) },
        { "wedding_user_to_patrol", attrs => new Trigger.WeddingUserToPatrol(attrs?["patrol_name"]?.Value ?? string.Empty, attrs?["entry_type"]?.Value ?? string.Empty, ParseInt(attrs?["patrol_index"]?.Value)) },
        { "wedding_vow_complete", _ => new Trigger.WeddingVowComplete() },
        { "widget_action", attrs => new Trigger.WidgetAction(attrs?["type"]?.Value ?? string.Empty, attrs?["func"]?.Value ?? string.Empty, attrs?["widget_arg"]?.Value ?? string.Empty, attrs?["desc"]?.Value ?? string.Empty, ParseInt(attrs?["widget_arg_num"]?.Value)) },
        { "write_log", attrs => new Trigger.WriteLog(attrs?["log_name"]?.Value ?? string.Empty, attrs?["event"]?.Value ?? string.Empty, ParseInt(attrs?["trigger_id"]?.Value), attrs?["sub_event"]?.Value ?? string.Empty, ParseInt(attrs?["level"]?.Value)) },

    };

    public static readonly Dictionary<string, Func<XmlAttributeCollection?, ICondition>> ConditionMap = new Dictionary<string, Func<XmlAttributeCollection?, ICondition>> {
        { "bonus_game_reward", attrs => new Trigger.BonusGameReward(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["type"]?.Value)) },
        { "check_any_user_additional_effect", attrs => new Trigger.CheckAnyUserAdditionalEffect(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["additional_effect_id"]?.Value), ParseInt(attrs?["level"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "check_dungeon_lobby_user_count", attrs => new Trigger.CheckDungeonLobbyUserCount(ParseBool(attrs?["negate"]?.Value)) },
        { "check_npc_additional_effect", attrs => new Trigger.CheckNpcAdditionalEffect(ParseInt(attrs?["spawn_id"]?.Value), ParseInt(attrs?["additional_effect_id"]?.Value), ParseInt(attrs?["level"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "npc_damage", attrs => new Trigger.NpcDamage(ParseInt(attrs?["spawn_id"]?.Value), ParseFloat(attrs?["damageRate"]?.Value), ParseOperatorType(attrs?["operator"]?.Value)) },
        { "npc_extra_data", attrs => new Trigger.NpcExtraData(ParseInt(attrs?["spawn_point_id"]?.Value), attrs?["extra_data_key"]?.Value ?? string.Empty, ParseInt(attrs?["extra_data_value"]?.Value), ParseOperatorType(attrs?["operator"]?.Value)) },
        { "npc_hp", attrs => new Trigger.NpcHp(ParseInt(attrs?["spawn_id"]?.Value), ParseBool(attrs?["is_relative"]?.Value), ParseInt(attrs?["value"]?.Value), ParseCompareType(attrs?["compare_type"]?.Value)) },
        { "check_same_user_tag", attrs => new Trigger.CheckSameUserTag(ParseInt(attrs?["box_id"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "check_user", attrs => new Trigger.CheckUser(ParseBool(attrs?["negate"]?.Value)) },
        { "user_count", attrs => new Trigger.UserCount(ParseInt(attrs?["check_count"]?.Value)) },
        { "count_users", attrs => new Trigger.CountUsers(ParseInt(attrs?["box_id"]?.Value), ParseInt(attrs?["user_tag_id"]?.Value), ParseInt(attrs?["min_users"]?.Value), ParseOperatorType(attrs?["operator"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "day_of_week", attrs => new Trigger.DayOfWeek(ParseIntArray(attrs?["day_of_weeks"]?.Value), attrs?["desc"]?.Value ?? string.Empty, ParseBool(attrs?["negate"]?.Value)) },
        { "detect_liftable_object", attrs => new Trigger.DetectLiftableObject(ParseIntArray(attrs?["box_ids"]?.Value), ParseInt(attrs?["item_id"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "dungeon_play_time", attrs => new Trigger.DungeonPlayTime(ParseInt(attrs?["play_seconds"]?.Value)) },
        { "dungeon_state", attrs => new Trigger.DungeonState(attrs?["check_state"]?.Value ?? string.Empty) },
        { "dungeon_first_user_mission_score", attrs => new Trigger.DungeonFirstUserMissionScore(ParseInt(attrs?["score"]?.Value), ParseOperatorType(attrs?["operator"]?.Value)) },
        { "dungeon_id", attrs => new Trigger.DungeonId(ParseInt(attrs?["dungeon_id"]?.Value)) },
        { "dungeon_level", attrs => new Trigger.DungeonLevel(ParseInt(attrs?["level"]?.Value)) },
        { "dungeon_max_user_count", attrs => new Trigger.DungeonMaxUserCount(ParseInt(attrs?["level"]?.Value)) },
        { "dungeon_round", attrs => new Trigger.DungeonRound(ParseInt(attrs?["round"]?.Value)) },
        { "dungeon_timeout", _ => new Trigger.DungeonTimeout() },
        { "dungeon_variable", attrs => new Trigger.DungeonVariable(ParseInt(attrs?["var_id"]?.Value), ParseInt(attrs?["value"]?.Value)) },
        { "guild_vs_game_scored_team", attrs => new Trigger.GuildVsGameScoredTeam(ParseInt(attrs?["team_id"]?.Value)) },
        { "guild_vs_game_winner_team", attrs => new Trigger.GuildVsGameWinnerTeam(ParseInt(attrs?["team_id"]?.Value)) },
        { "is_dungeon_room", attrs => new Trigger.IsDungeonRoom(ParseBool(attrs?["negate"]?.Value)) },
        { "is_playing_maple_survival", attrs => new Trigger.IsPlayingMapleSurvival(ParseBool(attrs?["negate"]?.Value)) },
        { "monster_dead", attrs => new Trigger.MonsterDead(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseBool(attrs?["auto_target"]?.Value)) },
        { "monster_in_combat", attrs => new Trigger.MonsterInCombat(ParseIntArray(attrs?["spawn_ids"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "npc_detected", attrs => new Trigger.NpcDetected(ParseInt(attrs?["box_id"]?.Value), ParseIntArray(attrs?["spawn_ids"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "npc_is_dead_by_string_id", attrs => new Trigger.NpcIsDeadByStringId(attrs?["string_id"]?.Value ?? string.Empty) },
        { "object_interacted", attrs => new Trigger.ObjectInteracted(ParseIntArray(attrs?["interact_ids"]?.Value), ParseInt(attrs?["state"]?.Value)) },
        { "pvp_zone_ended", attrs => new Trigger.PvpZoneEnded(ParseInt(attrs?["box_id"]?.Value)) },
        { "quest_user_detected", attrs => new Trigger.QuestUserDetected(ParseIntArray(attrs?["box_ids"]?.Value), ParseIntArray(attrs?["quest_ids"]?.Value), ParseIntArray(attrs?["quest_states"]?.Value), ParseInt(attrs?["job_code"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "random_condition", attrs => new Trigger.RandomCondition(ParseFloat(attrs?["weight"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "score_board_score", attrs => new Trigger.ScoreBoardScore(ParseInt(attrs?["score"]?.Value), ParseOperatorType(attrs?["compare_op"]?.Value)) },
        { "shadow_expedition_points", attrs => new Trigger.ShadowExpeditionPoints(ParseInt(attrs?["score"]?.Value)) },
        { "time_expired", attrs => new Trigger.TimeExpired(attrs?["timer_id"]?.Value ?? string.Empty) },
        { "user_detected", attrs => new Trigger.UserDetected(ParseIntArray(attrs?["box_ids"]?.Value), ParseJobCode(attrs?["job_code"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "user_value", attrs => new Trigger.UserValue(attrs?["key"]?.Value ?? string.Empty, ParseInt(attrs?["value"]?.Value), ParseBool(attrs?["negate"]?.Value)) },
        { "wait_and_reset_tick", attrs => new Trigger.WaitAndResetTick(ParseInt(attrs?["wait_tick"]?.Value)) },
        { "wait_seconds_user_value", attrs => new Trigger.WaitSecondsUserValue(attrs?["key"]?.Value ?? string.Empty, attrs?["desc"]?.Value ?? string.Empty) },
        { "wait_tick", attrs => new Trigger.WaitTick(ParseInt(attrs?["wait_tick"]?.Value)) },
        { "wedding_entry_in_field", attrs => new Trigger.WeddingEntryInField(attrs?["entry_type"]?.Value ?? string.Empty, ParseBool(attrs?["is_in_field"]?.Value)) },
        { "wedding_hall_state", attrs => new Trigger.WeddingHallState(attrs?["hallState"]?.Value ?? string.Empty, ParseBool(attrs?["success"]?.Value)) },
        { "wedding_mutual_agree_result", attrs => new Trigger.WeddingMutualAgreeResult(attrs?["agree_type"]?.Value ?? string.Empty) },
        { "widget_value", attrs => new Trigger.WidgetValue(attrs?["type"]?.Value ?? string.Empty, attrs?["widget_name"]?.Value ?? string.Empty, attrs?["condition"]?.Value ?? string.Empty, ParseBool(attrs?["negate"]?.Value), attrs?["desc"]?.Value ?? string.Empty) },
        { "any_one", _ => new Trigger.GroupAnyOne() },
        { "all_of", _ => new Trigger.GroupAllOf() },
        { "true", _ => new Trigger.GroupTrue() },
        { "always", _ => new Trigger.GroupAlways() },
    };

    private static JobCode ParseJobCode(string? value) {
        if (string.IsNullOrEmpty(value)) return JobCode.None;
        return Enum.TryParse<JobCode>(value, true, out var jobCode)
            ? jobCode
            : JobCode.None;
    }

    private static Weather ParseWeather(string? value) {
        if (string.IsNullOrEmpty(value)) return Weather.Clear;
        return Enum.TryParse(value, true, out Weather weather) ? weather : Weather.Clear;
    }

    private static BannerType ParseBannerType(string? value) {
        if (string.IsNullOrEmpty(value)) return BannerType.Text;
        return value switch {
            // "0"
            "1" => BannerType.Text,
            // "2"
            "3" => BannerType.Winner,
            "4" => BannerType.Lose,
            "5" => BannerType.GameOver,
            "6" => BannerType.Bonus,
            "7" => BannerType.Success,
            _ => BannerType.Text,
        };
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
                        if (start > end) (start, end) = (end, start);
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
                if (start > end) (start, end) = (end, start);
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
