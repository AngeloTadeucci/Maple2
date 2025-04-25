using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Manager.Field;
using Maple2.Tools.VectorMath;
using Maple2.Tools.Collision;
using Maple2.Database.Storage;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Model.ActorStateComponent;
using Maple2.Server.Game.Model.Skill;

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
    public StatsManager Stats { get; }
    public AnimationManager Animation { get; init; }
    public SkillState SkillState { get; init; }
    public SkillQueue ActiveSkills { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldActor"/> class, setting up its field association, metadata, stats, buffs, transform, animation manager, skill state, and active skills queue.
    /// </summary>
    /// <param name="field">The field manager that owns this actor.</param>
    /// <param name="npcMetadata">The metadata storage for NPCs associated with this actor.</param>
    public FieldActor(FieldManager field, NpcMetadataStorage npcMetadata) {
        Field = field;
        Stats = new StatsManager(this);
        Buffs = new BuffManager(this);
        Transform = new Transform();
        NpcMetadata = npcMetadata;
        Animation = new AnimationManager(this); // not meant to have animations
        SkillState = new SkillState(this);
        ActiveSkills = new SkillQueue();
    }

    /// <summary>
/// Performs no operation during the update cycle for this actor.
/// </summary>
public void Update(long tickCount) { }
}
