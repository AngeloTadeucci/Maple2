using System.Collections.Generic;

namespace Maple2.Model.Metadata;

public record AnimationMetadata(string Model, IReadOnlyDictionary<string, AnimationSequenceMetadata> Sequences);

public record AnimationSequenceMetadata(string Name, short Id, float Time, List<AnimationKey> Keys);

public record AnimationKey(string Name, float Time);
