using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Maple2.Tools.Extensions;

namespace Maple2.File.Ingest.Utils.Trigger;

internal partial class TriggerScript {
    public class Condition : IScriptBlock {
        public bool SingleLine => false;

        public readonly string Name;
        public readonly List<string> Comments = [];
        public string? LineComment;

        public bool Negated;
        public IList<PyParameter> Args = [];
        public readonly IList<IScriptBlock> Actions = [];
        public readonly IList<Condition> Group = [];
        public Transition? Transition;

        private readonly TriggerDefinitionOverride overrides;

        public Condition(string name) {
            Name = name;
            overrides = TriggerDefinitionOverride.ConditionOverride.GetValueOrDefault(name)!;
        }

        public void WriteTo(IndentedTextWriter writer, bool isCommented) {
            writer.WriteComments(Comments);
            if (LineComment != null && (LineComment.Contains('\n') || LineComment.Length > LineCommentMax)) {
                writer.WriteLineCommentString(LineComment, sameLine: false);

                // Set null to avoid writing twice.
                LineComment = null;
            }

            foreach ((string? argName, _) in overrides.Types.Where(o => o.Value.Default == "<required>")) {
                Debug.Assert(Args.Any(arg => arg.Name == argName), $"Condition {overrides.Name} is missing required arg: {argName}");
            }

            bool unconditional = overrides.Name is "true" or "always";
            if (unconditional) {
                if (LineComment != null) {
                    writer.WriteLineCommentString(LineComment, sameLine: false);
                }
            } else {
                writer.Write($"if {ConditionString()}:");
                if (LineComment != null) {
                    writer.WriteLineCommentString(LineComment);
                } else {
                    writer.WriteBlankLine();
                }
            }

            if (!unconditional) writer.Indent++;
            {
                bool hasBody = false;
                foreach (IScriptBlock action in Actions) {
                    action.WriteTo(writer, isCommented);
                    hasBody |= action.IsCode;
                }
                if (Transition != null) {
                    Transition.WriteTo(writer, isCommented);
                    hasBody = true;
                }
                if (!hasBody) {
                    writer.WriteLine("pass");
                }
            }
            if (!unconditional) writer.Indent--;
        }

        public ISet<string> Imports() {
            return Actions.SelectMany(a => a.Imports())
                .Concat(Args.Select(a => a.Import()).WhereNotNull())
                .Concat(Group.SelectMany(g => g.Imports()))
                .ToHashSet();
        }

        private string ConditionString() {
            // Comparison is overridden.
            if (overrides.Compare.Type != ScriptType.None) {
                string? compareOp;
                if ((compareOp = Args.SingleOrDefault(arg => arg.Name == overrides.Compare.Op)?.Value) == null) {
                    if (overrides.Types.TryGetValue(overrides.Compare.Op, out (ScriptType type, string? @default) compare) && compare.@default != null) {
                        compareOp = compare.@default;
                    } else {
                        compareOp = overrides.Compare.Default;
                    }
                }
                string? compareValue;
                if ((compareValue = Args.SingleOrDefault(arg => arg.Name == overrides.Compare.Field)?.FormatValue()) == null) {
                    if (overrides.Types.TryGetValue(overrides.Compare.Field, out (ScriptType type, string? @default) compare) && compare.@default != null) {
                        compareValue = compare.@default;
                    } else {
                        Debug.Assert(overrides.Compare.Type == ScriptType.Int);
                        compareValue = "0";
                    }
                }

                // We need special parsing here
                if (Name == "widget_condition") {
                    (compareOp, compareValue) = NormalizedWidgetCondition(compareOp, compareValue);
                }
                string op = compareOp switch {
                    "Equal" or "=" => Negated ? "!=" : "==",
                    "Less" or "lower" or "<" => Negated ? ">=" : "<",
                    "LessEqual" or "lowerEqual" or "<=" => Negated ? ">" : "<=",
                    "Greater" or "higher" or ">" => Negated ? "<=" : ">",
                    "GreaterEqual" or "higherEqual" or ">=" => Negated ? "<" : ">=",
                    "in" => Negated ? "not in" : "in",
                    _ => throw new ArgumentException($"Unexpected comparison operation: {compareOp}"),
                };

                foreach ((string? argName, _) in overrides.Types.Where(o => o.Value.Default == "<required>")) {
                    Debug.Assert(Args.Any(arg => arg.Name == argName), $"Condition {overrides.Name} is missing required arg: {argName}");
                }

                IEnumerable<string> args = Args.Where(arg => arg.Name != overrides.Compare.Field && arg.Name != overrides.Compare.Op)
                    .Where(arg => !arg.IsDefault(overrides.Types.GetValueOrDefault(arg.Name).Default))
                    .Select(arg => $"{arg.Name}={arg.FormatValue()}");
                if (overrides.Compare.Type == ScriptType.Bool) {
                    return compareValue switch {
                        "True" => $"self.{overrides.Name}({string.Join(", ", args)})",
                        "False" => $"not self.{overrides.Name}({string.Join(", ", args)})",
                        _ => throw new ArgumentException($"Invalid bool value for comparison operation: {compareValue}")
                    };
                }
                return $"self.{overrides.Name}({string.Join(", ", args)}) {op} {compareValue}";
            }

            return Name switch {
                "all_of" => string.Join(" and ", Group.Select(condition => condition.ConditionString())),
                "any_one" => string.Join(" or ", Group.Select(condition => condition.ConditionString())),
                "always" => "True",
                "true" => "True",
                _ => $"{(Negated ? "not " : "")}self.{overrides.Name}({string.Join(", ", Args.Select(arg => $"{arg.Name}={arg.FormatValue()}"))})",
            };
        }

        private static (string, string) NormalizedWidgetCondition(string compareOp, string compareValue) {
            if (Regex.Match(compareOp, @"\d+-\d+").Success) {
                compareValue = new PyParameter(ScriptType.IntList, "") {
                    Value = compareValue[1..^1],
                }.FormatValue();
                compareOp = "in";
            } else if (int.TryParse(compareOp, out int resultValue)) {
                compareValue = resultValue.ToString();
                compareOp = "=";
            } else if (Regex.Match(compareOp, @"= \d+").Success) {
                compareValue = compareOp.Replace("= ", "");
                compareOp = "=";
            } else if (Regex.Match(compareOp, @"\D\D?,\d+").Success) {
                compareValue = int.Parse(compareOp.Split(",")[1]).ToString();
                compareOp = compareOp.Split(",")[0];
            } else if (compareOp == "<placeholder>") {
                compareValue = "1"; // Use 1 as True
                compareOp = "=";
            } else {
                throw new ArgumentException($"Unknown compare for widget_condition: <{compareOp}>");
            }

            return (compareOp, compareValue);
        }
    }
}
