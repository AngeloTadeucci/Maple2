using System.Collections.Generic;
using Maple2.Tools;

namespace Maple2.Server.Tests.Tools;

public class WeightedSetTests {
    [Test]
    public void AddTest() {
        var set = new WeightedSet<int>();
        set.Add(1, 1);
        set.Add(2, 2);
        set.Add(3, 3);
        Assert.That(set, Has.Count.EqualTo(3));
    }

    [Test]
    public void GetTest() {
        var set = new WeightedSet<int>();
        set.Add(1, 1);
        set.Add(2, 2);
        set.Add(3, 3);
        const int iterations = 100_000;
        var values = new Dictionary<int, int>();
        for (int i = 0; i < iterations; i++) {
            int value = set.Get();
            if (!values.TryAdd(value, 1)) {
                values[value]++;
            }
        }

        Assert.Multiple(() => {
            Assert.That(values[1], Is.EqualTo(iterations / 6).Within(iterations / 100));
            Assert.That(values[2], Is.EqualTo(iterations / 3).Within(iterations / 100));
            Assert.That(values[3], Is.EqualTo(iterations / 2).Within(iterations / 100));
        });
    }

    [Test]
    public void GetTest2() {
        var set = new WeightedSet<int>();
        set.Add(1, 1);
        set.Add(2, 1);
        set.Add(3, 1);
        const int iterations = 100_000;
        var values = new Dictionary<int, int>();
        for (int i = 0; i < iterations; i++) {
            int value = set.Get();
            if (!values.TryAdd(value, 1)) {
                values[value]++;
            }
        }

        Assert.Multiple(() => {
            Assert.That(values[1], Is.EqualTo(iterations / 3).Within(iterations / 100));
            Assert.That(values[2], Is.EqualTo(iterations / 3).Within(iterations / 100));
            Assert.That(values[3], Is.EqualTo(iterations / 3).Within(iterations / 100));
        });
    }
}
