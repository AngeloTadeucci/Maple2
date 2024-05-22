using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Text;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class TriggerCommand : Command {
    private const string NAME = "trigger";
    private const string DESCRIPTION = "Reset triggers for current map.";

    private readonly GameSession session;

    public TriggerCommand(GameSession session) : base(NAME, DESCRIPTION) {
        this.session = session;

        var list = new Option<bool>(["--list", "-l"], () => false, "List all triggers");
        var triggerName = new Option<string?>(["--reset", "-r"], () => null, "Trigger name");
        var stateIndex = new Option<int>(["--state", "-s"], () => -1, "State index");

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

        List<FieldTrigger> fieldTriggers = session.Field!.EnumerateTrigger().ToList();
        if (list) {
            ctx.Console.Out.WriteLine($"Triggers: {fieldTriggers.Count}");
            foreach (FieldTrigger trigger in fieldTriggers) {
                string triggerName = trigger.Value.Name;
                string[] triggerStates = GetStateNames(trigger);
                var result = new StringBuilder($"TriggerStates for {triggerName}, count: {triggerStates.Length}\n");

                for (int i = 0; i < triggerStates.Length; i++) {
                    result.AppendLine($"  -[{i}] {triggerStates[i]}");
                }

                ctx.Console.Out.WriteLine(result.ToString());
            }
            return;
        }

        if (reset != null && stateIndex < 0) {
            FieldTrigger? fieldTrigger = fieldTriggers.FirstOrDefault(t => t.Value.Name.ToLower() == reset.ToLower());
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
        } else if (reset != null && stateIndex >= 0) {
            if (!session.Field.TryGetTrigger(reset, out FieldTrigger? currentFieldTrigger)) {
                ctx.Console.Error.WriteLine($"Trigger {reset} not found.");
                return;
            }

            string[] triggerStates = GetStateNames(currentFieldTrigger);
            if (stateIndex >= triggerStates.Length) {
                ctx.Console.Error.WriteLine($"Invalid state index for {reset}");
                return;
            }

            if (currentFieldTrigger.SetNextState(triggerStates[stateIndex])) {
                ctx.Console.Out.WriteLine($"Trigger {reset} state set to {triggerStates[stateIndex]}");
            }
        }
    }

    private static string[] GetStateNames(FieldTrigger trigger) {
        return trigger.Context.Scope.GetVariableNames()
            .Where(v => !v.StartsWith("__") && v != "trigger_api" && v != "initial_state")
            .ToArray();
    }
}
