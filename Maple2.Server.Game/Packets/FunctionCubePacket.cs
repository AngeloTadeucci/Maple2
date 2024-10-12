using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class FunctionCubePacket {
    private enum Command : byte {
        SendCubes = 2,
        Add = 3,
        Furniture = 5,
        SuccessLifeSkill = 8,
        FailLifeSkill = 9,
        Feed = 11,
    }

    public static ByteWriter SendCubes(List<PlotCube> cubes) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.SendCubes);
        pWriter.WriteInt(cubes.Count);
        foreach (PlotCube cube in cubes) {
            pWriter.WriteClass<InteractCube>(cube.Interact!);
        }
        return pWriter;
    }

    public static ByteWriter AddFunctionCube(PlotCube cube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteClass<InteractCube>(cube.Interact!);
        return pWriter;
    }

    public static ByteWriter UseFurniture(long characterId, PlotCube cube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.Furniture);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(cube.Interact!.Id);
        pWriter.WriteBool(cube.Interact.State is InteractCubeState.InUse);
        return pWriter;
    }

    public static ByteWriter SuccessLifeSkill(long characterId, PlotCube cube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.SuccessLifeSkill);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(cube.Interact!.Id);
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        pWriter.Write<InteractCubeState>(cube.Interact.State);
        return pWriter;
    }

    public static ByteWriter FailLifeSkill(long characterId, PlotCube cube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.FailLifeSkill);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(cube.Interact!.Id);
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        return pWriter;
    }

    public static ByteWriter Feed(long itemUid, PlotCube cube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.Feed);
        pWriter.WriteLong(itemUid);
        pWriter.WriteUnicodeString(cube.Interact!.Id);
        pWriter.Write<InteractCubeState>(cube.Interact.State);
        pWriter.WriteLong(cube.Id);
        pWriter.WriteInt(); // idk
        return pWriter;
    }
}
