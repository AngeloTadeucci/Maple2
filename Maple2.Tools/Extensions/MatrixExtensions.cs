using System.Numerics;
using System;
using System.Runtime.CompilerServices;

namespace Maple2.Tools.Extensions;

public static class MatrixExtensions {
    // Used to check if two floating point values are just about equal, within a margin of error.
    // Used to counteract floating point drift that accumulates over many floating point operations.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNearlyEqual(this Matrix4x4 matrix1, Matrix4x4 matrix2, float epsilon = 1e-5f) {
        bool allComponentsEqual = true;

        for (int x = 0; allComponentsEqual && x < 4; ++x) {
            for (int y = 0; allComponentsEqual && y < 4; ++y) {
                allComponentsEqual &= matrix1[x, y].IsNearlyEqual(matrix2[x, y], epsilon);
            }
        }

        return allComponentsEqual;
    }

    // Extracts the rotation matrix without position and scale from the transformation matrix.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 GetRotationMatrix(this Matrix4x4 matrix) {
        Vector3 right = Vector3.Normalize(new Vector3(matrix.M11, matrix.M12, matrix.M13));
        Vector3 front = Vector3.Normalize(new Vector3(matrix.M21, matrix.M22, matrix.M23));
        Vector3 up = Vector3.Normalize(new Vector3(matrix.M31, matrix.M32, matrix.M33));

        return new Matrix4x4(
            right.X, right.Y, right.Z, 0, // M11, M12, M13, M14; Right
            front.X, front.Y, front.Z, 0, // M21, M22, M23, M24; Front
               up.X, up.Y, up.Z, 0, // M31, M32, M33, M34; Up
                  0, 0, 0, 1  // M41, M42, M43, M44; Position
        );
    }

    // Extracts the Euler angles in MS2's space from the rotation matrix using the rotation matrix's definition;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetRotationAngles(this Matrix4x4 matrix, bool normalize = true) {
        Matrix4x4 rotation = normalize ? matrix.GetRotationMatrix() : matrix;

        float sin1 = 0;
        float cos1 = 0;

        float sin2 = -rotation.M32;

        float sin3 = 0;
        float cos3 = 1;

        if (Math.Abs(sin2) > 0.9999f) {
            // Handling gimbal lock
            sin1 = rotation.M21;
            cos1 = rotation.M23;
        } else {
            sin1 = rotation.M31;
            cos1 = rotation.M33;

            sin3 = rotation.M12;
            cos3 = rotation.M22;
        }

        float pitch = (float) Math.Asin(sin2);
        float roll = (float) Math.Atan2(sin1, cos1);
        float yaw = (float) Math.Atan2(sin3, cos3);

        return new Vector3(pitch, roll, yaw);
    }

    // Extracts a quaternion from the rotation matrix of the transform.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion GetQuaternion(this Matrix4x4 matrix, bool normalize = true) {

        Matrix4x4 rotation = normalize ? matrix.GetRotationMatrix() : matrix;

        Quaternion quaternion = Quaternion.CreateFromRotationMatrix(rotation);

        return normalize ? Quaternion.Normalize(quaternion) : quaternion;
    }

    // Gets the right facing axis of the object represented by the transform.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetRightAxis(this Matrix4x4 matrix) {
        return new Vector3(matrix.M11, matrix.M12, matrix.M13);
    }

    // Sets the right facing axis of the object represented by the transform.
    // Be careful using this as it might make the matrix no longer orthogonal, resulting in stretched and skewed dimensions when operating on it.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 SetRightAxis(this Matrix4x4 matrix, Vector3 value) {
        matrix.M11 = value.X;
        matrix.M12 = value.Y;
        matrix.M13 = value.Z;

        return matrix;
    }

    // Gets the up facing axis of the object represented by the transform.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetUpAxis(this Matrix4x4 matrix) {
        return new Vector3(matrix.M31, matrix.M32, matrix.M33);
    }

    // Sets the up facing axis of the object represented by the transform.
    // Be careful using this as it might make the matrix no longer orthogonal, resulting in stretched and skewed dimensions when operating on it.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 SetUpAxis(this Matrix4x4 matrix, Vector3 value) {
        matrix.M31 = value.X;
        matrix.M32 = value.Y;
        matrix.M33 = value.Z;

        return matrix;
    }

    // Gets the front facing axis of the object represented by the transform.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetFrontAxis(this Matrix4x4 matrix) {
        return new Vector3(matrix.M21, matrix.M22, matrix.M23);
    }

    // Sets the front facing axis of the object represented by the transform.
    // Be careful using this as it might make the matrix no longer orthogonal, resulting in stretched and skewed dimensions when operating on it.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 SetFrontAxis(this Matrix4x4 matrix, Vector3 value) {
        matrix.M21 = value.X;
        matrix.M22 = value.Y;
        matrix.M23 = value.Z;

        return matrix;
    }
}
