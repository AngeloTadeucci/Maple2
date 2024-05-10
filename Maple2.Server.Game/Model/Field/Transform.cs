using System.Numerics;

namespace Maple2.Server.Game.Model.Field;

// A transform is a container for object position & orientation data, stored in a transformation matrix.
// Transformation matrices are a fairly universal form of storage for positioning data.
// They're fairly well suited for game environments because they're a natural unit to work with when manipulating vertex data & hierarchies.
// They lend themselves fairly well to quick conversion between different desired input & output formats for positioning data.
public class Transform {
    public static readonly float Epsilon = 0.001f; // Used for computing equality within a range of error.

    public Matrix4x4 Transformation = Matrix4x4.Identity;

    // Describes the position. Matrix components: M41 - M43
    public Vector3 Position {
        get { return Transformation.Translation; }
        set { Transformation.Translation = value; }
    }

    // Describes the rotation with Euler angles in MS2's space.
    public Vector3 RotationAngles {
        get { return GetRotationAngles(); }
        set { SetRotationAngles(value); }
    }

    // Describes the rotation with Euler angles in MS2's space.
    public Vector3 RotationAnglesDegrees {
        get { return GetRotationAngles().AnglesToDegrees(); }
        set { SetRotationAngles(value.AnglesToRadians()); }
    }

    // Describes the rotation with a rotation around an axis.
    public (Vector3 axis, float angle) RotationAxis {
        get { return GetAxisAngle(); }
        set { SetAxisAngle(value); }
    }

    // Describes the rotation with a rotation around an axis.
    public (Vector3 axis, float angle) RotationAxisDegrees {
        get { return GetAxisAngleDegrees(); }
        set { SetAxisAngleDegrees(value); }
    }

    // Describes the rotation with a quaternion. Quaternions are 4D vectors of imaginary numbers that behave and look similarly to 3D axis angles.
    public Quaternion Quaternion {
        get { return GetQuaternion(); }
        set { SetQuaternion(value); }
    }

    // May be slow because of square root, only use for exact values. Comparisons of values can be done with ScaleSquared.
    // This is a float because Gamebryo uses float scalings that are uniform across all axis vs Vector3 scalings.
    public float Scale {
        get { return GetScale(); }
        set { SetScale(value); }
    }

    // Faster than Scale because it doesn't require square root. Good for comparing between two scales.
    // This is a float because Gamebryo uses float scalings that are uniform across all axis vs Vector3 scalings.
    public float ScaleSquared {
        get { return GetScaleSquared(); }
    }

    // Right facing axis of the object represented by the transform.
    public Vector3 RightAxis {
        get { return GetRightAxis(); }
        set { SetRightAxis(value); }
    }

    // Up facing axis of the object represented by the transform.
    public Vector3 UpAxis {
        get { return GetUpAxis(); }
        set { SetUpAxis(value); }
    }

    // Front facing axis of the object represented by the transform.
    public Vector3 FrontAxis {
        get { return GetFrontAxis(); }
        set { SetFrontAxis(value); }
    }

    public Transform() { }

    // May be slow because of square root, only use for exact values. Comparisons of values can be done with ScaleSquared.
    public float GetScale() {
        return UpAxis.Length();
    }

    public void SetScale(float newScale) {
        float scale = Scale;

        // Normalize each axis before scaling to prevent floating point drift.
        RightAxis = RightAxis.Normalize() * (newScale / scale);
        UpAxis = UpAxis.Normalize() * (newScale / scale);
        FrontAxis = FrontAxis.Normalize() * (newScale / scale);
    }

    // Faster than Scale because it doesn't require square root.
    public float GetScaleSquared() {
        return UpAxis.LengthSquared();
    }

    // Gets the right facing axis of the object represented by the transform.
    public Vector3 GetRightAxis() {
        return new Vector3(-Transformation.M11, -Transformation.M12, -Transformation.M13);
    }

    // Sets the right facing axis of the object represented by the transform.
    // Be careful using this as it might make the matrix no longer orthogonal, resulting in stretched and skewed dimensions when operating on it.
    public void SetRightAxis(Vector3 value) {
        Transformation.M11 = -value.X;
        Transformation.M12 = -value.Y;
        Transformation.M13 = -value.Z;
    }

