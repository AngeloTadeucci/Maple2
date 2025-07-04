using System.Numerics;
using Maple2.Tools.Collision;

namespace Maple2.Server.Tests.Tools.Collision;

public class RectangleTests {
    [Test]
    public void PointsTest() {
        var origin = new Vector2(1, 2);
        const float width = 4;
        const float length = 6;
        var rectangle = new Rectangle(origin, width, length, 0);

        var expectedPoints = new[] {
            new Vector2(-1, -1),
            new Vector2(3, -1),
            new Vector2(3, 5),
            new Vector2(-1, 5),
        };
        CollectionAssert.AreEqual(expectedPoints, rectangle.Points);
    }

    [Test]
    public void PointsWithAngleTest() {
        var origin = new Vector2(1, 2);
        const float width = 4;
        const float length = 6;
        const float angle = 90;
        var rectangle = new Rectangle(origin, width, length, angle);

        var expectedPoints = new[] {
            new Vector2(4, 0),
            new Vector2(4, 4),
            new Vector2(-2, 4),
            new Vector2(-2, 0),
        };
        CollectionAssert.AreEqual(expectedPoints, rectangle.Points);
    }

    [Test]
    public void ContainsTest() {
        var rectangle = new Rectangle(new Vector2(1, 1), 2, 3, 45);

        var insidePoint = new Vector2(1.5f, 1.5f);
        Assert.That(rectangle.Contains(insidePoint), Is.True);
        var edgePoint = new Vector2(2, 2);
        Assert.That(rectangle.Contains(edgePoint), Is.False);
        var outsidePoint = new Vector2(2, 2);
        Assert.That(rectangle.Contains(outsidePoint), Is.False);
    }
}
