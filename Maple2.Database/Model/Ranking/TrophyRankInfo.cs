using Maple2.Model.Game;

namespace Maple2.Database.Model.Ranking;

public record TrophyRankInfo(int Rank, long CharacterId, string Name, string Profile, AchievementInfo Trophy);
