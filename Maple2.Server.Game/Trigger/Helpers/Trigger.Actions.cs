using System.Numerics;

namespace Maple2.Server.Game.Trigger.Helpers;

public partial class Trigger {
    public class AddBalloonTalk(int spawnId, string msg, int duration, int delayTick, int npcId) : IAction {
        public void Execute(TriggerContext context) => context.AddBalloonTalk(spawnId, msg, duration, delayTick, npcId);
    }

    public class AddBuff(int[] boxIds, int skillId, int level, bool ignorePlayer, bool isSkillSet, string feature) : IAction {
        public void Execute(TriggerContext context) => context.AddBuff(boxIds, skillId, level, ignorePlayer, isSkillSet, feature);
    }

    public class AddCinematicTalk(int npcId, string illustId, string msg, int duration, Align align, int delayTick) : IAction {
        public void Execute(TriggerContext context) => context.AddCinematicTalk(npcId, illustId, msg, duration, align, delayTick);
    }

    public class AddEffectNif(int spawnId, string nifPath, bool isOutline, float scale, int rotateZ) : IAction {
        public void Execute(TriggerContext context) => context.AddEffectNif(spawnId, nifPath, isOutline, scale, rotateZ);
    }

    public class AddUserValue(string key, int value) : IAction {
        public void Execute(TriggerContext context) => context.AddUserValue(key, value);
    }

    public class AllocateBattlefieldPoints(int boxId, int points) : IAction {
        public void Execute(TriggerContext context) => context.AllocateBattlefieldPoints(boxId, points);
    }

    public class Announce(int type, string content, bool arg3) : IAction {
        public void Execute(TriggerContext context) => context.Announce(type, content, arg3);
    }

    public class ArcadeBoomBoomOceanClearRound(int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeBoomBoomOceanClearRound(round);
    }

    public class ArcadeBoomBoomOceanEndGame : IAction {
        public void Execute(TriggerContext context) => context.ArcadeBoomBoomOceanEndGame();
    }

