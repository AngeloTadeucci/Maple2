﻿using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.LuaFunctions;
using Maple2.Server.Core.Config;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class ExperienceManager {
    private readonly GameSession session;
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

    public int PrestigeLevel {
        get => session.Player.Value.Account.PrestigeLevel;
        set => session.Player.Value.Account.PrestigeLevel = value;
    }

    public int PrestigeLevelsGained {
        get => session.Player.Value.Account.PrestigeLevelsGained;
        set => session.Player.Value.Account.PrestigeLevelsGained = value;
    }

    public long PrestigeExp {
        get => session.Player.Value.Account.PrestigeExp;
        set => session.Player.Value.Account.PrestigeExp = value;
    }

    public long PrestigeCurrentExp {
        get => session.Player.Value.Account.PrestigeCurrentExp;
        set => session.Player.Value.Account.PrestigeCurrentExp = value;
    }

    public IList<PrestigeMission> PrestigeMissions => session.Player.Value.Account.PrestigeMissions;
    public IList<int> PrestigeRewardsClaimed => session.Player.Value.Account.PrestigeRewardsClaimed;

    private int ChainKillCount { get; set; }

    public ExperienceManager(GameSession session) {
        this.session = session;
        Init();
    }

    private void Init() {
        if (session.Player.Value.Account.PrestigeMissions.Count == 0) {
            foreach ((int missionId, PrestigeMissionMetadata _) in session.TableMetadata.PrestigeMissionTable.Entries) {
                session.Player.Value.Account.PrestigeMissions.Add(new PrestigeMission(missionId));
            }
        }
    }

    public void ResetChainKill() => ChainKillCount = 0;

    public void OnKill(IActor npc) {
        if (npc is not FieldNpc) {
            return;
        }
        FieldNpc fieldNpc = (npc as FieldNpc)!;
        // TODO: Check if there are level requirements for Chain Kill Count to count ?
        ChainKillCount++;
        float expRate = Lua.CalcKillCountBonusExpRate(ChainKillCount);

        // TODO: Using table ID 2. Need confirmation if particular maps (or dungeons) use a different table
        long expGained = fieldNpc.Value.Metadata.Basic.CustomExp;
        if (fieldNpc.Value.Metadata.Basic.CustomExp < 0) {
            if (!session.TableMetadata.ExpTable.ExpBase.TryGetValue(2, fieldNpc.Value.Metadata.Basic.Level, out expGained)) {
                return;
            }
        }
        float mult = ConfigProvider.Settings.ExpMultiplier(ExpType.monster);
        long scaledBase = ScaleExp(expGained, mult);
        expGained = scaledBase + GetRestExp((long) (scaledBase * expRate));
        LevelUp();
        session.Send(ExperienceUpPacket.Add(expGained, Exp, RestExp, ExpMessageCode.s_msg_take_exp, npc.ObjectId));
    }

    private long GetRestExp(long expGained) {
        long addedRestExp = Math.Min(RestExp, (long) (expGained * (Constant.RestExpAcquireRate / 10000.0f))); // convert int to a percentage
        RestExp = Math.Max(0, RestExp - addedRestExp);
        Exp += expGained;
        return addedRestExp;
    }

    // Treats the provided amount as already scaled (final) EXP.
    public long AddExp(long expGained, ExpMessageCode message = ExpMessageCode.s_msg_take_exp) {
        if (expGained <= 0) {
            return 0;
        }
        return AddScaledExp(expGained, message);
    }

    public long AddExp(ExpType expType, float modifier = 1f, long additionalExp = 0) {
        if (session.Field is null) return 0;
        if (!session.TableMetadata.CommonExpTable.Entries.TryGetValue(expType, out CommonExpTable.Entry? entry)
            || !session.TableMetadata.ExpTable.ExpBase.TryGetValue(entry.ExpTableId, out IReadOnlyDictionary<int, long>? expBase)) {
            return 0;
        }

        long expValue;
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
            case ExpType.miniGame:
            case ExpType.userMiniGame:
                if (!expBase.TryGetValue(Level, out expValue)) {
                    return 0;
                }
                break;
            case ExpType.taxi:
            case ExpType.mapCommon:
            case ExpType.mapHidden:
            case ExpType.telescope:
            case ExpType.rareChestFirst:
                int fieldLevel = session.Field.Metadata.Drop.Level;
                int correctedLevel = fieldLevel > Level ? Level : fieldLevel;
                if (!expBase.TryGetValue(correctedLevel, out expValue)) {
                    return 0;
                }
                break;
            default:
                Log.Logger.Warning("Unhandled ExpType: {ExpType}", expType);
                return 0;
        }

        long baseExp = (long) ((expValue * modifier) * entry.Factor);
        float mult = ConfigProvider.Settings.ExpMultiplier(expType);
        long scaled = ScaleExp(baseExp, mult);
        return AddScaledExp(scaled + additionalExp, expType.Message());
    }

    // Applies the appropriate multiplier for the given ExpType to a provided base amount (unscaled)
    public long AddBaseExp(long baseAmount, ExpType expType, ExpMessageCode? message = null) {
        if (baseAmount <= 0) return 0;
        float mult = ConfigProvider.Settings.ExpMultiplier(expType);
        long scaled = ScaleExp(baseAmount, mult);
        return AddScaledExp(scaled, message ?? expType.Message());
    }

    public void AddStaticExp(long amount) {
        if (amount <= 0) {
            return;
        }
        float mult = ConfigProvider.Settings.ExpMultiplier(ExpType.none);
        long scaled = ScaleExp(amount, mult);
        Exp += scaled;
        session.Send(ExperienceUpPacket.Add(scaled, Exp, RestExp, ExpMessageCode.s_msg_take_exp));
        session.ConditionUpdate(ConditionType.exp, counter: scaled);
        LevelUp();
    }

    private static long ScaleExp(long amount, float multiplier) {
        if (amount <= 0) return amount;
        double scaled = Math.Round(amount * multiplier);
        if (scaled <= 0) return 0;
        return scaled > long.MaxValue ? long.MaxValue : (long) scaled;
    }

    private long AddScaledExp(long scaledAmount, ExpMessageCode message) {
        long total = scaledAmount + GetRestExp(scaledAmount);
        LevelUp();
        AddPrestigeExp(message.Type());
        session.Send(ExperienceUpPacket.Add(total, Exp, RestExp, message));
        session.ConditionUpdate(ConditionType.exp, counter: total);
        return total;
    }

    public void AddMobExp(int moblevel, float modifier = 1f, long additionalExp = 0) {
        if (!session.TableMetadata.ExpTable.ExpBase.TryGetValue(2, out IReadOnlyDictionary<int, long>? expBase)) {
            return;
        }

        if (!expBase.TryGetValue(moblevel, out long expValue)) {
            return;
        }

        long baseExp = (long) (expValue * modifier) + additionalExp;
        AddBaseExp(baseExp, ExpType.monster);
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
            session.Player.Flag |= PlayerObjectFlag.Level;
            session.Dungeon.UpdateDungeonEnterLimit();
            session.Field?.Broadcast(LevelUpPacket.LevelUp(session.Player));
            session.ConditionUpdate(ConditionType.level_up, codeLong: (int) session.Player.Value.Character.Job.Code(), targetLong: Level);
            session.ConditionUpdate(ConditionType.level, targetLong: Level);
            session.Config.UpdateDeathPenalty(0);
            session.Stats.Refresh();

            session.PlayerInfo.SendUpdate(new PlayerUpdateRequest {
                AccountId = session.AccountId,
                CharacterId = session.CharacterId,
                Level = Level,
                Async = true,
            });
        }
        return startLevel != Level;
    }

    private void AddPrestigeExp(ExpType expType) {
        if (Level < Constant.AdventureLevelStartLevel) {
            return;
        }

        if (!session.ServerTableMetadata.PrestigeExpTable.Entries.TryGetValue(expType, out long amount)) {
            return;
        }

        if (PrestigeCurrentExp - PrestigeExp + (PrestigeLevelsGained * Constant.AdventureLevelLvUpExp) >= Constant.AdventureLevelLvUpExp) {
            amount = (long) (amount * Constant.AdventureLevelFactor);
        }

        PrestigeCurrentExp = Math.Min(amount + PrestigeCurrentExp, long.MaxValue);

        int startLevel = PrestigeLevel;
        for (int level = startLevel; level < Constant.AdventureLevelLimit; level++) {
            if (Constant.AdventureLevelLvUpExp > PrestigeCurrentExp) {
                break;
            }

            PrestigeCurrentExp -= Constant.AdventureLevelLvUpExp;
            PrestigeLevel++;
        }
        session.Send(PrestigePacket.AddExp(PrestigeCurrentExp, amount));
        if (PrestigeLevel > startLevel) {
            PrestigeLevelUp(PrestigeLevel - startLevel);
        }
    }

    public void PrestigeLevelUp(int amount = 1) {
        PrestigeLevel = Math.Clamp(PrestigeLevel + amount, amount, Constant.AdventureLevelLimit);
        PrestigeLevelsGained += amount;
        session.ConditionUpdate(ConditionType.adventure_level, counter: amount);
        session.ConditionUpdate(ConditionType.adventure_level_up, counter: amount);
        foreach (PrestigeMission mission in PrestigeMissions) {
            mission.GainedLevels += amount;
        }

        for (int i = 0; i < amount; i++) {
            Item? item = session.Field?.ItemDrop.CreateItem(Constant.AdventureLevelLvUpRewardItem);
            if (item == null) {
                break;
            }

            if (!session.Item.Inventory.Add(item, true)) {
                session.Item.MailItem(item);
            }
        }

        session.Send(PrestigePacket.LevelUp(session.Player.ObjectId, PrestigeLevel));
        session.Send(PrestigePacket.LoadMissions(session.Player.Value.Account));
        CheckPrestigeReward();
    }

    private void CheckPrestigeReward() {
        if (!session.TableMetadata.PrestigeLevelRewardTable.Entries.TryGetValue(PrestigeLevel, out PrestigeLevelRewardMetadata? metadata)) {
            return;
        }

        if (metadata.Type != PrestigeAwardType.statPoint) {
            return;
        }

        session.Config.AddStatPoint(AttributePointSource.Prestige, metadata.Value);
    }
}
