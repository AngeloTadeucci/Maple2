using System.Collections.Concurrent;
using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Model.Game.Ugc;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model.Field;
using Maple2.Server.Game.Model.Room;
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

                plot.Cubes.Clear();
                foreach (PlotCube result in results) {
                    plot.Cubes.Add(result.Position, result);
                }
            }
        }
    }
}
