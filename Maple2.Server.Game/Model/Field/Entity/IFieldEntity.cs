using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model;

/// <summary>
/// FieldEntities are active objects that are spawned on the field.
/// </summary>
public interface IFieldEntity : IFieldObject, IUpdatable {
    public FieldManager Field { get; }
}

public interface IFieldEntity<out T> : IFieldEntity {
    public T Value { get; }
}
