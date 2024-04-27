using System.Collections.Generic;

namespace Maple2.Model.Metadata;

public record MeretMarketCategoryTable(IReadOnlyDictionary<int, IReadOnlyDictionary<int, MeretMarketCategoryTable.Tab>> Entries) : Table {

    public record Tab(string[] Categories,
                      bool SortGender,
                      bool SortJob,
                      int[] SubTabIds);
}
