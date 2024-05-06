using System.Numerics;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model;

public abstract class FieldEntity<T>(FieldManager field, int objectId, T value) : IFieldEntity<T>, IUpdatable {
    public FieldManager Field { get; } = field;
    public int ObjectId { get; } = objectId;
    public T Value { get; } = value;

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public static implicit operator T(FieldEntity<T> fieldEntity) => fieldEntity.Value;

    public virtual void Update(long tickCount) { }
}
