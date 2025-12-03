using Maple2.Tools.Extensions;
using System;

namespace Maple2.Server.Tests.Tools;

public class LongExtensionsTests {
    [Test]
    public void Truncate32_WithZero_ReturnsZero() {
        long value = 0L;
        int result = value.Truncate32();
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Truncate32_WithPositiveValue_WithinIntRange_ReturnsSameValue() {
        long value = 123456789L;
        int result = value.Truncate32();
        Assert.That(result, Is.EqualTo(123456789));
    }

    [Test]
    public void Truncate32_WithNegativeValue_WithinIntRange_ReturnsSameValue() {
        long value = -123456789L;
        int result = value.Truncate32();
        Assert.That(result, Is.EqualTo(-123456789));
    }

    [Test]
    public void Truncate32_WithMaxInt_ReturnsMaxInt() {
        long value = int.MaxValue;
        int result = value.Truncate32();
        Assert.That(result, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void Truncate32_WithMinInt_ReturnsMinInt() {
        long value = int.MinValue;
        int result = value.Truncate32();
        Assert.That(result, Is.EqualTo(int.MinValue));
    }

    [Test]
    public void Truncate32_WithValueJustAboveMaxInt_Truncates() {
        // int.MaxValue + 1 should wrap around
        long value = (long) int.MaxValue + 1;
        int result = value.Truncate32();
        // When we add 1 to MaxInt (0x7FFFFFFF), we get 0x80000000
        // Which is MinInt when interpreted as signed int
        Assert.That(result, Is.EqualTo(int.MinValue));
    }

    [Test]
    public void Truncate32_WithValueJustBelowMinInt_Truncates() {
        // int.MinValue - 1 should wrap around
        long value = (long) int.MinValue - 1;
        int result = value.Truncate32();
        // When we subtract 1 from MinInt (0x80000000), we get 0x7FFFFFFF
        // Which is MaxInt when truncated to 32 bits
        Assert.That(result, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void Truncate32_WithLargePositiveValue_TruncatesCorrectly() {
        // Simulating a very large TickCount64 value
        long value = 0x123456789ABCDEF0L;
        int result = value.Truncate32();
        // Should only keep the lower 32 bits: 0x9ABCDEF0
        // When interpreted as signed int, this is negative
        Assert.That(result, Is.EqualTo(unchecked((int) 0x9ABCDEF0)));
    }

    [Test]
    public void Truncate32_MatchesEnvironmentTickCountBehavior() {
        // Test that Truncate32 matches what happens when Environment.TickCount wraps
        // At exactly the boundary where int overflows
        long value = 0x0000000100000000L; // Just past the 32-bit boundary
        int result = value.Truncate32();
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Truncate32_WithMultipleOf32BitBoundary_ReturnsZero() {
        long value = 0x0000000200000000L; // 2 * 2^32
        int result = value.Truncate32();
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Truncate32_WithTickCountScenario_30Minutes() {
        // Simulate a tick count representing 30 minutes (1,800,000 milliseconds)
        long startTick = 9000000000L; // A large starting tick
        int duration = 1800000; // 30 minutes in milliseconds
        long endTick = startTick + duration;

        int startTickInt = startTick.Truncate32();
        int endTickInt = endTick.Truncate32();

        // The difference should be the duration (assuming no wrap-around)
        long difference = (long) endTickInt - startTickInt;
        if (difference < 0) {
            // Handle wrap-around
            difference += 0x100000000L;
        }

        Assert.That(difference, Is.EqualTo(duration));
    }

    [Test]
    public void Truncate32_WithOverflowScenario_HandlesCorrectly() {
        // Test the scenario that was causing issues in production
        // When FieldTick is very large and causes int overflow
        long largeFieldTick = long.MaxValue - 1000000L;
        int truncated = largeFieldTick.Truncate32();

        // Should not throw an exception and should produce a valid int
        Assert.That(truncated, Is.InRange(int.MinValue, int.MaxValue));
    }

    [Test]
    public void Truncate32_ConsistentWithBitwiseAnd() {
        // Verify that our implementation matches the expected bitwise AND behavior
        long[] testValues = [
            0L,
            1L,
            -1L,
            int.MaxValue,
            int.MinValue,
            (long) int.MaxValue + 1,
            (long) int.MinValue - 1,
            0xFFFFFFFF,
            0x100000000L,
            0x123456789ABCDEF0L,
            long.MaxValue,
            long.MinValue,
        ];

        foreach (long value in testValues) {
            int result = value.Truncate32();
            int expected = (int) (0xFFFFFFFF & value);
            Assert.That(result, Is.EqualTo(expected),
                $"Failed for value: {value} (0x{value:X})");
        }
    }

    [Test]
    public void Truncate32_SimulatesTickCountWrapAround() {
        // Simulate what happens when Environment.TickCount wraps around
        // This happens approximately every 49.7 days
        long tickBeforeWrap = 0x00000000FFFFFFFEL; // Just before wrap
        long tickAfterWrap = 0x0000000100000001L; // Just after wrap

        int beforeInt = tickBeforeWrap.Truncate32();
        int afterInt = tickAfterWrap.Truncate32();

        // Before wrap: 0xFFFFFFFE as signed int is -2
        Assert.That(beforeInt, Is.EqualTo(-2));
        // After wrap should wrap to a small positive number
        Assert.That(afterInt, Is.EqualTo(1));
    }

    [TestCase(0L, 0)]
    [TestCase(1L, 1)]
    [TestCase(-1L, -1)]
    [TestCase(2147483647L, 2147483647)] // int.MaxValue
    [TestCase(-2147483648L, -2147483648)] // int.MinValue
    [TestCase(2147483648L, -2147483648)] // int.MaxValue + 1 wraps to int.MinValue
    [TestCase(4294967295L, -1)] // uint.MaxValue becomes -1 as signed int
    [TestCase(4294967296L, 0)] // 2^32 wraps to 0
    public void Truncate32_WithVariousValues_ReturnsExpectedResult(long input, int expected) {
        int result = input.Truncate32();
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Truncate32_DifferentFromDirectCast_InCheckedContext() {
        // In a checked context, direct casting throws OverflowException
        // but Truncate32 handles it gracefully
        long largeValue = 0x123456789ABCDEF0L;

        // This should NOT throw
        Assert.DoesNotThrow(() => {
            int result = largeValue.Truncate32();
            Assert.That(result, Is.EqualTo(unchecked((int) 0x9ABCDEF0)));
        });

        // But a direct cast in checked context WOULD throw
        Assert.Throws<OverflowException>(() => {
            checked {
                int result = (int) largeValue;
            }
        });
    }

    [Test]
    public void Truncate32_DifferentFromDirectCast_WithLargePositiveValue() {
        // Direct cast truncates to the most significant bits that fit
        // Truncate32 explicitly takes the lower 32 bits
        long value = 0x0000000280000001L; // Beyond int.MaxValue

        int truncated = value.Truncate32();
        int directCast = unchecked((int) value); // unchecked to avoid exception

        // Both should be the same in unchecked context
        // This test documents that behavior
        Assert.That(truncated, Is.EqualTo(directCast));
        Assert.That(truncated, Is.EqualTo(-2147483647)); // 0x80000001 as signed int
    }

    [Test]
    public void Truncate32_SafeForTickCountConversion_WhileDirectCastIsNot() {
        // This demonstrates the practical reason for Truncate32:
        // Environment.TickCount64 can be very large and would cause overflow with direct cast
        long tickCount64 = long.MaxValue - 1000L; // A realistic large TickCount64 value

        // Truncate32 works safely
        Assert.DoesNotThrow(() => {
            int tick32 = tickCount64.Truncate32();
            // Should get lower 32 bits
            Assert.That(tick32, Is.EqualTo(unchecked((int) 0xFFFFFC17)));
        });

        // Direct cast in checked context would throw
        Assert.Throws<OverflowException>(() => {
            checked {
                int tick32 = (int) tickCount64;
            }
        });
    }
}