    // Gets the up facing axis of the object represented by the transform.
    public Vector3 GetUpAxis() {
        return new Vector3(Transformation.M31, Transformation.M32, Transformation.M33);
    }

    // Sets the up facing axis of the object represented by the transform.
    // Be careful using this as it might make the matrix no longer orthogonal, resulting in stretched and skewed dimensions when operating on it.
    public void SetUpAxis(Vector3 value) {
        Transformation.M31 = value.X;
        Transformation.M32 = value.Y;
        Transformation.M33 = value.Z;
    }

    // Gets the front facing axis of the object represented by the transform.
    public Vector3 GetFrontAxis() {
        return new Vector3(-Transformation.M21, -Transformation.M22, -Transformation.M23);
    }

    // Sets the front facing axis of the object represented by the transform.
    // Be careful using this as it might make the matrix no longer orthogonal, resulting in stretched and skewed dimensions when operating on it.
    public void SetFrontAxis(Vector3 value) {
        Transformation.M21 = -value.X;
        Transformation.M22 = -value.Y;
        Transformation.M23 = -value.Z;
    }

    // Extracts the rotation matrix without position and scale from the transformation matrix.
    public Matrix4x4 GetRotationMatrix() {
        return Transformation.GetRotationMatrix();
    }

    // Extracts the Euler angles in MS2's space from the rotation matrix using the rotation matrix's definition;
    public Vector3 GetRotationAngles(bool normalize = true) {
        return Transformation.GetRotationAngles(normalize);
    }

    // Sets the rotation of the transform with Euler angles in MS2's space without overwriting position & scale.
    // Rotations happen in Z (yaw) -> Y (roll) -> X (pitch) order, which is MS2's Euler angles format.
    public void SetRotationAngles(Vector3 angles) {
        float scale = RightAxis.Length();
        Vector3 position = Position;

        Transformation = NewRotationAngles(angles);
        Position = position;
        Scale = scale;
    }

    // Extracts a quaternion from the rotation matrix of the transform.
    public Quaternion GetQuaternion(bool normalize = true) {
        return Transformation.GetQuaternion(normalize);
    }

    // Sets the rotation of the transform with a quaternion without overwriting position & scale.
    public void SetQuaternion(Quaternion quaternion) {
        float scale = RightAxis.Length();
        Vector3 position = Position;

        Transformation = Matrix4x4.CreateFromQuaternion(quaternion);
        Position = position;
        Scale = scale;
    }

    // Extracts the rotation from of the transform in the form of a rotation around an axis.
    public (Vector3 axis, float angle) GetAxisAngle() {
        Quaternion quaternion = GetQuaternion();

        Vector3 axis = new Vector3(quaternion.X, quaternion.Y, quaternion.Z).Normalize();
        float angle = (float) Math.Acos(quaternion.W) * 2;

        return (axis, angle);
    }

    // Sets the rotation of the transform with a rotation around an axis without overwriting position & scale.
    public void SetAxisAngle((Vector3 axis, float angle) rotation) {
        float scale = RightAxis.Length();
        Vector3 position = Position;

        Transformation = Matrix4x4.CreateFromAxisAngle(rotation.axis, rotation.angle);
        Position = position;
        Scale = scale;
    }

    // Extracts the rotation from of the transform in the form of a rotation around an axis.
    public (Vector3 axis, float angle) GetAxisAngleDegrees() {
        (Vector3 axis, float angle) rotation = GetAxisAngle();

        return (rotation.axis, 180 * rotation.angle / (float)Math.PI);
    }

    // Sets the rotation of the transform with a rotation around an axis without overwriting position & scale.
    public void SetAxisAngleDegrees((Vector3 axis, float angle) rotation) {
        SetAxisAngle((rotation.axis, (float)Math.PI * rotation.angle / 180));
    }

    // Rotations happen in Z (yaw) -> Y (roll) -> X (pitch) order, which is MS2's Euler angles format.
    public static Matrix4x4 NewRotationAngles(Vector3 angles) {
        return NewRotationAngles(angles.X, angles.Y, angles.Z);
    }

