using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.Model.Game;

public class PlotCube : HeldCube {
    public enum CubeType { Default, Construction, Liftable };

    private Vector3B position;
    public Vector3B Position {
        get => position;
        set {
            position = value;
            if (ItemType.IsInteractFurnishing) {
                InteractId = $"4_{AsHexadecimal()}";
            }
        }
    }
    public float Rotation { get; set; }
    public CubeType Type { get; set; }

    public int PlotId { get; set; }

    public string InteractId { get; set; } = "";
    public InteractCubeState InteractState { get; set; }
    public byte InteractUnkByte { get; set; }

    public HousingCategory HousingCategory { get; set; }

    public CubePortalSettings? PortalSettings { get; init; }

    public PlotCube(int itemId, long id = 0, UgcItemLook? template = null) {
        ItemId = itemId;
        ItemType = new ItemType(itemId);
        Id = id;
        Template = template;

        if (itemId is Constant.InteriorPortalCubeId) {
            PortalSettings = new CubePortalSettings();
        }
    }

    /// <summary>
    /// Get the cube coord, transform to hexa, reverse and then transform to long;
    /// Example: (-1, -1, 1);
    /// Reverse and transform to hexadecimal as string: '1FFFF';
    /// Convert the string above to long: 65535.
    /// </summary>
    private long AsHexadecimal() {
        string coordRevertedAsString = $"{Position.Z:X2}{Position.Y:X2}{Position.X:X2}";
        return Convert.ToInt64(coordRevertedAsString, 16);
    }
}
