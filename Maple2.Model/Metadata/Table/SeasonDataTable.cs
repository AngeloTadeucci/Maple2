namespace Maple2.Model.Metadata;

public record SeasonDataTable(IReadOnlyDictionary<int, SeasonDataTable.Entry> Arcade,
                              IReadOnlyDictionary<int, SeasonDataTable.Entry> Boss,
                              IReadOnlyDictionary<int, SeasonDataTable.Entry> DarkDescent,
                              IReadOnlyDictionary<int, SeasonDataTable.Entry> GuildPvp,
                              IReadOnlyDictionary<int, SeasonDataTable.Entry> Survival,
                              IReadOnlyDictionary<int, SeasonDataTable.Entry> SurvivalSquad,
                              IReadOnlyDictionary<int, SeasonDataTable.Entry> Pvp,
                              IReadOnlyDictionary<int, SeasonDataTable.Entry> UgcMapCommendation,
                              IReadOnlyDictionary<int, SeasonDataTable.Entry> WorldChampionship) : Table {
    public record Entry(
        int Id,
        DateTime StartTime,
        DateTime EndTime,
        int[] Grades);
}
