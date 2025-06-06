using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml;
using Serilog;

namespace Maple2.Server.Game.Trigger.Helpers;

public class TriggerState {
    private readonly Trigger trigger;
    private readonly Trigger.State triggerState;
    private readonly TriggerContext triggerContext;

    public readonly string Name;

    public TriggerState(Trigger trigger, TriggerContext context) {
        this.trigger = trigger;
        triggerState = trigger.States.First();
        triggerContext = context;
        Name = triggerState.Name;
    }

    public TriggerState(Trigger trigger, Trigger.State nextState, TriggerContext context) {
        this.trigger = trigger;
        triggerState = nextState;
        triggerContext = context;
        Name = triggerState.Name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TriggerState? OnEnter() {
        Trigger.State.OnEnter? onEnter = triggerState.Enter;
        if (onEnter == null) return null;

        foreach (IAction actionNode in onEnter.Actions) {
            ExecuteAction(actionNode);
        }

        if (onEnter.NextState is not null && GetNextState(onEnter.NextState, out Trigger.State? nextState)) {
            return new TriggerState(trigger, nextState, triggerContext);
        }

        return null;
    }


    public TriggerState? OnTick() {
        LinkedList<ICondition> conditions = triggerState.Conditions;
        if (conditions.Count == 0) return null;

        foreach (ICondition condition in conditions) {
            if (!CallCondition(condition)) continue;

            foreach (IAction actionNode in condition.Actions) {
                ExecuteAction(actionNode);
            }

            if (condition.NextState is not null && GetNextState(condition.NextState, out Trigger.State? nextState)) {
                return new TriggerState(trigger, nextState, triggerContext);
            }

            return null;
        }
        return null;
    }

    public void OnExit() {
        Trigger.State.OnExit? onExit = triggerState.Exit;
        if (onExit == null) return;
        foreach (IAction actionNode in onExit.Actions) {
            ExecuteAction(actionNode);
        }
    }

    private bool GetNextState(string nextStateName, [NotNullWhen(true)] out Trigger.State? nextState) {
        nextState = trigger.States.FirstOrDefault(x => x.Name == nextStateName);
        return nextState != null;
    }

    private void ExecuteAction(IAction action) {
        try {
            action.Execute(triggerContext);
        } catch (Exception e) {
            Log.Logger.Error(e, "Error executing action '{ActionName}'", nameof(action));
        }
    }

    private bool CallCondition(ICondition condition) {
        try {
            return condition.Evaluate(triggerContext);
        } catch (Exception e) {
            Log.Logger.Error(e, "Error evaluating condition '{ConditionName}'", nameof(condition));
            return false;
        }
    }
}
