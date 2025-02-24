using Maple2.Model.Common;
using Maple2.Model.Enum;

namespace Maple2.Model.Game;

public class PlotCube : HeldCube {
    public enum CubeType { Default, Construction, Liftable };

    public Vector3B Position { get; set; }
    public float Rotation { get; set; }
    public CubeType Type { get; set; }

    public int PlotId { get; set; }

    public HousingCategory HousingCategory { get; set; }

    public InteractCube? Interact { get; set; }

    public PlotCube(int itemId, long id = 0, UgcItemLook? template = null) {
        ItemId = itemId;
        ItemType = new ItemType(itemId);
        Id = id;
        Template = template;
    }
}
