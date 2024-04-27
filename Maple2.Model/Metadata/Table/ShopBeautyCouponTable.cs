using System.Collections.Generic;

namespace Maple2.Model.Metadata;

public record ShopBeautyCouponTable(IReadOnlyDictionary<int, IReadOnlyList<int>> Entries) : Table;
