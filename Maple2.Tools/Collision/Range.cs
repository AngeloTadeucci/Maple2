namespace Maple2.Tools.Collision;

public readonly record struct Range(float Min, float Max) {
    public float Length => Max - Min;

    public bool Overlaps(in Range other) {
        return Max >= other.Min && Min <= other.Max;
    }
}
