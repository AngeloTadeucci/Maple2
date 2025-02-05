using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record FunctionCubeMetadata(
    int Id,
    int RecipeId,
    InteractCubeState DefaultState,
    int[] AutoStateChange,
    int AutoStateChangeTime,
    FunctionCubeMetadata.NurturingData? Nurturing
) {
    public record NurturingData(
        RewardItem Feed,
        RewardItem RewardFeed,
        NurturingData.Growth[] RequiredGrowth,
        string QuestTag
    ) {
        public record Growth(int Exp, short Stage, RewardItem Reward);
    }
}
