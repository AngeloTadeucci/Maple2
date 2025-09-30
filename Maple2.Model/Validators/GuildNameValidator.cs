using System.Text.RegularExpressions;
using Maple2.Model.Error;
using Maple2.Model.Metadata;

namespace Maple2.Model.Validators;

public static partial class GuildNameValidator {
    private static readonly Regex ValidNamePatternAscii = NamePatternAsciiRegex();
    private static readonly Regex ValidNamePatternUnicode = NamePatternUnicodeRegex();

    /// <summary>
    /// Validates a guild name according to all rules.
    /// </summary>
    /// <param name="name">The guild name to validate</param>
    /// <returns>GuildError code if invalid, null if valid</returns>
    public static GuildError? ValidateName(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            return GuildError.s_guild_err_name_value;
        }
        if (name.Length is < Constant.GuildNameLengthMin or > Constant.GuildNameLengthMax) {
            return GuildError.s_guild_err_name_value;
        }
        Regex pattern = Constant.AllowUnicodeInNames ? ValidNamePatternUnicode : ValidNamePatternAscii;
        if (!pattern.IsMatch(name)) {
            return GuildError.s_guild_err_name_value;
        }
        if (name.All(c => !char.IsLetterOrDigit(c))) {
            return GuildError.s_guild_err_name_value;
        }
        return null; // Valid name
    }

    // ASCII only: A-Z, a-z, 0-9 (no dash, no underscore)
    [GeneratedRegex(@"^[A-Za-z0-9]+$", RegexOptions.Compiled)]
    private static partial Regex NamePatternAsciiRegex();
    // Unicode: \p{L} (all letters, including accents/CJK), 0-9 (no dash, no underscore)
    [GeneratedRegex(@"^[\p{L}0-9]+$", RegexOptions.Compiled)]
    private static partial Regex NamePatternUnicodeRegex();
}
