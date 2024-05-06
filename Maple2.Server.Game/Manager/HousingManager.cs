using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Manager;

public class HousingManager(GameSession session) {
    private Home Home => session.Player.Value.Home;

    private readonly ILogger logger = Log.Logger.ForContext<HousingManager>();

    public void SetPlot(PlotInfo? plot) {
        Home.Outdoor = plot;

        session.Field?.Broadcast(CubePacket.UpdateProfile(session.Player));
    }

    public void SetName(string name) {
        if (name.Length > Constant.HomeNameMaxLength) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_center));
            return;
        }

        Home.Indoor.Name = name;
        PlotInfo? plot = Home.Outdoor;
        if (plot != null) {
            plot.Name = name;
        }

        if (!SavePlots()) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return;
        }

        session.Field?.Broadcast(CubePacket.SetHomeName(Home));
        session.Field?.Broadcast(CubePacket.UpdateProfile(session.Player));
    }

    public void SetHomeMessage(string message) {
        if (message.Length > Constant.HomeMessageMaxLength) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_center));
            return;
        }

        Home.Message = message;
        if (!SaveHome()) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return;
        }

        session.Field?.Broadcast(CubePacket.SetHomeMessage(Home.Message));
    }

    public void SetPasscode(string passcode) {
        if (passcode.Length != 0 && (passcode.Length != Constant.HomePasscodeLength || !uint.TryParse(passcode, out _))) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_center));
            return;
        }

        Home.Passcode = passcode;
        using GameStorage.Request db = session.GameStorage.Context();
        if (!SaveHome()) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return;
        }

        session.Send(CubePacket.SetPasscode());
    }

    public bool SaveHome() {
        using GameStorage.Request db = session.GameStorage.Context();
        return db.SaveHome(Home);
    }

    public bool SavePlots() {
        using GameStorage.Request db = session.GameStorage.Context();
        if (Home.Outdoor != null) {
            return db.SavePlotInfo(Home.Indoor, Home.Outdoor);
        }
        return db.SavePlotInfo(Home.Indoor);
    }

    // Retrieves plot directly from field which includes cube data.
    public Plot? GetFieldPlot() {
        if (session.Field == null) {
            return null;
        }

        Plot? plot;
        if (session.AccountId == session.Field.OwnerId && session.Field.MapId == Home.Indoor.MapId) {
            session.Field.Plots.TryGetValue(Home.Indoor.Number, out plot);
            return plot;
        }

        if (Home.Outdoor == null) {
            return null;
        }

        session.Field.Plots.TryGetValue(Home.Outdoor.Number, out plot);
        return plot;
    }

    public bool SaveFieldPlot(int number) {
        if (session.Field?.Plots.TryGetValue(number, out Plot? plot) != true) {
            return false;
        }

        return true;
    }

    public bool BuyPlot(int plotNumber) {
        PlotInfo? plotInfo = Home.Outdoor;
        if (plotInfo != null) {
            session.Send(plotInfo.Number == plotNumber
                ? CubePacket.Error(UgcMapError.s_ugcmap_my_house)
                : CubePacket.Error(UgcMapError.s_ugcmap_cant_buy_more_than_two_house));
            return false;
        }

        if (session.Field == null || !session.Field.Plots.TryGetValue(plotNumber, out Plot? plot)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_not_a_buyable));
            return false;
        }

        if (plot.OwnerId != 0 || plot.State != PlotState.Open) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_already_owned));
            return false;
        }

        UgcMapGroup.Cost contract = plot.Metadata.ContractCost;
        if (!CheckCost(session, contract)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_not_enough_money));
            return false;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        plotInfo = db.BuyPlot(session.AccountId, plot, TimeSpan.FromDays(contract.Days));
        if (plotInfo == null) {
            logger.Warning("Failed to buy plot: {PlotId}, {OwnerId}", plot.Id, plot.OwnerId);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return false;
        }

        CheckCost(session, contract, deduct: true); // Deduct cost
        if (session.Field.UpdatePlotInfo(plotInfo) != true) {
            logger.Warning("Failed to update map plot in field: {PlotId}", plotInfo.Id);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_system_error));
            return false;
        }

        session.Field.Broadcast(CubePacket.BuyPlot(plotInfo));
        SetPlot(plotInfo);
        return true;
    }

    public PlotInfo? ForfeitPlot() {
        PlotInfo? plotInfo = Home.Outdoor;
        if (plotInfo == null) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_dont_have_ownership));
            return null;
        }

        if (DateTime.UtcNow - plotInfo.ExpiryTime.FromEpochSeconds() > TimeSpan.FromDays(3)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_not_extension_date));
            return null;
        }

        if (session.Field == null || !session.Field.Plots.TryGetValue(plotInfo.Number, out Plot? plot)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_system_error));
            return null;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        plotInfo = db.ForfeitPlot(session.AccountId, plot);
        if (plotInfo == null) {
            logger.Warning("Failed to forfeit plot: {PlotId}, {OwnerId}", plot.Id, plot.OwnerId);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return null;
        }

        if (session.Field.UpdatePlotInfo(plotInfo) != true) {
            logger.Warning("Failed to update map plot in field: {PlotId}", plotInfo.Id);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_system_error));
            return null;
        }

        session.Field.Broadcast(CubePacket.ForfeitPlot(plotInfo));
        SetPlot(null);

        return plotInfo;
    }

    public void ExtendPlot() {
        if (Home.Outdoor == null) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_dont_have_ownership));
            return;
        }

        if (Home.Outdoor.State != PlotState.Taken || Home.Outdoor.ExpiryTime <= DateTime.UtcNow.ToEpochSeconds()) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_expired_salable_group));
            return;
        }

        if (Home.Outdoor.ExpiryTime.FromEpochSeconds() - DateTime.UtcNow > TimeSpan.FromDays(30)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_not_extension_date));
            return;
        }

        UgcMapGroup.Cost extension = Home.Outdoor.Metadata.ExtensionCost;
        if (!CheckCost(session, extension)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_need_extansion_pay));
            return;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        PlotInfo? plotInfo = db.ExtendPlot(Home.Outdoor, TimeSpan.FromDays(extension.Days));
        if (plotInfo == null) {
            logger.Warning("Failed to extend plot: {PlotId}, {OwnerId}", Home.Outdoor.Id, Home.Outdoor.OwnerId);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return;
        }

        CheckCost(session, extension, deduct: true); // Deduct cost
        if (session.Field?.UpdatePlotInfo(plotInfo) != true) {
            logger.Warning("Failed to update map plot in field: {PlotId}", Home.Outdoor.Id);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_system_error));
            return;
        }

        session.Send(CubePacket.ExtendPlot(plotInfo));
        SetPlot(plotInfo);
    }

    private static bool CheckCost(GameSession session, UgcMapGroup.Cost cost, bool deduct = false) {
        switch (cost.ItemId) {
            case >= 90000001 and <= 90000003:
                if (session.Currency.Meso < cost.Amount) {
                    session.Send(CubePacket.Error(UgcMapError.s_err_ugcmap_not_enough_meso_balance));
                    return false;
                }

                if (deduct) {
                    session.Currency.Meso -= cost.Amount;
                }
                return true;
            case 90000004 or 90000011 or 90000015 or 90000016:
                if (session.Currency.Meret < cost.Amount) {
                    session.Send(CubePacket.Error(UgcMapError.s_err_ugcmap_not_enough_merat_balance));
                    return false;
                }

                if (deduct) {
                    session.Currency.Meret -= cost.Amount;
                }
                return true;
        }

        return false;
    }

    public void Save(GameStorage.Request db) {
        db.SaveHome(Home);
        if (Home.Outdoor != null) {
            db.SavePlotInfo(Home.Indoor, Home.Outdoor);
        } else {
            db.SavePlotInfo(Home.Indoor);
        }
    }
}
