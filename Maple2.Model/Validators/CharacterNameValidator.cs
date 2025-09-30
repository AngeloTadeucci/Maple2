using System.Text.RegularExpressions;
using Maple2.Model.Error;
using Maple2.Model.Metadata;

namespace Maple2.Model.Validators;

public static partial class CharacterNameValidator {
    // Regex pattern for valid character names (Unicode letters, numbers, and some special characters, no spaces)
    private static readonly Regex ValidNamePattern = NamePatternRegex();

    /// <summary>
    /// Validates a character name according to all rules.
    /// </summary>
    /// <param name="name">The character name to validate</param>
    /// <returns>CharacterCreateError code if invalid, null if valid</returns>
    public static CharacterCreateError? ValidateName(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            return CharacterCreateError.s_char_err_name;
        }

        // Use the original name for validation
        string validatedName = name;

        // Check length constraints
        if (validatedName.Length < Constant.CharacterNameLengthMin) {
            return CharacterCreateError.s_char_err_name;
        }

        if (validatedName.Length > Constant.CharacterNameLengthMax) {
            return CharacterCreateError.s_char_err_system;
        }

        // Check character pattern (Unicode letters, numbers, hyphens, underscores only, no spaces)
        if (!ValidNamePattern.IsMatch(validatedName)) {
            return CharacterCreateError.s_char_err_ban_all;
        }

        // Check for names that are only whitespace/special characters
        if (validatedName.All(c => !char.IsLetterOrDigit(c))) {
            return CharacterCreateError.s_char_err_name;
        }

        return null; // Valid name
    }

    [GeneratedRegex(@"^[\p{L}0-9\-_]+$", RegexOptions.Compiled)]
    private static partial Regex NamePatternRegex();
}
