using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Model.Skill;

public class InvokeRecord {
    public readonly AdditionalEffectMetadataInvokeEffect Metadata;
    public int SourceBuffId { get; init; }

    public float Value { get; init; }
    public float Rate { get; init; }

    public InvokeRecord(int sourceBuffId, AdditionalEffectMetadataInvokeEffect metadata) {
        Metadata = metadata;
        SourceBuffId = sourceBuffId;
    }
}
