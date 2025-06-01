using System.Numerics;
using System;
using System.Globalization;
using Maple2.Server.Game.Trigger.Helpers;

namespace Maple2.Server.Tests.Tools;

public class ParseXmlTests {
    [OneTimeSetUp]
    public void Setup() {
        CultureInfo.CurrentCulture = new("en-US");
    }

    [Test]
    public void ParseInt_ValidAndInvalidValues() {
        Assert.Multiple(() => {
            Assert.That(TriggerFunctionMapping.ParseInt("123"), Is.EqualTo(123));
            Assert.That(TriggerFunctionMapping.ParseInt("-42"), Is.EqualTo(-42));
            Assert.That(TriggerFunctionMapping.ParseInt(null), Is.EqualTo(0));
            Assert.That(TriggerFunctionMapping.ParseInt("notanumber"), Is.EqualTo(0));
            Assert.That(TriggerFunctionMapping.ParseInt(""), Is.EqualTo(0));
        });
    }

    [Test]
    public void ParseFloat_ValidAndInvalidValues() {
        Assert.Multiple(() => {
            Assert.That(TriggerFunctionMapping.ParseFloat("3.14"), Is.EqualTo(3.14f));
            Assert.That(TriggerFunctionMapping.ParseFloat("-2.5"), Is.EqualTo(-2.5f));
            Assert.That(TriggerFunctionMapping.ParseFloat(null), Is.EqualTo(0f));
            Assert.That(TriggerFunctionMapping.ParseFloat("notafloat"), Is.EqualTo(0f));
            Assert.That(TriggerFunctionMapping.ParseFloat(""), Is.EqualTo(0f));
        });
    }

    [Test]
    public void ParseBool_TrueAndFalseValues() {
        Assert.Multiple(() => {
            Assert.That(TriggerFunctionMapping.ParseBool("1"), Is.True);
            Assert.That(TriggerFunctionMapping.ParseBool("true"), Is.True);
            Assert.That(TriggerFunctionMapping.ParseBool("True"), Is.True);
            Assert.That(TriggerFunctionMapping.ParseBool("0"), Is.False);
            Assert.That(TriggerFunctionMapping.ParseBool("false"), Is.False);
            Assert.That(TriggerFunctionMapping.ParseBool(null), Is.False);
            Assert.That(TriggerFunctionMapping.ParseBool(""), Is.False);
            Assert.That(TriggerFunctionMapping.ParseBool("randomstring"), Is.False);
        });
    }

    [Test]
    public void ParseIntArray_ValidAndInvalidValues() {
        Assert.Multiple(() => {
            Assert.That(TriggerFunctionMapping.ParseIntArray("1,2,3"), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(TriggerFunctionMapping.ParseIntArray(""), Is.Empty);
            Assert.That(TriggerFunctionMapping.ParseIntArray(null), Is.Empty);
            Assert.That(TriggerFunctionMapping.ParseIntArray("1,2,a"), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(TriggerFunctionMapping.ParseIntArray("1-5"), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
            Assert.That(TriggerFunctionMapping.ParseIntArray("1-5,9,10"), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 9, 10 }));
            Assert.That(TriggerFunctionMapping.ParseIntArray("a,b,c"), Is.Empty);
            Assert.That(TriggerFunctionMapping.ParseIntArray("5"), Is.EqualTo(new[] { 5 }));
            Assert.That(TriggerFunctionMapping.ParseIntArray("all"), Is.EqualTo(new[] { -1 }));
            Assert.That(TriggerFunctionMapping.ParseIntArray("5-1"), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
            Assert.That(TriggerFunctionMapping.ParseIntArray("10-8,3"), Is.EqualTo(new[] { 8, 9, 10, 3 }));
        });
    }

    [Test]
    public void ParseVector3_ValidAndInvalidValues() {
        Assert.Multiple(() => {
            Assert.That(TriggerFunctionMapping.ParseVector3("1, 2, 3"), Is.EqualTo(new Vector3(1, 2, 3)));
            Assert.That(TriggerFunctionMapping.ParseVector3("-1, -2, -3"), Is.EqualTo(new Vector3(-1, -2, -3)));
            Assert.That(TriggerFunctionMapping.ParseVector3("0,0,0"), Is.EqualTo(Vector3.Zero));
            Assert.That(TriggerFunctionMapping.ParseVector3(null), Is.EqualTo(Vector3.Zero));
            Assert.That(TriggerFunctionMapping.ParseVector3(""), Is.EqualTo(Vector3.Zero));
            Assert.That(TriggerFunctionMapping.ParseVector3("1,2"), Is.EqualTo(Vector3.Zero));
            Assert.That(TriggerFunctionMapping.ParseVector3("a,b,c"), Is.EqualTo(Vector3.Zero));
            Assert.That(TriggerFunctionMapping.ParseVector3("1 2 3"), Is.EqualTo(Vector3.Zero));
        });
    }
}
