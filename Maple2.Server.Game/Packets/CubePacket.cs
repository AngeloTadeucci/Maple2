using System.Numerics;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Tools.Extensions;
// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Maple2.Server.Game.Packets;

public static class CubePacket {
    private enum Command : byte {
        // 0
        HoldCube = 1,
        BuyPlot = 2,
        ConfirmBuyPlot = 4,
        ForfeitPlot = 5,
        ConfirmForfeitPlot = 7,
        ExtendPlot = 9,
        PlaceCube = 10, // Also PurchaseCube?
        RemoveCube = 12,
        RotateCube = 14,
        ReplaceCube = 15,
        LiftupObject = 17,
        LiftupDrop = 18,
        UpdateProfile = 20,
        SetHomeName = 21,
        UpdatePlot = 22, // Buy/Extend/Forfeit
        SetPasscode = 24,
        ConfirmVote = 25,
        KickOut = 26,
        // 27
        ArchitectScore = 28,
        SetHomeMessage = 29,
        // 32
        // 33
        ReturnMap = 34,
        RequestLayout = 36,
        IncreaseArea = 37,
        DecreaseArea = 38,
        DesignRankReward = 39,
        // 40, 41
        EnablePermission = 42,
        SetPermission = 43,
        IncreaseHeight = 44,
        DecreaseHeight = 45,
        SaveLayout = 46,
        // 50
        SetBackground = 51,
        SetLighting = 52,
        GrantBuildPermissions = 53,
        SetCamera = 54,
        // 55
        UpdateBuildBudget = 56,
        AddBuildPermission = 57,
        RemoveBuildPermission = 58,
        LoadBuildPermission = 59,
        // Blueprint stuff:
        // 60, 61, 63, 64, 67, 69
        UpdateHomeAreaAndHeight = 62,
        CreateBlueprint = 63,
        SaveBlueprint = 64,
        CalculateEstimate = 67,
        FunctionCubeError = 71,
    }

