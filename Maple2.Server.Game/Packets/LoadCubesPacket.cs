using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class LoadCubesPacket {
    private enum Command : byte {
        Load = 0,
        PlotState = 1,
        LoadPlots = 2,
        PlotExpiry = 3,
    }

    public static ByteWriter Load(List<PlotCube> plotCubes) {
        var pWriter = Packet.Of(SendOp.LoadCubes);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteBool(false);
        pWriter.WriteInt(plotCubes.Count);
        foreach (PlotCube cube in plotCubes) {
            pWriter.Write<Vector3B>(cube.Position);
            pWriter.WriteLong(cube.Id);
            pWriter.WriteClass<PlotCube>(cube);
            pWriter.WriteInt(cube.PlotId);
            pWriter.WriteInt();
            pWriter.WriteBool(false);
            pWriter.WriteFloat(cube.Rotation);
            pWriter.WriteInt();
            pWriter.WriteBool(false); // Binding?
        }

        return pWriter;
    }

    public static ByteWriter PlotState(ICollection<PlotInfo> plots) {
        var pWriter = Packet.Of(SendOp.LoadCubes);
        pWriter.Write<Command>(Command.PlotState);
        pWriter.WriteInt(plots.Count);
        foreach (PlotInfo plot in plots) {
            pWriter.WriteInt(plot.Number);
            pWriter.Write<PlotState>(plot.State);
        }

        return pWriter;
    }

    public static ByteWriter PlotOwners(ICollection<Plot> plots) {
        var pWriter = Packet.Of(SendOp.LoadCubes);
        pWriter.Write<Command>(Command.LoadPlots);
        pWriter.WriteInt(plots.Count);
        foreach (Plot plot in plots) {
            pWriter.WriteInt(plot.Number);
            pWriter.WriteInt(plot.ApartmentNumber); // unsure
            pWriter.WriteUnicodeString(); // plot Name
            pWriter.WriteLong(plot.OwnerId);
        }

        return pWriter;
    }

    public static ByteWriter PlotExpiry(ICollection<PlotInfo> plots) {
        var pWriter = Packet.Of(SendOp.LoadCubes);
        pWriter.Write<Command>(Command.PlotExpiry);

        pWriter.WriteInt(plots.Count);
        foreach (PlotInfo plot in plots) {
            pWriter.WriteInt(plot.Number);
            pWriter.WriteInt(plot.ApartmentNumber); // unsure
            pWriter.Write<PlotState>(plot.State);
            pWriter.WriteLong(plot.ExpiryTime);
        }

        return pWriter;
    }
}
