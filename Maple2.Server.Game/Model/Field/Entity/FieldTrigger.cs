using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Trigger;
using Maple2.Trigger;

namespace Maple2.Server.Game.Model;

public class FieldTrigger : FieldEntity<TriggerModel> {
    private static readonly TriggerLoader TriggerLoader = new();

    private readonly TriggerContext context;

    private long nextTick;
    private TriggerState? state;
    private TriggerState? nextState;

    public FieldTrigger(FieldManager field, int objectId, TriggerModel value) : base(field, objectId, value) {
        if (!TriggerLoader.TryGetTrigger(Field.Metadata.XBlock, Value.Name.ToLower(), out Func<ITriggerContext, TriggerState>? initialState)) {
            throw new ArgumentException($"Invalid trigger for {Field.Metadata.XBlock}, {Value.Name.ToLower()}");
        }

        context = new TriggerContext(this);
        nextState = initialState(context);
        nextTick = Environment.TickCount64;
    }

    public bool Skip() {
        if (context.Skip != null) {
            nextState = context.Skip;
            Field.Broadcast(CinematicPacket.StartSkip());
            return true;
        }

        return false;
    }

    public override void Update(long tickCount) {
        context.Events.InvokeAll();

        if (tickCount < nextTick) {
            return;
        }

        nextTick += Constant.NextStateTriggerDefaultTick;

        if (nextState != null) {
            context.DebugLog("[OnExit] {State}", state?.GetType().ToString() ?? "null");
            state?.OnExit();
            state = nextState;
            context.StartTick = Environment.TickCount64;
            context.DebugLog("[OnEnter] {State}", state.GetType());
            state.OnEnter();
            nextState = null;
        }

        nextState = state?.Execute();
    }
    /// <summary>
    /// Should only be used for debugging
    /// </summary>
    public void SetNextState(TriggerState state) {
        nextState = state;
    }
}
