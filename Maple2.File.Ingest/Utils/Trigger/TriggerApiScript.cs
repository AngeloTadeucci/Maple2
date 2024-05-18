using System.CodeDom.Compiler;
using System.Diagnostics;

namespace Maple2.File.Ingest.Utils.Trigger;

internal class TriggerApiScript {
    public readonly SortedDictionary<(string, string?), Function> Actions = new();
    public readonly SortedDictionary<string, Function> Conditions = new();

    public void WriteTo(IndentedTextWriter writer) {
        writer.WriteLine("from typing import List");
        writer.WriteLine("from System.Numerics import Vector3");
        writer.WriteLine("from Maple2.Server.Game.Scripting.Trigger import Align, FieldGame, Locale, Weather");
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
        writer.WriteLine("using System.Numerics;");
        writer.WriteBlankLine();
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
            // These conditions are handled using "and"/"or"
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

    public void WriteEnums(IndentedTextWriter writer) {
        writer.WriteLine("namespace Maple2.Server.Game.Scripting.Trigger;");
        writer.WriteBlankLine();
        writer.WriteLine("""
                         [Flags]
                         public enum Align { Top = 0, Center = 1, Bottom = 2, Left = 4, Right = 8 }
                         """);
        writer.WriteBlankLine();
        writer.WriteLine("public enum FieldGame { Unknown, HideAndSeek, GuildVsGame, MapleSurvival, MapleSurvivalTeam, WaterGunBattle }");
        writer.WriteBlankLine();
        writer.WriteLine("public enum Locale { ALL, KR, CN, NA, JP, TH, TW }");
        writer.WriteBlankLine();
        writer.WriteLine("public enum Weather { Clear = 0, Snow = 1, HeavySnow = 2, Rain = 3, HeavyRain = 4, SandStorm = 5, CherryBlossom = 6, LeafFall = 7 }");
    }

    internal class Function : IComparable<Function> {
        public readonly string Name;

        public string? Description = null;
        public ScriptType ReturnType { get; }
        private readonly List<PyParameter> parameters = new();

        private readonly TriggerDefinitionOverride overrides;

        public Function(string name, string? splitArgValue, bool isCondition) {
            Name = name;

            if (isCondition) {
                overrides = TriggerDefinitionOverride.ConditionOverride.GetValueOrDefault(name)!;
                Debug.Assert(overrides != null, $"no overrides for {name}");
                ReturnType = ScriptType.Bool;
                if (overrides.Compare.Type != ScriptType.None) {
                    ReturnType = overrides.Compare.Type;
                }
            } else {
                overrides = TriggerDefinitionOverride.ActionOverride.GetValueOrDefault(name)!;
                if (splitArgValue != null) {
                    overrides = overrides.FunctionLookup[splitArgValue];
                }
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
            name = TriggerTranslate.ToSnakeCase(name);
            if (overrides.Names.ContainsKey(name)) {
                name = overrides.Names[name];
            }

            PyParameter? existing = parameters.FirstOrDefault(param => param.Name == name);
            if (existing != null) {
                return (existing.Type, existing.Name);
            }

            if (overrides.Types.ContainsKey(name)) {
                (ScriptType Type, string? Default) typeOverride = overrides.Types[name];
                type = typeOverride.Type;
                defaultValue = typeOverride.Default;
            }

            parameters.Add(new PyParameter(type, name, defaultValue));
            return (type, name);
        }

        public void WriteTo(IndentedTextWriter writer) {
            writer.Write($"def {overrides.Name}(self");
            PyParameter[] filteredParameters = parameters.Where(p => p.Type != ScriptType.None && !SkipParameter(p)).ToArray();
            PyParameter[] requiredParameters = filteredParameters.Where(p => p.Value == "<required>").ToArray();
            PyParameter[] optionalParameters = filteredParameters.Where(p => p.Value != "<required>").ToArray();
            // Must write required parameters first in function def.
            foreach (PyParameter parameter in requiredParameters) {
                writer.Write(", ");
                writer.Write($"{parameter.Name}: ");
                writer.Write(parameter.TypeStr());
            }
            foreach (PyParameter parameter in optionalParameters) {
                writer.Write(", ");
                writer.Write($"{parameter.Name}: ");
                writer.Write(parameter.TypeStr());
                writer.Write($"={parameter.FormatValue()}"); // Default value
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
                    foreach (string line in overrides.Description.Split(["\n", "\r\n"], StringSplitOptions.None)) {
                        writer.WriteLine(line);
                    }
                }
                if (parameters.Any(p => !SkipParameter(p))) {
                    writer.WriteBlankLine();
                    writer.WriteLine("Args:");
                    writer.Indent++;
                    // Must write required parameters first in function def.
                    foreach (PyParameter parameter in requiredParameters) {
                        writer.WriteLine($"{parameter.Name} ({parameter.TypeStr()}): _description_.");
                    }
                    foreach (PyParameter parameter in optionalParameters) {
                        writer.WriteLine($"{parameter.Name} ({parameter.TypeStr()}): _description_. Defaults to {parameter.FormatValue()}.");
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

            string pascalName = TriggerTranslate.ToPascalCase(overrides.Name);
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

        private readonly HashSet<string> csReserved = new() { "operator", "event", "string" };
        public void WriteInterface(IndentedTextWriter writer) {
            string pascalName = TriggerTranslate.ToPascalCase(overrides.Name);
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
                    ScriptType.Vector3 => "Vector3",
                    ScriptType.State => "dynamic",
                    // Enums
                    ScriptType.EnumAlign => "Align",
                    ScriptType.EnumFieldGame => "FieldGame",
                    ScriptType.EnumLocale => "Locale",
                    ScriptType.EnumWeather => "Weather",
                    _ => throw new ArgumentOutOfRangeException($"Invalid parameter type: {scriptType}"),
                };
            }
        }
    }
}
