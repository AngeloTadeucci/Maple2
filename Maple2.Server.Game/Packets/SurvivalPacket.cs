using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class SurvivalPacket {
    private enum Command : byte {
        JoinSolo = 0,
        WithdrawSolo = 1,
        MatchFound = 2,
        ClearMatchedQueue = 3,
        Results = 17,
        LastStanding = 20,
        Unknown22 = 22,
        UpdateStats = 23,
        NewSeason = 24,
        KillNotices = 25,
        UpdateKills = 26,
        SessionStats = 27,
        Poisoned = 29,
        LoadMedals = 30,
        ClaimRewards = 35,
    }

    public static ByteWriter UpdateStats(Account account, long expGained = 0) {
        var pWriter = Packet.Of(SendOp.Survival);
        pWriter.Write<Command>(Command.UpdateStats);
        pWriter.WriteLong(account.Id);
        pWriter.WriteInt();
        pWriter.WriteBool(account.ActiveGoldPass);
        pWriter.WriteLong(account.SurvivalExp);
        pWriter.WriteInt(account.SurvivalLevel);
        pWriter.WriteInt(account.SurvivalSilverLevelRewardClaimed);
        pWriter.WriteInt(account.SurvivalGoldLevelRewardClaimed);
        pWriter.WriteLong(expGained);

        return pWriter;
    }

    public static ByteWriter LoadMedals(IDictionary<MedalType, Dictionary<int, Medal>> inventory, IDictionary<MedalType, Medal> equips) {
        var pWriter = Packet.Of(SendOp.Survival);
        pWriter.Write<Command>(Command.LoadMedals);
        pWriter.WriteByte((byte) inventory.Keys.Count);
        foreach ((MedalType type, Dictionary<int, Medal> medals) in inventory) {
            pWriter.WriteInt(equips[type].Id);
            pWriter.WriteInt(medals.Count);
            foreach (Medal medal in medals.Values) {
                pWriter.WriteInt(medal.Id);
                pWriter.WriteLong(long.MaxValue);
            }
        }

        return pWriter;
    }
}
