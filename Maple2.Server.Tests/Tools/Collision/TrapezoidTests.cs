using System.Numerics;
using Maple2.Tools.Collision;

namespace Maple2.Server.Tests.Tools.Collision;

public class TrapezoidTests {
    [Test]
    public void PointsTest() {
        var origin = new Vector2(1, 2);
        const float width = 4;
        const float endWidth = 6;
        const float distance = 3;
        var trapezoid = new Trapezoid(origin, width, endWidth, distance, 0);

        var expectedPoints = new[] {
            new Vector2(-1, 2),
            new Vector2(3, 2),
            new Vector2(4, 5),
            new Vector2(-2, 5),
        };
        CollectionAssert.AreEqual(expectedPoints, trapezoid.Points);
    }

    [Test]
    public void PointsWithAngleTest() {
        var origin = new Vector2(1, 2);
        const float width = 4;
        const float endWidth = 6;
        const float distance = 3;
        const float angle = 90;
        var trapezoid = new Trapezoid(origin, width, endWidth, distance, angle);

        var expectedPoints = new[] {
            new Vector2(1, 0),
            new Vector2(1, 4),
            new Vector2(-2, 5),
            new Vector2(-2, -1),
        };
        CollectionAssert.AreEqual(expectedPoints, trapezoid.Points);
    }

    [Test]
    public void ContainsTest() {
        var trapezoid = new Trapezoid(new Vector2(1, 1), 2, 4, 2, 45);

        var insidePoint = new Vector2(1.5f, 1.5f);
        Assert.That(trapezoid.Contains(insidePoint), Is.True);

        var onEdgePoint = new Vector2(2, 2);
        Assert.That(trapezoid.Contains(onEdgePoint), Is.False, "Point on edge should be considered outside");

        var outsidePoint = new Vector2(5, 5);
        Assert.That(trapezoid.Contains(outsidePoint), Is.False, "Point clearly outside should be considered outside");
    }
}
