using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record FunctionCubeMetadata(
    int Id,
    InteractCubeState DefaultState,
    int[] AutoStateChange,
    int AutoStateChangeTime,
    FunctionCubeMetadata.NurturingData? Nurturing
) {
    public record NurturingData(
        NurturingData.Item Feed,
        NurturingData.Item RewardFeed,
        NurturingData.Growth[] RequiredGrowth,
        string QuestTag
    ) {
        public record Item(int Id, int Rarity, int Amount);

        public record Growth(int Exp, short Stage, Item Reward);
    }
}
