using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.PacketHandlers;

public class RequestCubeHandler : FieldPacketHandler {
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
        InteriorDesignCheckIn = 40,
        InteriorDesignReward = 41,
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
        CalculateEstimate = 66,
        PreviewBlueprint = 68,
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
            case Command.InteriorDesignCheckIn:
                HandleInteriorDesignCheckIn(session);
                break;
            case Command.InteriorDesignReward:
                HandleInteriorDesignReward(session, packet);
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
            case Command.CreateBlueprint:
                HandleCreateBlueprint(session);
                break;
            case Command.SaveBlueprint:
                HandleSaveBlueprint(session, packet);
                break;
            case Command.LoadBlueprint:
                HandleLoadBlueprint(session, packet);
                break;
            case Command.CalculateEstimate:
                HandleCalculateEstimate(session, packet);
                break;
            case Command.PreviewBlueprint:
                HandlePreviewBlueprint(session, packet);
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

        session.Field?.PlaceCube(session, cubeItem, position, rotation);
    }

    private void HandleRemoveCube(GameSession session, IByteReader packet) {
        if (session.Field == null) return;
        var position = packet.Read<Vector3B>();

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        if (!session.Housing.TryRemoveCube(plot, position, out PlotCube? cube)) {
            return;
        }

        session.Field.Broadcast(CubePacket.RemoveCube(session.Player.ObjectId, position));
        if (plot.MapId is not Constant.DefaultHomeMapId) {
            session.World.UpdateFieldPlot(new FieldPlotRequest {
                IgnoreChannel = session.Channel,
                MapId = session.Field.MapId,
                PlotNumber = plot.Number,
                UpdateBlock = new FieldPlotRequest.Types.UpdateBlock {
                    X = position.X,
                    Y = position.Y,
                    Z = position.Z,
                    Remove = new FieldPlotRequest.Types.UpdateBlock.Types.Remove(),
                },
            });
        }
        if (plot.IsPlanner) {
            return;
        }

        session.ConditionUpdate(ConditionType.uninstall_item, codeLong: cube.ItemId);
    }

    private void HandleRotateCube(GameSession session, IByteReader packet) {
        if (session.Field == null) return;
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

        cube.Rotate(clockwise);

        session.Field.Broadcast(CubePacket.RotateCube(session.Player.ObjectId, cube));
        if (plot.MapId is not Constant.DefaultHomeMapId) {
            session.World.UpdateFieldPlot(new FieldPlotRequest {
                IgnoreChannel = session.Channel,
                MapId = session.Field.MapId,
                PlotNumber = plot.Number,
                UpdateBlock = new FieldPlotRequest.Types.UpdateBlock {
                    X = position.X,
                    Y = position.Y,
                    Z = position.Z,
                    Rotate = new FieldPlotRequest.Types.UpdateBlock.Types.Rotate {
                        Clockwise = clockwise,
                    },
                },
            });
        }

        if (plot.IsPlanner) {
            return;
        }

        session.ConditionUpdate(ConditionType.rotate_cube, codeLong: cube.ItemId);
    }

    private void HandleReplaceCube(GameSession session, IByteReader packet) {
        if (session.Field == null) return;

        var position = packet.Read<Vector3B>();
        var cubeItem = packet.ReadClass<HeldCube>();
        float rotation = packet.ReadFloat();

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        if (!session.ItemMetadata.TryGet(cubeItem.ItemId, out ItemMetadata? metadata)) {
            return;
        }

        if (!session.Housing.TryPlaceCube(cubeItem, plot, metadata, position, rotation, out PlotCube? placedCube, isReplace: true)) {
            return;
        }

        session.Field.Broadcast(CubePacket.ReplaceCube(session.Player.ObjectId, placedCube));

        if (plot.MapId is not Constant.DefaultHomeMapId) {
            session.World.UpdateFieldPlot(new FieldPlotRequest {
                IgnoreChannel = session.Channel,
                MapId = session.Field.MapId,
                PlotNumber = plot.Number,
                UpdateBlock = new FieldPlotRequest.Types.UpdateBlock {
                    BlockUid = placedCube.Id,
                    Replace = new FieldPlotRequest.Types.UpdateBlock.Types.Replace { },
                },
            });
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
        session.Player.InBattle = true;
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
            if (session.Housing.TryRemoveCube(plot, cube.Position, out _)) {
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

        if (session.Housing.RequestLayout(layout, out (Dictionary<FurnishingCurrencyType, long> cubeCosts, int cubeCount) result)) {
            session.Send(CubePacket.BuyCubes(result.cubeCosts, result.cubeCount));
        }
    }

    private void HandleIncreaseArea(GameSession session) {
        if (session.Field is null) return;
        if (session.Player.Value.Home.IsPlanner) {
            int decorArea = session.Player.Value.Home.PlannerArea + 1;
            if (session.Player.Value.Home.SetPlannerArea(decorArea)) {
                session.Field.Broadcast(CubePacket.IncreaseArea((byte) decorArea));
            }
            return;
        }

        int area = session.Player.Value.Home.Area + 1;
        if (session.Player.Value.Home.SetArea(area) && session.Housing.SaveHome()) {
            session.Field.Broadcast(CubePacket.IncreaseArea((byte) area));
        }
    }

    private void HandleDecreaseArea(GameSession session) {
        if (session.Field is null) return;
        int newArea;
        if (session.Player.Value.Home.IsPlanner) {
            newArea = session.Player.Value.Home.PlannerArea - 1;
            if (session.Player.Value.Home.SetPlannerArea(newArea)) {
                session.Field.Broadcast(CubePacket.DecreaseArea((byte) newArea));
            }
        } else {
            newArea = session.Player.Value.Home.Area - 1;
            if (session.Player.Value.Home.SetArea(newArea) && session.Housing.SaveHome()) {
                session.Field.Broadcast(CubePacket.DecreaseArea((byte) newArea));
            }
        }

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        // Remove cubes that are now outside the new area
        List<PlotCube> cubesToRemove = plot.Cubes.Values.Where(cube => session.Housing.IsCoordOutsideArea(cube.Position)).ToList();
        foreach (PlotCube cube in cubesToRemove) {
            if (session.Housing.TryRemoveCube(plot, cube.Position, out _)) {
                session.Field.Broadcast(CubePacket.RemoveCube(session.Player.ObjectId, cube.Position));
            }
        }

        Vector3 safeCoord = session.Player.Value.Home.CalculateSafePosition(plot.Cubes.Values.ToList());

        foreach (FieldPlayer fieldPlayer in session.Field.Players.Values) {
            fieldPlayer.MoveToPosition(safeCoord, default);
        }
    }

    private void HandleIncreaseHeight(GameSession session) {
        if (session.Field is null) return;
        if (session.Player.Value.Home.IsPlanner) {
            int decorHeight = session.Player.Value.Home.PlannerHeight + 1;
            if (session.Player.Value.Home.SetPlannerHeight(decorHeight)) {
                session.Field.Broadcast(CubePacket.IncreaseHeight((byte) decorHeight));
            }
            return;
        }

        int height = session.Player.Value.Home.Height + 1;
        if (session.Player.Value.Home.SetHeight(height) && session.Housing.SaveHome()) {
            session.Field.Broadcast(CubePacket.IncreaseHeight((byte) height));
        }
    }

    private void HandleDecreaseHeight(GameSession session) {
        if (session.Field is null) return;
        int newHeight;
        if (session.Player.Value.Home.IsPlanner) {
            newHeight = session.Player.Value.Home.PlannerHeight - 1;
            if (session.Player.Value.Home.SetPlannerHeight(newHeight)) {
                session.Field.Broadcast(CubePacket.DecreaseHeight((byte) newHeight));
            }
        } else {
            newHeight = session.Player.Value.Home.Height - 1;
            if (session.Player.Value.Home.SetHeight(newHeight) && session.Housing.SaveHome()) {
                session.Field.Broadcast(CubePacket.DecreaseHeight((byte) newHeight));
            }
        }

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        // Remove cubes that are now outside the new height
        List<PlotCube> cubesToRemove = plot.Cubes.Values.Where(cube => cube.Position.Z > newHeight).ToList();
        foreach (PlotCube cube in cubesToRemove) {
            if (session.Housing.TryRemoveCube(plot, cube.Position, out _)) {
                session.Field.Broadcast(CubePacket.RemoveCube(session.Player.ObjectId, cube.Position));
            }
        }

        Vector3 safeCoord = session.Player.Value.Home.CalculateSafePosition(plot.Cubes.Values.ToList());
        foreach (FieldPlayer fieldPlayer in session.Field.Players.Values) {
            fieldPlayer.MoveToPosition(safeCoord, default);
        }
    }

    private void HandleInteriorDesignCheckIn(GameSession session) {
        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null) {
            return;
        }

        if (plot.OwnerId != session.AccountId) {
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_dont_have_ownership));
            return;
        }

        session.Housing.InteriorCheckIn(plot);
    }

    private void HandleInteriorDesignReward(GameSession session, IByteReader packet) {
        byte rewardId = packet.ReadByte();
        session.Housing.InteriorReward(rewardId);
    }

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

        using GameStorage.Request db = session.GameStorage.Context();

        HomeLayout? layout = home.Layouts.FirstOrDefault(homeLayout => homeLayout.Id == slot);
        if (layout is not null) {
            home.Layouts.Remove(layout);
            db.RemoveHomeLayout(layout);
        }

        byte area = home.IsPlanner ? home.PlannerArea : home.Area;
        byte height = home.IsPlanner ? home.PlannerHeight : home.Height;
        layout = db.SaveHomeLayout(new HomeLayout(slot, name, area, height, DateTimeOffset.Now, plot.Cubes.Values.ToList()));
        if (layout is null) {
            Logger.Error("Failed to save layout for {AccountId}", session.AccountId);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return;
        }
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

        HomeLayout? layout;
        // blueprint load
        if (slot is 0) {
            if (session.Housing.StagedItemBlueprint is null) {
                return;
            }

            using GameStorage.Request db = session.GameStorage.Context();
            layout = db.GetHomeLayout(session.Housing.StagedItemBlueprint.BlueprintUid);
            if (layout is null) {
                Logger.Error("Failed to load layout for {AccountId}", session.AccountId);
                session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
                return;
            }
        } else {
            layout = home.Layouts.FirstOrDefault(homeLayout => homeLayout.Id == slot);
            if (layout is null) {
                Logger.Error("Failed to find layout {Slot} for {AccountId}", slot, session.AccountId);
                session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
                return;
            }
        }

        session.Housing.StagedItemBlueprint = null;
        session.Housing.ApplyLayout(plot, layout, isBlueprint: slot is 0);
    }


    private void HandleKickOut(GameSession session) { }

    private void HandleSetBackground(GameSession session, IByteReader packet) {
        if (session.Field is null) return;
        var background = packet.Read<HomeBackground>();
        if (session.Player.Value.Home.SetBackground(background)) {
            session.Field.Broadcast(CubePacket.SetBackground(background));
        }
    }

    private void HandleSetLighting(GameSession session, IByteReader packet) {
        if (session.Field is null) return;
        var lighting = packet.Read<HomeLighting>();
        if (session.Player.Value.Home.SetLighting(lighting)) {
            session.Field.Broadcast(CubePacket.SetLighting(lighting));
        }
    }

    private void HandleSetCamera(GameSession session, IByteReader packet) {
        if (session.Field is null) return;
        var camera = packet.Read<HomeCamera>();
        if (session.Player.Value.Home.SetCamera(camera)) {
            session.Field.Broadcast(CubePacket.SetCamera(camera));
        }
    }

    private void HandleCreateBlueprint(GameSession session) {
        Plot? plot = session.Housing.GetFieldPlot();
        if (plot is null) {
            return;
        }

        if (!TableMetadata.UgcDesignTable.Entries.TryGetValue(Constant.BlueprintId, out UgcDesignTable.Entry? design)) {
            Logger.Error("Failed to find design {BlueprintId}", Constant.BlueprintId);
            return;
        }

        long blueprintCost = design.CreatePrice;

        long negAmount = -1 * blueprintCost;
        if (session.Currency.CanAddMeret(negAmount) != negAmount) {
            session.Send(CubePacket.Error(UgcMapError.s_err_ugcmap_not_enough_meso_balance));
            return;
        }

        session.Currency.Meret -= blueprintCost;

        Item? item = session.Field?.ItemDrop.CreateItem(Constant.BlueprintId);
        if (item is null) {
            return;
        }

        Home home = session.Player.Value.Home;
        byte area = home.PlannerArea;
        byte height = home.PlannerHeight;
        using GameStorage.Request db = session.GameStorage.Context();

        var homeLayout = new HomeLayout(0, "Blueprint", area, height, DateTimeOffset.Now, plot.Cubes.Values.ToList()) {
            Background = home.Background,
            Lighting = home.Lighting,
            Camera = home.Camera,
        };
        HomeLayout? layout = db.SaveHomeLayout(homeLayout);
        if (layout is null) {
            Logger.Error("Failed to save layout for {AccountId}", session.AccountId);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return;
        }

        item.Blueprint = new ItemBlueprint {
            BlueprintUid = layout.Uid,
            Width = home.PlannerArea,
            Length = home.PlannerArea,
            Height = home.PlannerHeight,
            CreationTime = DateTimeOffset.Now,
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            CharacterName = session.PlayerName,
        };

        session.Item.Inventory.Add(item, notifyNew: true);

        session.StagedUgcItem = item;
        session.Send(CubePacket.CreateBlueprint(item.Uid, item.Blueprint!));
    }

    private void HandleSaveBlueprint(GameSession session, IByteReader packet) {
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

        using GameStorage.Request db = session.GameStorage.Context();

        HomeLayout? layout = home.Blueprints.FirstOrDefault(homeLayout => homeLayout.Id == slot);
        if (layout is not null) {
            home.Blueprints.Remove(layout);
            db.RemoveHomeLayout(layout);
        }

        byte area = home.PlannerArea;
        byte height = home.PlannerHeight;
        var homeLayout = new HomeLayout(slot, name, area, height, DateTimeOffset.Now, plot.Cubes.Values.ToList()) {
            Background = home.Background,
            Lighting = home.Lighting,
            Camera = home.Camera,
        };
        layout = db.SaveHomeLayout(homeLayout);
        if (layout is null) {
            Logger.Error("Failed to save layout for {AccountId}", session.AccountId);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return;
        }
        home.Blueprints.Add(layout);

        session.Housing.SaveHome();

        session.Send(CubePacket.SaveBlueprint(session.AccountId, layout));
    }

    private void HandleLoadBlueprint(GameSession session, IByteReader packet) {
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
        HomeLayout? layout = home.Blueprints.FirstOrDefault(homeLayout => homeLayout.Id == slot);
        if (layout is null) {
            return;
        }

        session.Housing.ApplyLayout(plot, layout, isBlueprint: true);
    }

    private void HandleCalculateEstimate(GameSession session, IByteReader packet) {
        long blueprintId = packet.ReadLong();

        using GameStorage.Request db = session.GameStorage.Context();
        HomeLayout? layout = db.GetHomeLayout(blueprintId);
        if (layout is null) {
            Logger.Error("Failed to load layout for {AccountId}", session.AccountId);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return;
        }

        if (session.Housing.RequestLayout(layout, out (Dictionary<FurnishingCurrencyType, long> cubeCosts, int cubeCount) result)) {
            session.Send(CubePacket.CalculateEstimate(result.cubeCosts, result.cubeCount));
        }
    }

    private void HandlePreviewBlueprint(GameSession session, IByteReader packet) {
        long blueprintId = packet.ReadLong();

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot is null) {
            return;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        HomeLayout? layout = db.GetHomeLayout(blueprintId);
        if (layout is null) {
            Logger.Error("Failed to load layout for {AccountId}", session.AccountId);
            session.Send(CubePacket.Error(UgcMapError.s_ugcmap_db));
            return;
        }

        session.Housing.ApplyLayout(plot, layout, isBlueprint: true);
    }
}
