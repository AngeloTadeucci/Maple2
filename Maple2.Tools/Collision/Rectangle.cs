using System;
using System.Numerics;

namespace Maple2.Tools.Collision;

public sealed class Rectangle : Polygon {
    public override Vector2[] Points { get; }

    public Rectangle(in Vector2 origin, float width, float length, float angle) {
        const float toRadians = MathF.PI / 180;

        float radians = angle * toRadians;
        float halfWidth = width / 2;
        float halfLength = length / 2;

        Matrix3x2 transform = Matrix3x2.CreateRotation(radians) * Matrix3x2.CreateTranslation(origin);

        Points = [
            Vector2.Transform(new Vector2(-halfWidth, -halfLength), transform),
            Vector2.Transform(new Vector2(halfWidth, -halfLength), transform),
            Vector2.Transform(new Vector2(halfWidth, halfLength), transform),
            Vector2.Transform(new Vector2(-halfWidth, halfLength), transform),
        ];
    }
}
