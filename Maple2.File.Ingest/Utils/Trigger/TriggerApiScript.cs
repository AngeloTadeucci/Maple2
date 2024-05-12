using System.CodeDom.Compiler;
using System.Diagnostics;

namespace Maple2.File.Ingest.Utils.Trigger;

internal class TriggerApiScript {
    public readonly SortedDictionary<(string, string?), Function> Actions = new();
    public readonly SortedDictionary<string, Function> Conditions = new();

    public void WriteTo(IndentedTextWriter writer) {
        writer.WriteLine("from typing import List");
        writer.WriteBlankLine();

        writer.WriteBlankLine();
        writer.WriteLine("class Trigger:");
        writer.Indent++;
        writer.WriteLine("def __init__(self, ctx: ...):");
        writer.Indent++;
        writer.WriteLine("self.ctx = ctx");
        writer.Indent--;
        writer.WriteBlankLine();
        writer.WriteLine("def on_enter(self) -> 'Trigger':");
        writer.Indent++;
        writer.WriteLine(@"""""""Invoked after transitioning to this state.""""""");
        writer.WriteLine("pass");
        writer.Indent--;
        writer.WriteBlankLine();
        writer.WriteLine("def on_tick(self) -> 'Trigger':");
        writer.Indent++;
        writer.WriteLine(@"""""""Periodically invoked while in this state.""""""");
        writer.WriteLine("pass");
        writer.Indent--;
        writer.WriteBlankLine();
        writer.WriteLine("def on_exit(self) -> None:");
        writer.Indent++;
        writer.WriteLine(@"""""""Invoked before transitioning to another state.""""""");
        writer.WriteLine("pass");
        writer.Indent--;

        writer.WriteBlankLine();
        writer.WriteLine(@""""""" Actions """"""");
        foreach (Function action in Actions.Values) {
            action.WriteTo(writer);
            writer.WriteBlankLine();
        }

        writer.WriteBlankLine();
        writer.WriteLine(@""""""" Conditions """"""");
        foreach (Function condition in Conditions.Values) {
            // These conditions are handled used "and"/"or"
            if (condition.Name is "true" or "always" or "all_of" or "any_one") {
                continue;
            }

            condition.WriteTo(writer);
            writer.WriteBlankLine();
        }

        writer.Indent--;
    }

    public void WriteInterface(IndentedTextWriter writer) {
        writer.WriteLine("namespace Maple2.Server.Game.Scripting.Trigger;");
        writer.WriteBlankLine();
        writer.WriteLine("public interface ITriggerContext {");
        writer.Indent++;
        writer.WriteLine("// Actions");
        foreach (Function action in Actions.Values) {
            action.WriteInterface(writer);
            writer.WriteBlankLine();
        }
        writer.WriteBlankLine();
        writer.WriteLine("// Conditions");
        foreach (Function condition in Conditions.Values) {
            // These conditions are handled used "and"/"or"
            if (condition.Name is "true" or "always" or "all_of" or "any_one") {
                continue;
            }

            condition.WriteInterface(writer);
            writer.WriteBlankLine();
        }
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteBlankLine();
    }

    internal class Function : IComparable<Function> {
        public readonly string Name;
        private readonly string? splitName;

        public string? Description = null;
        public ScriptType ReturnType { get; }
        private readonly List<PyParameter> parameters = new();

        private readonly TriggerDefinitionOverride overrides;

