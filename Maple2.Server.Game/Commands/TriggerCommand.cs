using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Text;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class TriggerCommand : GameCommand {
    private const string NAME = "trigger";
    private const string DESCRIPTION = "Manage triggers for current map.";
    public const AdminPermissions RequiredPermission = AdminPermissions.Debug;

    public TriggerCommand(GameSession session) : base(RequiredPermission, NAME, DESCRIPTION) {
        AddCommand(new ListCommand(session));
        AddCommand(new ResetCommand(session));
        AddCommand(new RunCommand(session));
    }

    private class ListCommand : Command {
        private readonly GameSession session;

        public ListCommand(GameSession session) : base("list", "List all triggers") {
            this.session = session;

            this.SetHandler<InvocationContext>(Handle);
        }

        private void Handle(InvocationContext ctx) {
            List<FieldTrigger> fieldTriggers = session.Field!.EnumerateTrigger().ToList();
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
        }
    }

    private class ResetCommand : Command {
        private readonly GameSession session;

        public ResetCommand(GameSession session) : base("reset", "Reset a specific trigger") {
            this.session = session;

            var triggerName = new Argument<string>("triggerName", "Name of the trigger to reset");
            var stateOption = new Option<int>(["--state", "-s"], () => -1, "State index to set");

            AddArgument(triggerName);
            AddOption(stateOption);
            this.SetHandler<InvocationContext, string, int>(Handle, triggerName, stateOption);
        }

        private void Handle(InvocationContext ctx, string triggerName, int stateIndex) {
            if (!session.Field!.TryGetTrigger(triggerName, out FieldTrigger? currentFieldTrigger)) {
                ctx.Console.Error.WriteLine($"Trigger {triggerName} not found.");
                return;
            }

            if (stateIndex < 0) {
                // Reset the trigger
                TriggerModel triggerModel = currentFieldTrigger.Value;
                var newFieldTrigger = new FieldTrigger(session.Field, FieldManager.NextGlobalId(), triggerModel) {
                    Position = triggerModel.Position,
                    Rotation = triggerModel.Rotation,
                };

                session.Field.ReplaceTrigger(currentFieldTrigger, newFieldTrigger);
                ctx.Console.Out.WriteLine($"Trigger {triggerName} reset.");
            } else {
                // Set trigger to specific state
                string[] triggerStates = GetStateNames(currentFieldTrigger);
                if (stateIndex >= triggerStates.Length) {
                    ctx.Console.Error.WriteLine($"Invalid state index for {triggerName}");
                    return;
                }

                if (currentFieldTrigger.SetNextState(triggerStates[stateIndex])) {
                    ctx.Console.Out.WriteLine($"Trigger {triggerName} state set to {triggerStates[stateIndex]}");
                }
            }
        }
    }

    private class RunCommand : Command {
        private readonly GameSession session;
        public RunCommand(GameSession session) : base("run", "Run a trigger function") {
            this.session = session;

            var functionName = new Argument<string>("functionName", "Name of the function to run");
            var parameters = new Argument<string[]>("parameters", "Parameters to pass to the function");

            AddArgument(functionName);
            AddArgument(parameters);

            this.SetHandler<InvocationContext, string, string[]>(Handle, functionName, parameters);
        }

        private void Handle(InvocationContext ctx, string functionName, string[] parameters) {
            functionName = functionName.ToLower();
            List<string> functionNames = ["set_skill", "spawn_monster", "move_npc", "set_npc_emotion_sequence", "destroy_monster"];

            if (!functionNames.Contains(functionName)) {
                ctx.Console.Error.WriteLine($"Function {functionName} not found.");
                ctx.Console.Out.WriteLine($"Available functions: {string.Join(", ", functionNames)}");
                return;
            }

            FieldTrigger? trigger = session.Field.EnumerateTrigger().FirstOrDefault();
            if (trigger is null) {
                ctx.Console.Error.WriteLine("No trigger found.");
                return;
            }

            switch (functionName) {
                case "set_skill":
                    if (parameters.Length < 1) {
                        ctx.Console.Error.WriteLine("Usage: trigger run set_skill <skillIds> <enabled>");
                        return;
                    }

                    var skillIds = parameters[0].Split(',').Select(int.Parse).ToArray();
                    bool enabled = parameters.Length > 1 && bool.Parse(parameters[1]);

                    trigger.Context.SetSkill(skillIds, enabled);
                    break;
                case "spawn_monster":
                    if (parameters.Length < 1) {
                        ctx.Console.Error.WriteLine("Usage: trigger run spawn_monster <spawnIds> <spawnAnimation>");
                        return;
                    }

                    var spawnIds = parameters[0].Split(',').Select(int.Parse).ToArray();
                    bool spawnAnimation = parameters.Length > 1 && bool.Parse(parameters[1]);

                    trigger.Context.SpawnMonster(spawnIds, spawnAnimation, 0);
                    break;
                case "move_npc":
                    if (parameters.Length < 2) {
                        ctx.Console.Error.WriteLine("Usage: trigger run move_npc <spawnId> <patrolName>");
                        return;
                    }

                    int spawnId = int.Parse(parameters[0]);
                    string patrolName = parameters[1];

                    trigger.Context.MoveNpc(spawnId, patrolName);
                    break;

                case "set_npc_emotion_sequence":
                    if (parameters.Length < 2) {
                        ctx.Console.Error.WriteLine("Usage: trigger run set_npc_emotion_sequence <spawnId> <sequenceName> <duration>");
                        return;
                    }
                    int npcSpawnId = int.Parse(parameters[0]);
                    string sequenceName = parameters[1];
                    int duration = parameters.Length > 2 ? int.Parse(parameters[2]) : 0;

                    trigger.Context.SetNpcEmotionSequence(npcSpawnId, sequenceName, duration);
                    break;
                case "destroy_monster":
                    if (parameters.Length < 1) {
                        ctx.Console.Error.WriteLine("Usage: trigger run destroy_monster <spawnId>");
                        return;
                    }

                    int[] monsterSpawnIds = parameters[0].Split(',').Select(int.Parse).ToArray();

                    trigger.Context.DestroyMonster(monsterSpawnIds, false);
                    break;
            }

            ctx.Console.Out.WriteLine($"Function {functionName} executed.");
        }
    }

    private static string[] GetStateNames(FieldTrigger trigger) {
        return trigger.Context.Scope.GetVariableNames()
            .Where(v => !v.StartsWith("__") && v != "trigger_api" && v != "initial_state")
            .ToArray();
    }
}