    // Rotations happen in yaw -> roll -> pitch order, which is MS2's Euler angles format.
    public static Matrix4x4 NewRotationAngles(float pitch, float roll, float yaw) {
        return NewYawRotation(yaw) * NewRollRotation(roll) * NewPitchRotation(pitch);
    } 

    // Rotates around around the Z, up & down axis.
    public static Matrix4x4 NewYawRotation(float angle) {
        //Matrix4x4 rotation = new Matrix4x4();
        //
        //float sine = (float)Math.Sin(angle);
        //float cosine = (float)Math.Cos(angle);
        //
        //rotation.M11 = cosine;
        //rotation.M12 = sine;
        //
        //rotation.M21 = -sine;
        //rotation.M22 = cosine;

        return Matrix4x4.CreateRotationZ(angle);
    }

    // Rotates around the Y axis.
    public static Matrix4x4 NewRollRotation(float angle) {
        //Matrix4x4 rotation = new Matrix4x4();
        //
        //float sine = (float) Math.Sin(angle);
        //float cosine = (float) Math.Cos(angle);
        //
        //rotation.M11 = cosine;
        //rotation.M13 = -sine;
        //
        //rotation.M31 = sine;
        //rotation.M33 = cosine;

        return Matrix4x4.CreateRotationY(angle);
    }

    // Rotates around the X axis.
    public static Matrix4x4 NewPitchRotation(float angle) {
        //Matrix4x4 rotation = new Matrix4x4();
        //
        //float sine = (float) Math.Sin(angle);
        //float cosine = (float) Math.Cos(angle);
        //
        //rotation.M22 = cosine;
        //rotation.M23 = sine;
        //
        //rotation.M32 = -sine;
        //rotation.M33 = cosine;

        return Matrix4x4.CreateRotationX(angle);
    }
}

public static class VectorMathExtensions {
    // Returns a unit length vector, a vector with a length of 1
    public static Vector3 Normalize(this Vector3 vector) {
        float length = vector.Length();

        return (1 / length) * vector;
    }

    // Returns a value indicating how similar the direction of two vectors is. Positive for same direction, negative for opposite.
    // Equivalent to vector1.Length() * vector2.Length() * cosine where cosine is the cosine of the angle between the two vectors.
    public static float Dot(this Vector3 vector1, Vector3 vector2) {
        float dot = vector1.X * vector2.X;
        dot += vector1.Y * vector2.Y;
        dot += vector1.Z * vector2.Z;

        return dot;
    }

    // Returns a vector that is perpendicular to both vector1 and vector2.
    // The length of the cross product is equal to vector1.Length() * vector2.Length() * sine where sine is the  sine of the angle between the two vectors.
    // The length of the cross product is also equal to the area of the parallelogram formed by the two vectors.
    // It is good for computing normal vectors or the axis of rotation between two vectors.
    public static Vector3 Cross(this Vector3 vector1, Vector3 vector2) {
        return new Vector3(
            vector1.Y * vector2.Z - vector1.Z * vector2.Y,
            vector1.Z * vector2.X - vector1.X * vector2.Z,
            vector1.X * vector2.Y - vector1.Y * vector2.X
        );
    }

    // Converts Euler angles in radians to degrees
    public static Vector3 AnglesToDegrees(this Vector3 vector) {
        return new Vector3(
            180 * vector.X / (float)Math.PI,
            180 * vector.Y / (float)Math.PI,
            180 * vector.Z / (float)Math.PI
        );
    }

    // Converts Euler angles in degrees to radians
    public static Vector3 AnglesToRadians(this Vector3 vector) {
        return new Vector3(
            (float)Math.PI * vector.X / 180,
            (float)Math.PI * vector.Y / 180,
            (float)Math.PI * vector.Z / 180
        );
    }

    // Converts the quaternion back into a normalized form. This is good for mitigating floating point drift.
    public static Quaternion Normalize(this Quaternion quaternion) {
        float scale = 1 / quaternion.Length();

        return new Quaternion(scale * quaternion.X, scale * quaternion.Y, scale * quaternion.Z, scale * quaternion.W);
    }

