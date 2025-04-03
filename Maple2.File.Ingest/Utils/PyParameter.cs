using System.Diagnostics;
using System.Globalization;

namespace Maple2.File.Ingest.Utils;

internal enum ScriptType {
    None = 0, Str, Int, Float, IntList, StrList, StateList, Vector3, Bool, State,
    EnumAlign, EnumFieldGame, EnumLocale, EnumWeather, EnumBannerType
}

internal record PyParameter(ScriptType Type, string Name) {
    public string? Value;

    public PyParameter(ScriptType type, string name, string? value) : this(type, name) {
        Value = value;
    }

    public string? Import() {
        return Type switch {
            ScriptType.Vector3 => "Vector3",
            ScriptType.EnumAlign => "Align",
            ScriptType.EnumFieldGame => "FieldGame",
            ScriptType.EnumLocale => "Locale",
            ScriptType.EnumWeather => "Weather",
            ScriptType.EnumBannerType => "BannerType",
            _ => null,
        };
    }

    public bool IsDefault(string? defaultOverride) {
        if (string.IsNullOrWhiteSpace(Value)) {
            return true;
        }
        if (defaultOverride == "<required>") {
            return false;
        }

        string valueStr = FormatValue();
        // Alternative format that's also equivalent to default.
        if (Type == ScriptType.Vector3 && valueStr == "Vector3(0,0,0)") {
            valueStr = "Vector3()";
        }

        return valueStr == (defaultOverride ?? DefaultStr());
    }

    public string TypeStr() {
        return TypeStr(Type);
    }

    public static string TypeStr(ScriptType type) {
        return type switch {
            ScriptType.None => "None",
            ScriptType.Str => "str",
            ScriptType.Int => "int",
            ScriptType.Float => "float",
            ScriptType.IntList => "List[int]",
            ScriptType.StrList => "List[str]",
            ScriptType.StateList => "List['Trigger']",
            ScriptType.Vector3 => "Vector3",
            ScriptType.Bool => "bool",
            ScriptType.State => "'Trigger'",
            // Enums
            ScriptType.EnumAlign => "Align",
            ScriptType.EnumFieldGame => "FieldGame",
            ScriptType.EnumLocale => "Locale",
            ScriptType.EnumWeather => "Weather",
            ScriptType.EnumBannerType => "BannerType",
            _ => throw new ArgumentException($"Invalid parameter type: {type}"),
        };
    }

    public string DefaultStr() {
        return Type switch {
            ScriptType.Str => "''",
            ScriptType.Int => "0",
            ScriptType.Float => "0.0",
            ScriptType.IntList => "[]",
            ScriptType.StrList => "[]",
            ScriptType.StateList => "[]",
            ScriptType.Vector3 => "Vector3()",
            ScriptType.Bool => "False",
            ScriptType.State => "None",
            // Enums
            ScriptType.EnumAlign => "Align.center",
            ScriptType.EnumLocale => "Locale.ALL",
            ScriptType.EnumWeather => "Weather.Clear",
            ScriptType.EnumBannerType => "BannerType.Lose",
            _ => throw new ArgumentException($"Invalid parameter type: {Type}"),
        };
    }

    public string FormatValue(bool validate = true) {
        if (validate) ValidateValue();

        try {
            return FormatValue(Type, Value) ?? DefaultStr();
        } catch (Exception) {
            if (validate) {
                throw;
            }
            return Value ?? "None";
        }
    }

