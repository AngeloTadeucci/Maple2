using System;
using System.Text.RegularExpressions;

namespace Maple2.Tools;

/// <summary>
/// Simple duration parser for admin commands.
/// Format: <number><unit> with NO combination.
/// Units (case sensitive where noted):
///   s = seconds
///   m = minutes
///   h = hours
///   d = days
///   w = weeks (7 days)
///   M = months (treated as 30 days)
///   y = years (treated as 365 days)
/// Returns false if invalid or zero.
/// </summary>
public static partial class DurationParser {
    private static readonly Regex Pattern = DurationPattern();

    /// <summary>
    /// Parse a single duration token of form <number><unit>.
    /// Default allowed units: s m h d w M y (seconds, minutes, hours, days, weeks, months[30d], years[365d]).
    /// Use allowedUnits to restrict (e.g. "d" for days only). Returns false if invalid or unit not permitted.
    /// </summary>
    public static bool TryParse(string? input, out TimeSpan duration, string allowedUnits = "smhdwMy") {
        duration = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(input)) return false;
        input = input.Trim();

        Match m = Pattern.Match(input);
        if (!m.Success) return false;

        if (!int.TryParse(m.Groups["value"].Value, out int value) || value <= 0) return false;
        char unit = m.Groups["unit"].Value[0];
        if (!allowedUnits.Contains(unit)) return false;

        // Map units. Months and years approximated as 30 / 365 days respectively.
        duration = unit switch {
            's' => TimeSpan.FromSeconds(value),
            'm' => TimeSpan.FromMinutes(value),
            'h' => TimeSpan.FromHours(value),
            'd' => TimeSpan.FromDays(value),
            'w' => TimeSpan.FromDays(7 * value),
            'M' => TimeSpan.FromDays(30 * value),
            'y' => TimeSpan.FromDays(365 * value),
            _ => TimeSpan.Zero,
        };
        return duration > TimeSpan.Zero;
    }

    [GeneratedRegex("^(?<value>\\d+)(?<unit>[smhdwMy])$", RegexOptions.Compiled)]
    private static partial Regex DurationPattern();
}
