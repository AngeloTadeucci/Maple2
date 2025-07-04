using Maple2.Tools.VectorMath;
using System.Numerics;

namespace Maple2.Server.Game.Model;

public class FieldObject<T> : IFieldObject<T> {
    public int ObjectId { get; }
    public T Value { get; }

    public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
    public Vector3 Rotation { get => Transform.RotationAnglesDegrees; set => Transform.RotationAnglesDegrees = value; }
    public Transform Transform { get; init; }

    public FieldObject(int objectId, T value) {
        ObjectId = objectId;
        Value = value;
        Transform = new Transform();
    }

    public static implicit operator T(FieldObject<T> fieldObject) => fieldObject.Value;
}
