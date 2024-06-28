
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Scripting.Trigger;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext {
    public void EndMiniGame(int winnerBoxId, string gameName, bool isOnlyWinner) { }

    public void EndMiniGameRound(int winnerBoxId, float expRate, float meso, bool isOnlyWinner, bool isGainLoserBonus, string gameName) { }

    public void MiniGameCameraDirection(int boxId, int cameraId) { }

    public void MiniGameGiveExp(int boxId, float expRate, bool isOutSide) { }

    public void MiniGameGiveReward(int winnerBoxId, string contentType, string type) { }

    public void SetMiniGameAreaForHack(int boxId) { }

    public void StartMiniGame(int boxId, int round, string type, bool isShowResultUi) { }

    public void StartMiniGameRound(int boxId, int round) { }

    public void UnsetMiniGameAreaForHack() { }

    public void UseState(int id, bool randomize) {
        if (!Field.States.TryGetValue(id, out List<object>? states)) {
            return;
        }

        if (randomize) {
            // randomize order
            states = states.OrderBy(_ => Random.Shared.Next()).ToList();
        }

        // get first state and remove from list
        object state = states.First();
        states.RemoveAt(0);
        Field.States[id] = states;

        TriggerState? triggerState = CreateState(state);

        // They only have OnEnter() method
        triggerState?.OnEnter();
    }

    #region CathyMart
    public void AddEffectNif(int spawnPointId, string nifPath, bool isOutline, float scale, int rotateZ) { }

    public void RemoveEffectNif(int spawnPointId) { }
    #endregion

    #region HideAndSeek
    public void CreateFieldGame(FieldGame type, bool reset) { }

    public void FieldGameConstant(string key, string value, string feature, Locale locale) { }

    public void FieldGameMessage(int custom, string type, bool arg1, string script, int duration) { }
    #endregion

    #region Conditions
    public int BonusGameReward(int boxId) {
        return -1;
    }
    #endregion
}
