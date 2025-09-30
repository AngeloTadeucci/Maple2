using Maple2.Model.Error;
using Maple2.Model.Metadata;
using Maple2.Model.Validators;
using NUnit.Framework;

namespace Maple2.Server.Tests.Validators;

public class ClubNameValidatorTests {
    [Test]
    public void ValidName_ShouldReturnNull() {
        Assert.That(ClubNameValidator.ValidateName("ClubName"), Is.Null);
        Assert.That(ClubNameValidator.ValidateName("Club123"), Is.Null);
        Assert.That(ClubNameValidator.ValidateName("ab"), Is.Null); // minimum length
        Assert.That(ClubNameValidator.ValidateName("0123456789012345678912345"), Is.Null); // maximum length
    }

    [Test]
    public void TooShortName_ShouldReturnNameValueError() {
        Assert.That(ClubNameValidator.ValidateName("Club_Name"), Is.EqualTo(ClubError.s_club_err_name_value));
        Assert.That(ClubNameValidator.ValidateName("a"), Is.EqualTo(ClubError.s_club_err_name_value));
        Assert.That(ClubNameValidator.ValidateName(""), Is.EqualTo(ClubError.s_club_err_name_value));
        Assert.That(ClubNameValidator.ValidateName(" "), Is.EqualTo(ClubError.s_club_err_name_value));
        Assert.That(ClubNameValidator.ValidateName("ThisNameIsWayTooLongForClub"), Is.EqualTo(ClubError.s_club_err_name_value));
    }

    [Test]
    public void InvalidCharacters_ShouldReturnNameValueError() {
        Assert.That(ClubNameValidator.ValidateName("club-name"), Is.EqualTo(ClubError.s_club_err_name_value)); // dash not allowed
        Assert.That(ClubNameValidator.ValidateName("club name"), Is.EqualTo(ClubError.s_club_err_name_value)); // space not allowed
        Assert.That(ClubNameValidator.ValidateName("club@name"), Is.EqualTo(ClubError.s_club_err_name_value));
        Assert.That(ClubNameValidator.ValidateName("club#name"), Is.EqualTo(ClubError.s_club_err_name_value));
        Assert.That(ClubNameValidator.ValidateName("club$name"), Is.EqualTo(ClubError.s_club_err_name_value));
    }

    [Test]
    public void OnlySpecialCharacters_ShouldReturnNameValueError() {
        Assert.That(ClubNameValidator.ValidateName("__"), Is.EqualTo(ClubError.s_club_err_name_value));
        Assert.That(ClubNameValidator.ValidateName("  "), Is.EqualTo(ClubError.s_club_err_name_value));
    }

    [Test]
    public void DashesAndUnderscores_ShouldReturnNameValueError() {
        Assert.That(ClubNameValidator.ValidateName("Club-Name"), Is.EqualTo(ClubError.s_club_err_name_value));
        Assert.That(ClubNameValidator.ValidateName("Club_Name"), Is.EqualTo(ClubError.s_club_err_name_value));
        Assert.That(ClubNameValidator.ValidateName("A-B_C-D"), Is.EqualTo(ClubError.s_club_err_name_value));
    }

    [Test]
    public void UnicodeNames_ShouldRespectAllowUnicodeFlag() {
        if (!Constant.AllowUnicodeInNames) {
            return;
        }
        Assert.That(ClubNameValidator.ValidateName("김철수"), Is.Null);
        Assert.That(ClubNameValidator.ValidateName("王小明"), Is.Null);
        Assert.That(ClubNameValidator.ValidateName("さくら"), Is.Null);
        Assert.That(ClubNameValidator.ValidateName("Renée"), Is.Null);
        Assert.That(ClubNameValidator.ValidateName("김철수"), Is.Null);
        Assert.That(ClubNameValidator.ValidateName("王小明"), Is.Null);
        Assert.That(ClubNameValidator.ValidateName("さくら"), Is.Null);
        Assert.That(ClubNameValidator.ValidateName("Renée"), Is.Null);
    }
}
