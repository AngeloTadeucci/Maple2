using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Model.Skill;

public class InvokeRecord(int sourceBuffId, AdditionalEffectMetadataInvokeEffect metadata) {
    public readonly AdditionalEffectMetadataInvokeEffect Metadata = metadata;
    public int SourceBuffId { get; init; } = sourceBuffId;

    public float Value { get; init; }
    public float Rate { get; init; }

}
