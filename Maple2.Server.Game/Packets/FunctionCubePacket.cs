using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
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

    public static ByteWriter SendCubes(ICollection<FieldFunctionInteract> cubes) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.SendCubes);
        pWriter.WriteInt(cubes.Count);
        foreach (FieldFunctionInteract cube in cubes) {
            pWriter.WriteClass<InteractCube>(cube.InteractCube);
            pWriter.WriteByte();
        }
        return pWriter;
    }

    private const byte StateAdd = 0;
    private const byte StateUpdate = 1;

    public static ByteWriter AddFunctionCube(InteractCube interactCube) =>
        WriteFunctionCube(interactCube, StateAdd);

    public static ByteWriter UpdateFunctionCube(InteractCube interactCube) =>
        WriteFunctionCube(interactCube, StateUpdate);

    private static ByteWriter WriteFunctionCube(InteractCube interactCube, byte state) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteClass<InteractCube>(interactCube);
        pWriter.WriteByte(state); // 0 Add, 1 Update
        return pWriter;
    }

    public static ByteWriter UseFurniture(long characterId, InteractCube interactCube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.Furniture);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(interactCube.Id);
        pWriter.WriteBool(interactCube.State is InteractCubeState.InUse);
        return pWriter;
    }

    public static ByteWriter SuccessLifeSkill(long characterId, InteractCube interactCube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.SuccessLifeSkill);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(interactCube.Id);
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        pWriter.Write<InteractCubeState>(interactCube.State);
        return pWriter;
    }

    public static ByteWriter FailLifeSkill(long characterId, InteractCube interactCube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.FailLifeSkill);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(interactCube.Id);
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        return pWriter;
    }

    public static ByteWriter Feed(long itemUid, long cubeId, InteractCube cube) {
        var pWriter = Packet.Of(SendOp.FunctionCube);
        pWriter.Write<Command>(Command.Feed);
        pWriter.WriteLong(itemUid);
        pWriter.WriteUnicodeString(cube.Id);
        pWriter.Write<InteractCubeState>(cube.State);
        pWriter.WriteLong(cubeId);
        pWriter.WriteInt(); // idk
        return pWriter;
    }
}
