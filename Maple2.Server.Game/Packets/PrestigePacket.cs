using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class PrestigePacket {
    private enum Command : byte {
        Load = 0,
        AddExp = 1,
        LevelUp = 2,
        ClaimReward = 4,
        UpdateMissions = 6,
        LoadMissions = 7,
    }

    public static ByteWriter Load(Account account) {
        var pWriter = Packet.Of(SendOp.Prestige);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteLong(account.PrestigeCurrentExp);
        pWriter.WriteInt(account.PrestigeLevel - account.PrestigeLevelsGained);
        pWriter.WriteLong(account.PrestigeExp);
        pWriter.WriteInt(account.PrestigeRewardsClaimed.Count);
        foreach (int level in account.PrestigeRewardsClaimed) {
            pWriter.WriteInt(level);
        }

        return pWriter;
    }

    public static ByteWriter AddExp(long currentExp, long gainedExp) {
        var pWriter = Packet.Of(SendOp.Prestige);
        pWriter.Write<Command>(Command.AddExp);
        pWriter.WriteLong(currentExp);
        pWriter.WriteLong(gainedExp);

        return pWriter;
    }

    public static ByteWriter LevelUp(int playerObjectId, int level) {
        var pWriter = Packet.Of(SendOp.Prestige);
        pWriter.Write<Command>(Command.LevelUp);
        pWriter.WriteInt(playerObjectId);
        pWriter.WriteInt(level);

        return pWriter;
    }

    public static ByteWriter ClaimReward(int level) {
        var pWriter = Packet.Of(SendOp.Prestige);
        pWriter.Write<Command>(Command.ClaimReward);
        pWriter.WriteByte(1);
        pWriter.WriteInt(1); // count
        pWriter.WriteInt(level);

        return pWriter;
    }

    public static ByteWriter UpdateMissions(Account account) {
        var pWriter = Packet.Of(SendOp.Prestige);
        pWriter.Write<Command>(Command.UpdateMissions);
        pWriter.WriteBool(true);
        pWriter.WriteInt(account.PrestigeMissions.Count);
        foreach (PrestigeMission mission in account.PrestigeMissions) {
            pWriter.WriteClass<PrestigeMission>(mission);
        }

        return pWriter;
    }

    public static ByteWriter LoadMissions(Account account) {
        var pWriter = Packet.Of(SendOp.Prestige);
        pWriter.Write<Command>(Command.LoadMissions);
        pWriter.WriteInt(account.PrestigeMissions.Count);
        foreach (PrestigeMission mission in account.PrestigeMissions) {
            pWriter.WriteClass<PrestigeMission>(mission);
        }

        return pWriter;
    }
}
