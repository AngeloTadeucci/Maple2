using System.Diagnostics;
using System.Globalization;

namespace Maple2.File.Ingest.Utils;

internal enum ScriptType { None = 0, Str, Int, Float, IntList, StrList, StateList, Vector3, Bool, State }

internal record Parameter(ScriptType Type, string Name) {
    public string? Value;

    public Parameter(ScriptType type, string name, string? value) : this(type, name) {
        Value = value;
    }

    public string TypeStr() {
        return Type switch {
            ScriptType.Str => "str",
            ScriptType.Int => "int",
            ScriptType.Float => "float",
            ScriptType.IntList => "List[int]",
            ScriptType.StrList => "List[str]",
            ScriptType.StateList => "List[common.Trigger]",
            ScriptType.Vector3 => "List[float]",
            ScriptType.Bool => "bool",
            ScriptType.State => "'Trigger'",
            _ => throw new ArgumentException($"Invalid parameter type: {Type}"),
        };
    }

    public string DefaultStr() {
        return Value ?? Type switch {
            ScriptType.Str => "None",
            ScriptType.Int => "0",
            ScriptType.Float => "0.0",
            ScriptType.IntList => "[]",
            ScriptType.StrList => "[]",
            ScriptType.StateList => "[]",
            ScriptType.Vector3 => "[0,0,0]",
            ScriptType.Bool => "False",
            ScriptType.State => "None",
            _ => throw new ArgumentException($"Invalid parameter type: {Type}"),
        };
    }

    public string FormatValue() {
        ValidateValue();
        return Type switch {
            ScriptType.Str => string.IsNullOrWhiteSpace(Value) ? "None" : $"'{Value.Replace("'", "\\'")}'",
            ScriptType.Int => string.IsNullOrWhiteSpace(Value) ? "0" : long.Parse(Value).ToString(),
            ScriptType.Float => string.IsNullOrWhiteSpace(Value) ? "0.0" : float.Parse(Value).ToString(CultureInfo.InvariantCulture),
            ScriptType.IntList => string.IsNullOrWhiteSpace(Value) ? "[]"
                : $"[{string.Join(",", GetIntList(Value).Select(int.Parse))}]",
            ScriptType.StrList => string.IsNullOrWhiteSpace(Value) ? "[]"
                : $"[{string.Join(",", Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(str => $"'{str.Replace("'", "\\'")}'"))}]",
            ScriptType.StateList => string.IsNullOrWhiteSpace(Value) ? "[]"
                : $"[{string.Join(",", Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))}]",
            ScriptType.Vector3 => string.IsNullOrWhiteSpace(Value) ? "[]"
                : $"[{string.Join(",", Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))}]",
            ScriptType.Bool => Value?.ToLower() == "true" || Value == "1" ? "True" : "False",
            ScriptType.State => string.IsNullOrWhiteSpace(Value) ? "None" : Value,
            _ => throw new ArgumentException($"Unexpected Type: {Type} for {Name}"),
        };
    }

    private void ValidateValue() {
        if (string.IsNullOrWhiteSpace(Value) || Type == ScriptType.None) {
            return;
        }

        switch (Type) {
            case ScriptType.Str:
                Debug.Assert(Value != null, $"Invalid: {this}");
                return;
            case ScriptType.Int:
                Debug.Assert(long.TryParse(Value, out _), $"Invalid: {this}");
                return;
            case ScriptType.Float:
                Debug.Assert(double.TryParse(Value, out _), $"Invalid: {this}");
                return;
            case ScriptType.IntList:
                // Handle destroy_monster(all)
                if (Value.Equals("all", StringComparison.OrdinalIgnoreCase)) {
                    Value = "-1";
                }
                foreach (string value in GetIntList(Value)) {
                    Debug.Assert(long.TryParse(value, out _), $"Invalid({value}): {this}");
                }
                return;
            case ScriptType.StrList:
                return;
            case ScriptType.StateList:
                return;
            case ScriptType.Vector3:
                string[] values = Value.Split(",", StringSplitOptions.RemoveEmptyEntries);
                Debug.Assert(values.Length == 3, $"Invalid: {this}");
                foreach (string value in values) {
                    Debug.Assert(double.TryParse(value, out _), $"Invalid: {this}");
                }
                return;
            case ScriptType.Bool:
                if (Name == "returnView" && Value == "11000032") {
                    Value = "1";
                }
                string boolValue = Value.ToLower();
                Debug.Assert(boolValue is "true" or "false" or "1" or "0", $"Invalid: {this}");
                return;
            case ScriptType.State:
                return;
            default:
                throw new ArgumentException($"Unexpected Type: {Type} for {Name}");
        }
    }

    private static IList<string> GetIntList(string value) {
        string[] splits = value.Split(new[] { ',', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();
        foreach (string split in splits) {
            string[] range = split.Split("-");
            if (range.Length == 2 && long.TryParse(range[0], out long min) && long.TryParse(range[1], out long max)) {
                for (long i = min; i <= max; i++) {
                    result.Add("" + i);
                }
            } else {
                result.Add(split);
            }
        }

        return result;
    }
}
