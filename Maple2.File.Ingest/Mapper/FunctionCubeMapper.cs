using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml.Object;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Nurturing = Maple2.File.Parser.Xml.Object.Nurturing;

namespace Maple2.File.Ingest.Mapper;

public class FunctionCubeMapper : TypeMapper<FunctionCubeMetadata> {
    private readonly FunctionCubeParser parser;

    public FunctionCubeMapper(M2dReader xmlReader) {
        parser = new FunctionCubeParser(xmlReader);
    }

    protected override IEnumerable<FunctionCubeMetadata> Map() {
        foreach ((int id, FunctionCubeRoot functionCubeRoot) in parser.Parse()) {
            FunctionCube functionCube = functionCubeRoot.FunctionCube;

            ConfigurableCube? configurableCube = functionCubeRoot.ConfigurableCube;
            yield return new FunctionCubeMetadata(
                Id: id,
                RecipeId: functionCube.receipeID,
                ConfigurableCubeType: configurableCube is not null ? (ConfigurableCubeType) configurableCube.id : ConfigurableCubeType.None,
                DefaultState: (InteractCubeState) functionCube.DefaultState,
                ControlType: Enum.TryParse(functionCube.ControlType, out InteractCubeControlType controlType) ? controlType : InteractCubeControlType.None,
                AutoStateChange: functionCube.AutoStateChange,
                AutoStateChangeTime: functionCube.AutoStateChangeTime,
                Nurturing: ParseNurturing(functionCube.nurturing)
            );
        }
    }

    private static FunctionCubeMetadata.NurturingData? ParseNurturing(Nurturing? functionCubeNurturing) {
        if (functionCubeNurturing is null || functionCubeNurturing.rewardItem.Length == 0 || functionCubeNurturing.rewardItemByFeeding.Length == 0) {
            return null;
        }
        return new FunctionCubeMetadata.NurturingData(
            Feed: new RewardItem(
                itemId: functionCubeNurturing.rewardItem[0],
                rarity: (short) functionCubeNurturing.rewardItem[1],
                amount: functionCubeNurturing.rewardItem[2]
            ),
            RewardFeed: new RewardItem(
                itemId: functionCubeNurturing.rewardItemByFeeding[0],
                rarity: (short) functionCubeNurturing.rewardItemByFeeding[1],
                amount: functionCubeNurturing.rewardItemByFeeding[2]
            ),
            RequiredGrowth: ParseRequiredGrowth(functionCubeNurturing),
            QuestTag: functionCubeNurturing.nurturingQuestTag
        );

        FunctionCubeMetadata.NurturingData.Growth[] ParseRequiredGrowth(Nurturing nurturing) {
            List<FunctionCubeMetadata.NurturingData.Growth> result = [];
            for (int i = 0; i < nurturing.rewardItemByGrowth.Length; i += 3) {
                result.Add(new FunctionCubeMetadata.NurturingData.Growth(
                    Exp: nurturing.requiredGrowth[i / 3],
                    Stage: (short) (i / 3 + 1),
                    Reward: new RewardItem(
                        itemId: nurturing.rewardItemByGrowth[i],
                        rarity: (short) nurturing.rewardItemByGrowth[i + 1],
                        amount: nurturing.rewardItemByGrowth[i + 2]
                    )
                ));
            }
            return result.ToArray();
        }
    }
}
