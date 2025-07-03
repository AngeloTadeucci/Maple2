namespace Maple2.Server.Game.Trigger.Helpers;

public partial class Trigger {
    public List<State> States { get; }

    public Trigger(List<State>? states) {
        States = states ?? [];
    }

    public class State {
        public string Name { get; }

        public OnEnter? Enter { get; }
        public LinkedList<ICondition> Conditions { get; }
        public OnExit? Exit { get; }

        public State(string name, LinkedList<ICondition>? conditions, OnEnter? enter, OnExit? exit) {
            Name = name;
            Conditions = conditions ?? new LinkedList<ICondition>();
            Enter = enter;
            Exit = exit;
        }

        public class OnEnter {
            public string? NextState { get; set; }
            public LinkedList<IAction> Actions { get; }

            public OnEnter(LinkedList<IAction>? actions, string? nextState) {
                Actions = actions ?? new LinkedList<IAction>();
                NextState = nextState;
            }
        }

        public class OnExit {
            public LinkedList<IAction> Actions { get; }

            public OnExit(LinkedList<IAction>? actions) {
                Actions = actions ?? new LinkedList<IAction>();
            }
        }
    }
}

public interface IAction {
    void Execute(TriggerContext context);
}

public interface ICondition {
    string Name { get; }
    string? NextState { get; set; }
    LinkedList<IAction> Actions { get; set; }

    bool Evaluate(TriggerContext context);
}

public interface IGroupCondition : ICondition {
    LinkedList<ICondition> Conditions { get; set; }
}
