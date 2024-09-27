using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class FunctionCubePacket {
    private enum Command : byte {
        SendCubes = 2,
        Add = 3,
        Furniture = 5,
        SuccessLifeSkill = 8,
        FailLifeSkill = 9,
    }


    public static ByteWriter SendCubes(List<PlotCube> cubes) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.SendCubes);
        pWriter.WriteInt(cubes.Count);
        foreach (PlotCube cube in cubes) {
            pWriter.WriteUnicodeString(cube.InteractId);
            pWriter.Write<InteractCubeState>(cube.InteractState);
            pWriter.WriteByte(cube.InteractUnkByte);
        }
        return pWriter;
    }

    public static ByteWriter AddFunctionCube(PlotCube cube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteUnicodeString(cube.InteractId);
        pWriter.Write<InteractCubeState>(cube.InteractState);
        pWriter.WriteByte(cube.InteractUnkByte);
        return pWriter;
    }

    public static ByteWriter UseFurniture(long characterId, PlotCube cube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.Furniture);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(cube.InteractId);
        pWriter.WriteBool(cube.InteractState is InteractCubeState.InUse);
        return pWriter;
    }

    public static ByteWriter SuccessLifeSkill(long characterId, PlotCube cube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.SuccessLifeSkill);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(cube.InteractId);
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        pWriter.Write<InteractCubeState>(cube.InteractState);
        return pWriter;
    }

    public static ByteWriter FailLifeSkill(long characterId, PlotCube cube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.FailLifeSkill);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(cube.InteractId);
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        return pWriter;
    }
}
