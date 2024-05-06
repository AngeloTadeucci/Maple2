using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model;

public class FieldPortal(FieldManager field, int objectId, Portal value) : FieldEntity<Portal>(field, objectId, value) {
    public bool Visible = value.Visible;
    public bool Enabled = value.Enable;
    public bool MinimapVisible = value.MinimapVisible;
    public int EndTick;
    public string Model = "";
    public long HomeId;
    public string OwnerName = "";
    public string Password = "";

    public override void Update(long tickCount) {
        if (EndTick != 0 && tickCount > EndTick) {
            Field.RemovePortal(ObjectId);
        }
    }
}
