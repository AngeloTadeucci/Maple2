using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext {
    public void DungeonClear(string uiType) {
        ErrorLog("[DungeonClear] uiType:{UiType}", uiType);
    }

    public void DungeonClearRound(int round) {
        ErrorLog("[DungeonClearRound] round:{Round}", round);
    }

    public void DungeonCloseTimer() {
        ErrorLog("[DungeonCloseTimer]");
    }

    public void DungeonDisableRanking() {
        ErrorLog("[DungeonDisableRanking]");
    }

    public void DungeonEnableGiveUp(bool enabled) {
        ErrorLog("[DungeonEnableGiveUp] enabled:{Enabled}", enabled);
    }

    public void DungeonFail() {
        ErrorLog("[DungeonFail]");
    }

    public void DungeonMissionComplete(string feature, int missionId) {
        ErrorLog("[DungeonMissionComplete] missionId:{MissionId}, feature:{Feature}", missionId, feature);
    }

    public void DungeonMoveLapTimeToNow(int id) {
        ErrorLog("[DungeonMoveLapTimeToNow] id:{Id}", id);
    }

    public void DungeonResetTime(int seconds) {
        ErrorLog("[DungeonResetTime] seconds:{Seconds}", seconds);
    }

    public void DungeonSetEndTime() {
        ErrorLog("[DungeonSetEndTime]");
    }

    public void DungeonSetLapTime(int id, int lapTime) {
        ErrorLog("[DungeonSetLapTime] id:{Id}, lapTime:{LapTime}", id, lapTime);
    }

    public void DungeonStopTimer() {
        ErrorLog("[DungeonStopTimer]");
    }

    public void RandomAdditionalEffect(
        string target,
        int boxId,
        int spawnId,
        int targetCount,
        int tick,
        int waitTick,
        string targetEffect,
        int additionalEffectId
    ) {
        ErrorLog("[RandomAdditionalEffect] target:{Target}, boxId:{BoxId}, spawnId:{SpawnId}, targetCount:{Count}, targetEffect:{Effect}, additionalEffectId:{Id}",
            target, boxId, spawnId, targetCount, targetEffect, additionalEffectId);
    }

    public void SetDungeonVariable(int varId, int value) {
        ErrorLog("[SetDungeonVariable] varId:{VarId}, value:{Value}", varId, value);
    }

    public void SetUserValueFromDungeonRewardCount(string key, int dungeonRewardId) {
        ErrorLog("[SetUserValueFromDungeonRewardCount] key:{Key}, dungeonRewardId:{RewardId}", key, dungeonRewardId);
    }

    public void StartTutorial() {
        ErrorLog("[StartTutorial]");
    }

    #region DarkStream
    public void DarkStreamSpawnMonster(int[] spawnIds, int score) {
        ErrorLog("[DarkStreamSpawnMonster]");
    }

    public void DarkStreamStartGame(int round) {
        ErrorLog("[DarkStreamStartGame]");
    }

    public void DarkStreamStartRound(int round, int uiDuration, int damagePenalty) {
        ErrorLog("[DarkStreamStartRound]");
    }

    public void DarkStreamClearRound(int round) {
        ErrorLog("[DarkStreamClearRound]");
    }
    #endregion

    #region ShadowExpedition
    public void ShadowExpeditionOpenBossGauge(int maxGaugePoint, string title) {
        ErrorLog("[ShadowExpeditionOpenBossGauge]");
    }

    public void ShadowExpeditionCloseBossGauge() {
        ErrorLog("[ShadowExpeditionCloseBossGauge]");
    }
    #endregion

    #region Conditions
    public bool CheckDungeonLobbyUserCount() {
        if (Field is not DungeonFieldManager dungeonField) {
            return false;
        }

        if (dungeonField.Party is null) {
            return Field.Players.Values.Count >= 1;
        }

        DebugLog("[CheckDungeonLobbyUserCount]");
        return dungeonField.Party.Members.Count == Field.Players.Values.Count;
    }

    public bool DungeonTimeout() {
        ErrorLog("[DungeonTimeout]");
        return false;
    }

    public bool IsDungeonRoom() {
        if (Field is not DungeonFieldManager dungeonField) {
            return false;
        }

        if (dungeonField.Party is not null) {
            return true;
        }
        DebugLog("[IsDungeonRoom]");
        return false;
    }

    public bool IsPlayingMapleSurvival() {
        ErrorLog("[IsPlayingMapleSurvival]");
        return false;
    }
    #endregion
}
