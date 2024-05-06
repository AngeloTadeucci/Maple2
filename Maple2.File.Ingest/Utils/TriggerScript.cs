using System.CodeDom.Compiler;

namespace Maple2.File.Ingest.Utils;

internal class TriggerScript {
    public readonly IList<State> States = new List<State>();
    public readonly IList<string> Imports = new List<string>();
    public bool Shared { get; init; }

    public class State {
        public readonly List<string> Comments = [];
        public string Name = string.Empty;
        public IList<Action> OnEnter = new List<Action>();
        public IList<Condition> Conditions = new List<Condition>();
        public IList<Action> OnExit = new List<Action>();

        public void WriteTo(IndentedTextWriter writer) {
            foreach (string comment in Comments) {
                writer.WriteLine(CommentString(comment));
            }
            writer.WriteLine($"class {Name}(trigger_api.Trigger):");

            bool hasBody = false;
            writer.Indent++;
            if (OnEnter.Count > 0) {
                writer.WriteLine("def on_enter(self):");
                writer.Indent++;
                foreach (Action action in OnEnter) {
                    action.WriteTo(writer);
                }
                writer.Indent--;
                WriteBlankLine(writer);
                hasBody = true;
            }
            if (Conditions.Count > 0) {
                writer.WriteLine("def on_tick(self) -> trigger_api.Trigger:");
                writer.Indent++;
                foreach (Condition condition in Conditions) {
                    condition.WriteTo(writer);
                }
                writer.Indent--;
                WriteBlankLine(writer);
                hasBody = true;
            }
            if (OnExit.Count > 0) {
                writer.WriteLine("def on_exit(self):");
                writer.Indent++;
                foreach (Action action in OnExit) {
                    action.WriteTo(writer);
                }
                writer.Indent--;
                WriteBlankLine(writer);
                hasBody = true;
            }

            if (!hasBody) {
                writer.WriteLine("pass");
                WriteBlankLine(writer);
            }
            writer.Indent--;
            WriteBlankLine(writer);
        }
    }

    public class Action {
        public readonly List<string> Comments = [];
        public string? LineComment;

        public string Name = string.Empty;
        public IList<Parameter> Args = new List<Parameter>();

        public void WriteTo(IndentedTextWriter writer) {
            foreach (string comment in Comments) {
                writer.WriteLine(CommentString(comment));
            }
            IEnumerable<string> args = Args
                .Where(arg => !string.IsNullOrWhiteSpace(arg.Value))
                .Select(arg => $"{arg.Name}={arg.FormatValue()}");
            writer.Write($"self.{Name}({string.Join(", ", args)})");
            if (LineComment != null) {
                LineComment = LineComment.Trim();
                if (LineComment.Contains("action name=")) {
                    writer.WriteLine();
                    foreach (string line in LineComment.Split('\n')) {
                        writer.WriteLine($"# {line.Trim()}");
                    }
                } else {
                    writer.WriteLine($" # {LineComment}");
                }
            } else {
                writer.WriteLine();
            }
        }
    }

    public class Condition {
        public readonly List<string> Comments = [];
        public string? LineComment;
        public string? TransitionComment;

        public string Name = string.Empty;
        public bool Negated;
        public IList<Parameter> Args = new List<Parameter>();
        public IList<Action> Actions = new List<Action>();
        public string? Transition;

