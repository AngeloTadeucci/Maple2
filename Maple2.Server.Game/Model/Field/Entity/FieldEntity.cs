using System.Numerics;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model;

public abstract class FieldEntity<T> : IFieldEntity<T>, IUpdatable {
    public FieldManager Field { get; }
    public int ObjectId { get; }
    public T Value { get; }

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    protected FieldEntity(FieldManager field, int objectId, T value) {
        Field = field;
        ObjectId = objectId;
        Value = value;
    }

    public static implicit operator T(FieldEntity<T> fieldEntity) => fieldEntity.Value;

    public virtual void Update(long tickCount) { }
}
