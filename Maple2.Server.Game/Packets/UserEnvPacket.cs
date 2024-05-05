using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class UserEnvPacket {
    private enum Command : byte {
        AddTitle = 0,
        UpdateTitles = 1,
        LoadTitles = 2,
        ItemCollects = 3,
        InteractedObjects = 4,
        GatheringCounts = 8,
        MasteryRewardsClaimed = 9,
    }

    public static ByteWriter AddTitle(int titleId) {
        var pWriter = Packet.Of(SendOp.UserEnv);
        pWriter.Write<Command>(Command.AddTitle);
        pWriter.WriteInt(titleId);
        return pWriter;
    }

    public static ByteWriter UpdateTitle(int objectId, int titleId) {
        var pWriter = Packet.Of(SendOp.UserEnv);
        pWriter.Write<Command>(Command.UpdateTitles);
        pWriter.WriteInt(objectId);
        pWriter.WriteInt(titleId);
        return pWriter;
    }

    public static ByteWriter LoadTitles(ISet<int> titles) {
        var pWriter = Packet.Of(SendOp.UserEnv);
        pWriter.Write<Command>(Command.LoadTitles);
        pWriter.WriteInt(titles.Count);
        foreach (int title in titles) {
            pWriter.WriteInt(title);
        }

        return pWriter;
    }

    public static ByteWriter ItemCollects(IDictionary<int, byte> itemCollects) {
        var pWriter = Packet.Of(SendOp.UserEnv);
        pWriter.Write<Command>(Command.ItemCollects);
        pWriter.WriteInt(itemCollects.Count);
        foreach ((int itemId, byte quantity) in itemCollects) {
            pWriter.WriteInt(itemId);
            pWriter.WriteByte(quantity);
        }

        return pWriter;
    }

    public static ByteWriter InteractedObjects(ISet<int> interactedObjects) {
        var pWriter = Packet.Of(SendOp.UserEnv);
        pWriter.Write<Command>(Command.InteractedObjects);
        pWriter.WriteInt(interactedObjects.Count);
        foreach (int id in interactedObjects) {
            pWriter.WriteInt(id);
        }

        return pWriter;
    }

    public static ByteWriter GatheringCounts(IDictionary<int, int> gatheringCounts) {
        var pWriter = Packet.Of(SendOp.UserEnv);
        pWriter.Write<Command>(Command.GatheringCounts);
        pWriter.WriteInt(gatheringCounts.Count);
        foreach ((int recipeId, int count) in gatheringCounts) {
            pWriter.WriteInt(recipeId);
            pWriter.WriteInt(count);
        }
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter LoadClaimedRewards(IDictionary<int, bool> claimedRewards) {
        var pWriter = Packet.Of(SendOp.UserEnv);
        pWriter.Write<Command>(Command.MasteryRewardsClaimed);
        pWriter.WriteInt(claimedRewards.Count);
        foreach ((int rewardId, bool isClaimed) in claimedRewards) {
            pWriter.WriteInt(rewardId);
            pWriter.WriteBool(isClaimed);
        };

        return pWriter;
    }
}