        public void WriteTo(IndentedTextWriter writer) {
            foreach (string comment in Comments) {
                writer.WriteLine(CommentString(comment));
            }
            writer.Write($"if {(Negated ? "not " : "")}self.{Name}({string.Join(", ", Args.Select(arg => $"{arg.Name}={arg.FormatValue()}"))}):");
            if (LineComment != null) {
                if (!LineComment.Contains('\n') && !LineComment.TrimStart().StartsWith("<condition")) {
                    writer.Write($" # {LineComment.Trim()}");
                    // Clear after write, so we don't write again.
                    LineComment = null;
                }
            }
            writer.WriteLine();
            writer.Indent++;
            foreach (Action action in Actions) {
                action.WriteTo(writer);
            }

            if (Transition == null) {
                if (TransitionComment != null) {
                    writer.WriteLine($"return None # Missing State: {TransitionComment}");
                } else {
                    writer.WriteLine("return None");
                }
            } else {
                writer.WriteLine($"return {Transition}(self.ctx)");
            }
            writer.Indent--;
            if (LineComment != null) {
                writer.WriteLine(@"""""""");
                writer.WriteLine(LineComment.Trim().Replace("\t", "    "));
                writer.WriteLine(@"""""""");
            }
        }
    }

    public void WriteTo(IndentedTextWriter writer) {
        writer.WriteLine("import trigger_api");
        if (Imports.Count > 0) {
            writer.WriteLine();
            foreach (string import in Imports) {
                writer.WriteLine($"#include {import.Replace('.', '/')}.py");
                writer.WriteLine($"from {import} import *");
            }
        }
        writer.WriteLine();
        writer.WriteLine();

        foreach (State state in States) {
            state.WriteTo(writer);
        }

        // No initialization for dungeon_common
        if (!Shared) {
            writer.WriteLine($"initial_state = {States.First().Name}");
        }
    }

    public static void WriteBlankLine(IndentedTextWriter writer) {
        int indent = writer.Indent;
        writer.Indent = 0;
        writer.WriteLine();
        writer.Indent = indent;
    }

    public static string CommentString(string str) {
        return $"# {str.Trim().Replace("\n", "\n# ")}";
    }
}

internal class TriggerScriptCommon {
    public readonly SortedDictionary<string, Function> Actions = new();
    public readonly SortedDictionary<string, Function> Conditions = new();

    public void WriteTo(IndentedTextWriter writer) {
        writer.WriteLine("from typing import List");
        writer.WriteLine();

        TriggerScript.WriteBlankLine(writer);
        writer.WriteLine("class Trigger:");
        writer.Indent++;
        writer.WriteLine("def __init__(self, ctx: ...):");
        writer.Indent++;
        writer.WriteLine("self.ctx = ctx");
        writer.Indent--;
        TriggerScript.WriteBlankLine(writer);
        writer.WriteLine("def on_enter(self):");
        writer.Indent++;
        writer.WriteLine(@"""""""Invoked after transitioning to this state.""""""");
        writer.WriteLine("pass");
        writer.Indent--;
        TriggerScript.WriteBlankLine(writer);
        writer.WriteLine("def on_tick(self) -> 'Trigger':");
        writer.Indent++;
        writer.WriteLine(@"""""""Periodically invoked while in this state.""""""");
        writer.WriteLine("pass");
        writer.Indent--;
        TriggerScript.WriteBlankLine(writer);
        writer.WriteLine("def on_exit(self):");
        writer.Indent++;
        writer.WriteLine(@"""""""Invoked before transitioning to another state.""""""");
        writer.WriteLine("pass");
        writer.Indent--;

        TriggerScript.WriteBlankLine(writer);
        writer.WriteLine(@""""""" Actions """"""");
        foreach (Function action in Actions.Values) {
            action.WriteTo(writer);
            TriggerScript.WriteBlankLine(writer);
        }

        TriggerScript.WriteBlankLine(writer);
        writer.WriteLine(@""""""" Conditions """"""");
        foreach (Function condition in Conditions.Values) {
            condition.WriteTo(writer);
            TriggerScript.WriteBlankLine(writer);
        }

        writer.Indent--;
    }

    internal class Function : IComparable<Function> {
        public string? Description = null;
        public readonly string Name;
        public ScriptType ReturnType { get; init; }
        private readonly List<Parameter> parameters = [];

        private readonly Dictionary<string, (ScriptType, string?)> typeOverrides;
        private readonly Dictionary<string, string> nameOverrides;

