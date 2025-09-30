using Maple2.Model.Error;
using Maple2.Model.Metadata;
using Maple2.Model.Validators;
using NUnit.Framework;

namespace Maple2.Server.Tests.Validators;

public class GuildNameValidatorTests {
    [Test]
    public void ValidName_ShouldReturnNull() {
        Assert.That(GuildNameValidator.ValidateName("GuildName"), Is.Null);
        Assert.That(GuildNameValidator.ValidateName("Guild123"), Is.Null);
        Assert.That(GuildNameValidator.ValidateName("abc"), Is.Null); // minimum length
        Assert.That(GuildNameValidator.ValidateName("0123456789012345678912345"), Is.Null); // maximum length
    }

    [Test]
    public void TooShortName_ShouldReturnNameValueError() {
        Assert.That(GuildNameValidator.ValidateName("a"), Is.EqualTo(GuildError.s_guild_err_name_value));
        Assert.That(GuildNameValidator.ValidateName(""), Is.EqualTo(GuildError.s_guild_err_name_value));
        Assert.That(GuildNameValidator.ValidateName(" "), Is.EqualTo(GuildError.s_guild_err_name_value));
        Assert.That(GuildNameValidator.ValidateName("ThisNameIsWayTooLongForGuild"), Is.EqualTo(GuildError.s_guild_err_name_value));
    }

    [Test]
    public void InvalidCharacters_ShouldReturnNameValueError() {
        Assert.That(GuildNameValidator.ValidateName("guild-name"), Is.EqualTo(GuildError.s_guild_err_name_value)); // dash not allowed
        Assert.That(GuildNameValidator.ValidateName("Guild_Name"), Is.EqualTo(GuildError.s_guild_err_name_value));
        Assert.That(GuildNameValidator.ValidateName("guild name"), Is.EqualTo(GuildError.s_guild_err_name_value)); // space not allowed
        Assert.That(GuildNameValidator.ValidateName("guild@name"), Is.EqualTo(GuildError.s_guild_err_name_value));
        Assert.That(GuildNameValidator.ValidateName("guild#name"), Is.EqualTo(GuildError.s_guild_err_name_value));
        Assert.That(GuildNameValidator.ValidateName("guild$name"), Is.EqualTo(GuildError.s_guild_err_name_value));
    }

    [Test]
    public void OnlySpecialCharacters_ShouldReturnNameValueError() {
        Assert.That(GuildNameValidator.ValidateName("__"), Is.EqualTo(GuildError.s_guild_err_name_value));
        Assert.That(GuildNameValidator.ValidateName("  "), Is.EqualTo(GuildError.s_guild_err_name_value));
    }

    [Test]
    public void DashesAndUnderscores_ShouldReturnNameValueError() {
        Assert.That(GuildNameValidator.ValidateName("Guild-Name"), Is.EqualTo(GuildError.s_guild_err_name_value));
        Assert.That(GuildNameValidator.ValidateName("Guild_Name"), Is.EqualTo(GuildError.s_guild_err_name_value));
        Assert.That(GuildNameValidator.ValidateName("A-B_C-D"), Is.EqualTo(GuildError.s_guild_err_name_value));
    }

    [Test]
    public void UnicodeNames_ShouldRespectAllowUnicodeFlag() {
        if (!Constant.AllowUnicodeInNames) {
            return;
        }
        Assert.That(GuildNameValidator.ValidateName("김철수"), Is.Null);
        Assert.That(GuildNameValidator.ValidateName("王小明"), Is.Null);
        Assert.That(GuildNameValidator.ValidateName("さくら"), Is.Null);
        Assert.That(GuildNameValidator.ValidateName("Renée"), Is.Null);
        Assert.That(GuildNameValidator.ValidateName("김철수"), Is.Null);
        Assert.That(GuildNameValidator.ValidateName("王小明"), Is.Null);
        Assert.That(GuildNameValidator.ValidateName("さくら"), Is.Null);
        Assert.That(GuildNameValidator.ValidateName("Renée"), Is.Null);
    }
}
