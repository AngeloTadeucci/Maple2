using Maple2.Model.Game;
using Maple2.Server.Core.Constants;

namespace Maple2.Server.Tests.Lua;

[NonParallelizable]
public class LuaTests {
    private readonly Maple2.Lua.Lua maple2Lua = new Maple2.Lua.Lua(Target.LOCALE);

    [OneTimeTearDown]
    public void OneTimeTearDown() {
        maple2Lua.Dispose();
    }

    [Test]
    public void TestCalcItemLevel() {
        // Arrange
        const int gearScore = 67;
        const int rarity = 4;
        var itemType = new ItemType(13460307);

        // Act
        int maple2LuaResult = maple2Lua.CalcItemLevel(gearScore, rarity, itemType.Type, 0, 0).Item1;
        int luaResult = Server.Game.LuaFunctions.Lua.CalcItemLevel(gearScore, rarity, itemType.Type, 0, 0).Item1;

        // Assert
        Assert.Multiple(() => {
            Assert.That(maple2LuaResult, Is.EqualTo(8355));
            Assert.That(luaResult, Is.EqualTo(8355));
        });
    }

    [TestCase(0, 0, 1f)]
    [TestCase(100, 0, 1.1f)]
    [TestCase(1000, 0, 2f)]
    [TestCase(2000, 0, 2.5f)]
    [TestCase(3000, 14, 4f)]
    [TestCase(3500, 14, 4.5f)]
    [TestCase(4000, 14, 5f)]
    public void TestCalcCritDamage(int criticalDamage, int mode, float expected) {
        // Act
        float maple2LuaResult = maple2Lua.CalcCritDamage(criticalDamage, mode);
        float luaResult = Server.Game.LuaFunctions.Lua.CalcCritDamage(criticalDamage, mode);

        // Assert
        Assert.Multiple(() => {
            Assert.That(maple2LuaResult, Is.EqualTo(expected).Within(0.01f));
            Assert.That(luaResult, Is.EqualTo(expected).Within(0.01f));
        });
    }

    [TestCase(80, 586, 111, 50, 0, 0, 0.13f)] // random test character
    public void TestCalcPlayerCritRate(int jobCode, long luk, long critRate, long critResistance, int finalCapV, int mode, float expected) {
        // Act
        float maple2LuaResult = maple2Lua.CalcPlayerCritRate(jobCode, luk, critRate, critResistance, finalCapV, mode);
        float luaResult = Server.Game.LuaFunctions.Lua.CalcPlayerCritRate(jobCode, luk, critRate, critResistance, finalCapV, mode);

        // Assert
        Assert.Multiple(() => {
            Assert.That(maple2LuaResult, Is.EqualTo(expected).Within(0.01f));
            Assert.That(luaResult, Is.EqualTo(expected).Within(0.01f));
        });
    }

    [TestCase(100, 50, 0.08f)] // guessed random values
    public void TestCalcNpcCritRate(long critRate, long critResistance, float expected) {
        // Act
        float maple2LuaResult = maple2Lua.CalcNpcCritRate(0, critRate, critResistance);
        float luaResult = Server.Game.LuaFunctions.Lua.CalcNpcCritRate(0, critRate, critResistance);

        // Assert
        Assert.Multiple(() => {
            Assert.That(maple2LuaResult, Is.EqualTo(expected).Within(0.01f));
            Assert.That(luaResult, Is.EqualTo(expected).Within(0.01f));
        });
    }

    [TestCase(1, 0)]
    [TestCase(8, 0)]
    [TestCase(10, 100)]
    [TestCase(90, 900)]
    public void TestCalcRevivalPrice(int level, int expected) {
        // Act
        int maple2LuaResult = maple2Lua.CalcRevivalPrice((ushort) level);
        int luaResult = Server.Game.LuaFunctions.Lua.CalcRevivalPrice((ushort) level);

        // Assert
        Assert.Multiple(() => {
            Assert.That(maple2LuaResult, Is.EqualTo(expected));
            Assert.That(luaResult, Is.EqualTo(expected));
        });
    }

