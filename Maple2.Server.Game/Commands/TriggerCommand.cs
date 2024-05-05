using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Trigger;
using Maple2.Trigger;

namespace Maple2.Server.Game.Commands;

public class TriggerCommand : Command {
    private const string NAME = "trigger";
    private const string DESCRIPTION = "Reset triggers for current map.";

    private readonly GameSession session;


    public TriggerCommand(GameSession session) : base(NAME, DESCRIPTION) {
        this.session = session;

        var list = new Option<bool>(new string[] { "--list", "-l" }, () => false, "List all triggers");
        var triggerName = new Option<string?>(new string[] { "--reset", "-r" }, () => null, "Trigger name");
        var stateIndex = new Option<int>(new string[] { "--state", "-s" }, () => -1, "State index");

        AddOption(list);
        AddOption(triggerName);
        AddOption(stateIndex);

        this.SetHandler<InvocationContext, bool, string?, int>(Handle, list, triggerName, stateIndex);
    }

    private void Handle(InvocationContext ctx, bool list, string? reset, int stateIndex) {
        if (!list && reset is null) {
            ctx.Console.Error.WriteLine("No action specified. Use --list or --reset <trigger name>");
            return;
        }

        if (session.Field is null) {
            ctx.Console.Error.WriteLine("Field not found.");
            return;
        }

        TriggerLoaderDebugger triggerDebugger = new();
        List<FieldTrigger> fieldTriggers = session.Field!.EnumerateTrigger().ToList();

        if (list) {
            ctx.Console.Out.WriteLine($"Triggers: {fieldTriggers.Count}");
            foreach (FieldTrigger trigger in fieldTriggers) {
                var context = new TriggerContext(trigger);
                string triggerName = trigger.Value.Name;
                string result = "";

                IReadOnlyDictionary<string, List<Func<ITriggerContext, TriggerState>>> triggerTable = triggerDebugger.GetTriggerTable(session.Field!.Metadata.XBlock);
                foreach (List<Func<ITriggerContext, TriggerState>> states in triggerTable.Values) {
                    if (states is null) {
                        ctx.Console.Error.WriteLine($"Invalid trigger for {session.Field.Metadata.XBlock}");
                        return;
                    }

                    result += $"TriggerStates for {triggerName}, count: {states.Count}\n";

                    int i = 0;
                    foreach (Func<ITriggerContext, TriggerState>? initialState in states) {
                        TriggerState nextState = initialState(context);
                        result += $"  -[{i++}] {nextState.GetType().ToString().Replace("Maple2.Trigger.", "")}\n";
                    }
                }

                ctx.Console.Out.WriteLine(result);
            }
            return;
        }

        if (reset is not null && stateIndex < 0) {
            FieldTrigger? fieldTrigger = fieldTriggers.FirstOrDefault(t => t.Value.Name == reset);
            if (fieldTrigger is null) {
                ctx.Console.Error.WriteLine($"Trigger {reset} not found.");
                return;
            }

            TriggerModel triggerModel = fieldTrigger.Value;
            var newFieldTrigger = new FieldTrigger(session.Field, FieldManager.NextGlobalId(), triggerModel) {
                Position = triggerModel.Position,
                Rotation = triggerModel.Rotation,
            };

            session.Field.ReplaceTrigger(fieldTrigger, newFieldTrigger);
            ctx.Console.Out.WriteLine($"Trigger {reset} reset.");
        }


        if (reset is not null && stateIndex >= 0) {
            session.Field.TryGetTrigger(reset, out FieldTrigger? currentFieldTrigger);
            if (currentFieldTrigger is null) {
                ctx.Console.Error.WriteLine($"Trigger {reset} not found.");
                return;
            }

            IReadOnlyDictionary<string, List<Func<ITriggerContext, TriggerState>>> triggerTable = triggerDebugger.GetTriggerTable(session.Field!.Metadata.XBlock);
            if (!triggerTable.TryGetValue(reset, out List<Func<ITriggerContext, TriggerState>>? triggerStates)) {
                ctx.Console.Error.WriteLine($"Invalid trigger for {session.Field.Metadata.XBlock}");
                return;
            }

            if (stateIndex >= triggerStates.Count) {
                ctx.Console.Error.WriteLine($"Invalid state index for {reset}");
                return;
            }

            TriggerState nextState = triggerStates.ElementAt(stateIndex)(new TriggerContext(currentFieldTrigger));
            currentFieldTrigger.SetNextState(nextState);
            ctx.Console.Out.WriteLine($"Trigger {reset} state set to {nextState.GetType().ToString().Replace("Maple2.Trigger.", "")}");
        }
    }
}
