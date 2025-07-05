using System.Collections.Concurrent;
using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Game.Ugc;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Manager.Field;

public partial class FieldManager {
    public readonly ConcurrentDictionary<int, Plot> Plots = new();
    public readonly ConcurrentDictionary<long, FieldUgcBanner> Banners = new();
    private DateTimeOffset lastBannerUpdate = DateTimeOffset.MinValue;

    private void UpdateBanners() {
        if (DateTimeOffset.UtcNow - lastBannerUpdate < TimeSpan.FromMinutes(1)) {
            return;
        }

        DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
        foreach (FieldUgcBanner ugcBanner in Banners.Values) {
            ugcBanner.Update(FieldTick);
        }

        using GameStorage.Request db = GameStorage.Context();
        foreach (UgcBanner ugcBanner in Banners.Values) {
            List<BannerSlot> expiredSlots = ugcBanner.Slots.Where(x => x.Expired).ToList();

            db.RemoveBannerSlots(expiredSlots);
            foreach (BannerSlot slot in expiredSlots) {
                ugcBanner.Slots.Remove(slot);
            }
        }

        lastBannerUpdate = dateTimeOffset;
    }

    public bool UpdatePlotInfo(PlotInfo plotInfo) {
        if (MapId != plotInfo.MapId || !Plots.ContainsKey(plotInfo.Number)) {
            return false;
        }

        if (plotInfo is Plot plot) {
            Plots[plot.Number] = plot;
        } else {
            plot = Plots[plotInfo.Number];
            plot.OwnerId = plotInfo.OwnerId;
            plot.Name = plotInfo.Name;
            plot.ExpiryTime = plotInfo.ExpiryTime;
            plot.MapId = plotInfo.MapId;
        }

        Broadcast(CubePacket.UpdatePlot(plot));
        return true;
    }

    private void CommitPlot(GameSession session) {
        Home home = session.Player.Value.Home;
        using GameStorage.Request db = GameStorage.Context();
        if (this is HomeFieldManager homeField) {
            if (session.AccountId == homeField.OwnerId && home.Indoor.MapId == MapId && Plots.TryGetValue(home.Indoor.Number, out Plot? indoorPlot) && !indoorPlot.IsPlanner) {
                SavePlot(indoorPlot);
            }
        }

        if (home.Outdoor != null && home.Outdoor.MapId == MapId && Plots.TryGetValue(home.Outdoor.Number, out Plot? outdoorPlot)) {
            SavePlot(outdoorPlot);
        }

        void SavePlot(Plot plot) {
            lock (Plots) {
                ICollection<PlotCube>? results = db.SaveCubes(plot, plot.Cubes.Values);
                if (results == null) {
                    logger.Fatal("Failed to save plot cubes: {PlotId}", plot.Id);
                    throw new InvalidOperationException($"Failed to save plot cubes: {plot.Id}");
                }

                List<PlotCube> portalCubes = plot.Cubes.Values.Where(c => c.Interact?.PortalSettings is not null).ToList();

                plot.Cubes.Clear();
                foreach (PlotCube result in results) {
                    if (result.Interact?.PortalSettings is not null) {
                        PlotCube? existingCube = portalCubes.Find(x => x.Position == result.Position && x.ItemId == result.ItemId);
                        if (existingCube is not null) {
                            result.Interact = existingCube.Interact;
                        }
                    }
                    plot.Cubes.Add(result.Position, result);
                }
            }
        }
    }

