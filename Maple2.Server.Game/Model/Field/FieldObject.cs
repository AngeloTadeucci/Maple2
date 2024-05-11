using System.Numerics;

namespace Maple2.Server.Game.Model;

public class FieldObject<T> : IFieldObject<T> {
    public int ObjectId { get; }
    public T Value { get; }

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public FieldObject(int objectId, T value) {
        ObjectId = objectId;
        Value = value;
    }

    public static implicit operator T(FieldObject<T> fieldObject) => fieldObject.Value;
}
