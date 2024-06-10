using System.Collections.Generic;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record SurvivalSkinInfoTable(IReadOnlyDictionary<int, MedalType> Entries) : Table;
