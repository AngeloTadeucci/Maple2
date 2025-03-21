
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Game.Dungeon;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Scripting.Trigger;
using Locale = Maple2.Server.Game.Scripting.Trigger.Locale;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext {
    public void EndMiniGame(int winnerBoxId, string gameName, bool isOnlyWinner) {
        IEnumerable<FieldPlayer> players = PlayersInBox(winnerBoxId);
        foreach (FieldPlayer player in players) {
            if (player.Session.MiniGameRecord == null) {
                continue;
            }

            if (player.Session.MiniGameRecord.ShowResult) {
                player.Session.Send(DungeonRewardPacket.MiniGame(player.Session.MiniGameRecord));
            }
            player.Session.MiniGameRecord = null;
        }

        if (isOnlyWinner) {
            return;
        }

        IEnumerable<FieldPlayer> losingPlayers = PlayersNotInBox(winnerBoxId);
        foreach (FieldPlayer player in losingPlayers) {
            if (player.Session.MiniGameRecord != null) {
                if (player.Session.MiniGameRecord.ShowResult) {
                    player.Session.Send(DungeonRewardPacket.MiniGame(player.Session.MiniGameRecord));
                }
                player.Session.MiniGameRecord = null;
            }
        }
    }

    public void EndMiniGameRound(int winnerBoxId, float expRate, float meso, bool isOnlyWinner, bool isGainLoserBonus, string gameName) {
        ExpType expType = gameName switch {
            "UserMassive_Crazyrunner" or "UserMassive_Springbeach" or "UserMassive_Escape" => ExpType.userMiniGame,
            _ => ExpType.miniGame,
        };
        IEnumerable<FieldPlayer> players = PlayersInBox(winnerBoxId);
        foreach (FieldPlayer player in players) {
            MiniGameUserRecord? record = player.Session.MiniGameRecord;
            if (record == null) {
                continue;
            }

            long totalExp = player.Session.Exp.AddExp(expType, expRate);
            record.Rewards[DungeonRewardType.Exp] += (int) totalExp;

            if (meso > 0.0f) {
                player.Session.Currency.Meso += (long) meso;
                record.Rewards[DungeonRewardType.Meso] += (int) meso;
            }
            record.ClearedRounds++;
        }
        if (isOnlyWinner) {
            return;
        }

        IEnumerable<FieldPlayer> losingPlayers = PlayersNotInBox(winnerBoxId);
        foreach (FieldPlayer player in losingPlayers) {
            if (player.Session.MiniGameRecord != null) {
                if (isGainLoserBonus) {
                    long totalExp = player.Session.Exp.AddExp(ExpType.miniGame, expRate);
                    player.Session.MiniGameRecord.Rewards[DungeonRewardType.Exp] += (int) totalExp;
                }
                if (player.Session.MiniGameRecord.ShowResult) {
                    player.Session.Send(DungeonRewardPacket.MiniGame(player.Session.MiniGameRecord));
                }
                player.Session.MiniGameRecord = null;
            }
        }
    }

    public void MiniGameCameraDirection(int boxId, int cameraId) {
        IEnumerable<FieldPlayer> players = PlayersInBox(boxId);
        foreach (FieldPlayer player in players) {
            player.Session.Send(CameraPacket.Local(cameraId, true));
        }
    }

    public void MiniGameGiveExp(int boxId, float expRate, bool isOutSide) {
        IEnumerable<FieldPlayer> players = PlayersInBox(boxId);
        foreach (FieldPlayer player in players) {
            if (player.Session.MiniGameRecord == null) {
                continue;
            }

            long totalExp = player.Session.Exp.AddExp(ExpType.miniGame, expRate);
            player.Session.MiniGameRecord.Rewards[DungeonRewardType.Exp] += (int) totalExp;
        }
    }

    public void MiniGameGiveReward(int winnerBoxId, string contentType, string type) {
        if (!Constant.ContentRewards.TryGetValue(contentType, out int rewardId)) {
            WarnLog("MiniGameGiveReward: {Type} not found", type);
            return;
        }
        IEnumerable<FieldPlayer> players = PlayersInBox(winnerBoxId);
        foreach (FieldPlayer player in players) {
            if (player.Session.MiniGameRecord == null) {
                continue;
            }
            RewardRecord rewardRecord = player.Session.GetRewardContent(rewardId);
            player.Session.MiniGameRecord.Add(rewardRecord);
        }
    }

    public void SetMiniGameAreaForHack(int boxId) { }

    public void StartMiniGame(int boxId, int round, string type, bool isShowResultUi) {
        IEnumerable<FieldPlayer> players = PlayersInBox(boxId);

        foreach (FieldPlayer player in players) {
            player.Session.MiniGameRecord = new MiniGameUserRecord(player.Session.CharacterId) {
                TotalRounds = round,
                ShowResult = isShowResultUi,
                MinRound = 1,
            };
        }
    }

    public void StartMiniGameRound(int boxId, int round) {
        IEnumerable<FieldPlayer> players = PlayersInBox(boxId);

        foreach (FieldPlayer player in players) {
            if (player.Session.MiniGameRecord == null) {
                continue;
            }
            if (player.Session.MiniGameRecord.TotalRounds == player.Session.MiniGameRecord.MinRound) {
                return;
            }
            player.Session.Send(MassiveEventPacket.Round(round, player.Session.MiniGameRecord.TotalRounds, player.Session.MiniGameRecord.MinRound));
        }
    }

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