    public void PlaceCube(GameSession session, HeldCube cubeItem, in Vector3B position, float rotation) {
        if (!session.ItemMetadata.TryGet(cubeItem.ItemId, out ItemMetadata? itemMetadata)) {
            logger.Error("Failed to get item metadata for cube {cubeId}.", cubeItem.ItemId);
            return;
        }
        Plot? plot;
        switch (session.HeldCube) {
            case PlotCube _:
                if (itemMetadata.Install is null || itemMetadata.Housing is null) {
                    logger.Error("Item {CubeItemItemId} is not a housing item.", cubeItem.ItemId);
                    return;
                }
                plot = session.Housing.GetFieldPlot();
                if (plot == null) {
                    return;
                }

                if (!session.Housing.TryPlaceCube(cubeItem, plot, itemMetadata, position, rotation, out PlotCube? plotCube)) {
                    return;
                }

                session.Field!.Broadcast(CubePacket.PlaceCube(session.Player.ObjectId, plot, plotCube));

                if (plot.MapId is not Constant.DefaultHomeMapId) {
                    session.World.UpdateFieldPlot(new FieldPlotRequest {
                        IgnoreChannel = GameServer.GetChannel(),
                        MapId = session.Field.MapId,
                        PlotNumber = plot.Number,
                        UpdateBlock = new FieldPlotRequest.Types.UpdateBlock {
                            BlockUid = plotCube.Id,
                            Add = new FieldPlotRequest.Types.UpdateBlock.Types.Add { },
                        },
                    });
                }

                if (plot.IsPlanner) {
                    return;
                }
                break;
            case LiftableCube liftable:
                plot = session.Field!.Plots[0];

                FieldLiftable? fieldLiftable = session.Field.AddLiftable($"4_{position.ConvertToInt()}", liftable.Liftable);
                if (fieldLiftable == null) {
                    return;
                }

                session.HeldCube = null;
                fieldLiftable.Count = 1;
                fieldLiftable.State = LiftableState.Default;
                fieldLiftable.Position = position;
                fieldLiftable.Rotation = new Vector3(0, 0, rotation);
                fieldLiftable.FinishTick = session.Field.FieldTick + fieldLiftable.Value.ItemLifetime + fieldLiftable.Value.FinishTime;

                if (session.Field.Entities.LiftableTargetBoxes.TryGetValue(position, out LiftableTargetBox? liftableTarget)) {
                    session.ConditionUpdate(ConditionType.item_move, codeLong: cubeItem.ItemId, targetLong: liftableTarget.LiftableTarget);
                }

                Broadcast(LiftablePacket.Add(fieldLiftable));
                var cube = new PlotCube(itemMetadata, liftable.Id) {
                    Type = PlotCube.CubeType.Liftable,
                    Position = position,
                    Rotation = rotation,
                };
                if (plot.Cubes.TryAdd(position, cube)) {
                    Broadcast(CubePacket.PlaceLiftable(session.Player.ObjectId, liftable, position, rotation));
                }
                Broadcast(SetCraftModePacket.Stop(session.Player.ObjectId));
                Broadcast(LiftablePacket.Update(fieldLiftable));
                break;
        }

        session.ConditionUpdate(ConditionType.install_item, codeLong: cubeItem.ItemId);
    }

    public void UpdateAllPlots(int plotNumber = 0) {
        using GameStorage.Request db = GameStorage.Context();
        Plots.TryGetValue(plotNumber, out Plot? oldPlot);
        foreach (Plot plot in db.LoadPlotsForMap(MapId)) {
            Plots[plot.Number] = plot;
        }

        lock (Plots) {
            List<Plot> allPlots = Plots.Values.ToList();
            List<Plot> updateExpirationTime = allPlots.Where(x => x.State is not PlotState.Open).ToList();

            Plots.TryGetValue(plotNumber, out Plot? plot);
            if (oldPlot?.State is PlotState.Pending && plot?.State is PlotState.Open) {
                updateExpirationTime.Add(plot);
            }
            Broadcast(LoadCubesPacket.PlotOwners(allPlots.Where(x => x.State is PlotState.Taken).ToList()));
            Broadcast(LoadCubesPacket.PlotState(allPlots));
            Broadcast(LoadCubesPacket.PlotExpiry(updateExpirationTime));
        }
    }
}
