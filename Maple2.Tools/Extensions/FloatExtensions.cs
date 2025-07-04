using System;
using System.Runtime.CompilerServices;

namespace Maple2.Tools.Extensions;

public static class FloatExtensions {
    // Used to check if two floating point values are just about equal, within a margin of error.
    // Used to counteract floating point drift that accumulates over many floating point operations.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNearlyEqual(this float number1, float number2, float epsilon = 1e-5f) {
        return Math.Abs(number1 - number2) < epsilon;
    }
}
