using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Manager.Field;
using Maple2.Tools.VectorMath;
using Maple2.Tools.Collision;
using Maple2.Server.Game.Model.Field.Actor.ActorState;
using Maple2.Database.Storage;

namespace Maple2.Server.Game.Model;

// Dummy Actor for field to own entities.
internal sealed class FieldActor : IActor {
    public FieldManager Field { get; }
    public NpcMetadataStorage NpcMetadata { get; init; }

    public int ObjectId => 0;
    public bool IsDead => false;
    public IPrism Shape => new PointPrism(Position);
    public ActorState State => ActorState.None;
    public ActorSubState SubState => ActorSubState.None;
    public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
    public Vector3 Rotation { get => Transform.RotationAnglesDegrees; set => Transform.RotationAnglesDegrees = value; }
    public Transform Transform { get; init; }
    public BuffManager Buffs { get; }
    public Stats Stats { get; }
    public AnimationState AnimationState { get; init; }

    public FieldActor(FieldManager field, NpcMetadataStorage npcMetadata) {
        Field = field;
        Stats = new Stats(0, 0);
        Buffs = new BuffManager(this);
        Transform = new Transform();
        NpcMetadata = npcMetadata;
        AnimationState = new AnimationState(this, string.Empty); // not meant to have animations
    }

    public void Update(long tickCount) { }
}
