using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Model.Skill;

public class ReflectRecord(int id, AdditionalEffectMetadataReflect metadata) {
    public readonly int SourceBuffId = id;
    public readonly AdditionalEffectMetadataReflect Metadata = metadata;

    public int Counter = 0;

}
