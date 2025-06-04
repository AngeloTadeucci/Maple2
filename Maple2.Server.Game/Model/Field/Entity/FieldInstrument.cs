using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model;

public class FieldInstrument(FieldManager field, int objectId, InstrumentMetadata value) : FieldEntity<InstrumentMetadata>(field, objectId, value) {
    public int OwnerId { get; init; }
    public bool Improvising { get; set; }
    public long StartTick { get; set; }
    public bool Ensemble { get; set; }
    public Item? Score { get; set; }
}
