using Maple2.Model.Error;
using Maple2.Model.Metadata;
using Maple2.Model.Validators;

namespace Maple2.Server.Tests.Validators;

public class CharacterNameValidatorTests {
    [Test]
    public void ValidName_ShouldReturnNull() {
        // Valid names should return null (no error)
        Assert.That(CharacterNameValidator.ValidateName("ValidName"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("Test123"), Is.Null);

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
    public void InvalidCharacters_ShouldReturnNameError() {
        // Names with invalid characters should return name error
        Assert.That(CharacterNameValidator.ValidateName("test@name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("name#test"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("test$name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("test%name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("test*name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));

        // Dashes and underscores are not allowed
        Assert.That(CharacterNameValidator.ValidateName("User_Name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Cool-Name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
    }

    [Test]
    public void OnlySpecialCharacters_ShouldReturnNameError() {
        // Names with only special characters should return name error
        Assert.That(CharacterNameValidator.ValidateName("--"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("__"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("  "), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("-_-"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
    }

    [Test]
    public void NullOrWhitespace_ShouldReturnNameError() {
        // Null or whitespace names should return name error
        Assert.That(CharacterNameValidator.ValidateName(null!), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName(""), Is.EqualTo(CharacterCreateError.s_char_err_name));
        Assert.That(CharacterNameValidator.ValidateName("   "), Is.EqualTo(CharacterCreateError.s_char_err_name));
    }

    [Test]
    public void SpaceValidation_ShouldWork() {
        // Names with spaces should be invalid
        Assert.That(CharacterNameValidator.ValidateName("Test Name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Cool Player"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("a b"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));

        // Names with leading or trailing spaces should be invalid
        Assert.That(CharacterNameValidator.ValidateName(" ValidName"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("ValidName "), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName(" ValidName "), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));

        // Names with multiple consecutive spaces should be invalid
        Assert.That(CharacterNameValidator.ValidateName("Test  Name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Test   Name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("A  B"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
    }

    [Test]
    public void JapaneseCharacterNames_ShouldBeValid() {
        if (!Constant.AllowUnicodeInNames) {
            return;
        }
        // Japanese Hiragana characters should be valid
        Assert.That(CharacterNameValidator.ValidateName("さくら"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("ひろし"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("あいうえお"), Is.Null);

        // Japanese Katakana characters should be valid
        Assert.That(CharacterNameValidator.ValidateName("サクラ"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("ヒロシ"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("アイウエオ"), Is.Null);

        // Japanese Kanji characters should be valid
        Assert.That(CharacterNameValidator.ValidateName("田中"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("山田"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("佐藤"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("鈴木"), Is.Null);

        // Mixed Japanese characters should be valid
        Assert.That(CharacterNameValidator.ValidateName("さくら123"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("田中ひろし"), Is.Null);
        // Mixed with dashes/underscores should be invalid
        Assert.That(CharacterNameValidator.ValidateName("サクラ_田中"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
    }

    [Test]
    public void KoreanCharacterNames_ShouldBeValid() {
        if (!Constant.AllowUnicodeInNames) {
            return;
        }
        // Korean Hangul characters should be valid
        Assert.That(CharacterNameValidator.ValidateName("김철수"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("이영희"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("박민수"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("정수진"), Is.Null);

        // Korean with numbers should be valid
        Assert.That(CharacterNameValidator.ValidateName("김철수123"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("이영희456"), Is.Null);

        // Korean with dashes/underscores should be invalid
        Assert.That(CharacterNameValidator.ValidateName("박민수-정"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("정수진_김"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
    }

    [Test]
    public void ChineseCharacterNames_ShouldBeValid() {
        if (!Constant.AllowUnicodeInNames) {
            return;
        }
        // Simplified Chinese characters should be valid
        Assert.That(CharacterNameValidator.ValidateName("王小明"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("李小红"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("张三"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("刘德华"), Is.Null);

        // Traditional Chinese characters should be valid
        Assert.That(CharacterNameValidator.ValidateName("王小明"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("陳大文"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("黃志強"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("林美玲"), Is.Null);

        // Chinese with numbers should be valid
        Assert.That(CharacterNameValidator.ValidateName("王小明123"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("李小红456"), Is.Null);

        // Chinese with dashes/underscores should be invalid
        Assert.That(CharacterNameValidator.ValidateName("张三-李四"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("刘德华_陈"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
    }

    [Test]
    public void SpecialCharacters_ShouldReturnNameError() {
        // Names with special or non-ASCII symbols should return name error
        Assert.That(CharacterNameValidator.ValidateName("Name★"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Name."), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Name!"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Name<3"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Name♪"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Name~"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Name*"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Name♥"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
    }

    [Test]
    public void DashesAndUnderscores_ShouldReturnNameError() {
        Assert.That(CharacterNameValidator.ValidateName("Name-Name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("Name_Name"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
        Assert.That(CharacterNameValidator.ValidateName("A-B_C-D"), Is.EqualTo(CharacterCreateError.s_char_err_ban_all));
    }

    [Test]
    public void AccentedNames_ShouldRespectAllowAccentsFlag() {
        if (!Constant.AllowUnicodeInNames) {
            return;
        }
        Assert.That(CharacterNameValidator.ValidateName("José"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("Renée"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("Beyoncé"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("José"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("Renée"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("Beyoncé"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("André"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("Zoë"), Is.Null);
        Assert.That(CharacterNameValidator.ValidateName("François"), Is.Null);
    }
}
