using Maple2.Model.Metadata;

namespace Maple2.Model.Game;

public class LiftableCube : HeldCube {
    public readonly Liftable Liftable;

    public LiftableCube(Liftable liftable) {
        Liftable = liftable;
        ItemId = liftable.ItemId;
        Id = Random.Shared.NextInt64();
    }
}
