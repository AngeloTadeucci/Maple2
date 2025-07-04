using System;
using System.Numerics;

namespace Maple2.Tools.Collision;

public sealed class Trapezoid : Polygon {
    public override Vector2[] Points { get; }

    public Trapezoid(in Vector2 origin, float width, float endWidth, float distance, float angle) {
        const float toRadians = MathF.PI / 180;

        float radians = angle * toRadians;
        float halfWidth = width / 2;
        float halfEndWidth = endWidth / 2;

        Matrix3x2 transform = Matrix3x2.CreateRotation(radians) * Matrix3x2.CreateTranslation(origin);
        Points = [
            Vector2.Transform(new Vector2(-halfWidth, 0), transform),
            Vector2.Transform(new Vector2(halfWidth, 0), transform),
            Vector2.Transform(new Vector2(halfEndWidth, distance), transform),
            Vector2.Transform(new Vector2(-halfEndWidth, distance), transform),
        ];
    }
}
