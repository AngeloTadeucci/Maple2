using Maple2.Model.Game.Dungeon;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext {
    public void DungeonClear(string uiType) {
        DebugLog("[DungeonClear] uiType:{UiType}", uiType);
        if (Field is not DungeonFieldManager dungeonField) {
            return;
        }

        if (uiType == "None") {
            // Do not send dungeon clear UI
            return;
        }

        dungeonField.ChangeState(Maple2.Model.Enum.DungeonState.Clear);
    }

    public void DungeonClearRound(int round) {
        DebugLog("[DungeonClearRound] round:{Round}", round);

        foreach (FieldPlayer player in Field.Players.Values) {
            if (player.Session.Dungeon.UserRecord is null) {
                continue;
            }

            player.Session.Dungeon.UserRecord.Round = round;
        }
    }

    public void DungeonCloseTimer() {
        ErrorLog("[DungeonCloseTimer]");
    }

    public void DungeonDisableRanking() {
        ErrorLog("[DungeonDisableRanking]");
    }

    public void DungeonEnableGiveUp(bool enabled) {
        DebugLog("[DungeonEnableGiveUp] enabled:{Enabled}", enabled);
        Field.Broadcast(DungeonMissionPacket.SetAbandon(enabled));
    }

    public void DungeonFail() {
        ErrorLog("[DungeonFail]");
    }

    public void DungeonMissionComplete(string feature, int missionId) {
        DebugLog("[DungeonMissionComplete] missionId:{MissionId}, feature:{Feature}", missionId, feature);

        foreach (FieldPlayer player in Field.Players.Values) {
            if (player.Session.Dungeon.UserRecord is null) {
                continue;
            }
            if (!player.Session.Dungeon.UserRecord.Missions.TryGetValue(missionId, out DungeonMission? missionRecord)) {
                continue;
            }

            missionRecord.Complete();
            player.Session.Send(DungeonMissionPacket.Update(missionRecord));
        }
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
        DebugLog("[CheckDungeonLobbyUserCount]");
        if (Field is not DungeonFieldManager dungeonField) {
            return false;
        }

        return Field.Players.Values.Count >= dungeonField.Size;
    }

    public bool DungeonTimeout() {
        ErrorLog("[DungeonTimeout]");
        return false;
    }

    public bool IsDungeonRoom() {
        DebugLog("[IsDungeonRoom]");
        return Field is DungeonFieldManager;
    }

    public bool IsPlayingMapleSurvival() {
        ErrorLog("[IsPlayingMapleSurvival]");
        return false;
    }
    #endregion
}
