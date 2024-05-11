using System.Numerics;
using Maple2.Server.Game.Manager.Field;
using Maple2.Tools.VectorMath;

namespace Maple2.Server.Game.Model;

public abstract class FieldEntity<T> : IFieldEntity<T>, IUpdatable {
    public FieldManager Field { get; }
    public int ObjectId { get; }
    public T Value { get; }

    public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
    public Vector3 Rotation { get => Transform.RotationAnglesDegrees; set => Transform.RotationAnglesDegrees = value; }
    public Transform Transform { get; init; }

    protected FieldEntity(FieldManager field, int objectId, T value) {
        Field = field;
        ObjectId = objectId;
        Value = value;
        Transform = new Transform();
    }

    public static implicit operator T(FieldEntity<T> fieldEntity) => fieldEntity.Value;

    public virtual void Update(long tickCount) { }
}
