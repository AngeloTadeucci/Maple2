using System.Numerics;
using System.Runtime.CompilerServices;

namespace Maple2.Tools.Extensions;

public static class QuaternionExtensions {
    // Used to check if two floating point values are just about equal, within a margin of error.
    // Used to counteract floating point drift that accumulates over many floating point operations.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNearlyEqual(this Quaternion quaternion1, Quaternion quaternion2, float epsilon = 1e-5f) {
        bool allComponentsEqual = quaternion1.X.IsNearlyEqual(quaternion2.X, epsilon);
        allComponentsEqual &= quaternion1.Y.IsNearlyEqual(quaternion2.Y, epsilon);
        allComponentsEqual &= quaternion1.Z.IsNearlyEqual(quaternion2.Z, epsilon);
        allComponentsEqual &= quaternion1.W.IsNearlyEqual(quaternion2.W, epsilon);

        return allComponentsEqual;
    }
}

