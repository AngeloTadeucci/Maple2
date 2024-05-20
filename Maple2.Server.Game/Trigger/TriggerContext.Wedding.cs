using System.Numerics;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext {
    public void WeddingBroken() { }

    public void WeddingMoveUser(string type, int mapId, int[] portalIds, int boxId) { }

    public void WeddingMutualAgree(string type) { }

    public void WeddingMutualCancel(string type) { }

    public void WeddingSetUserEmotion(string type, int id) { }

    public void WeddingSetUserLookAt(string type, string lookAtType, bool immediate) { }

    public void WeddingSetUserRotation(string type, Vector3 rotation, bool immediate) { }

    public void WeddingUserToPatrol(string patrolName, string type, int patrolIndex) { }

    public void WeddingVowComplete() { }

    #region Conditions
    public bool WeddingEntryInField(string entryType, bool isInField) {
        return false;
    }

    public string WeddingHallState(bool success) {
        return string.Empty;
    }

    public bool WeddingMutualAgreeResult(string type) {
        return false;
    }
    #endregion
}
