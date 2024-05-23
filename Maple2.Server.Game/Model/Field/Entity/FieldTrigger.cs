using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Scripting.Trigger;
using Maple2.Server.Game.Trigger;

namespace Maple2.Server.Game.Model;

public class FieldTrigger : FieldEntity<TriggerModel> {
    private static readonly TriggerScriptLoader TriggerLoader = new();

    public readonly TriggerContext Context;

    private long nextTick;
    private TriggerState? state;
    private TriggerState? nextState;

    public FieldTrigger(FieldManager field, int objectId, TriggerModel value) : base(field, objectId, value) {
        Context = TriggerLoader.CreateContext(this);

        // We load the initial_state as nextState so on_enter() will be called.
        if (!TriggerLoader.TryInitScript(Context, Field.Metadata.XBlock, Value.Name.ToLower(), out nextState)) {
            throw new ArgumentException($"Invalid trigger for {Field.Metadata.XBlock}, {Value.Name.ToLower()}");
        }

        nextTick = Environment.TickCount64;
    }

    public bool Skip() {
        if (Context.TryGetSkip(out TriggerState? skip)) {
            nextState = skip;
            Field.Broadcast(CinematicPacket.StartSkip());
            return true;
        }

        return false;
    }

    public override void Update(long tickCount) {
        Context.Events.InvokeAll();

        if (tickCount < nextTick) {
            return;
        }

        nextTick += Constant.NextStateTriggerDefaultTick;

        if (nextState != null) {
            Context.DebugLog("[OnExit] {State}", state?.Name?.Value ?? "null");
            state?.OnExit();
            state = nextState;
            Context.StartTick = Environment.TickCount64;
            Context.DebugLog("[OnEnter] {State}", state.Name);
            nextState = state.OnEnter();

            // If OnEnter transitions to nextState, we skip OnTick.
            if (nextState != null) {
                return;
            }
        }

        nextState = state?.OnTick();
    }

    /// <summary>
    /// Should only be used for debugging
    /// </summary>
    public bool SetNextState(string next) {
        dynamic? stateClass = Context.Scope.GetVariable(next);
        if (stateClass == null) {
            return false;
        }

        nextState = Context.CreateState(stateClass);
        return nextState != null;
    }
}
