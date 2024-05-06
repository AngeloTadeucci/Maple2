using System.Numerics;

namespace Maple2.Server.Game.Model;

public class FieldObject<T>(int objectId, T value) : IFieldObject<T> {
    public int ObjectId { get; } = objectId;
    public T Value { get; } = value;

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public static implicit operator T(FieldObject<T> fieldObject) => fieldObject.Value;
}