    // Extracts the rotation matrix without position and scale from the transformation matrix.
    public static Matrix4x4 GetRotationMatrix(this Matrix4x4 matrix) {
        Vector3 right = new Vector3(matrix.M11, matrix.M12, matrix.M13).Normalize();
        Vector3 front = new Vector3(matrix.M21, matrix.M22, matrix.M23).Normalize();
        Vector3 up = new Vector3(matrix.M31, matrix.M32, matrix.M33).Normalize();

        return new Matrix4x4(
            right.X, right.Y, right.Z, 0, // M11, M12, M13, M14; Right
            front.X, front.Y, front.Z, 0, // M21, M22, M23, M24; Front
               up.X,    up.Y,    up.Z, 0, // M31, M32, M33, M34; Up
                  0,       0,       0, 1  // M41, M42, M43, M44; Position
        );
    }

    // Extracts the Euler angles in MS2's space from the rotation matrix using the rotation matrix's definition;
    public static Vector3 GetRotationAngles(this Matrix4x4 matrix, bool normalize = true) {
        Matrix4x4 rotation = normalize ? matrix.GetRotationMatrix() : matrix;

        float sin1 = 0;
        float cos1 = 0;

        float sin2 = -rotation.M13;

        float sin3 = 0;
        float cos3 = 1;

        if (sin2 > Transform.Epsilon) {
            // Handling gimbal lock
            sin1 = rotation.M32;
            cos1 = rotation.M31;
        } else {
            sin1 = rotation.M12;
            cos1 = rotation.M11;

            sin3 = rotation.M23;
            cos3 = rotation.M33;
        }

        float yaw = (float) Math.Atan2(sin1, cos1);
        float roll = (float) Math.Asin(sin2);
        float pitch = (float) Math.Atan2(sin3, cos3);

        return new Vector3(pitch, roll, yaw);
    }

    // Extracts a quaternion from the rotation matrix of the transform.
    public static Quaternion GetQuaternion(this Matrix4x4 matrix, bool normalize = true) {
        Matrix4x4 rotation = normalize ? matrix.GetRotationMatrix() : matrix;

        float traceW = rotation.M11 + rotation.M22 + rotation.M33;
        float traceX = rotation.M11 - rotation.M22 - rotation.M33;
        float traceY = -rotation.M11 + rotation.M22 - rotation.M33;
        float traceZ = -rotation.M11 - rotation.M22 + rotation.M33;

        float axis1 = rotation.M23 + rotation.M32;
        float axis2 = rotation.M31 + rotation.M13;
        float axis3 = rotation.M12 + rotation.M21;

        float axis1N = rotation.M23 - rotation.M32;
        float axis2N = rotation.M31 - rotation.M13;
        float axis3N = rotation.M12 - rotation.M21;

        if (traceW > traceX && traceW > traceY && traceW > traceZ) {
            float w = 0.5f * (float) Math.Sqrt(1 + traceW);

            float x = axis1N;
            float y = axis2N;
            float z = axis3N;

            x /= 4 * w;
            y /= 4 * w;
            z /= 4 * w;

            return new Quaternion(x, y, z, w).Normalize();
        }

        if (traceX > traceY && traceX > traceZ) {
            float w = axis1N;
            float x = 0.5f * (float) Math.Sqrt(1 + traceX);
            float y = axis3;
            float z = axis2;

            w /= 4 * x;
            y /= 4 * x;
            z /= 4 * x;

            return new Quaternion(x, y, z, w).Normalize();
        }

        if (traceY > traceZ) {
            float w = axis2N;
            float x = axis3;
            float y = 0.5f * (float) Math.Sqrt(1 + traceY);
            float z = axis1;

            w /= 4 * y;
            x /= 4 * y;
            z /= 4 * y;

            return new Quaternion(x, y, z, w).Normalize();
        }

        {
            float w = axis3N;
            float x = axis2;
            float y = axis1;
            float z = 0.5f * (float) Math.Sqrt(1 + traceZ);

            w /= 4 * z;
            x /= 4 * z;
            y /= 4 * z;

            return new Quaternion(x, y, z, w).Normalize();
        }
    }
}
