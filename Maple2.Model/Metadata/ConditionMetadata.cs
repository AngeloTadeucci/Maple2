using System.Numerics;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record ConditionMetadata(
    ConditionType Type,
    long Value,
    ConditionMetadata.Parameters? Codes,
    ConditionMetadata.Parameters? Target,
    int PartyCount = 0,
    int GuildPartyCount = 0) {

    public record Parameters(
        string[]? Strings = null,
        Range<int>? Range = null,
        int[]? Integers = null);

    public readonly record struct Range<T>(T Min, T Max) where T : INumber<T>;
}