    // A lot of ops contain this logic, we are just reusing Command.BuyPlot
    public static ByteWriter Error(UgcMapError error) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.BuyPlot);
        pWriter.Write<UgcMapError>(error);

        return pWriter;
    }

    public static ByteWriter HoldCube(int objectId, HeldCube cube) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.HoldCube);
        pWriter.WriteInt(objectId);
        pWriter.WriteClass<HeldCube>(cube);

        return pWriter;
    }

    public static ByteWriter BuyPlot(PlotInfo plot) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.BuyPlot);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteInt(plot.Number);
        pWriter.WriteInt(plot.ApartmentNumber);
        pWriter.WriteUnicodeString(plot.Name);
        pWriter.WriteLong(plot.ExpiryTime);
        pWriter.WriteLong(plot.OwnerId);

        return pWriter;
    }

    public static ByteWriter ConfirmBuyPlot() {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.ConfirmBuyPlot);

        return pWriter;
    }

    public static ByteWriter ForfeitPlot(PlotInfo plot) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.ForfeitPlot);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteInt(plot.Number);
        pWriter.WriteInt(plot.ApartmentNumber);
        pWriter.WriteBool(false); // This might be state?

        return pWriter;
    }

    public static ByteWriter ConfirmForfeitPlot(PlotInfo plot) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.ConfirmForfeitPlot);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteShort();
        pWriter.WriteInt(plot.MapId);
        pWriter.WriteInt(plot.Number);

        return pWriter;
    }

    public static ByteWriter ExtendPlot(PlotInfo plot) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.ExtendPlot);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteLong(plot.OwnerId);

        return pWriter;
    }

    public static ByteWriter PurchaseCube(int objectId) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.PlaceCube);
        pWriter.Write<UgcMapError>(UgcMapError.s_empty_string);
        pWriter.WriteInt(objectId); // Owner
        pWriter.WriteInt(objectId); // Player

        return pWriter;
    }

    public static ByteWriter PlaceCube(int objectId, PlotInfo plot, PlotCube cube) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.PlaceCube);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteInt(objectId); // Owner
        pWriter.WriteInt(objectId); // Player
        pWriter.WriteInt(plot.Number);
        pWriter.WriteInt(plot.ApartmentNumber);
        pWriter.Write<Vector3B>(cube.Position);
        pWriter.WriteLong(cube.Id);
        pWriter.WriteClass<HeldCube>(cube);
        pWriter.WriteBool(false); // Unknown
        pWriter.WriteFloat(cube.Rotation);
        pWriter.WriteInt(); // Unknown
        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter PlaceLiftable(int objectId, LiftableCube liftable, in Vector3 position, float rotation, bool isBound = false) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.PlaceCube);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteInt(objectId); // Owner
        pWriter.WriteInt(objectId); // Player
        pWriter.WriteInt(); // plot #
        pWriter.WriteInt();
        pWriter.Write<Vector3B>(position);
        pWriter.WriteLong(liftable.Id);
        pWriter.WriteClass<HeldCube>(liftable);
        pWriter.WriteBool(true); // Unknown
        pWriter.WriteFloat(rotation);
        pWriter.WriteInt(); // Unknown
        pWriter.WriteBool(isBound);
        if (isBound) {
            pWriter.WriteString();
            pWriter.WriteByte();
        }

        return pWriter;
    }

    public static ByteWriter RemoveCube(int objectId, in Vector3B position) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.RemoveCube);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteInt(objectId); // Owner
        pWriter.WriteInt(objectId); // Player
        pWriter.Write<Vector3B>(position);
        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter RotateCube(int objectId, PlotCube cube) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.RotateCube);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteInt(objectId); // Owner
        pWriter.WriteInt(objectId); // Player
        pWriter.Write<Vector3B>(cube.Position);
        pWriter.WriteFloat(cube.Rotation);

        return pWriter;
    }

    public static ByteWriter ReplaceCube(int objectId, PlotCube cube) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.ReplaceCube);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteInt(objectId); // Owner
        pWriter.WriteInt(objectId); // Player
        pWriter.Write<Vector3B>(cube.Position);
        pWriter.WriteLong(cube.Id);
        pWriter.WriteClass<HeldCube>(cube);
        pWriter.WriteBool(false); // Unknown
        pWriter.WriteFloat(cube.Rotation);
        pWriter.WriteInt(); // Unknown

        return pWriter;
    }

    public static ByteWriter LiftupObject(IActor<Player> player, LiftupWeapon liftupWeapon) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.LiftupObject);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteInt(player.ObjectId);
        pWriter.Write<Vector3B>(liftupWeapon.Object.Position);
        pWriter.WriteInt(liftupWeapon.ItemId);
        pWriter.WriteInt(Environment.TickCount + liftupWeapon.Object.RespawnTick);

        return pWriter;
    }

    public static ByteWriter LiftupDrop(IActor<Player> player) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.LiftupDrop);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteInt(player.ObjectId);

        return pWriter;
    }

    public static ByteWriter SetHomeName(Home home) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.SetHomeName);
        pWriter.WriteBool(false);
        pWriter.WriteLong(home.AccountId);
        pWriter.WriteInt(home.PlotNumber);
        pWriter.WriteInt(home.ApartmentNumber);
        pWriter.WriteUnicodeString(home.Indoor.Name);

        return pWriter;
    }

    public static ByteWriter UpdateProfile(Player player, bool load = false) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.UpdateProfile);
        pWriter.WriteInt(player.ObjectId);
        pWriter.WriteInt(player.Home.Indoor.MapId);
        pWriter.WriteInt(player.Home.PlotMapId);
        pWriter.WriteInt(player.Home.PlotNumber);
        pWriter.WriteInt(player.Home.ApartmentNumber);
        pWriter.WriteUnicodeString(player.Home.Name);
        pWriter.WriteLong(player.Home.PlotExpiryTime);
        pWriter.WriteLong(player.Home.LastModified);
        pWriter.WriteBool(load);

        return pWriter;
    }

    public static ByteWriter UpdatePlot(PlotInfo plot) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.UpdatePlot);
        pWriter.WriteInt(plot.Number);
        pWriter.WriteInt(plot.ApartmentNumber);
        pWriter.Write<PlotState>(plot.State);
        pWriter.WriteLong(plot.ExpiryTime);

        return pWriter;
    }
    public static ByteWriter SetPasscode() {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.SetPasscode);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);

        return pWriter;
    }

    public static ByteWriter ConfirmVote() {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.ConfirmVote);

        return pWriter;
    }

    public static ByteWriter KickOut() {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.KickOut);

        return pWriter;
    }

    public static ByteWriter ArchitectScore() {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.ArchitectScore);

        return pWriter;
    }

    public static ByteWriter SetHomeMessage(string message) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.SetHomeMessage);
        pWriter.WriteBool(false);
        pWriter.WriteUnicodeString(message);

        return pWriter;
    }

    public static ByteWriter ReturnMap(int mapId) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.ReturnMap);
        pWriter.WriteInt(mapId);

        return pWriter;
    }

    public static ByteWriter BuyCubes(Dictionary<FurnishingCurrencyType, long> cubeCosts, int cubeCount) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.RequestLayout);
        pWriter.WriteByte((byte) cubeCosts.Keys.Count);
        pWriter.WriteInt(cubeCount);
        foreach ((FurnishingCurrencyType moneyType, long amount) in cubeCosts) {
            pWriter.Write<FurnishingCurrencyType>(moneyType);
            pWriter.WriteLong(amount);
        }

        return pWriter;
    }

    public static ByteWriter IncreaseArea(byte area) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.IncreaseArea);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteByte(area);

        return pWriter;
    }

    public static ByteWriter DecreaseArea(byte area) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.DecreaseArea);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteByte(area);

        return pWriter;
    }

    public static ByteWriter DesignRankReward(Home home) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.DesignRankReward);
        pWriter.WriteLong(home.AccountId);
        pWriter.WriteLong(home.DecorationRewardTimestamp);
        pWriter.WriteLong(home.DecorationLevel);
        pWriter.WriteLong(home.DecorationExp);
        pWriter.WriteInt(home.InteriorRewardsClaimed.Count);

        foreach (int rewardId in home.InteriorRewardsClaimed) {
            pWriter.WriteInt(rewardId);
        }

        return pWriter;
    }

    public static ByteWriter EnablePermission(HomePermission permission, bool enabled) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.EnablePermission);
        pWriter.Write<HomePermission>(permission);
        pWriter.WriteBool(enabled);

        return pWriter;
    }

    public static ByteWriter SetPermission(HomePermission permission, HomePermissionSetting setting) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.SetPermission);
        pWriter.Write<HomePermission>(permission);
        pWriter.Write<HomePermissionSetting>(setting);

        return pWriter;
    }

    public static ByteWriter IncreaseHeight(byte height) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.IncreaseHeight);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteByte(height);

        return pWriter;
    }

    public static ByteWriter DecreaseHeight(byte height) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.DecreaseHeight);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteByte(height);

        return pWriter;
    }

    public static ByteWriter SaveLayout(long accountId, HomeLayout layout) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.SaveLayout);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteLong(accountId);
        pWriter.WriteClass<HomeLayout>(layout);

        return pWriter;
    }

    public static ByteWriter SetBackground(HomeBackground background) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.SetBackground);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.Write<HomeBackground>(background);

        return pWriter;
    }

    public static ByteWriter SetLighting(HomeLighting lighting) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.SetLighting);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.Write<HomeLighting>(lighting);

        return pWriter;
    }

    public static ByteWriter GrantBuildPermissions() {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.GrantBuildPermissions);

        return pWriter;
    }

    public static ByteWriter SetCamera(HomeCamera camera) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.SetCamera);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.Write<HomeCamera>(camera);

        return pWriter;
    }

    public static ByteWriter UpdateBuildBudget() {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.UpdateBuildBudget);

        return pWriter;
    }

    public static ByteWriter AddBuildPermission() {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.AddBuildPermission);

        return pWriter;
    }

    public static ByteWriter RemoveBuildPermission() {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.RemoveBuildPermission);

        return pWriter;
    }

    public static ByteWriter LoadBuildPermission() {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.LoadBuildPermission);

        return pWriter;
    }

    public static ByteWriter UpdateHomeAreaAndHeight(byte area, byte height) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.UpdateHomeAreaAndHeight);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteByte(area);
        pWriter.WriteByte(height);

        return pWriter;
    }

    public static ByteWriter CreateBlueprint(long itemUid, ItemBlueprint blueprint) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.CreateBlueprint);
        pWriter.Write<UgcMapError>(UgcMapError.s_empty_string);
        pWriter.WriteLong(itemUid);
        pWriter.WriteClass<ItemBlueprint>(blueprint);

        return pWriter;
    }

    public static ByteWriter SaveBlueprint(long accountId, HomeLayout layout) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.SaveBlueprint);
        pWriter.Write<UgcMapError>(UgcMapError.s_ugcmap_ok);
        pWriter.WriteLong(accountId);
        pWriter.WriteClass<HomeLayout>(layout);

        return pWriter;
    }

    public static ByteWriter CalculateEstimate(Dictionary<FurnishingCurrencyType, long> cubeCosts, int cubeCount) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.CalculateEstimate);
        pWriter.WriteByte((byte) cubeCosts.Keys.Count);
        pWriter.WriteInt(cubeCount);
        foreach ((FurnishingCurrencyType moneyType, long amount) in cubeCosts) {
            pWriter.Write<FurnishingCurrencyType>(moneyType);
            pWriter.WriteLong(amount);
        }
        pWriter.WriteBool(true); // Always true, if false doesn't show the estimate

        return pWriter;
    }

    public static ByteWriter FunctionCubeError(FunctionCubeError error) {
        var pWriter = Packet.Of(SendOp.ResponseCube);
        pWriter.Write<Command>(Command.FunctionCubeError);
        pWriter.Write<FunctionCubeError>(error);

        return pWriter;
    }
}
