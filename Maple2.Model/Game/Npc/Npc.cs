using System.Collections.Generic;
using Maple2.Model.Metadata;

namespace Maple2.Model.Game;

public class Npc(NpcMetadata metadata, AnimationMetadata? animation) {
    public readonly NpcMetadata Metadata = metadata;
    public readonly IReadOnlyDictionary<string, AnimationSequence> Animations = animation?.Sequences ?? new Dictionary<string, AnimationSequence>();

    public int Id => Metadata.Id;

    public bool IsBoss => Metadata.Basic.Friendly == 0 && Metadata.Basic.Class >= 3;

}
