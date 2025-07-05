using Maple2.Model.Error;
using Maple2.Model.Validators;

namespace Maple2.Server.Tests.Validators;

public class CharacterNameValidatorTests {
    [Test]
    public void ValidName_ShouldReturnNull() {
        // Valid names should return null (no error)
        Assert.That(CharacterNameValidator.ValidateName("ValidName"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("Test123"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("User_Name"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("Cool-Name"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("ab"), Is.Null); // minimum length
        Assert.That(CharacterNameValidator.ValidateName("abcdefghijkl"), Is.Null); // maximum length
    }

    [Test]
    public void TooShortName_ShouldReturnNameError() {
        // Names shorter than minimum should return name error
        Assert.That(CharacterNameValidator.ValidateName("a"), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName(""), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName(" "), Is.EqualTo(CharacterCreateError.s_char_err_name));
    }

    [Test]
    public void TooLongName_ShouldReturnSystemError() {
        // Names longer than maximum should return system error
        Assert.That(CharacterNameValidator.ValidateName("abcdefghijklm"), Is.EqualTo(CharacterCreateError.s_char_err_system));
        Assert.That(CharacterNameValidator.ValidateName("ThisNameIsTooLong"), Is.EqualTo(CharacterCreateError.s_char_err_system));
    }

    [Test]
    public void BannedName_ShouldReturnBanAllError() {
        // Completely banned names should return ban_all error
        Assert.That(CharacterNameValidator.ValidateName("admin"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("ADMIN"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("moderator"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("gm"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("system"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("maple"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
    }

    [Test]
    public void ForbiddenWord_ShouldReturnBanAnyError() {
        // Names containing forbidden words should return ban_any error
        Assert.That(CharacterNameValidator.ValidateName("TestAdmin"), Is.EqualTo(CharacterCreateError.s_char_err_ban_any));
        Assert.That(CharacterNameValidator.ValidateName("MyStaff"), Is.EqualTo(CharacterCreateError.s_char_err_ban_any));
        Assert.That(CharacterNameValidator.ValidateName("CoolBot"), Is.EqualTo(CharacterCreateError.s_char_err_ban_any));
        Assert.That(CharacterNameValidator.ValidateName("fucked"), Is.EqualTo(CharacterCreateError.s_char_err_ban_any));
        Assert.That(CharacterNameValidator.ValidateName("shitty"), Is.EqualTo(CharacterCreateError.s_char_err_ban_any));
    }

    [Test]
    public void InvalidCharacters_ShouldReturnNameError() {
        // Names with invalid characters should return name error
        Assert.That(CharacterNameValidator.ValidateName("test@name"), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("name#test"), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("test$name"), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("test%name"), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("test*name"), Is.EqualTo(CharacterCreateError.s_char_err_name));
    }

    [Test]
    public void OnlySpecialCharacters_ShouldReturnNameError() {
        // Names with only special characters should return name error
        Assert.That(CharacterNameValidator.ValidateName("--"), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("__"), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("  "), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("-_-"), Is.EqualTo(CharacterCreateError.s_char_err_name));
    }

    [Test]
    public void NullOrWhitespace_ShouldReturnNameError() {
        // Null or whitespace names should return name error
        Assert.That(CharacterNameValidator.ValidateName(null!), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName(""), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("   "), Is.EqualTo(CharacterCreateError.s_char_err_name));
    }

    [Test]
    public void GetForbiddenWord_ShouldReturnCorrectWord() {
        // Should return the forbidden word found in the name
        Assert.That(CharacterNameValidator.GetForbiddenWord("TestAdmin"), Is.EqualTo("admin"));
        Assert.That(CharacterNameValidator.GetForbiddenWord("MyStaff"), Is.EqualTo("staff"));
        Assert.That(CharacterNameValidator.GetForbiddenWord("CoolBot"), Is.EqualTo("bot"));
        Assert.That(CharacterNameValidator.GetForbiddenWord("fucked"), Is.EqualTo("fuck"));
        Assert.That(CharacterNameValidator.GetForbiddenWord("ValidName"), Is.Null);
    }

    [Test]
    public void CaseInsensitiveValidation_ShouldWork() {
        // Validation should be case insensitive
        Assert.That(CharacterNameValidator.ValidateName("ADMIN"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Admin"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("aDmIn"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));

        Assert.That(CharacterNameValidator.ValidateName("TESTADMIN"), Is.EqualTo(CharacterCreateError.s_char_err_ban_any));
        Assert.That(CharacterNameValidator.ValidateName("TestAdmin"), Is.EqualTo(CharacterCreateError.s_char_err_ban_any));
        Assert.That(CharacterNameValidator.ValidateName("testADMIN"), Is.EqualTo(CharacterCreateError.s_char_err_ban_any));
    }

    [Test]
    public void SpaceValidation_ShouldWork() {
        // Names with spaces should be valid
        Assert.That(CharacterNameValidator.ValidateName("Test Name"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("Cool Player"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("a b"), Is.Null); // minimum length with space

        // Names with leading or trailing spaces should be invalid
        Assert.That(CharacterNameValidator.ValidateName(" ValidName"), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("ValidName "), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName(" ValidName "), Is.EqualTo(CharacterCreateError.s_char_err_name));

        // Names with multiple consecutive spaces should be invalid
        Assert.That(CharacterNameValidator.ValidateName("Test  Name"), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("Test   Name"), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("A  B"), Is.EqualTo(CharacterCreateError.s_char_err_name));
    }
}
