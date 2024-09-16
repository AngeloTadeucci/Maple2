using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Items;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.PacketHandlers;

public class RequestCubeHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RequestCube;

    private enum Command : byte {
        HoldCube = 1,
        BuyPlot = 2,
        ForfeitPlot = 6,
        ExtendPlot = 9,
        PlaceCube = 10,
        RemoveCube = 12,
        RotateCube = 14,
        ReplaceCube = 15,
        LiftupObject = 17,
        LiftupDrop = 18,
        SetHomeName = 21,
        SetPasscode = 24,
        VoteHome = 25,
        SetHomeMessage = 29,
        ClearCubes = 31,
        RequestLayout = 35,
        IncreaseArea = 37,
        DecreaseArea = 38,
        DesignRankReward = 40,
        EnablePermission = 42,
        SetPermission = 43,
        IncreaseHeight = 44,
        DecreaseHeight = 45,
        SaveLayout = 46,
        DecorPlannerLoadLayout = 47,
        LoadLayout = 48,
        KickOut = 49,
        SetBackground = 51,
        SetLighting = 52,
        SetCamera = 54,
        CreateBlueprint = 63,
        SaveBlueprint = 64,
        LoadBlueprint = 65,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.HoldCube:
                HandleHoldCube(session, packet);
                return;
            case Command.BuyPlot:
                HandleBuyPlot(session, packet);
                return;
            case Command.ForfeitPlot:
                HandleForfeitPlot(session);
                break;
            case Command.ExtendPlot:
                HandleExtendPlot(session);
                break;
            case Command.PlaceCube:
                HandlePlaceCube(session, packet);
                break;
            case Command.RemoveCube:
                HandleRemoveCube(session, packet);
                break;
            case Command.RotateCube:
                HandleRotateCube(session, packet);
                break;
            case Command.ReplaceCube:
                HandleReplaceCube(session, packet);
                break;
            case Command.LiftupObject:
                HandleLiftupObject(session, packet);
                break;
            case Command.LiftupDrop:
                HandleLiftupDrop(session);
                break;
            case Command.SetHomeName:
                HandleSetHomeName(session, packet);
                break;
            case Command.SetPasscode:
                HandleSetPasscode(session, packet);
                break;
            case Command.VoteHome:
                HandleVoteHome(session);
                break;
            case Command.SetHomeMessage:
                HandleSetHomeMessage(session, packet);
                break;
            case Command.ClearCubes:
                HandleClearCubes(session);
                break;
            case Command.RequestLayout:
                HandleRequestLayout(session, packet);
                break;
            case Command.IncreaseArea:
                HandleIncreaseArea(session);
                break;
            case Command.DecreaseArea:
                HandleDecreaseArea(session);
                break;
            case Command.DesignRankReward:
                HandleDesignRankReward(session);
                break;
            case Command.EnablePermission:
                HandleEnablePermission(session, packet);
                break;
            case Command.SetPermission:
                HandleSetPermission(session, packet);
                break;
            case Command.IncreaseHeight:
                HandleIncreaseHeight(session);
                break;
            case Command.DecreaseHeight:
                HandleDecreaseHeight(session);
                break;
            case Command.SaveLayout:
                HandleSaveLayout(session, packet);
                break;
            case Command.DecorPlannerLoadLayout:
            case Command.LoadLayout:
                HandleLoadLayout(session, packet);
                break;
            case Command.KickOut:
                HandleKickOut(session);
                break;
            case Command.SetBackground:
                HandleSetBackground(session, packet);
                break;
            case Command.SetLighting:
                HandleSetLighting(session, packet);
                break;
            case Command.SetCamera:
                HandleSetCamera(session, packet);
                break;
            case Command.SaveBlueprint:
                HandleSaveBlueprint(session, packet);
                break;
            case Command.LoadBlueprint:
                HandleLoadBlueprint(session, packet);
                break;
        }
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    private void HandleHoldCube(GameSession session, IByteReader packet) {
        var cubeItem = packet.ReadClass<PlotCube>();

        if (session.GuideObject == null || session.GuideObject.Value.Type != GuideObjectType.Construction) {
            return;
        }

        session.HeldCube = cubeItem;
        session.Field?.Broadcast(CubePacket.HoldCube(session.Player.ObjectId, session.HeldCube));
    }

    private void HandleBuyPlot(GameSession session, IByteReader packet) {
        int plotNumber = packet.ReadInt();
        packet.ReadInt(); // ApartmentNumber?

        if (session.Housing.BuyPlot(plotNumber)) {
            session.Send(CubePacket.ConfirmBuyPlot());
        }
    }

    private void HandleForfeitPlot(GameSession session) {
        PlotInfo? plot = session.Housing.ForfeitPlot();
        if (plot != null) {
            session.Send(CubePacket.ConfirmForfeitPlot(plot));
        }
    }

    private void HandleExtendPlot(GameSession session) {
        session.Housing.ExtendPlot();
    }

    private void HandlePlaceCube(GameSession session, IByteReader packet) {
        var position = packet.Read<Vector3B>();
        var cubeItem = packet.ReadClass<HeldCube>();
        float rotation = packet.ReadFloat();

        if (session.Field == null) {
            return;
        }

        switch (session.HeldCube) {
            case PlotCube _:
                Plot? plot = session.Housing.GetFieldPlot();
                if (plot == null) {
                    return;
                }

                if (!TryPlaceCube(session, cubeItem, plot, position, rotation, out PlotCube? plotCube)) {
                    return;
                }

                session.Field.Broadcast(CubePacket.PlaceCube(session.Player.ObjectId, plot, plotCube));

                if (plot.IsPlanner) {
                    return;
                }
                break;
            case LiftableCube liftable:
                FieldLiftable? fieldLiftable = session.Field.AddLiftable(position.ToString(), liftable.Liftable);
                if (fieldLiftable == null) {
                    return;
                }

                session.HeldCube = null;
                fieldLiftable.Count = 1;
                fieldLiftable.State = LiftableState.Disabled;
                fieldLiftable.Position = position;
                fieldLiftable.Rotation = new Vector3(0, 0, rotation);
                fieldLiftable.FinishTick = session.Field.FieldTick + fieldLiftable.Value.FinishTime + fieldLiftable.Value.ItemLifetime;

                if (session.Field.Entities.LiftableTargetBoxes.TryGetValue(position, out LiftableTargetBox? liftableTarget)) {
                    session.ConditionUpdate(ConditionType.item_move, codeLong: cubeItem.ItemId, targetLong: liftableTarget.LiftableTarget);
                }

                session.Field.Broadcast(LiftablePacket.Add(fieldLiftable));
                session.Field.Broadcast(CubePacket.PlaceLiftable(session.Player.ObjectId, liftable, position, rotation));
                session.Field.Broadcast(SetCraftModePacket.Stop(session.Player.ObjectId));
                session.Field.Broadcast(LiftablePacket.Update(fieldLiftable));
                break;
        }

        session.ConditionUpdate(ConditionType.install_item, codeLong: cubeItem.ItemId);
    }

    private void HandleRemoveCube(GameSession session, IByteReader packet) {
        var position = packet.Read<Vector3B>();

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        if (!TryRemoveCube(session, plot, position, out PlotCube? cube)) {
            return;
        }

        session.Field.Broadcast(CubePacket.RemoveCube(session.Player.ObjectId, position));
        if (plot.IsPlanner) {
            return;
        }

        session.ConditionUpdate(ConditionType.uninstall_item, codeLong: cube.ItemId);
    }

    private void HandleRotateCube(GameSession session, IByteReader packet) {
        var position = packet.Read<Vector3B>();
        bool clockwise = packet.ReadBool();

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }
        if (!plot.Cubes.TryGetValue(position, out PlotCube? cube)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_no_cube_to_rotate));
            return;
        }

        if (clockwise) {
            cube.Rotation = (cube.Rotation + 270f) % 360f; // Rotate clockwise
        } else {
            cube.Rotation = (cube.Rotation + 90f) % 360f; // Rotate counter-clockwise
        }

        session.Field?.Broadcast(CubePacket.RotateCube(session.Player.ObjectId, cube));
        if (plot.IsPlanner) {
            return;
        }

        session.ConditionUpdate(ConditionType.rotate_cube, codeLong: cube.ItemId);
    }

    private void HandleReplaceCube(GameSession session, IByteReader packet) {
        var position = packet.Read<Vector3B>();
        var cubeItem = packet.ReadClass<HeldCube>();
        float rotation = packet.ReadFloat();

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        if (TryPlaceCube(session, cubeItem, plot, position, rotation, out PlotCube? placedCube, isReplace: true)) {
            session.Field?.Broadcast(CubePacket.ReplaceCube(session.Player.ObjectId, placedCube));
        }
    }

    private void HandleLiftupObject(GameSession session, IByteReader packet) {
        var position = packet.Read<Vector3B>();

        if (session.Field == null || session.HeldLiftup != null) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_not_allowed_item));
            return;
        }

        if (!session.Field.LiftupCube(position, out LiftupWeapon? weapon)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_no_cube_to_lift));
            return;
        }

        session.HeldLiftup = weapon;
        session.Field.Broadcast(CubePacket.LiftupObject(session.Player, session.HeldLiftup));
    }

    private void HandleLiftupDrop(GameSession session) {
        if (session.Field == null || session.HeldLiftup == null) {
            return;
        }

        session.HeldLiftup = null;
        session.Field.Broadcast(CubePacket.LiftupDrop(session.Player));
    }

    private void HandleSetHomeName(GameSession session, IByteReader packet) {
        string name = packet.ReadUnicodeString();
        session.Housing.SetName(name);
    }

    private void HandleSetPasscode(GameSession session, IByteReader packet) {
        bool hasPasscode = packet.ReadBool();
        string passcode = string.Empty;
        if (hasPasscode) {
            passcode = packet.ReadUnicodeString();
        }

        session.Housing.SetPasscode(passcode);
    }

    private void HandleVoteHome(GameSession session) { }

    private void HandleSetHomeMessage(GameSession session, IByteReader packet) {
        string message = packet.ReadUnicodeString();
        session.Housing.SetHomeMessage(message);
    }

    private void HandleClearCubes(GameSession session) {
        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        foreach (PlotCube cube in plot.Cubes.Values) {
            if (TryRemoveCube(session, plot, cube.Position, out _)) {
                session.Field?.Broadcast(CubePacket.RemoveCube(session.Player.ObjectId, cube.Position));
            }
        }

        session.Send(NoticePacket.Message(StringCode.s_ugcmap_package_automatic_removal_completed, NoticePacket.Flags.Message | NoticePacket.Flags.Alert));
    }

    private void HandleRequestLayout(GameSession session, IByteReader packet) {
        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        if (plot.Cubes.Count != 0) {
            session.Send(NoticePacket.Message(StringCode.s_err_ugcmap_package_clear_indoor_first, NoticePacket.Flags.Message | NoticePacket.Flags.Alert));
            return;
        }

        int slot = packet.ReadInt();

        HomeLayout? layout = session.Player.Value.Home.Layouts.FirstOrDefault(homeLayout => homeLayout.Id == slot);
        if (layout == null) {
            return;
        }

        Dictionary<int, int> groupedCubes = layout.Cubes.GroupBy(plotCube => plotCube.ItemId).ToDictionary(grouping => grouping.Key, grouping => grouping.Count()); // Dictionary<item id, count>
        int cubeCount = 0;
        Dictionary<FurnishingCurrencyType, long> cubeCosts = new();
        cubeCosts.Add(FurnishingCurrencyType.Meso, 0);
        cubeCosts.Add(FurnishingCurrencyType.Meret, 0);
        foreach ((int id, int amount) in groupedCubes) {
            TableMetadata.FurnishingShopTable.Entries.TryGetValue(id, out FurnishingShopTable.Entry? shopEntry);
            if (shopEntry is null) {
                Logger.Error("Failed to get shop entry for cube {cubeId}.", id);
                session.Send(CubePacket.Error(UgcMapError.s_err_cannot_buy_limited_item_more));
                return;
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

        session.Send(CubePacket.BuyCubes(cubeCosts, cubeCount));
    }

    private void HandleIncreaseArea(GameSession session) {
        if (session.Player.Value.Home.IsPlanner) {
            int decorArea = session.Player.Value.Home.PlannerArea + 1;
            if (session.Player.Value.Home.SetPlannerArea(decorArea)) {
                session.Field?.Broadcast(CubePacket.IncreaseArea((byte) decorArea));
            }
            return;
        }

        int area = session.Player.Value.Home.Area + 1;
        if (session.Player.Value.Home.SetArea(area) && session.Housing.SaveHome()) {
            session.Field?.Broadcast(CubePacket.IncreaseArea((byte) area));
        }
    }

    private void HandleDecreaseArea(GameSession session) {
        int newArea;
        if (session.Player.Value.Home.IsPlanner) {
            newArea = session.Player.Value.Home.PlannerArea - 1;
            if (session.Player.Value.Home.SetPlannerArea(newArea)) {
                session.Field?.Broadcast(CubePacket.DecreaseArea((byte) newArea));
            }
        } else {
            newArea = session.Player.Value.Home.Area - 1;
            if (session.Player.Value.Home.SetArea(newArea) && session.Housing.SaveHome()) {
                session.Field?.Broadcast(CubePacket.DecreaseArea((byte) newArea));
            }
        }

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        // Remove cubes that are now outside the new area
        List<PlotCube> cubesToRemove = plot.Cubes.Values.Where(cube => IsCoordOutsideArea(cube.Position, session.Player.Value.Home)).ToList();
        foreach (PlotCube cube in cubesToRemove) {
            if (TryRemoveCube(session, plot, cube.Position, out _)) {
                session.Field?.Broadcast(CubePacket.RemoveCube(session.Player.ObjectId, cube.Position));
            }
        }

        Vector3 safeCoord = session.Player.Value.Home.CalculateSafePosition(plot.Cubes.Values.ToList());

        foreach (FieldPlayer fieldPlayer in session.Field.Players.Values) {
            fieldPlayer.MoveToPosition(safeCoord, default);
        }
    }

    private void HandleIncreaseHeight(GameSession session) {
        if (session.Player.Value.Home.IsPlanner) {
            int decorHeight = session.Player.Value.Home.PlannerHeight + 1;
            if (session.Player.Value.Home.SetPlannerHeight(decorHeight)) {
                session.Field?.Broadcast(CubePacket.IncreaseHeight((byte) decorHeight));
            }
            return;
        }

        int height = session.Player.Value.Home.Height + 1;
        if (session.Player.Value.Home.SetHeight(height) && session.Housing.SaveHome()) {
            session.Field?.Broadcast(CubePacket.IncreaseHeight((byte) height));
        }
    }

    private void HandleDecreaseHeight(GameSession session) {
        int newHeight;
        if (session.Player.Value.Home.IsPlanner) {
            newHeight = session.Player.Value.Home.PlannerHeight - 1;
            if (session.Player.Value.Home.SetPlannerHeight(newHeight)) {
                session.Field?.Broadcast(CubePacket.DecreaseHeight((byte) newHeight));
            }
        } else {
            newHeight = session.Player.Value.Home.Height - 1;
            if (session.Player.Value.Home.SetHeight(newHeight) && session.Housing.SaveHome()) {
                session.Field?.Broadcast(CubePacket.DecreaseHeight((byte) newHeight));
            }
        }

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        // Remove cubes that are now outside the new height
        List<PlotCube> cubesToRemove = plot.Cubes.Values.Where(cube => cube.Position.Z > newHeight).ToList();
        foreach (PlotCube cube in cubesToRemove) {
            if (TryRemoveCube(session, plot, cube.Position, out _)) {
                session.Field?.Broadcast(CubePacket.RemoveCube(session.Player.ObjectId, cube.Position));
            }
        }

        Vector3 safeCoord = session.Player.Value.Home.CalculateSafePosition(plot.Cubes.Values.ToList());
        foreach (FieldPlayer fieldPlayer in session.Field!.Players.Values) {
            fieldPlayer.MoveToPosition(safeCoord, default);
        }
    }

    private void HandleDesignRankReward(GameSession session) { }

    private void HandleEnablePermission(GameSession session, IByteReader packet) {
        var permission = packet.Read<HomePermission>();
        bool enabled = packet.ReadBool();

        if (enabled) {
            session.Player.Value.Home.Permissions[permission] = HomePermissionSetting.None;
        } else {
            session.Player.Value.Home.Permissions.Remove(permission);
        }

        session.Field?.Broadcast(CubePacket.EnablePermission(permission, enabled));
    }

    private void HandleSetPermission(GameSession session, IByteReader packet) {
        var permission = packet.Read<HomePermission>();
        var setting = packet.Read<HomePermissionSetting>();

        if (session.Player.Value.Home.Permissions.ContainsKey(permission)) {
            session.Player.Value.Home.Permissions[permission] = setting;
        } else {
            setting = HomePermissionSetting.None;
        }

        session.Field?.Broadcast(CubePacket.SetPermission(permission, setting));
    }

    private void HandleSaveLayout(GameSession session, IByteReader packet) {
        int slot = packet.ReadInt();
        string name = packet.ReadUnicodeString();

        Home home = session.Player.Value.Home;
        if (slot is > Constant.HomeMaxLayoutSlots or < 0) {
            return;
        }

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        HomeLayout? layout = home.Layouts.FirstOrDefault(homeLayout => homeLayout.Id == slot);
        if (layout is not null) {
            home.Layouts.Remove(layout);
        }

        byte area = home.IsPlanner ? home.PlannerArea : home.Area;
        byte height = home.IsPlanner ? home.PlannerHeight : home.Height;
        layout = new HomeLayout(slot, name, area, height, DateTimeOffset.Now, plot.Cubes.Values.ToList());
        home.Layouts.Add(layout);

        session.Housing.SaveHome();

        session.Send(CubePacket.SaveLayout(session.AccountId, layout));
    }

    private void HandleLoadLayout(GameSession session, IByteReader packet) {
        Plot? plot = session.Housing.GetFieldPlot();
        if (plot is null) {
            return;
        }

        if (plot.Cubes.Count != 0) {
            session.Send(NoticePacket.Message(StringCode.s_err_ugcmap_package_clear_indoor_first, NoticePacket.Flags.Message | NoticePacket.Flags.Alert));
            return;
        }

        int slot = packet.ReadInt();

        Home home = session.Player.Value.Home;
        HomeLayout? layout = home.Layouts.FirstOrDefault(homeLayout => homeLayout.Id == slot);
        if (layout is null) {
            return;
        }

        if (plot.IsPlanner) {
            home.SetPlannerArea(layout.Area);
            home.SetPlannerHeight(layout.Height);
        } else {
            home.SetArea(layout.Area);
            home.SetHeight(layout.Height);
        }
        session.Field.Broadcast(CubePacket.UpdateHomeAreaAndHeight(home.Area, home.Height));

        foreach (PlotCube cube in layout.Cubes) {
            if (!TryPlaceCube(session, cube, plot, cube.Position, cube.Rotation, out PlotCube? plotCube)) {
                return;
            }

            ByteWriter sendPacket;
            if (cube.Position.Z == 0) {
                sendPacket = CubePacket.ReplaceCube(session.Player.ObjectId, plotCube);
            } else {
                sendPacket = CubePacket.PlaceCube(session.Player.ObjectId, plot, plotCube);
            }

            session.Field.Broadcast(sendPacket);
        }

        Vector3 position = home.CalculateSafePosition(plot.Cubes.Values.ToList());
        foreach (FieldPlayer fieldPlayer in session.Field.Players.Values) {
            fieldPlayer.MoveToPosition(position, default);
        }

        session.Item.Furnishing.SendStorageCount();
        session.Housing.SaveHome();
        session.Field.Broadcast(NoticePacket.Message(StringCode.s_ugcmap_package_automatic_creation_completed, NoticePacket.Flags.Message | NoticePacket.Flags.Alert));
    }

    private void HandleKickOut(GameSession session) { }

    private void HandleSetBackground(GameSession session, IByteReader packet) {
        var background = packet.Read<HomeBackground>();
        if (session.Player.Value.Home.SetBackground(background)) {
            session.Field?.Broadcast(CubePacket.SetBackground(background));
        }
    }

    private void HandleSetLighting(GameSession session, IByteReader packet) {
        var lighting = packet.Read<HomeLighting>();
        if (session.Player.Value.Home.SetLighting(lighting)) {
            session.Field?.Broadcast(CubePacket.SetLighting(lighting));
        }
    }

    private void HandleSetCamera(GameSession session, IByteReader packet) {
        var camera = packet.Read<HomeCamera>();
        if (session.Player.Value.Home.SetCamera(camera)) {
            session.Field?.Broadcast(CubePacket.SetCamera(camera));
        }
    }

    private void HandleSaveBlueprint(GameSession session, IByteReader packet) {
        int slot = packet.ReadInt();
        string name = packet.ReadUnicodeString();
    }

    private void HandleLoadBlueprint(GameSession session, IByteReader packet) {
        int slot = packet.ReadInt();
    }

    #region Helpers
    private bool TryPlaceCube(GameSession session, HeldCube cube, Plot plot, in Vector3B position, float rotation,
                              [NotNullWhen(true)] out PlotCube? result, bool isReplace = false) {
        result = null;
        if (!session.ItemMetadata.TryGet(cube.ItemId, out ItemMetadata? itemMetadata) || itemMetadata.Install is null) {
            Logger.Error("Failed to get item metadata for cube {cubeId}.", cube.ItemId);
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
            TryRemoveCube(session, plot, position, out _);
        }

        //TODO: check outside plot - coords belongs to plot

        // TODO: check outside plot bounds

        if (IsCoordOutsideArea(position, session.Player.Value.Home)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_area_limit));
            return false;
        }

        if (plot.IsPlanner) {
            result = new PlotCube(cube.ItemId, id: FurnishingManager.NextCubeId(), template: cube.Template) {
                Position = position,
                Rotation = rotation,
            };

            plot.Cubes.Add(position, result);
            return true;
        }

        TableMetadata.FurnishingShopTable.Entries.TryGetValue(cube.ItemId, out FurnishingShopTable.Entry? shopEntry);
        if (shopEntry is null) {
            session.Send(CubePacket.Error(UgcMapError.s_err_cannot_buy_limited_item_more));
            return false;
        }

        if (!session.Item.Furnishing.PurchaseCube(shopEntry)) {
            return false;
        }

        if (!session.Item.Furnishing.TryPlaceCube(cube.Id, out result)) {
            long itemUid = session.Item.Furnishing.AddCube(cube.ItemId);
            if (itemUid == 0) {
                session.Send(CubePacket.Error(UgcMapError.s_ugcmap_not_for_sale));
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
        plot.Cubes.Add(position, result);
        return true;
    }

    private static bool TryRemoveCube(GameSession session, Plot plot, in Vector3B position, [NotNullWhen(true)] out PlotCube? cube) {
        if (!plot.Cubes.Remove(position, out cube)) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_no_cube_to_remove));
            return false;
        }

        if (plot.IsPlanner) {
            return true;
        }

        if (!session.Item.Furnishing.RetrieveCube(cube.Id)) {
            throw new InvalidOperationException($"Failed to deposit cube {cube.Id} back into storage.");
        }

        return true;
    }

    private static bool IsCoordOutsideArea(Vector3B position, Home home) {
        int height = home.IsPlanner ? home.PlannerHeight : home.Height;
        int area = home.IsPlanner ? home.PlannerArea : home.Area;

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
    #endregion
}
