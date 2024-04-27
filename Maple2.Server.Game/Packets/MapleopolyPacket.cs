using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class MapleopolyPacket {
    private enum Command : byte {
        Load = 0,
        Roll = 2,
        Result = 4,
        Error = 6,
    }

    public static ByteWriter Load(IList<BlueMarbleTile> tiles, int totalTileCount, int freeRollAmount, int playerTicketAmount) {
        var pWriter = Packet.Of(SendOp.Mapleopoly);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(totalTileCount);
        pWriter.WriteInt(freeRollAmount);
        pWriter.WriteInt(Constant.MapleopolyTicketItemId);
        pWriter.WriteInt(playerTicketAmount);
        pWriter.WriteInt(tiles.Count);

        foreach (BlueMarbleTile tile in tiles) {
            pWriter.WriteClass<BlueMarbleTile>(tile);
        }

        return pWriter;
    }

    public static ByteWriter Roll(int tileLocation, int dice1, int dice2, MapleopolyError error = MapleopolyError.ok) {
        var pWriter = Packet.Of(SendOp.Mapleopoly);
        pWriter.Write<Command>(Command.Roll);
        pWriter.Write<MapleopolyError>(error);
        pWriter.WriteInt(tileLocation);
        pWriter.WriteInt(dice1);
        pWriter.WriteInt(dice2);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter Result(BlueMarbleTile tile, int totalTileCount, int freeRollAmount) {
        var pWriter = Packet.Of(SendOp.Mapleopoly);
        pWriter.Write<Command>(Command.Result);
        pWriter.Write<BlueMarbleTileType>(tile.Type);
        pWriter.WriteInt(tile.MoveAmount);
        pWriter.WriteInt(totalTileCount);
        pWriter.WriteInt(freeRollAmount);
        pWriter.Write<BlueMarbleItem>(tile.Item);

        return pWriter;
    }

    public static ByteWriter Error(MapleopolyError error) {
        var pWriter = Packet.Of(SendOp.Mapleopoly);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<MapleopolyError>(error);

        return pWriter;
    }
}
