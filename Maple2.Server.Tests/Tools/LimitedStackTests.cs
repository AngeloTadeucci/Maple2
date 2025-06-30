using System;
using Maple2.Tools;

namespace Maple2.Server.Tests.Tools;

public class LimitedStackTests {
    [Test]
    public void PushAndCountTest() {
        var stack = new LimitedStack<int>(3);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        Assert.That(stack.Count, Is.EqualTo(3));
    }

    [Test]
    public void LimitEnforcedTest() {
        var stack = new LimitedStack<int>(3);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        stack.Push(4); // Should discard 1
        Assert.That(stack.Count, Is.EqualTo(3));
        Assert.That(stack.Contains(1), Is.False);
        Assert.That(stack.Contains(2), Is.True);
        Assert.That(stack.Contains(3), Is.True);
        Assert.That(stack.Contains(4), Is.True);
    }

    [Test]
    public void PopTest() {
        var stack = new LimitedStack<int>(3);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        Assert.That(stack.Pop(), Is.EqualTo(3));
        Assert.That(stack.Pop(), Is.EqualTo(2));
        Assert.That(stack.Pop(), Is.EqualTo(1));
        Assert.That(stack.IsEmpty, Is.True);
    }

    [Test]
    public void PeekTest() {
        var stack = new LimitedStack<int>(3);
        stack.Push(1);
        stack.Push(2);
        Assert.That(stack.Peek(), Is.EqualTo(2));
        stack.Push(3);
        Assert.That(stack.Peek(), Is.EqualTo(3));
    }

    [Test]
    public void PopEmptyThrowsTest() {
        var stack = new LimitedStack<int>(2);
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
    }

    [Test]
    public void PeekEmptyThrowsTest() {
        var stack = new LimitedStack<int>(2);
        Assert.Throws<InvalidOperationException>(() => stack.Peek());
    }
}
