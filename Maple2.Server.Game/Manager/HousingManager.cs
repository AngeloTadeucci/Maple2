using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Items;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Serilog;
using Serilog.Core;

namespace Maple2.Server.Game.Manager;

public class HousingManager {
    private readonly GameSession session;
    private readonly TableMetadataStorage tableMetadata;
    private Home Home => session.Player.Value.Home;

    private readonly ILogger logger = Log.Logger.ForContext<HousingManager>();

    public ItemBlueprint? StagedItemBlueprint = null;

    public HousingManager(GameSession session, TableMetadataStorage tableMetadata) {
        this.session = session;
        this.tableMetadata = tableMetadata;
    }

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
        if (session.Field.MapId == Home.Indoor.MapId) {
            session.Field.Plots.TryGetValue(Home.Indoor.Number, out plot);
            return plot;
        }

        if (Home.Outdoor == null) {
            return null;
        }

        session.Field.Plots.TryGetValue(Home.Outdoor.Number, out plot);
        return plot;
    }

    public Plot? GetIndoorPlot() {
        if (session.Field == null) {
            return null;
        }

        if (session.AccountId != session.Field.OwnerId || session.Field.MapId != Home.Indoor.MapId) return null;

        session.Field.Plots.TryGetValue(Home.Indoor.Number, out Plot? plot);
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
        if (!CheckAndRemoveCost(session, contract)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_not_enough_money));
            return false;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        plotInfo = db.BuyPlot(session.PlayerName, session.AccountId, plot, TimeSpan.FromDays(contract.Days));
        if (plotInfo == null) {
            logger.Warning("Failed to buy plot: {PlotId}, {OwnerId}", plot.Id, plot.OwnerId);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return false;
        }

        session.ConditionUpdate(ConditionType.buy_house);
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
        if (!CheckAndRemoveCost(session, extension)) {
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

        session.ConditionUpdate(ConditionType.extend_house);
        if (session.Field?.UpdatePlotInfo(plotInfo) != true) {
            logger.Warning("Failed to update map plot in field: {PlotId}", Home.Outdoor.Id);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_system_error));
            return;
        }

        session.Send(CubePacket.ExtendPlot(plotInfo));
        SetPlot(plotInfo);
    }

    private static bool CheckAndRemoveCost(GameSession session, UgcMapGroup.Cost cost) {
        switch (cost.ItemId) {
            case >= 90000001 and <= 90000003:
                if (session.Currency.Meso < cost.Amount) {
                    return false;
                }

                session.Currency.Meso -= cost.Amount;
                return true;
            case 90000004 or 90000011 or 90000015 or 90000016:
                if (session.Currency.Meret < cost.Amount) {
                    return false;
                }

                session.Currency.Meret -= cost.Amount;
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

    public void InitNewHome(string characterName, ExportedUgcMapMetadata? template) {
        Home.Indoor.Name = characterName;
        Home.Indoor.ExpiryTime = new DateTimeOffset(2900, 12, 31, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        Home.Message = "Thanks for visiting. Come back soon!";
        Home.DecorationLevel = 1;
        Home.Passcode = "*****";

        using GameStorage.Request db = session.GameStorage.Context();
        if (template is null) {
            Home.SetArea(10);
            Home.SetHeight(4);

            db.SaveHome(Home);
            db.SavePlotInfo(Home.Indoor);
            return;
        }

        Home.SetArea(template.IndoorSize[0]);
        Home.SetHeight(template.IndoorSize[2]);

        List<PlotCube> plotCubes = [];
        foreach (ExportedUgcMapMetadata.Cube cube in template.Cubes) {
            long itemUid = session.Item.Furnishing.AddCube(cube.ItemId);
            if (itemUid == 0) {
                logger.Error("Failed to add cube {cubeId} to storage.", cube.ItemId);
                continue;
            }
            session.Item.Furnishing.TryPlaceCube(itemUid, out PlotCube? plotCube);
            if (plotCube is null) {
                logger.Error("Failed to place cube {cubeId}.", cube.ItemId);
                continue;
            }
            plotCube.Position = template.BaseCubePosition + cube.OffsetPosition;
            plotCube.Rotation = cube.Rotation;
            plotCubes.Add(plotCube);
        }

        db.SaveHome(Home);
        db.SavePlotInfo(Home.Indoor);
        db.SaveCubes(Home.Indoor, plotCubes);
    }

    #region Helpers
    public bool TryPlaceCube(HeldCube cube, Plot plot, in Vector3B position, float rotation,
                             [NotNullWhen(true)] out PlotCube? result, bool isReplace = false) {
        result = null;
        if (!session.ItemMetadata.TryGet(cube.ItemId, out ItemMetadata? itemMetadata) || itemMetadata.Install is null || itemMetadata.Housing is null) {
            logger.Error("Failed to get item metadata for cube {cubeId}.", cube.ItemId);
            return false;
        }

        if (plot.PlotMode is PlotMode.BlueprintPlanner && itemMetadata.Housing.IsNotAllowedInBlueprint) {
            if (itemMetadata.Housing.HousingCategory is HousingCategory.Farming or HousingCategory.Ranching) {
                session.Send(CubePacket.Error(UgcMapError.s_err_cannot_install_nurturing_in_design_home));
            } else if (cube.ItemId is Constant.InteriorPortalCubeId) {
                session.Send(CubePacket.Error(UgcMapError.s_err_cannot_install_magic_portal));
            } else {
                session.Send(CubePacket.Error(UgcMapError.s_err_cannot_install_blueprint));
            }
            return false;
        }

        bool isSolidCube = itemMetadata.Install.IsSolidCube;
        bool isOnGround = position.Z == 0; // TODO: Handle outside plots
        bool allowWaterOnGround = itemMetadata.Install.MapAttribute is MapAttribute.water && Constant.AllowWaterOnGround;

        // If the cube is not a solid cube and it's replacing ground, it's not allowed.
        if ((!isSolidCube && isOnGround) && !allowWaterOnGround) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_cant_create_on_place));
            return false;
        }

        // Cannot overlap cubes if not replacing
        if (plot.Cubes.ContainsKey(position) && !isReplace) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_cant_create_on_place));
            return false;
        }

        if (isReplace && plot.Cubes.ContainsKey(position)) {
            TryRemoveCube(plot, position, out _);
        }

        //TODO: check outside plot - coords belongs to plot

        // TODO: check outside plot bounds

        if (IsCoordOutsideArea(position)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_area_limit));
            return false;
        }

        session.FunctionCubeMetadata.TryGet(itemMetadata.Install.InteractId, out FunctionCubeMetadata? functionCubeMetadata);

        if (plot.IsPlanner) {
            result = new PlotCube(cube.ItemId, FurnishingManager.NextCubeId(), cube.Template) {
                Position = position,
                Rotation = rotation,
                HousingCategory = itemMetadata.Housing.HousingCategory,
            };

            result.CubePortalSettings?.SetName(position);
            if (result.ItemType.IsInteractFurnishing && functionCubeMetadata is not null) {
                result.Interact = new InteractCube(position, functionCubeMetadata);
            }

            plot.Cubes.Add(position, result);
            return true;
        }

        if (!session.Item.Furnishing.TryPlaceCube(cube.Id, out result)) {
            long itemUid = session.Item.Furnishing.AddCube(cube.ItemId);
            if (itemUid == 0) {
                session.Send(CubePacket.Error(UgcMapError.s_ugcmap_not_for_sale));
                return false;
            }

            tableMetadata.FurnishingShopTable.Entries.TryGetValue(cube.ItemId, out FurnishingShopTable.Entry? shopEntry);
            if (shopEntry is null) {
                session.Send(CubePacket.Error(UgcMapError.s_err_cannot_buy_limited_item_more));
                return false;
            }

            if (!session.Item.Furnishing.PurchaseCube(shopEntry)) {
                return false;
            }

            session.Send(CubePacket.PurchaseCube(session.Player.ObjectId));
            // Now that we have purchased the cube, it must be placeable.
            if (!session.Item.Furnishing.TryPlaceCube(itemUid, out result)) {
                session.Send(CubePacket.Error(UgcMapError.s_ugcmap_not_owned_item));
                return false;
            }
        }

        result.Position = position;
        result.Rotation = rotation;
        result.HousingCategory = itemMetadata.Housing.HousingCategory;
        result.CubePortalSettings?.SetName(position);
        if (result.ItemType.IsInteractFurnishing && functionCubeMetadata is not null) {
            result.Interact = new InteractCube(position, functionCubeMetadata);
        }
        plot.Cubes.Add(position, result);
        return true;
    }

    public bool TryRemoveCube(Plot plot, in Vector3B position, [NotNullWhen(true)] out PlotCube? cube) {
        if (!plot.Cubes.Remove(position, out cube)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_no_cube_to_remove));
            return false;
        }

        if (cube.ItemId is Constant.InteriorPortalCubeId && cube.CubePortalSettings is not null) {
            session.Field.RemovePortal(cube.CubePortalSettings.PortalObjectId);
        }

        if (plot.IsPlanner) {
            return true;
        }

        if (!session.Item.Furnishing.RetrieveCube(cube.Id)) {
            throw new InvalidOperationException($"Failed to deposit cube {cube.Id} back into storage.");
        }

        return true;
    }

    public bool IsCoordOutsideArea(Vector3B position) {
        int height = Home.IsPlanner ? Home.PlannerHeight : Home.Height;
        int area = Home.IsPlanner ? Home.PlannerArea : Home.Area;

        // Check if the position is outside the planar area bounds
        if (position.X > 0 || position.Y > 0 || position.Z < 0) {
            return true;
        }

        area *= -1;
        if (position.X <= area || position.Y <= area) {
            return true;
        }

        // Check if the position is outside the height bounds
        if (position.Z > height) {
            return true;
        }

        return false;
    }

    public bool RequestLayout(HomeLayout layout, out (Dictionary<FurnishingCurrencyType, long> cubeCosts, int cubeCount) result) {
        Dictionary<int, int> groupedCubes = layout.Cubes.GroupBy(plotCube => plotCube.ItemId)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Count()); // Dictionary<item id, count>
        int cubeCount = 0;
        Dictionary<FurnishingCurrencyType, long> cubeCosts = new() {
            { FurnishingCurrencyType.Meso, 0 },
            { FurnishingCurrencyType.Meret, 0 },
        };

        foreach ((int id, int amount) in groupedCubes) {
            tableMetadata.FurnishingShopTable.Entries.TryGetValue(id, out FurnishingShopTable.Entry? shopEntry);
            if (shopEntry is null) {
                Log.Logger.Error("Failed to get shop entry for cube {cubeId}.", id);
                session.Send(CubePacket.Error(UgcMapError.s_err_cannot_buy_limited_item_more));
                result = (cubeCosts, cubeCount);
                return false;
            }

            Item? item = session.Item.Furnishing.GetItem(id);
            if (item is null) {
                cubeCosts[shopEntry.FurnishingTokenType] += shopEntry.Price * amount;
                cubeCount += amount;
                continue;
            }

            if (item.Amount >= amount) {
                continue;
            }

            int missingCubes = amount - item.Amount;
            cubeCosts[shopEntry.FurnishingTokenType] += shopEntry.Price * missingCubes;
            cubeCount += missingCubes;
        }

        result = (cubeCosts, cubeCount);
        return true;
    }

    public void ApplyLayout(Plot plot, HomeLayout layout, bool isBlueprint = false) {
        if (plot.IsPlanner) {
            Home.SetPlannerArea(layout.Area);
            Home.SetPlannerHeight(layout.Height);
        } else {
            Home.SetArea(layout.Area);
            Home.SetHeight(layout.Height);
        }

        session.Field.Broadcast(CubePacket.UpdateHomeAreaAndHeight(Home.Area, Home.Height));
        if (isBlueprint && plot.IsPlanner) {
            // If it's planner, only send packets don't save properties
            session.Field.Broadcast(CubePacket.SetBackground(layout.Background));
            session.Field.Broadcast(CubePacket.SetLighting(layout.Lighting));
            session.Field.Broadcast(CubePacket.SetCamera(layout.Camera));
        } else if (isBlueprint) {
            if (session.Player.Value.Home.SetBackground(layout.Background)) {
                session.Field.Broadcast(CubePacket.SetBackground(layout.Background));
            }

            if (session.Player.Value.Home.SetLighting(layout.Lighting)) {
                session.Field.Broadcast(CubePacket.SetLighting(layout.Lighting));
            }

            if (session.Player.Value.Home.SetCamera(layout.Camera)) {
                session.Field.Broadcast(CubePacket.SetCamera(layout.Camera));
            }
        }

        foreach (PlotCube cube in layout.Cubes) {
            Item? item = session.Item.Furnishing.GetItem(cube.ItemId);
            cube.Id = item?.Uid ?? 0;
            if (!TryPlaceCube(cube, plot, cube.Position, cube.Rotation, out PlotCube? plotCube)) {
                return;
            }

            ByteWriter sendPacket;
            if (plotCube.Position.Z == 0) {
                sendPacket = CubePacket.ReplaceCube(session.Player.ObjectId, plotCube);
            } else {
                sendPacket = CubePacket.PlaceCube(session.Player.ObjectId, plot, plotCube);
            }

            if (plotCube.ItemType.IsInteractFurnishing) {
                session.Field.Broadcast(FunctionCubePacket.AddFunctionCube(plotCube));
            }

            if (cube.CubePortalSettings is not null) {
                plotCube.CubePortalSettings = cube.CubePortalSettings;
            }

            session.Field.Broadcast(sendPacket);
        }

        List<PlotCube> cubePortals = plot.Cubes.Values
            .Where(x => x.ItemId is Constant.InteriorPortalCubeId && x.CubePortalSettings is not null)
            .ToList();

        foreach (PlotCube cubePortal in cubePortals) {
            FieldPortal fieldPortal = session.Field.SpawnCubePortal(cubePortal);
            session.Field.Broadcast(PortalPacket.Add(fieldPortal));
        }

        Vector3 position = Home.CalculateSafePosition(plot.Cubes.Values.ToList());
        foreach (FieldPlayer fieldPlayer in session.Field.Players.Values) {
            fieldPlayer.MoveToPosition(position, default);
        }

        session.Item.Furnishing.SendStorageCount();
        session.Housing.SaveHome();
        session.Field.Broadcast(NoticePacket.Message(StringCode.s_ugcmap_package_automatic_creation_completed, NoticePacket.Flags.Message | NoticePacket.Flags.Alert));
    }
    #endregion
}
