using System.Text.RegularExpressions;
using Maple2.Model.Error;
using Maple2.Model.Metadata;

namespace Maple2.Model.Validators;

public static partial class CharacterNameValidator {
    // Regex patterns for valid character names
    private static readonly Regex ValidNamePatternAscii = NamePatternAsciiRegex();
    private static readonly Regex ValidNamePatternUnicode = NamePatternUnicodeRegex();

    /// <summary>
    /// Validates a character name according to all rules.
    /// </summary>
    /// <param name="name">The character name to validate</param>
    /// <returns>CharacterCreateError code if invalid, null if valid</returns>
    public static CharacterCreateError? ValidateName(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            return CharacterCreateError.s_char_err_name;
        }

        string validatedName = name;

        // Check length constraints
        if (validatedName.Length is < Constant.CharacterNameLengthMin) {
            return CharacterCreateError.s_char_err_name;
        }
        if (validatedName.Length > Constant.CharacterNameLengthMax) {
            return CharacterCreateError.s_char_err_system;
        }

        // Select pattern
        Regex pattern = Constant.AllowUnicodeInNames ? ValidNamePatternUnicode : ValidNamePatternAscii;
        if (!pattern.IsMatch(validatedName)) {
            return CharacterCreateError.s_char_err_ban_all;
        }

        // Check for names that are only special characters
        if (validatedName.All(c => !char.IsLetterOrDigit(c))) {
            return CharacterCreateError.s_char_err_name;
        }

        return null; // Valid name
    }

    // ASCII only: A-Z, a-z, 0-9 (no dash, no underscore)
    [GeneratedRegex(@"^[A-Za-z0-9]+$", RegexOptions.Compiled)]
    private static partial Regex NamePatternAsciiRegex();

    // Unicode: \p{L} (all letters, including accents), 0-9 (no dash, no underscore)
    [GeneratedRegex(@"^[\p{L}0-9]+$", RegexOptions.Compiled)]
    private static partial Regex NamePatternUnicodeRegex();
}
