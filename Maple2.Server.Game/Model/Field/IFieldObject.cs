using Maple2.Tools.VectorMath;
using System.Numerics;

namespace Maple2.Server.Game.Model;

/// <summary>
/// FieldObjects are passive objects that are spawned on the field.
/// </summary>
public interface IFieldObject {
    public int ObjectId { get; }

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Transform Transform { get; init; }
}

/// <summary>
/// FieldObjects are passive objects that are spawned on the field.
/// </summary>
/// <typeparam name="T">The type contained by this object</typeparam>
public interface IFieldObject<out T> : IFieldObject {
    public T Value { get; }
}
