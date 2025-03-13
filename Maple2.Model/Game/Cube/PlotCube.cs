using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.Model.Game;

public class PlotCube : HeldCube {
    public enum CubeType { Default, Construction, Liftable };
    public readonly ItemMetadata Metadata;

    public Vector3B Position { get; set; }
    public float Rotation { get; set; }
    public required CubeType Type { get; set; }

    public int PlotId { get; set; }

    public InteractCube? Interact { get; set; }

    public PlotCube(ItemMetadata metadata, long id = 0, UgcItemLook? template = null) {
        ItemId = metadata.Id;
        Metadata = metadata;
        ItemType = new ItemType(metadata.Id);
        Id = id;
        Template = template;
    }
}
