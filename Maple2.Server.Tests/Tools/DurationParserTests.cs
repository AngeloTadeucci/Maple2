using System;
using Maple2.Tools;

namespace Maple2.Server.Tests.Tools;

[TestFixture]
public class DurationParserTests {
    [TestCase("15s", true, 15)]
    [TestCase("30m", true, 30 * 60)]
    [TestCase("2h", true, 2 * 3600)]
    [TestCase("7d", true, 7 * 86400)]
    [TestCase("3w", true, 3 * 7 * 86400)]
    [TestCase("6M", true, 6 * 30 * 86400)]
    [TestCase("1y", true, 365 * 86400)]
    [TestCase("0d", false, 0)]
    [TestCase("", false, 0)]
    [TestCase("1", false, 0)]
    [TestCase("1h30m", false, 0)]
    [TestCase("abc", false, 0)]
    [TestCase("-5d", false, 0)]
    [TestCase("10q", false, 0)]
    public void Parse_DefaultUnits(string token, bool expectedSuccess, int expectedTotalSeconds) {
        bool ok = DurationParser.TryParse(token, out TimeSpan span);
        Assert.That(ok, Is.EqualTo(expectedSuccess), $"Token: {token}");
        if (ok) {
            Assert.That((int) span.TotalSeconds, Is.EqualTo(expectedTotalSeconds), $"Token: {token}");
        }
    }

    [TestCase("1d", true, 1)]
    [TestCase("30d", true, 30)]
    [TestCase("1h", false, 0)]
    [TestCase("5w", false, 0)]
    [TestCase("2M", false, 0)]
    public void Parse_DaysOnly(string token, bool expectedSuccess, int expectedDays) {
        bool ok = DurationParser.TryParse(token, out TimeSpan span, "d");
        Assert.That(ok, Is.EqualTo(expectedSuccess), $"Token: {token}");
        if (ok) {
            Assert.That(span, Is.EqualTo(TimeSpan.FromDays(expectedDays)));
        }
    }
}

