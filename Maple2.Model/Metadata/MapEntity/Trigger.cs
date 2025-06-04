using System.Numerics;

namespace Maple2.Model.Metadata;

public abstract record Trigger(int TriggerId, bool Visible) : MapBlock;

public record Ms2TriggerActor(
    string InitialSequence,
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerAgent(
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerBox(
    Vector3 Position,
    Vector3 Dimensions,
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerCamera(
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerCube(
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerEffect(
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerLadder(
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerMesh(
    float Scale,
    int TriggerId,
    bool Visible,
    bool MinimapInvisible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerPortal(
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerRope(
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerSkill(
    int SkillId,
    short Level,
    Vector3 Position,
    Vector3 Rotation,
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);

public record Ms2TriggerSound(
    int TriggerId,
    bool Visible)
: Trigger(TriggerId, Visible);
