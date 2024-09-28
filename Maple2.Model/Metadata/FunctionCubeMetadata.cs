using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record FunctionCubeMetadata(
    int Id,
    InteractCubeState DefaultState,
    int[] AutoStateChange,
    int AutoStateChangeTime
);
