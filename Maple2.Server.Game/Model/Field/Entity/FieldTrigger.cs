using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Trigger;
using Maple2.Server.Game.Trigger.Helpers;
using Serilog;

namespace Maple2.Server.Game.Model;

public class FieldTrigger : FieldEntity<TriggerModel> {
    public readonly TriggerContext Context;
    private readonly Trigger.Helpers.Trigger trigger;

    private long nextTick;
    private TriggerState? state;
    private TriggerState? nextState;

    public FieldTrigger(FieldManager field, int objectId, TriggerModel value) : base(field, objectId, value) {
        Context = new TriggerContext(this);
        if (field.TriggerCache.TryGet(field.Metadata.XBlock, Value.Name.ToLower(), out Trigger.Helpers.Trigger? cachedTrigger)) {
            trigger = cachedTrigger;
        } else {
            Log.Error("Trigger {TriggerName} not found in {MapXBlock}.", Value.Name, field.Metadata.XBlock);
            throw new ArgumentException($"Trigger {Value.Name} not found in {field.Metadata.XBlock}.");
        }

        nextState = new TriggerState(trigger, Context);
        nextTick = field.FieldTick;
    }

    public List<TriggerState> GetStates(string[] names) {
        if (names.Length == 0) {
            throw new ArgumentException("At least one state name must be provided.");
        }

        List<TriggerState> states = [];
        foreach (Trigger.Helpers.Trigger.State triggerState in trigger.States) {
            if (!names.Contains(triggerState.Name)) {
                continue;
            }
            states.Add(new TriggerState(trigger, triggerState, Context));
        }

        return states;
    }

    public TriggerState? GetState(string name) {
        Trigger.Helpers.Trigger.State? triggerState = trigger.States.FirstOrDefault(x => x.Name == name);
        return triggerState == null ? null : new TriggerState(trigger, triggerState, Context);
    }

    public List<string> GetStateNames() => trigger.States.Select(x => x.Name).ToList();

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
            Context.DebugLog("[OnExit] {State}", state?.Name ?? "null");
            state?.OnExit();
            state = nextState;
            Context.StartTick = Field.FieldTick;
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
        TriggerState? stateClass = GetState(next);
        if (stateClass == null) {
            return false;
        }

        nextState = stateClass;
        return nextState != null;
    }
}
