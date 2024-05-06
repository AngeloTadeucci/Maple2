using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class ExperienceManager(GameSession session, Lua.Lua lua) {
    private long Exp {
        get => session.Player.Value.Character.Exp;
        set => session.Player.Value.Character.Exp = value;
    }
    private long RestExp {
        get => session.Player.Value.Character.RestExp;
        set => session.Player.Value.Character.RestExp = value;
    }

    private short Level {
        get => session.Player.Value.Character.Level;
        set => session.Player.Value.Character.Level = value;
    }

    //public long PrestigeExp => session.Player.Value.Account.PrestigeExp;
    private int ChainKillCount { get; set; }

    public void ResetChainKill() => ChainKillCount = 0;

    public void OnKill(IActor npc) {
        if (npc is not FieldNpc) {
            return;
        }
        FieldNpc fieldNpc = (npc as FieldNpc)!;
        // TODO: Check if there are level requirements for Chain Kill Count to count ?
        ChainKillCount++;
        float expRate = lua.CalcKillCountBonusExpRate(ChainKillCount);

        // TODO: Using table ID 2. Need confirmation if particular maps (or dungeons) use a different table
        long expGained = fieldNpc.Value.Metadata.Basic.CustomExp;
        if (fieldNpc.Value.Metadata.Basic.CustomExp < 0) {
            if (!session.TableMetadata.ExpTable.ExpBase.TryGetValue(2, fieldNpc.Value.Metadata.Basic.Level, out expGained)) {
                return;
            }
        }
        expGained += GetRestExp((long) (expGained * expRate));
        LevelUp();
        session.Send(ExperienceUpPacket.Add(expGained, Exp, RestExp, ExpMessageCode.s_msg_take_exp, npc.ObjectId));
    }

    private long GetRestExp(long expGained) {
        long addedRestExp = Math.Min(RestExp, (long) (expGained * (Constant.RestExpAcquireRate / 10000.0f))); // convert int to a percentage
        RestExp = Math.Max(0, RestExp - addedRestExp);
        Exp += expGained;
        return addedRestExp;
    }

    public void AddExp(ExpMessageCode message, long expGained) {
        expGained += GetRestExp(expGained);
        if (expGained <= 0) {
            return;
        }
        LevelUp();
        session.Send(ExperienceUpPacket.Add(expGained, Exp, RestExp, message));
    }

    public void AddExp(ExpType expType, float modifier = 1f, long additionalExp = 0) {
        if (session.Field == null
            || !session.TableMetadata.CommonExpTable.Entries.TryGetValue(expType, out CommonExpTable.Entry? entry)
            || !session.TableMetadata.ExpTable.ExpBase.TryGetValue(entry.ExpTableId, out IReadOnlyDictionary<int, long>? expBase)) {
            return;
        }

        long expValue = 0;
        switch (expType) {
            case ExpType.fishing:
            case ExpType.musicMastery1:
            case ExpType.musicMastery2:
            case ExpType.musicMastery3:
            case ExpType.musicMastery4:
            case ExpType.manufacturing:
            case ExpType.gathering:
            case ExpType.arcade:
            case ExpType.expDrop:
                if (!expBase.TryGetValue(session.Player.Value.Character.Level, out expValue)) {
                    return;
                }
                break;
            case ExpType.taxi:
            case ExpType.mapCommon:
            case ExpType.mapHidden:
            case ExpType.telescope:
                if (!expBase.TryGetValue(session.Field.Metadata.Drop.Level, out expValue)) {
                    return;
                }
                break;
            default:
                Log.Logger.Warning("Unhandled ExpType: {ExpType}", expType);
                return;
        }

        ExpMessageCode message = expType switch {
            ExpType.mapCommon or ExpType.mapHidden => ExpMessageCode.s_msg_take_map_exp,
            ExpType.taxi => ExpMessageCode.s_msg_take_taxi_exp,
            ExpType.telescope => ExpMessageCode.s_msg_take_telescope_exp,
            ExpType.rareChestFirst => ExpMessageCode.s_msg_take_normal_rare_first_exp,
            ExpType.rareChest => ExpMessageCode.s_msg_take_normal_rare_exp,
            ExpType.normalChest => ExpMessageCode.s_msg_take_normal_chest_exp,
            ExpType.musicMastery1 or ExpType.musicMastery2 or ExpType.musicMastery3 or ExpType.musicMastery4 => ExpMessageCode.s_msg_take_play_instrument_exp,
            ExpType.arcade => ExpMessageCode.s_msg_take_arcade_exp,
            ExpType.fishing => ExpMessageCode.s_msg_take_fishing_exp,
            _ => ExpMessageCode.s_msg_take_exp,
        };

        AddExp(message, (long) ((expValue * modifier) * entry.Factor) + additionalExp);
    }

    public bool LevelUp() {
        int startLevel = Level;
        for (int level = startLevel; level < Constant.characterMaxLevel; level++) {
            if (!session.TableMetadata.ExpTable.NextExp.TryGetValue(level, out long expToNextLevel) || expToNextLevel > Exp) {
                break;
            }

            Exp -= expToNextLevel;
            Level++;
        }
        if (Level > startLevel) {
            session.Field?.Broadcast(LevelUpPacket.LevelUp(session.Player));
            session.ConditionUpdate(ConditionType.level_up, codeLong: (int) session.Player.Value.Character.Job.Code(), targetLong: session.Player.Value.Character.Level);
            session.ConditionUpdate(ConditionType.level, codeLong: session.Player.Value.Character.Level);

            session.PlayerInfo.SendUpdate(new PlayerUpdateRequest {
                AccountId = session.AccountId,
                CharacterId = session.CharacterId,
                Level = session.Player.Value.Character.Level,
                Async = true,
            });
        }
        return startLevel != Level;
    }
}
