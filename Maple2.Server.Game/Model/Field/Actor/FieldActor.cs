using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Manager.Field;
using Maple2.Tools.Collision;

namespace Maple2.Server.Game.Model;

// Dummy Actor for field to own entities.
internal sealed class FieldActor : IActor {
    public FieldManager Field { get; }

    public int ObjectId => 0;
    public bool IsDead => false;
    public IPrism Shape => new PointPrism(Position);
    public ActorState State => ActorState.None;
    public ActorSubState SubState => ActorSubState.None;
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public BuffManager Buffs { get; }
    public Stats Stats { get; }

    public FieldActor(FieldManager field) {
        Field = field;
        Stats = new Stats(0, 0);
        Buffs = new BuffManager(this);
    }

    public void Update(long tickCount) { }
}
