using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model;

public class FieldQuestPortal : FieldPortal {
    public readonly FieldPlayer Owner;

    public FieldQuestPortal(FieldPlayer owner, FieldManager field, int objectId, Portal value) : base(field, objectId, value) {
        Owner = owner;
    }
}
