﻿using Maple2.Model.Error;
using Maple2.Model.Game.Dungeon;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class DungeonRoomPacket {
    private enum Command : byte {
        Load = 5,
        Update = 6,
        Error = 18,
        RankRewards = 19,
    }

    public static ByteWriter Load(IDictionary<int, DungeonRecord> records) {
        var pWriter = Packet.Of(SendOp.RoomDungeon);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(records.Count);
        foreach ((int dungeonId, DungeonRecord record) in records) {
            pWriter.WriteInt(dungeonId);
            pWriter.WriteClass<DungeonRecord>(record);
        }

        return pWriter;
    }

    public static ByteWriter Update(DungeonRecord record) {
        var pWriter = Packet.Of(SendOp.RoomDungeon);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteClass<DungeonRecord>(record);

        return pWriter;
    }

    public static ByteWriter Error(DungeonRoomError error, int arg = 0) {
        var pWriter = Packet.Of(SendOp.RoomDungeon);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<DungeonRoomError>(error);
        pWriter.WriteInt(arg);

        return pWriter;
    }

    public static ByteWriter RankRewards(IDictionary<int, DungeonRankReward> rankRewards) {
        var pWriter = Packet.Of(SendOp.RoomDungeon);
        pWriter.Write<Command>(Command.RankRewards);
        pWriter.WriteInt(rankRewards.Count);
        foreach ((int id, DungeonRankReward reward) in rankRewards) {
            pWriter.WriteInt(id);
            pWriter.WriteClass<DungeonRankReward>(reward);
        }

        return pWriter;
    }
}
