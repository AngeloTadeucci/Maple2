using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model.Common;
using Maple2.Model.Game;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<FieldPlotResponse> UpdateFieldPlot(FieldPlotRequest request, ServerCallContext context) {
        if (request.IgnoreChannel == GameServer.GetChannel()) {
            logger.Debug("Ignoring UpdateFieldPlot request for channel {Channel}", request.IgnoreChannel);
            return Task.FromResult(new FieldPlotResponse());
        }

        if (request.MapId <= 0) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid map ID"));
        }

        switch (request.RequestCase) {
            case FieldPlotRequest.RequestOneofCase.UpdatePlot:
                HandleUpdatePlot(request.MapId, request.PlotNumber, request.UpdatePlot);
                break;
            case FieldPlotRequest.RequestOneofCase.UpdateBlock:
                HandleUpdateBlock(request.MapId, request.PlotNumber, request.UpdateBlock);
                break;
            case FieldPlotRequest.RequestOneofCase.None:
            default:
                logger.Error("Invalid request type for UpdateFieldPlot: {RequestType}", request.RequestCase);
                break;
        }

        return Task.FromResult(new FieldPlotResponse());
    }

    private void HandleUpdatePlot(int mapId, int plotNumber, FieldPlotRequest.Types.UpdatePlot request) {
        GameSession? session = null;
        if (request.AccountId > 0) {
            session = server.GetSessionByAccountId(request.AccountId);
            if (session is not null) {
                // Player is online and their plot expired, update their furnishing inventory with the new items
                session.Item.ReInstantiateFurnishing();
                // set their outside plot to null
                session.Housing.SetPlot(null);
            }
        }

        FieldManager? fieldManager = server.GetField(mapId);
        if (fieldManager == null) {
            return;
        }

        if (request.Forfeit) {
            // remove all cubes from the plot
            if (fieldManager.Plots.TryGetValue(plotNumber, out Plot? plot)) {
                foreach (KeyValuePair<Vector3B, PlotCube> plotCube in plot.Cubes) {
                    fieldManager.Broadcast(CubePacket.RemoveCube(fieldManager.FieldActor.ObjectId, plotCube.Key));
                }
            }
        }

        fieldManager.UpdateAllPlots(plotNumber);
        if (request.Forfeit) {
            if (fieldManager.Plots.TryGetValue(plotNumber, out Plot? plot)) {
                fieldManager.Broadcast(CubePacket.ForfeitPlot(plot));
                session?.Send(CubePacket.ConfirmForfeitPlot(plot));
            }
        }
    }

    private void HandleUpdateBlock(int mapId, int plotNumber, FieldPlotRequest.Types.UpdateBlock request) {
        FieldManager? fieldManager = server.GetField(mapId);
        if (fieldManager == null) {
            return;
        }
        if (!fieldManager.Plots.TryGetValue(plotNumber, out Plot? plot)) {
            logger.Error("Plot {PlotNumber} not found in map {MapId}", plotNumber, mapId);
            return;
        }

        using GameStorage.Request db = gameStorage.Context();

        switch (request.RequestCase) {
            case FieldPlotRequest.Types.UpdateBlock.RequestOneofCase.Replace:
            case FieldPlotRequest.Types.UpdateBlock.RequestOneofCase.Add:
                bool isReplace = request.RequestCase == FieldPlotRequest.Types.UpdateBlock.RequestOneofCase.Replace;

                PlotCube? plotCube = db.GetCube(request.BlockUid);
                if (plotCube == null) {
                    logger.Error("Plot cube with UID {BlockUid} not found in database", request.BlockUid);
                    return;
                }

                if (plot.Cubes.ContainsKey(plotCube.Position) && !isReplace) {
                    logger.Error("Cube already exists at position {Position} in plot {PlotNumber}", plotCube.Position, plotNumber);
                    return;
                }

                if (isReplace && plot.Cubes.Remove(plotCube.Position, out PlotCube? cube)) {
                    if (cube.Interact is not null) {
                        fieldManager.RemoveFieldFunctionInteract(cube.Interact.Id);
                    }
                }

                plotCube.PlotId = plot.Number;

                plot.Cubes.Add(plotCube.Position, plotCube);
                if (plotCube.Interact is not null) {
                    fieldManager.AddFieldFunctionInteract(plotCube);
                }
                if (isReplace) {
                    fieldManager.Broadcast(CubePacket.ReplaceCube(fieldManager.FieldActor.ObjectId, plotCube));
                } else {
                    fieldManager.Broadcast(CubePacket.PlaceCube(fieldManager.FieldActor.ObjectId, plot, plotCube));
                }
                break;
            case FieldPlotRequest.Types.UpdateBlock.RequestOneofCase.Remove:
                var positionToRemove = new Vector3B(request.X, request.Y, request.Z);
                if (!plot.Cubes.TryGetValue(positionToRemove, out PlotCube? cubeToRemove)) {
                    logger.Error("No cube found at position {Position} in plot {PlotNumber}", positionToRemove, plotNumber);
                    return;
                }
                if (!plot.Cubes.Remove(positionToRemove)) {
                    logger.Error("Failed to remove cube at position {Position} in plot {PlotNumber}", positionToRemove, plotNumber);
                    return;
                }
                if (cubeToRemove.Interact is not null) {
                    fieldManager.RemoveFieldFunctionInteract(cubeToRemove.Interact.Id);
                }
                fieldManager.Broadcast(CubePacket.RemoveCube(fieldManager.FieldActor.ObjectId, positionToRemove));
                break;
            case FieldPlotRequest.Types.UpdateBlock.RequestOneofCase.Rotate:
                var position = new Vector3B(request.X, request.Y, request.Z);
                if (!plot.Cubes.TryGetValue(position, out PlotCube? existingCube)) {
                    logger.Error("No cube found at position {Position} in plot {PlotNumber}", position, plotNumber);
                    return;
                }

                existingCube.Rotate(request.Rotate.Clockwise);

                fieldManager.Broadcast(CubePacket.RotateCube(fieldManager.FieldActor.ObjectId, existingCube));
                break;
            case FieldPlotRequest.Types.UpdateBlock.RequestOneofCase.None:
            default:
                logger.Error("Invalid request type for UpdateBlock: {RequestType}", request.RequestCase);
                break;
        }
    }
}