    public class ArcadeBoomBoomOceanSetSkillScore(int id, int score) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeBoomBoomOceanSetSkillScore(id, score);
    }

    public class ArcadeBoomBoomOceanStartGame(int lifeCount) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeBoomBoomOceanStartGame(lifeCount);
    }

    public class ArcadeBoomBoomOceanStartRound(int round, int roundDuration, int timeScoreRate) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeBoomBoomOceanStartRound(round, roundDuration, timeScoreRate);
    }

    public class ArcadeSpringFarmClearRound(int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeSpringFarmClearRound(round);
    }

    public class ArcadeSpringFarmEndGame : IAction {
        public void Execute(TriggerContext context) => context.ArcadeSpringFarmEndGame();
    }

    public class ArcadeSpringFarmSetInteractScore(int id, int score) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeSpringFarmSetInteractScore(id, score);
    }

    public class ArcadeSpringFarmSpawnMonster(int[] spawnIds, int score) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeSpringFarmSpawnMonster(spawnIds, score);
    }

    public class ArcadeSpringFarmStartGame(int lifeCount) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeSpringFarmStartGame(lifeCount);
    }

    public class ArcadeSpringFarmStartRound(int uiDuration, int round, string timeScoreType, int timeScoreRate, int roundDuration) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeSpringFarmStartRound(uiDuration, round, timeScoreType, timeScoreRate, roundDuration);
    }

    public class ArcadeThreeTwoOneClearRound(int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOneClearRound(round);
    }

    public class ArcadeThreeTwoOneEndGame : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOneEndGame();
    }

    public class ArcadeThreeTwoOneResultRound(int resultDirection) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOneResultRound(resultDirection);
    }

    public class ArcadeThreeTwoOneResultRound2(int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOneResultRound2(round);
    }

    public class ArcadeThreeTwoOneStartGame(int lifeCount, int initScore) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOneStartGame(lifeCount, initScore);
    }

    public class ArcadeThreeTwoOneStartRound(int uiDuration, int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOneStartRound(uiDuration, round);
    }

    public class ArcadeThreeTwoOne2ClearRound(int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne2ClearRound(round);
    }

    public class ArcadeThreeTwoOne2EndGame : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne2EndGame();
    }

    public class ArcadeThreeTwoOne2ResultRound(int resultDirection) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne2ResultRound(resultDirection);
    }

    public class ArcadeThreeTwoOne2ResultRound2(int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne2ResultRound2(round);
    }

    public class ArcadeThreeTwoOne2StartGame(int lifeCount, int initScore) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne2StartGame(lifeCount, initScore);
    }

    public class ArcadeThreeTwoOne2StartRound(int uiDuration, int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne2StartRound(uiDuration, round);
    }

    public class ArcadeThreeTwoOne3ClearRound(int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne3ClearRound(round);
    }

    public class ArcadeThreeTwoOne3EndGame : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne3EndGame();
    }

    public class ArcadeThreeTwoOne3ResultRound(int resultDirection) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne3ResultRound(resultDirection);
    }

    public class ArcadeThreeTwoOne3ResultRound2(int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne3ResultRound2(round);
    }

    public class ArcadeThreeTwoOne3StartGame(int lifeCount, int initScore) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne3StartGame(lifeCount, initScore);
    }

    public class ArcadeThreeTwoOne3StartRound(int uiDuration, int round) : IAction {
        public void Execute(TriggerContext context) => context.ArcadeThreeTwoOne3StartRound(uiDuration, round);
    }

    public class ChangeBackground(string dds) : IAction {
        public void Execute(TriggerContext context) => context.ChangeBackground(dds);
    }

    public class ChangeMonster(int fromSpawnId, int toSpawnId) : IAction {
        public void Execute(TriggerContext context) => context.ChangeMonster(fromSpawnId, toSpawnId);
    }

    public class CloseCinematic : IAction {
        public void Execute(TriggerContext context) => context.CloseCinematic();
    }

    public class CreateFieldGame(FieldGame type, bool reset) : IAction {
        public void Execute(TriggerContext context) => context.CreateFieldGame(type, reset);
    }

    public class CreateItem(int[] spawnIds, int triggerId, int itemId, int arg5) : IAction {
        public void Execute(TriggerContext context) => context.CreateItem(spawnIds, triggerId, itemId, arg5);
    }

    public class CreateWidget(string type) : IAction {
        public void Execute(TriggerContext context) => context.CreateWidget(type);
    }

    public class DarkStreamClearRound(int round) : IAction {
        public void Execute(TriggerContext context) => context.DarkStreamClearRound(round);
    }

    public class DarkStreamSpawnMonster(int[] spawnIds, int score) : IAction {
        public void Execute(TriggerContext context) => context.DarkStreamSpawnMonster(spawnIds, score);
    }

    public class DarkStreamStartGame(int round) : IAction {
        public void Execute(TriggerContext context) => context.DarkStreamStartGame(round);
    }

    public class DarkStreamStartRound(int round, int uiDuration, int damagePenalty) : IAction {
        public void Execute(TriggerContext context) => context.DarkStreamStartRound(round, uiDuration, damagePenalty);
    }

    public class DebugString(string value, string feature) : IAction {
        public void Execute(TriggerContext context) => context.DebugString(value, feature);
    }

    public class DestroyMonster(int[] spawnIds, bool arg2) : IAction {
        public void Execute(TriggerContext context) => context.DestroyMonster(spawnIds, arg2);
    }

    public class DungeonClear(string uiType) : IAction {
        public void Execute(TriggerContext context) => context.DungeonClear(uiType);
    }

    public class DungeonClearRound(int round) : IAction {
        public void Execute(TriggerContext context) => context.DungeonClearRound(round);
    }

    public class DungeonCloseTimer : IAction {
        public void Execute(TriggerContext context) => context.DungeonCloseTimer();
    }

    public class DungeonDisableRanking : IAction {
        public void Execute(TriggerContext context) => context.DungeonDisableRanking();
    }

    public class DungeonEnableGiveUp(bool isEnable) : IAction {
        public void Execute(TriggerContext context) => context.DungeonEnableGiveUp(isEnable);
    }

    public class DungeonFail : IAction {
        public void Execute(TriggerContext context) => context.DungeonFail();
    }

    public class DungeonMissionComplete(string feature, int missionId) : IAction {
        public void Execute(TriggerContext context) => context.DungeonMissionComplete(feature, missionId);
    }

    public class DungeonMoveLapTimeToNow(int id) : IAction {
        public void Execute(TriggerContext context) => context.DungeonMoveLapTimeToNow(id);
    }

    public class DungeonResetTime(int seconds) : IAction {
        public void Execute(TriggerContext context) => context.DungeonResetTime(seconds);
    }

    public class DungeonSetEndTime : IAction {
        public void Execute(TriggerContext context) => context.DungeonSetEndTime();
    }

    public class DungeonSetLapTime(int id, int lapTime) : IAction {
        public void Execute(TriggerContext context) => context.DungeonSetLapTime(id, lapTime);
    }

    public class DungeonStopTimer : IAction {
        public void Execute(TriggerContext context) => context.DungeonStopTimer();
    }

    public class SetDungeonVariable(int varId, int value) : IAction {
        public void Execute(TriggerContext context) => context.SetDungeonVariable(varId, value);
    }

    public class EnableLocalCamera(bool isEnable) : IAction {
        public void Execute(TriggerContext context) => context.EnableLocalCamera(isEnable);
    }

    public class EnableSpawnPointPc(int spawnId, bool isEnable) : IAction {
        public void Execute(TriggerContext context) => context.EnableSpawnPointPc(spawnId, isEnable);
    }

    public class EndMiniGame(int winnerBoxId, string gameName, bool isOnlyWinner) : IAction {
        public void Execute(TriggerContext context) => context.EndMiniGame(winnerBoxId, gameName, isOnlyWinner);
    }

    public class EndMiniGameRound(int winnerBoxId, float expRate, float meso, bool isOnlyWinner, bool isGainLoserBonus, string gameName) : IAction {
        public void Execute(TriggerContext context) => context.EndMiniGameRound(winnerBoxId, expRate, meso, isOnlyWinner, isGainLoserBonus, gameName);
    }

    public class FaceEmotion(int spawnId, string emotionName) : IAction {
        public void Execute(TriggerContext context) => context.FaceEmotion(spawnId, emotionName);
    }

    public class FieldGameConstant(string key, string value, string feature, Locale locale) : IAction {
        public void Execute(TriggerContext context) => context.FieldGameConstant(key, value, feature, locale);
    }

    public class FieldGameMessage(int custom, string type, bool arg1, string script, int duration) : IAction {
        public void Execute(TriggerContext context) => context.FieldGameMessage(custom, type, arg1, script, duration);
    }

    public class FieldWarEnd(bool isClear) : IAction {
        public void Execute(TriggerContext context) => context.FieldWarEnd(isClear);
    }

    public class GiveExp(int boxId, float rate, bool arg3) : IAction {
        public void Execute(TriggerContext context) => context.GiveExp(boxId, rate, arg3);
    }

    public class GiveGuildExp(int boxId, int type) : IAction {
        public void Execute(TriggerContext context) => context.GiveGuildExp(boxId, type);
    }

    public class GiveRewardContent(int rewardId) : IAction {
        public void Execute(TriggerContext context) => context.GiveRewardContent(rewardId);
    }

    public class GuideEvent(int eventId) : IAction {
        public void Execute(TriggerContext context) => context.GuideEvent(eventId);
    }

    public class GuildVsGameEndGame : IAction {
        public void Execute(TriggerContext context) => context.GuildVsGameEndGame();
    }

    public class GuildVsGameGiveContribution(int teamId, bool isWin, string desc) : IAction {
        public void Execute(TriggerContext context) => context.GuildVsGameGiveContribution(teamId, isWin, desc);
    }

    public class GuildVsGameGiveReward(string type, int teamId, bool isWin, string desc) : IAction {
        public void Execute(TriggerContext context) => context.GuildVsGameGiveReward(type, teamId, isWin, desc);
    }

    public class GuildVsGameLogResult(string desc) : IAction {
        public void Execute(TriggerContext context) => context.GuildVsGameLogResult(desc);
    }

    public class GuildVsGameLogWonByDefault(int teamId, string desc) : IAction {
        public void Execute(TriggerContext context) => context.GuildVsGameLogWonByDefault(teamId, desc);
    }

    public class GuildVsGameResult(string desc) : IAction {
        public void Execute(TriggerContext context) => context.GuildVsGameResult(desc);
    }

    public class GuildVsGameScoreByUser(int boxId, int score, string desc) : IAction {
        public void Execute(TriggerContext context) => context.GuildVsGameScoreByUser(boxId, score, desc);
    }

    public class HideGuideSummary(int entityId, int textId) : IAction {
        public void Execute(TriggerContext context) => context.HideGuideSummary(entityId, textId);
    }

    public class InitNpcRotation(int[] spawnIds) : IAction {
        public void Execute(TriggerContext context) => context.InitNpcRotation(spawnIds);
    }

    public class KickMusicAudience(int boxId, int portalId) : IAction {
        public void Execute(TriggerContext context) => context.KickMusicAudience(boxId, portalId);
    }

    public class LimitSpawnNpcCount(int limitCount, string desc) : IAction {
        public void Execute(TriggerContext context) => context.LimitSpawnNpcCount(limitCount, desc);
    }

    public class LockMyPc(bool isLock) : IAction {
        public void Execute(TriggerContext context) => context.LockMyPc(isLock);
    }

    public class MiniGameCameraDirection(int boxId, int cameraId) : IAction {
        public void Execute(TriggerContext context) => context.MiniGameCameraDirection(boxId, cameraId);
    }

    public class MiniGameGiveExp(int boxId, float expRate, bool isOutside) : IAction {
        public void Execute(TriggerContext context) => context.MiniGameGiveExp(boxId, expRate, isOutside);
    }

    public class MiniGameGiveReward(int winnerBoxId, string contentType, string gameName) : IAction {
        public void Execute(TriggerContext context) => context.MiniGameGiveReward(winnerBoxId, contentType, gameName);
    }

    public class MoveNpc(int spawnId, string patrolName) : IAction {
        public void Execute(TriggerContext context) => context.MoveNpc(spawnId, patrolName);
    }

    public class MoveNpcToPos(int spawnId, Vector3 pos, Vector3 rot) : IAction {
        public void Execute(TriggerContext context) => context.MoveNpcToPos(spawnId, pos, rot);
    }

    public class MoveRandomUser(int mapId, int portalId, int boxId, int count) : IAction {
        public void Execute(TriggerContext context) => context.MoveRandomUser(mapId, portalId, boxId, count);
    }

    public class MoveToPortal(int userTagId, int portalId, int boxId) : IAction {
        public void Execute(TriggerContext context) => context.MoveToPortal(userTagId, portalId, boxId);
    }

    public class MoveUser(int mapId, int portalId, int boxId) : IAction {
        public void Execute(TriggerContext context) => context.MoveUser(mapId, portalId, boxId);
    }

    public class MoveUserPath(string patrolName) : IAction {
        public void Execute(TriggerContext context) => context.MoveUserPath(patrolName);
    }

    public class MoveUserToBox(int boxId, int portalId) : IAction {
        public void Execute(TriggerContext context) => context.MoveUserToBox(boxId, portalId);
    }

    public class MoveUserToPos(Vector3 pos, Vector3 rot) : IAction {
        public void Execute(TriggerContext context) => context.MoveUserToPos(pos, rot);
    }

    public class Notice(int type, string script, bool arg3) : IAction {
        public void Execute(TriggerContext context) => context.Notice(type, script, arg3);
    }

    public class NpcRemoveAdditionalEffect(int spawnId, int additionalEffectId) : IAction {
        public void Execute(TriggerContext context) => context.NpcRemoveAdditionalEffect(spawnId, additionalEffectId);
    }

    public class NpcToPatrolInBox(int boxId, int npcId, string spawnId, string patrolName) : IAction {
        public void Execute(TriggerContext context) => context.NpcToPatrolInBox(boxId, npcId, spawnId, patrolName);
    }

    public class PatrolConditionUser(string patrolName, int patrolIndex, int additionalEffectId) : IAction {
        public void Execute(TriggerContext context) => context.PatrolConditionUser(patrolName, patrolIndex, additionalEffectId);
    }

    public class PlaySceneMovie(string fileName, int movieId, string skipType) : IAction {
        public void Execute(TriggerContext context) => context.PlaySceneMovie(fileName, movieId, skipType);
    }

    public class PlaySystemSoundByUserTag(int userTagId, string soundKey) : IAction {
        public void Execute(TriggerContext context) => context.PlaySystemSoundByUserTag(userTagId, soundKey);
    }

    public class PlaySystemSoundInBox(string sound, int[] boxIds) : IAction {
        public void Execute(TriggerContext context) => context.PlaySystemSoundInBox(sound, boxIds);
    }

    public class RandomAdditionalEffect(string target, int boxId, int spawnId, int targetCount, int tick, int waitTick, string targetEffect, int additionalEffectId) : IAction {
        public void Execute(TriggerContext context) => context.RandomAdditionalEffect(target, boxId, spawnId, targetCount, tick, waitTick, targetEffect, additionalEffectId);
    }

    public class RemoveBalloonTalk(int spawnId) : IAction {
        public void Execute(TriggerContext context) => context.RemoveBalloonTalk(spawnId);
    }

    public class RemoveBuff(int boxId, int skillId, bool isPlayer) : IAction {
        public void Execute(TriggerContext context) => context.RemoveBuff(boxId, skillId, isPlayer);
    }

    public class RemoveCinematicTalk : IAction {
        public void Execute(TriggerContext context) => context.RemoveCinematicTalk();
    }

    public class RemoveEffectNif(int spawnId) : IAction {
        public void Execute(TriggerContext context) => context.RemoveEffectNif(spawnId);
    }

    public class ResetCamera(float interpolationTime) : IAction {
        public void Execute(TriggerContext context) => context.ResetCamera(interpolationTime);
    }

    public class ResetTimer(string timerId) : IAction {
        public void Execute(TriggerContext context) => context.ResetTimer(timerId);
    }

    public class RoomExpire : IAction {
        public void Execute(TriggerContext context) => context.RoomExpire();
    }

    public class ScoreBoardCreate(string type, string title, int maxScore) : IAction {
        public void Execute(TriggerContext context) => context.ScoreBoardCreate(type, title, maxScore);
    }

    public class ScoreBoardRemove : IAction {
        public void Execute(TriggerContext context) => context.ScoreBoardRemove();
    }

    public class ScoreBoardSetScore(int score) : IAction {
        public void Execute(TriggerContext context) => context.ScoreBoardSetScore(score);
    }

    public class SelectCamera(int triggerId, bool enable) : IAction {
        public void Execute(TriggerContext context) => context.SelectCamera(triggerId, enable);
    }

    public class SelectCameraPath(int[] pathIds, bool returnView) : IAction {
        public void Execute(TriggerContext context) => context.SelectCameraPath(pathIds, returnView);
    }

    public class SetAchievement(int triggerId, string type, string achieve) : IAction {
        public void Execute(TriggerContext context) => context.SetAchievement(triggerId, type, achieve);
    }

    public class SetActor(int triggerId, bool visible, string initialSequence, bool arg4, bool arg5) : IAction {
        public void Execute(TriggerContext context) => context.SetActor(triggerId, visible, initialSequence, arg4, arg5);
    }

    public class SetAgent(int[] triggerIds, bool visible) : IAction {
        public void Execute(TriggerContext context) => context.SetAgent(triggerIds, visible);
    }

    public class SetAiExtraData(string key, int value, bool isModify, int boxId) : IAction {
        public void Execute(TriggerContext context) => context.SetAiExtraData(key, value, isModify, boxId);
    }

    public class SetAmbientLight(Vector3 primary, Vector3 secondary, Vector3 tertiary) : IAction {
        public void Execute(TriggerContext context) => context.SetAmbientLight(primary, secondary, tertiary);
    }

    public class SetBreakable(int[] triggerIds, bool enable) : IAction {
        public void Execute(TriggerContext context) => context.SetBreakable(triggerIds, enable);
    }

    public class SetCinematicIntro(string text) : IAction {
        public void Execute(TriggerContext context) => context.SetCinematicIntro(text);
    }

    public class SetCinematicUi(int type, string script, bool arg3) : IAction {
        public void Execute(TriggerContext context) => context.SetCinematicUi(type, script, arg3);
    }

    public class SetCube(int[] triggerIds, bool isVisible, int randomCount) : IAction {
        public void Execute(TriggerContext context) => context.SetCube(triggerIds, isVisible, randomCount);
    }

    public class SetDialogue(int type, int spawnId, string script, int time, int arg5, Align align) : IAction {
        public void Execute(TriggerContext context) => context.SetDialogue(type, spawnId, script, time, arg5, align);
    }

    public class SetDirectionalLight(Vector3 diffuseColor, Vector3 specularColor) : IAction {
        public void Execute(TriggerContext context) => context.SetDirectionalLight(diffuseColor, specularColor);
    }

    public class SetEffect(int[] triggerIds, bool visible, int startDelay, int interval) : IAction {
        public void Execute(TriggerContext context) => context.SetEffect(triggerIds, visible, startDelay, interval);
    }

    public class SetEventUiCountdown(string script, int[] roundCountdown, string[] boxIds) : IAction {
        public void Execute(TriggerContext context) => context.SetEventUiCountdown(script, roundCountdown, boxIds);
    }

    public class SetEventUiRound(int[] rounds, int vOffset, int arg3) : IAction {
        public void Execute(TriggerContext context) => context.SetEventUiRound(rounds, vOffset, arg3);
    }

    public class SetEventUiScript(BannerType type, string script, int duration, string[] boxIds) : IAction {
        public void Execute(TriggerContext context) => context.SetEventUiScript(type, script, duration, boxIds);
    }

    public class SetGravity(float gravity) : IAction {
        public void Execute(TriggerContext context) => context.SetGravity(gravity);
    }

    public class SetInteractObject(int[] triggerIds, int state, bool arg4, bool arg3) : IAction {
        public void Execute(TriggerContext context) => context.SetInteractObject(triggerIds, state, arg4, arg3);
    }

    public class SetLadder(int[] triggerIds, bool visible, bool enable, int fade) : IAction {
        public void Execute(TriggerContext context) => context.SetLadder(triggerIds, visible, enable, fade);
    }

    public class SetLocalCamera(int cameraId, bool enable) : IAction {
        public void Execute(TriggerContext context) => context.SetLocalCamera(cameraId, enable);
    }

    public class SetMesh(int[] triggerIds, bool visible, int startDelay, int interval, float fade, string desc) : IAction {
        public void Execute(TriggerContext context) => context.SetMesh(triggerIds, visible, startDelay, interval, fade, desc);
    }

    public class SetMeshAnimation(int[] triggerIds, bool visible, int startDelay, int interval) : IAction {
        public void Execute(TriggerContext context) => context.SetMeshAnimation(triggerIds, visible, startDelay, interval);
    }

    public class SetMiniGameAreaForHack(int boxId) : IAction {
        public void Execute(TriggerContext context) => context.SetMiniGameAreaForHack(boxId);
    }

    public class SetNpcDuelHpBar(bool isOpen, int spawnId, int durationTick, int npcHpStep) : IAction {
        public void Execute(TriggerContext context) => context.SetNpcDuelHpBar(isOpen, spawnId, durationTick, npcHpStep);
    }

    public class SetNpcEmotionLoop(int spawnId, string sequenceName, float duration) : IAction {
        public void Execute(TriggerContext context) => context.SetNpcEmotionLoop(spawnId, sequenceName, duration);
    }

    public class SetNpcEmotionSequence(int spawnId, string sequenceName, int durationTick) : IAction {
        public void Execute(TriggerContext context) => context.SetNpcEmotionSequence(spawnId, sequenceName, durationTick);
    }

    public class SetNpcRotation(int spawnId, float rotation) : IAction {
        public void Execute(TriggerContext context) => context.SetNpcRotation(spawnId, rotation);
    }

    public class SetOnetimeEffect(int id, bool enable, string path) : IAction {
        public void Execute(TriggerContext context) => context.SetOnetimeEffect(id, enable, path);
    }

    public class SetPcEmotionLoop(string sequenceName, float duration, bool loop) : IAction {
        public void Execute(TriggerContext context) => context.SetPcEmotionLoop(sequenceName, duration, loop);
    }

    public class SetPcEmotionSequence(string[] sequenceNames) : IAction {
        public void Execute(TriggerContext context) => context.SetPcEmotionSequence(sequenceNames);
    }

    public class SetPcRotation(Vector3 rotation) : IAction {
        public void Execute(TriggerContext context) => context.SetPcRotation(rotation);
    }

    public class SetPhotoStudio(bool isEnable) : IAction {
        public void Execute(TriggerContext context) => context.SetPhotoStudio(isEnable);
    }

    public class SetPortal(int portalId, bool visible, bool enable, bool minimapVisible, bool arg5) : IAction {
        public void Execute(TriggerContext context) => context.SetPortal(portalId, visible, enable, minimapVisible, arg5);
    }

    public class SetPvpZone(int boxId, int prepareTime, int matchTime, int additionalEffectId, int type, int[] boxIds) : IAction {
        public void Execute(TriggerContext context) => context.SetPvpZone(boxId, prepareTime, matchTime, additionalEffectId, type, boxIds);
    }

    public class SetQuestAccept(int questId) : IAction {
        public void Execute(TriggerContext context) => context.SetQuestAccept(questId);
    }

    public class SetQuestComplete(int questId) : IAction {
        public void Execute(TriggerContext context) => context.SetQuestComplete(questId);
    }

    public class SetRandomMesh(int[] triggerIds, bool visible, int startDelay, int interval, int fade) : IAction {
        public void Execute(TriggerContext context) => context.SetRandomMesh(triggerIds, visible, startDelay, interval, fade);
    }

    public class SetRope(int triggerId, bool visible, bool enable, int fade) : IAction {
        public void Execute(TriggerContext context) => context.SetRope(triggerId, visible, enable, fade);
    }

    public class SetSceneSkip(string state, string nextState) : IAction {
        public void Execute(TriggerContext context) => context.SetSceneSkip(state, nextState);
    }

    public class SetSkill(int[] triggerIds, bool enable) : IAction {
        public void Execute(TriggerContext context) => context.SetSkill(triggerIds, enable);
    }

    public class SetSkip(string state) : IAction {
        public void Execute(TriggerContext context) => context.SetSkip(state);
    }

    public class SetSound(int triggerId, bool enable) : IAction {
        public void Execute(TriggerContext context) => context.SetSound(triggerId, enable);
    }

    public class SetState(int id, string[] states, bool randomize) : IAction {
        public void Execute(TriggerContext context) => context.SetState(id, states, randomize);
    }

    public class SetTimeScale(bool enable, float startScale, float endScale, float duration, int interpolator) : IAction {
        public void Execute(TriggerContext context) => context.SetTimeScale(enable, startScale, endScale, duration, interpolator);
    }

    public class SetTimer(string timerId, int seconds, bool autoRemove, bool display, int vOffset, string type, string desc) : IAction {
        public void Execute(TriggerContext context) => context.SetTimer(timerId, seconds, autoRemove, display, vOffset, type, desc);
    }

    public class SetUserValue(int triggerId, string key, int value) : IAction {
        public void Execute(TriggerContext context) => context.SetUserValue(triggerId, key, value);
    }

    public class SetUserValueFromDungeonRewardCount(string key, int dungeonRewardId) : IAction {
        public void Execute(TriggerContext context) => context.SetUserValueFromDungeonRewardCount(key, dungeonRewardId);
    }

    public class SetUserValueFromGuildVsGameScore(int teamId, string key) : IAction {
        public void Execute(TriggerContext context) => context.SetUserValueFromGuildVsGameScore(teamId, key);
    }

    public class SetUserValueFromUserCount(int triggerBoxId, string key, int userTagId) : IAction {
        public void Execute(TriggerContext context) => context.SetUserValueFromUserCount(triggerBoxId, key, userTagId);
    }

    public class SetVisibleBreakableObject(int[] triggerIds, bool visible) : IAction {
        public void Execute(TriggerContext context) => context.SetVisibleBreakableObject(triggerIds, visible);
    }

    public class SetVisibleUi(string[] uiNames, bool visible) : IAction {
        public void Execute(TriggerContext context) => context.SetVisibleUi(uiNames, visible);
    }

    public class ShadowExpeditionCloseBossGauge : IAction {
        public void Execute(TriggerContext context) => context.ShadowExpeditionCloseBossGauge();
    }

    public class ShadowExpeditionOpenBossGauge(int maxGaugePoint, string title) : IAction {
        public void Execute(TriggerContext context) => context.ShadowExpeditionOpenBossGauge(maxGaugePoint, title);
    }

    public class ShowCaption(string type, string title, string desc, Align align, float offsetRateX, float offsetRateY, int duration, float scale) : IAction {
        public void Execute(TriggerContext context) => context.ShowCaption(type, title, desc, align, offsetRateX, offsetRateY, duration, scale);
    }

    public class ShowCountUi(string text, int stage, int count, int soundType) : IAction {
        public void Execute(TriggerContext context) => context.ShowCountUi(text, stage, count, soundType);
    }

    public class ShowEventResult(string type, string text, int duration, int userTagId, int triggerBoxId, bool isOutside) : IAction {
        public void Execute(TriggerContext context) => context.ShowEventResult(type, text, duration, userTagId, triggerBoxId, isOutside);
    }

    public class ShowGuideSummary(int entityId, int textId, int duration) : IAction {
        public void Execute(TriggerContext context) => context.ShowGuideSummary(entityId, textId, duration);
    }

    public class ShowRoundUi(int round, int duration, bool isFinalRound) : IAction {
        public void Execute(TriggerContext context) => context.ShowRoundUi(round, duration, isFinalRound);
    }

    public class SideNpcCutin(string illust, int duration) : IAction {
        public void Execute(TriggerContext context) => context.SideNpcCutin(illust, duration);
    }

    public class SideNpcMovie(string usm, int duration) : IAction {
        public void Execute(TriggerContext context) => context.SideNpcMovie(usm, duration);
    }

    public class SideNpcTalk(int npcId, string illust, int duration, string script, string voice) : IAction {
        public void Execute(TriggerContext context) => context.SideNpcTalk(npcId, illust, duration, script, voice);
    }

    public class SideNpcTalkBottom(int npcId, string illust, int duration, string script) : IAction {
        public void Execute(TriggerContext context) => context.SideNpcTalkBottom(npcId, illust, duration, script);
    }

    public class SightRange(bool enable, int range, int rangeZ, int border) : IAction {
        public void Execute(TriggerContext context) => context.SightRange(enable, range, rangeZ, border);
    }

    public class SpawnItemRange(int[] rangeIds, int randomPickCount) : IAction {
        public void Execute(TriggerContext context) => context.SpawnItemRange(rangeIds, randomPickCount);
    }

    public class SpawnMonster(int[] spawnIds, bool autoTarget, int delay) : IAction {
        public void Execute(TriggerContext context) => context.SpawnMonster(spawnIds, autoTarget, delay);
    }

    public class SpawnNpcRange(int[] rangeIds, bool isAutoTargeting, int randomPickCount, int score) : IAction {
        public void Execute(TriggerContext context) => context.SpawnNpcRange(rangeIds, isAutoTargeting, randomPickCount, score);
    }

    public class StartCombineSpawn(int[] groupId, bool isStart) : IAction {
        public void Execute(TriggerContext context) => context.StartCombineSpawn(groupId, isStart);
    }

    public class StartMiniGame(int boxId, int round, string gameName, bool isShowResultUi) : IAction {
        public void Execute(TriggerContext context) => context.StartMiniGame(boxId, round, gameName, isShowResultUi);
    }

    public class StartMiniGameRound(int boxId, int round) : IAction {
        public void Execute(TriggerContext context) => context.StartMiniGameRound(boxId, round);
    }

    public class StartTutorial : IAction {
        public void Execute(TriggerContext context) => context.StartTutorial();
    }

    public class TalkNpc(int spawnId) : IAction {
        public void Execute(TriggerContext context) => context.TalkNpc(spawnId);
    }

    public class UnsetMiniGameAreaForHack : IAction {
        public void Execute(TriggerContext context) => context.UnsetMiniGameAreaForHack();
    }

    public class UseState(int id, bool randomize) : IAction {
        public void Execute(TriggerContext context) => context.UseState(id, randomize);
    }

    public class UserTagSymbol(string symbol1, string symbol2) : IAction {
        public void Execute(TriggerContext context) => context.UserTagSymbol(symbol1, symbol2);
    }

    public class UserValueToNumberMesh(string key, int startMeshId, int digitCount) : IAction {
        public void Execute(TriggerContext context) => context.UserValueToNumberMesh(key, startMeshId, digitCount);
    }

    public class VisibleMyPc(bool isVisible) : IAction {
        public void Execute(TriggerContext context) => context.VisibleMyPc(isVisible);
    }

    public class SetWeather(Weather weatherType) : IAction {
        public void Execute(TriggerContext context) => context.SetWeather(weatherType);
    }

    public class WeddingBroken : IAction {
        public void Execute(TriggerContext context) => context.WeddingBroken();
    }

    public class WeddingMoveUser(string entryType, int mapId, int[] portalIds, int boxId) : IAction {
        public void Execute(TriggerContext context) => context.WeddingMoveUser(entryType, mapId, portalIds, boxId);
    }

    public class WeddingMutualAgree(string agreeType) : IAction {
        public void Execute(TriggerContext context) => context.WeddingMutualAgree(agreeType);
    }

    public class WeddingMutualCancel(string agreeType) : IAction {
        public void Execute(TriggerContext context) => context.WeddingMutualCancel(agreeType);
    }

    public class WeddingSetUserEmotion(string entryType, int id) : IAction {
        public void Execute(TriggerContext context) => context.WeddingSetUserEmotion(entryType, id);
    }

    public class WeddingSetUserLookAt(string entryType, string lookAtEntryType, bool immediate) : IAction {
        public void Execute(TriggerContext context) => context.WeddingSetUserLookAt(entryType, lookAtEntryType, immediate);
    }

    public class WeddingSetUserRotation(string entryType, Vector3 rotation, bool immediate) : IAction {
        public void Execute(TriggerContext context) => context.WeddingSetUserRotation(entryType, rotation, immediate);
    }

    public class WeddingUserToPatrol(string patrolName, string entryType, int patrolIndex) : IAction {
        public void Execute(TriggerContext context) => context.WeddingUserToPatrol(patrolName, entryType, patrolIndex);
    }

    public class WeddingVowComplete : IAction {
        public void Execute(TriggerContext context) => context.WeddingVowComplete();
    }

    public class WidgetAction(string type, string func, string widgetArg, string desc, int widgetArgNum) : IAction {
        public void Execute(TriggerContext context) => context.WidgetAction(type, func, widgetArg, desc, widgetArgNum);
    }

    public class WriteLog(string logName, string eventName, int triggerId, string subEvent, int level) : IAction {
        public void Execute(TriggerContext context) => context.WriteLog(logName, eventName, triggerId, subEvent, level);
    }
}
