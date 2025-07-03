using Maple2.Model.Common;

namespace Maple2.Server.Tests.Vector;

public class Vector3BTests {

    [Test]
    public void ConvertFromInt() {
        // Arrange
        (sbyte x, sbyte y, sbyte z) = Vector3B.ConvertFromInt(129016);

        // Assert
        Assert.That(x, Is.EqualTo(-8));
        Assert.That(y, Is.EqualTo(-9));
        Assert.That(z, Is.EqualTo(1));
    }

    [Test]
    public void ConvertToInt() {
        // Arrange
        var coord = new Vector3B(-8, -9, 1);

        // Act
        int result = coord.ConvertToInt();

        // Assert
        Assert.That(result, Is.EqualTo(129016));
    }
}
