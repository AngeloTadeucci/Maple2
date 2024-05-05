using System.Numerics;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class FishingPacket {
    private enum Command : byte {
        Prepare = 0,
        Stop = 1,
        Error = 2,
        IncreaseMastery = 3,
        LoadTiles = 4,
        CatchItem = 5,
        PrizeFish = 6,
        LoadAlbum = 7,
        CatchFish = 8,
        Start = 9,
        Auto = 10,
        Simulate = 11,
    }

    public static ByteWriter Prepare(long rodUid) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.Prepare);
        pWriter.WriteLong(rodUid);

        return pWriter;
    }

    public static ByteWriter Stop() {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.Stop);

        return pWriter;
    }

    public static ByteWriter Error(FishingError error) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<FishingError>(error);

        return pWriter;
    }

    public static ByteWriter IncreaseMastery(int fishId, short level, int exp, CaughtFishType fishType) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.IncreaseMastery);
        pWriter.WriteInt(fishId);
        pWriter.WriteInt(exp);
        pWriter.Write<CaughtFishType>(fishType);
        pWriter.WriteShort(level);

        return pWriter;
    }

    public static ByteWriter LoadTiles(IList<Vector3> tiles) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.LoadTiles);
        pWriter.WriteByte(); // disable loading tiles
        pWriter.WriteInt(tiles.Count);
        foreach (Vector3 tile in tiles) {
            pWriter.Write<Vector3B>(tile);
            pWriter.WriteInt(10000001); // fish id ?
            pWriter.WriteInt(25); // unk
            pWriter.WriteInt(15000); // fishing time minus any rod or buff time reduction
            pWriter.WriteShort(1); // unk
        }

        return pWriter;
    }

    public static ByteWriter CatchItem(IList<FishingRewardTable.Entry> rewards) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.CatchItem);
        pWriter.WriteInt(rewards.Count);
        foreach (FishingRewardTable.Entry item in rewards) {
            pWriter.WriteInt(item.Id);
            pWriter.WriteInt(item.Amount);
        }

        return pWriter;
    }

    public static ByteWriter PrizeFish(string playerName, int fishId) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.PrizeFish);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteInt(fishId);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter LoadAlbum(ICollection<FishEntry> fishAlbum) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.LoadAlbum);
        pWriter.WriteInt(fishAlbum.Count);
        foreach (FishEntry fish in fishAlbum) {
            pWriter.WriteClass<FishEntry>(fish);
        }

        return pWriter;
    }

    public static ByteWriter CatchFish(int id, int size, bool autoFish = false, FishEntry? fish = null) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.CatchFish);
        pWriter.WriteInt(id);
        pWriter.WriteInt(size);
        pWriter.WriteBool(fish != null);
        pWriter.WriteBool(autoFish);

        if (fish != null) {
            pWriter.WriteClass<FishEntry>(fish);
        }

        return pWriter;
    }

    public static ByteWriter Start(int durationTick, bool miniGame) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.Start);
        pWriter.WriteBool(miniGame);
        pWriter.WriteInt(Environment.TickCount + durationTick);

        return pWriter;
    }

    public static ByteWriter Auto(bool autoFish) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.Auto);
        pWriter.WriteBool(autoFish);

        return pWriter;
    }

    public static ByteWriter Simulate(ICollection<FishEntry> fishEntries) {
        var pWriter = Packet.Of(SendOp.Fishing);
        pWriter.Write<Command>(Command.Simulate);
        pWriter.WriteInt(fishEntries.Count);
        foreach (FishEntry fish in fishEntries) {
            pWriter.WriteInt(fish.Id);
            pWriter.WriteInt(); // pick count
            pWriter.WriteInt(); // catch count
            pWriter.WriteInt(); // count
            // loop for each count above
            pWriter.WriteInt(); // unk
            // end loop
            pWriter.WriteInt(); // prize count
        }

        return pWriter;
    }
}
