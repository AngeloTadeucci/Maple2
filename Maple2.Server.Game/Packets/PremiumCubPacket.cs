using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class PremiumCubPacket {
    private enum Command : byte {
        Activate = 0,
        LoadItems = 1,
        ClaimItem = 2,
        LoadPackages = 3,
        PurchasePackage = 4,
    }

    public static ByteWriter Activate(int objectId, long expiration) {
        var pWriter = Packet.Of(SendOp.PremiumClub);
        pWriter.Write<Command>(Command.Activate);
        pWriter.WriteInt(objectId);
        pWriter.WriteLong(expiration);

        return pWriter;
    }

    public static ByteWriter LoadItems(IList<int> itemIds) {
        var pWriter = Packet.Of(SendOp.PremiumClub);
        pWriter.Write<Command>(Command.LoadItems);
        pWriter.WriteInt(itemIds.Count);
        foreach (int itemId in itemIds) {
            pWriter.WriteInt(itemId);
        }

        return pWriter;
    }

    public static ByteWriter ClaimItem(int benefitId) {
        var pWriter = Packet.Of(SendOp.PremiumClub);
        pWriter.Write<Command>(Command.ClaimItem);
        pWriter.WriteInt(benefitId);

        return pWriter;
    }

    public static ByteWriter LoadPackages() {
        var pWriter = Packet.Of(SendOp.PremiumClub);
        pWriter.Write<Command>(Command.LoadPackages);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter PurchasePackage(int packageId) {
        var pWriter = Packet.Of(SendOp.PremiumClub);
        pWriter.Write<Command>(Command.PurchasePackage);
        pWriter.WriteInt(packageId);

        return pWriter;
    }
}
