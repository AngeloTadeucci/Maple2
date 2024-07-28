using System;
using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class Home : IByteSerializable {
    private const byte HOME_PERMISSION_COUNT = 9;

    public long AccountId { get; init; }
    public long LastModified { get; set; }

    public byte Area { get; private set; }
    public byte Height { get; private set; }

    public int CurrentArchitectScore { get; set; }
    public int ArchitectScore { get; set; }

    // Interior Settings
    public HomeBackground Background { get; private set; }
    public HomeLighting Lighting { get; private set; }
    public HomeCamera Camera { get; private set; }
    public string? Passcode { get; set; }
    public readonly IDictionary<HomePermission, HomePermissionSetting> Permissions;

    public long DecorationExp { get; set; }
    public long DecorationLevel { get; set; }
    public long DecorationRewardTimestamp { get; set; }
    public List<int> InteriorRewardsClaimed { get; set; }

    private string message;
    public string Message {
        get => message;
        set {
            if (!string.IsNullOrWhiteSpace(value)) {
                message = value;
            }
        }
    }

    public PlotInfo Indoor { get; set; } = null!; // Required, when getting a home, this should be set always. Setting to null! to please the compiler.
    public PlotInfo? Outdoor { get; set; }

    public string Name => Outdoor?.Name ?? Indoor.Name;
    public int PlotMapId => Outdoor?.MapId ?? 0;
    public int PlotNumber => Outdoor?.Number ?? 0;
    public int ApartmentNumber => Outdoor?.ApartmentNumber ?? 0;
    public long PlotExpiryTime => Outdoor?.ExpiryTime ?? (string.IsNullOrEmpty(Name) ? 0 : Indoor.ExpiryTime); // If the name is empty, the plot is not setup yet.
    public PlotState State => Outdoor?.State ?? PlotState.Open;

    public Home() {
        message = string.Empty;
        Permissions = new Dictionary<HomePermission, HomePermissionSetting>();
        InteriorRewardsClaimed = [];
    }

    public bool SetArea(int area) {
        if (Area == area) return false;
        Area = (byte) Math.Clamp(area, Constant.MinHomeArea, Constant.MaxHomeArea);
        return Area == area;
    }

    public bool SetHeight(int height) {
        if (Height == height) return false;
        Height = (byte) Math.Clamp(height, Constant.MinHomeHeight, Constant.MaxHomeHeight);
        return Height == height;
    }

    public bool SetBackground(HomeBackground background) {
        if (Background == background || !System.Enum.IsDefined(background)) {
            return false;
        }

        Background = background;
        return true;
    }

    public bool SetLighting(HomeLighting lighting) {
        if (Lighting == lighting || !System.Enum.IsDefined(lighting)) {
            return false;
        }

        Lighting = lighting;
        return true;
    }

    public bool SetCamera(HomeCamera camera) {
        if (Camera == camera || !System.Enum.IsDefined(camera)) {
            return false;
        }

        Camera = camera;
        return true;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(AccountId);
        writer.WriteUnicodeString(Indoor.Name);
        writer.WriteUnicodeString(Message);
        writer.WriteByte();
        writer.WriteInt(CurrentArchitectScore);
        writer.WriteInt(ArchitectScore);
        writer.WriteInt(PlotMapId);
        writer.WriteInt(PlotNumber);
        writer.WriteByte(); // (1=Removes Top-Right UI)
        writer.WriteByte(Area);
        writer.WriteByte(Height);
        writer.Write<HomeBackground>(Background);
        writer.Write<HomeLighting>(Lighting);
        writer.Write<HomeCamera>(Camera);

        writer.WriteByte(HOME_PERMISSION_COUNT);
        for (byte i = 0; i < HOME_PERMISSION_COUNT; i++) {
            bool enabled = Permissions.TryGetValue((HomePermission) i, out HomePermissionSetting setting);
            writer.WriteBool(enabled);
            if (enabled) {
                writer.Write<HomePermissionSetting>(setting);
            }
        }
    }
}