    [TestCase(5, 10, 137)]
    [TestCase(10, 10, 275)]
    [TestCase(15, 20, 1099)]
    [TestCase(5, 50, 2635)]
    [TestCase(5, 90, 7214)]
    public void CalcTaxiCharge(int distance, int level, int expected) {
        // Act
        int maple2LuaResult = maple2Lua.CalcTaxiCharge(distance, (ushort) level);
        int luaResult = Server.Game.LuaFunctions.Lua.CalcTaxiCharge(distance, level);

        // Assert
        Assert.Multiple(() => {
            Assert.That(maple2LuaResult, Is.EqualTo(expected));
            Assert.That(luaResult, Is.EqualTo(expected));
        });
    }

    [TestCase(10, 30000)]
    [TestCase(50, 50000)]
    [TestCase(90, 70000)]
    public void CalcAirTaxiCharge(int level, int expected) {
        // Act
        int maple2LuaResult = maple2Lua.CalcAirTaxiCharge((ushort) level);
        int luaResult = Server.Game.LuaFunctions.Lua.CalcAirTaxiCharge(level);

        // Assert
        Assert.Multiple(() => {
            Assert.That(maple2LuaResult, Is.EqualTo(expected));
            Assert.That(luaResult, Is.EqualTo(expected));
        });
    }

    [TestCase(0, 1, 34, 0, 80, 6, 70, 10)] // fire prism star
    public void TestConstantValueCap(int baseValue, int deviation, int itemType, int jobCode, int optionLevelFactor, int grade, int levelLimit, int expected) {
        // Act
        (float, float) maple2LuaResult = maple2Lua.ConstantValueCap(baseValue, deviation, itemType, jobCode, optionLevelFactor, grade, (ushort) levelLimit);
        (float, float) luaResult = Server.Game.LuaFunctions.Lua.ConstantValueCap(baseValue, deviation, itemType, jobCode, optionLevelFactor, grade, (ushort) levelLimit);

        // Assert
        Assert.Multiple(() => {
            Assert.That(maple2LuaResult.Item1, Is.EqualTo(expected));
            Assert.That(luaResult.Item1, Is.EqualTo(expected));
        });
    }

    [TestCase(0, 1, 34, 0, 80, 6, 70, 67659f)] // fire prism star
    public void TestConstantValueWapMax(int baseValue, int deviation, int itemType, int jobCode, int optionLevelFactor, int grade, int levelLimit, float expected) {
        // Act
        (float, float) maple2LuaResult = maple2Lua.ConstantValueWapMax(baseValue, deviation, itemType, jobCode, optionLevelFactor, grade, (ushort) levelLimit);
        (float, float) luaResult = Server.Game.LuaFunctions.Lua.ConstantValueWapMax(baseValue, deviation, itemType, jobCode, optionLevelFactor, grade, (ushort) levelLimit);

        // Assert
        Assert.Multiple(() => {
            Assert.That(maple2LuaResult.Item1, Is.EqualTo(expected).Within(0.01f));
            Assert.That(luaResult.Item1, Is.EqualTo(expected).Within(0.01f));
        });
    }

    [TestCase(0, 1, 34, 0, 80, 6, 70, 50008f)] // fire prism star
    public void TestConstantValueWapMin(int baseValue, int deviation, int itemType, int jobCode, int optionLevelFactor, int grade, int levelLimit, float expected) {
        // Act
        (float, float) maple2LuaResult = maple2Lua.ConstantValueWapMin(baseValue, deviation, itemType, jobCode, optionLevelFactor, grade, (ushort) levelLimit);
        (float, float) luaResult = Server.Game.LuaFunctions.Lua.ConstantValueWapMin(baseValue, deviation, itemType, jobCode, optionLevelFactor, grade, (ushort) levelLimit);

        // Assert
        Assert.Multiple(() => {
            Assert.That(maple2LuaResult.Item1, Is.EqualTo(expected).Within(0.01f));
            Assert.That(luaResult.Item1, Is.EqualTo(expected).Within(0.01f));
        });
    }
}
