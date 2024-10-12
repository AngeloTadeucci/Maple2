using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml.Object;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class FunctionCubeMapper : TypeMapper<FunctionCubeMetadata> {
    private readonly FunctionCubeParser parser;

    public FunctionCubeMapper(M2dReader xmlReader) {
        parser = new FunctionCubeParser(xmlReader);
    }

    protected override IEnumerable<FunctionCubeMetadata> Map() {
        foreach ((int id, FunctionCube functionCube) in parser.Parse()) {
            yield return new FunctionCubeMetadata(
                Id: id,
                DefaultState: (InteractCubeState) functionCube.DefaultState,
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
            Feed: new FunctionCubeMetadata.NurturingData.Item(
                Id: functionCubeNurturing.rewardItem[0],
                Rarity: functionCubeNurturing.rewardItem[1],
                Amount: functionCubeNurturing.rewardItem[2]
            ),
            RewardFeed: new FunctionCubeMetadata.NurturingData.Item(
                Id: functionCubeNurturing.rewardItemByFeeding[0],
                Rarity: functionCubeNurturing.rewardItemByFeeding[1],
                Amount: functionCubeNurturing.rewardItemByFeeding[2]
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
                    Reward: new FunctionCubeMetadata.NurturingData.Item(
                        Id: nurturing.rewardItemByGrowth[i],
                        Rarity: nurturing.rewardItemByGrowth[i + 1],
                        Amount: nurturing.rewardItemByGrowth[i + 2]
                    )
                ));
            }
            return result.ToArray();
        }
    }
}