        public Function(string name, string? splitName, bool isCondition) {
            Name = name;
            this.splitName = splitName;
            if (isCondition) {
                overrides = TriggerDefinitionOverride.ConditionOverride.GetValueOrDefault(name)!;
                Debug.Assert(overrides != null, $"no overrides for {name}");
                ReturnType = ScriptType.Bool;
                if (overrides.Compare.Type != ScriptType.None) {
                    ReturnType = overrides.Compare.Type;
                }
            } else {
                overrides = TriggerDefinitionOverride.ActionOverride.GetValueOrDefault(name)!;
                Debug.Assert(overrides != null, $"no overrides for {name}");
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
            if (overrides.Names.ContainsKey(name)) {
                name = overrides.Names[name];
            }

            string snakeName = TriggerTranslate.ToSnakeCase(name);
            PyParameter? existing = parameters.FirstOrDefault(param => param.Name == snakeName);
            if (existing != null) {
                return (existing.Type, existing.Name);
            }

            if (overrides.Types.ContainsKey(name)) {
                (ScriptType, string?) typeOverride = overrides.Types[name];
                type = typeOverride.Item1;
                defaultValue = typeOverride.Item2;
            }

            parameters.Add(new PyParameter(type, snakeName, defaultValue));
            return (type, snakeName);
        }

        public void WriteTo(IndentedTextWriter writer) {
            string fullName = overrides.Name;
            if (splitName != null) {
                fullName += $"_{splitName}";
            }

            writer.Write($"def {fullName}(self");
            foreach (PyParameter parameter in parameters) {
                if (parameter.Type == ScriptType.None || SkipParameter(parameter)) {
                    continue;
                }

                writer.Write(", ");
                writer.Write($"{parameter.Name}: ");
                writer.Write(parameter.TypeStr());
                writer.Write($"={parameter.DefaultStr()}");
            }

            writer.Write(")");
            string returnTypeStr = PyParameter.TypeStr(ReturnType);
            if (!string.IsNullOrWhiteSpace(returnTypeStr)) {
                writer.Write($" -> {returnTypeStr}");
            }
            writer.WriteLine(":");
            writer.Indent++;

            // Write docstring
            if (parameters.Count == 0 && string.IsNullOrWhiteSpace(overrides.Description) && string.IsNullOrWhiteSpace(returnTypeStr)) {
                writer.WriteLine($@"""""""{Description}"""""""); // Single-line
            } else {
                writer.WriteLine($@"""""""{Description}");
                if (!string.IsNullOrWhiteSpace(overrides.Description)) {
                    writer.WriteBlankLine();
                    foreach (string line in overrides.Description.Split(new []{"\n", "\r\n"}, StringSplitOptions.None)) {
                        writer.WriteLine(line);
                    }
                }
                if (parameters.Any(p => !SkipParameter(p))) {
                    writer.WriteBlankLine();
                    writer.WriteLine("Args:");
                    writer.Indent++;
                    foreach (PyParameter parameter in parameters) {
                        if (parameter.Type == ScriptType.None || SkipParameter(parameter)) {
                            continue;
                        }

                        writer.WriteLine($"{parameter.Name} ({parameter.TypeStr()}): _description_. Defaults to {parameter.DefaultStr()}.");
                    }
                    writer.Indent--;
                }
                if (!string.IsNullOrWhiteSpace(returnTypeStr)) {
                    writer.WriteBlankLine();
                    if (overrides.Compare.Type == ScriptType.None) {
                        writer.WriteLine($"Returns: None");
                    } else {
                        writer.WriteLine("Returns:");
                        writer.Indent++;
                        writer.WriteLine($"{returnTypeStr}: {overrides.Compare.Field}");
                        writer.Indent--;
                    }
                }
                writer.WriteLine(@"""""""");
            }

            string pascalName = TriggerTranslate.ToPascalCase(fullName);
            if (ReturnType != ScriptType.None) {
                writer.WriteLine($"return self.ctx.{pascalName}({string.Join(", ", parameters.Where(p => !SkipParameter(p)).Select(p => p.Name))})");
            } else {
                writer.WriteLine($"self.ctx.{pascalName}({string.Join(", ", parameters.Where(p => !SkipParameter(p)).Select(p => p.Name))})");
            }
            writer.Indent--;
        }

        // Skip parameter because it's used for comparison override OR function splitting.
        private bool SkipParameter(PyParameter parameter) {
            if (overrides.FunctionSplitter == parameter.Name) {
                return true;
            }
            if (overrides.Compare.Type == ScriptType.None) {
                return false;
            }

            return parameter.Name == overrides.Compare.Field || parameter.Name == overrides.Compare.Op;
        }

        private readonly HashSet<string> csReserved = new() {"operator", "event", "string"};
        public void WriteInterface(IndentedTextWriter writer) {
            string fullName = overrides.Name;
            if (splitName != null) {
                fullName += $"_{splitName}";
            }

            string pascalName = TriggerTranslate.ToPascalCase(fullName);
            IEnumerable<string> paramList = parameters.Where(p => !SkipParameter(p))
                .Select(p => {
                    string paramName = $"{TriggerTranslate.ToCamelName(p.Name)}";
                    if (csReserved.Contains(paramName)) {
                        paramName = $"@{paramName}";
                    }
                    return $"{TypeString(p.Type)} {paramName}";
                });

            writer.Write($"public {TypeString(ReturnType)} {pascalName}({string.Join(", ", paramList)});");
            return;

            string TypeString(ScriptType scriptType) {
                return scriptType switch {
                    ScriptType.None => "void",
                    ScriptType.Bool => "bool",
                    ScriptType.Str => "string",
                    ScriptType.Int => "int",
                    ScriptType.Float => "float",
                    ScriptType.IntList => "int[]",
                    ScriptType.StrList => "string[]",
                    ScriptType.StateList => "dynamic[]",
                    ScriptType.Vector3 => "int[]",
                    ScriptType.State => "dynamic",
                    _ => throw new ArgumentOutOfRangeException($"Invalid parameter type: {scriptType}"),
                };
            }
        }
    }
}