    private static string? FormatValue(ScriptType type, string? value) {
        return type switch {
            ScriptType.Str => string.IsNullOrWhiteSpace(value) ? null : $"'{value.Replace(@"\", @"\\").Replace("'", @"\'")}'",
            ScriptType.Int => string.IsNullOrWhiteSpace(value) ? null : long.Parse(value).ToString(),
            ScriptType.Float => string.IsNullOrWhiteSpace(value) ? null : float.Parse(value).ToString("0.0###", CultureInfo.InvariantCulture),
            ScriptType.IntList => string.IsNullOrWhiteSpace(value) ? null
                : $"[{string.Join(",", GetIntList(value).Select(int.Parse))}]",
            ScriptType.StrList => string.IsNullOrWhiteSpace(value) ? null
                : $"[{string.Join(",", value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(str => $"'{str.Replace(@"\", @"\\").Replace("'", @"\'")}'"))}]",
            ScriptType.StateList => string.IsNullOrWhiteSpace(value) ? null
                : $"[{string.Join(",", value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))}]",
            ScriptType.Vector3 => string.IsNullOrWhiteSpace(value) ? null
                : $"Vector3({string.Join(",", value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))})",
            ScriptType.Bool => string.IsNullOrWhiteSpace(value) ? null : value.ToLower() == "true" || value == "1" ? "True" : "False",
            ScriptType.State => string.IsNullOrWhiteSpace(value) ? null : value,
            // Enums
            ScriptType.EnumAlign => string.IsNullOrWhiteSpace(value) ? null : value.ToLower() switch {
                "center" => "Align.center",
                "left" => "Align.left",
                "right" => "Align.right",
                "topcenter" => "Align.topCenter",
                "centerleft" => "Align.centerLeft",
                "centerright" => "Align.centerRight",
                "bottomleft" => "Align.bottomLeft",
                "bottomright" => "Align.bottomRight",
                _ => throw new ArgumentException($"Unexpected Align: {value}"),
            },
            ScriptType.EnumFieldGame => string.IsNullOrWhiteSpace(value) ? null : $"FieldGame.{value}",
            ScriptType.EnumLocale => string.IsNullOrWhiteSpace(value) ? null : $"Locale.{value.ToUpper()}",
            ScriptType.EnumWeather => string.IsNullOrWhiteSpace(value) ? null : value switch {
                "None" => "Weather.Clear",
                _ => $"Weather.{TriggerTranslate.ToPascalCase(value)}",
            },
            ScriptType.EnumBannerType => string.IsNullOrWhiteSpace(value) ? null : value switch {
                // The type values intentionally do not match the BannerType enum values.
                //"0" => "",
                "1" => "BannerType.Text",
                //"2" => "",
                "3" => "BannerType.Winner",
                "4" => "BannerType.Lose",
                "5" => "BannerType.GameOver",
                "6" => "BannerType.Bonus",
                "7" => "BannerType.Success",
                _ => throw new ArgumentException($"Unexpected BannerType: {value}"),
            },
            _ => throw new ArgumentException($"Unexpected Type: {type} for {value}"),
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
                if (!long.TryParse(Value, out _)) {
                    Value = "-1";
                }
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
                if (Name == "return_view" && Value == "11000032") {
                    Value = "1";
                }
                string boolValue = Value.ToLower();
                Debug.Assert(boolValue is "true" or "false" or "1" or "0", $"Invalid: {this}");
                return;
            case ScriptType.State:
                return;
            case ScriptType.EnumAlign:
                if (Value == "Reft") {
                    Value = "Left";
                }
                string alignValue = Value.ToLower();
                Debug.Assert(alignValue is "top" or "center" or "bottom" or "left" or "right" or "topcenter"
                        or "centerleft" or "centerright" or "bottomleft" or "bottomright", $"Invalid: {this}");
                return;
            case ScriptType.EnumFieldGame:
                if (Value == "MapleSurvive") {
                    Value = "MapleSurvival";
                }
                Debug.Assert(Value is "HideAndSeek" or "GuildVsGame" or "MapleSurvival" or "MapleSurvivalTeam" or "WaterGunBattle", $"Invalid: {this}");
                break;
            case ScriptType.EnumLocale:
                string localeValue = Value.ToUpper();
                Debug.Assert(localeValue is "KR" or "CN" or "NA" or "JP" or "TH" or "TW", $"Invalid: {this}");
                break;
            case ScriptType.EnumWeather:
                string weatherValue = Value.ToLower();
                Debug.Assert(weatherValue is "none" or "snow" or "heavysnow" or "rain" or "heavyrain" or "sandstorm" or "cherryblossom" or "leaffall", $"Invalid: {this}");
                return;
            case ScriptType.EnumBannerType:
                if (!byte.TryParse(Value, out byte bannerTypeValue)) {
                    bannerTypeValue = byte.MaxValue;
                }
                Debug.Assert(bannerTypeValue is >= 0 and < 9, $"Invalid: {this}");
                return;
            default:
                throw new ArgumentException($"Unexpected Type: {Type} for {Name}");
        }
    }

    private static IList<string> GetIntList(string value) {
        string[] splits = value.Split(new[] { ',', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        List<string> result = [];
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
