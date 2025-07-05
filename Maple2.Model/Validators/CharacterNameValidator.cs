using System.Text.RegularExpressions;
using Maple2.Model.Error;
using Maple2.Model.Metadata;

namespace Maple2.Model.Validators;

public static class CharacterNameValidator {
    // Common forbidden words that should not be allowed in character names
    private static readonly HashSet<string> ForbiddenWords = new(StringComparer.OrdinalIgnoreCase) {
        "admin", "moderator", "gm", "gamemaster", "staff", "bot", "system", "server",
        "maple", "nexon", "maplestory", "maple2", "administrator", "support", "helper",
        "fuck", "shit", "bitch", "damn", "ass", "hell", "crap", "piss"
    };

    // Names that are completely banned
    private static readonly HashSet<string> BannedNames = new(StringComparer.OrdinalIgnoreCase) {
        "admin", "administrator", "moderator", "gm", "gamemaster", "staff", "system", "server",
        "maple", "nexon", "maplestory", "maple2", "support", "helper", "bot", "null", "undefined"
    };

    // Regex pattern for valid character names (letters, numbers, and some special characters, no spaces)
    private static readonly Regex ValidNamePattern = new(@"^[a-zA-Z0-9\-_]+$", RegexOptions.Compiled);

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

        // Check if the name is completely banned
        if (BannedNames.Contains(validatedName)) {
            return CharacterCreateError.s_char_err_ban_all;
        }

        // Check for forbidden words
        if (ContainsForbiddenWord(validatedName)) {
            return CharacterCreateError.s_char_err_ban_any;
        }

        // Check character pattern (letters, numbers, hyphens, underscores only, no spaces)
        if (!ValidNamePattern.IsMatch(validatedName)) {
            return CharacterCreateError.s_char_err_name;
        }

        // Check for names that are only whitespace/special characters
        if (validatedName.All(c => !char.IsLetterOrDigit(c))) {
            return CharacterCreateError.s_char_err_name;
        }

        return null; // Valid name
    }

    /// <summary>
    /// Checks if the name contains any forbidden words.
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <returns>True if it contains forbidden words</returns>
    private static bool ContainsForbiddenWord(string name) {
        string lowerName = name.ToLowerInvariant();
        return ForbiddenWords.Any(word => lowerName.Contains(word));
    }

    /// <summary>
    /// Gets the forbidden word that was found in the name (for error messages).
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <returns>The forbidden word found, or null if none</returns>
    public static string? GetForbiddenWord(string name) {
        string lowerName = name.ToLowerInvariant();
        return ForbiddenWords.FirstOrDefault(word => lowerName.Contains(word));
    }
}