        public Function(string name, bool isCondition) {
            Name = name;
            if (isCondition) {
                nameOverrides = TriggerDefinitionOverride.ConditionNameOverride.GetValueOrDefault(name, new Dictionary<string, string>());
                typeOverrides = TriggerDefinitionOverride.ConditionTypeOverride.GetValueOrDefault(name, new Dictionary<string, (ScriptType, string?)>());
            } else {
                nameOverrides = TriggerDefinitionOverride.ActionNameOverride.GetValueOrDefault(name, new Dictionary<string, string>());
                typeOverrides = TriggerDefinitionOverride.ActionTypeOverride.GetValueOrDefault(name, new Dictionary<string, (ScriptType, string?)>());
            }
        }

        public int CompareTo(Function? other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            int nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
            if (nameComparison != 0) return nameComparison;

            return ReturnType.CompareTo(other.ReturnType);
        }

        // Returns normalized parameter name
        public (ScriptType, string) AddParameter(ScriptType type, string name, string? defaultValue = null) {
            if (nameOverrides.ContainsKey(name)) {
                name = nameOverrides[name];
            }

            Parameter? existing = parameters.FirstOrDefault(param => param.Name == name);
            if (existing != null) {
                return (existing.Type, existing.Name);
            }

            if (typeOverrides.ContainsKey(name)) {
                (ScriptType, string?) typeOverride = typeOverrides[name];
                type = typeOverride.Item1;
                defaultValue = typeOverride.Item2;
            }

            parameters.Add(new Parameter(type, name, defaultValue));
            return (type, name);
        }

        public void WriteTo(IndentedTextWriter writer) {
            writer.Write($"def {Name}(self");
            foreach (Parameter parameter in parameters) {
                if (parameter.Type == ScriptType.None) {
                    continue;
                }

                writer.Write(", ");
                writer.Write($"{parameter.Name}: ");
                writer.Write(parameter.TypeStr());
                writer.Write($"={parameter.DefaultStr()}");
            }

            writer.Write(")");
            string returnTypeStr = ReturnType switch {
                ScriptType.None => "",
                ScriptType.Str => "str",
                ScriptType.Int => "int",
                ScriptType.Float => "float",
                ScriptType.Bool => "bool",
                _ => throw new ArgumentException($"Invalid return type: {ReturnType}")
            };
            if (!string.IsNullOrWhiteSpace(returnTypeStr)) {
                writer.Write($" -> {returnTypeStr}");
            }
            writer.WriteLine(":");
            writer.Indent++;

            // Write docstring
            if (parameters.Count == 0 && string.IsNullOrWhiteSpace(returnTypeStr)) { // Single-line
                writer.WriteLine($@"""""""{Description}""""""");
            } else {
                writer.WriteLine($@"""""""{Description}");
                if (parameters.Count > 0) {
                    TriggerScript.WriteBlankLine(writer);
                    writer.WriteLine("Args:");
                    writer.Indent++;
                    foreach (Parameter parameter in parameters) {
                        if (parameter.Type == ScriptType.None) {
                            continue;
                        }

                        writer.WriteLine($"{parameter.Name} ({parameter.TypeStr()}): _description_. Defaults to {parameter.DefaultStr()}.");
                    }
                    writer.Indent--;
                }
                if (!string.IsNullOrWhiteSpace(returnTypeStr)) {
                    TriggerScript.WriteBlankLine(writer);
                    writer.WriteLine("Returns:");
                    writer.Indent++;
                    writer.WriteLine($"{returnTypeStr}: _description_");
                    writer.Indent--;
                }
                writer.WriteLine(@"""""""");
            }

            if (ReturnType == ScriptType.Bool) {
                writer.WriteLine("return False");
            } else {
                writer.WriteLine("pass");
            }
            writer.Indent--;
        }
    }
}
