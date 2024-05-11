using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Enum;
using Maple2.File.Parser.Xml.Achieve;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using ConditionType = Maple2.Model.Enum.ConditionType;

namespace Maple2.File.Ingest.Mapper;

public class AchievementMapper : TypeMapper<AchievementMetadata> {
    private readonly AchieveParser parser;

    public AchievementMapper(M2dReader xmlReader) {
        parser = new AchieveParser(xmlReader);
    }

    protected override IEnumerable<AchievementMetadata> Map() {
        foreach ((int id, string name, AchieveData data) in parser.Parse()) {
            var grades = new Dictionary<int, AchievementMetadataGrade>();
            foreach (Grade grade in data.grade) {
                grades.Add(grade.value, new AchievementMetadataGrade(
                    Grade: grade.value,
                    Condition: new ConditionMetadata(
                        Type: (ConditionType) grade.condition.type,
                        Value: grade.condition.value,
                        Codes: grade.condition.code.ConvertCodes(),
                        Target: grade.condition.target.ConvertCodes()),
                    Reward: grade.reward.type == AchieveRewardType.unknown ? null : new AchievementMetadataReward(
                        Type: (AchievementRewardType) grade.reward.type,
                        Code: grade.reward.code,
                        Value: grade.reward.value,
                        Rank: grade.reward.rank)));
            }

            var category = AchievementCategory.Life;
            string[] tags = data.categoryTag;
            if (data.categoryTag.Length > 0) {
                // skip the first in the array and use it as the trophy category
                category = GetTrophyCategory(tags[0]);
                tags = tags.Skip(1).ToArray();
            }

            yield return new AchievementMetadata(
                Id: id,
                Name: name,
                AccountWide: data.account,
                Category: category,
                CategoryTags: tags,
                Grades: grades);
        }
    }

    private static AchievementCategory GetTrophyCategory(string tag) {
        return tag switch {
            "combat" => AchievementCategory.Combat,
            "adventure" => AchievementCategory.Adventure,
            "living" => AchievementCategory.Life,
            _ => AchievementCategory.Life,
        };
    }
}
