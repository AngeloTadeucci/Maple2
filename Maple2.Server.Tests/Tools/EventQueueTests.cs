﻿using System;
using System.Threading;
using Maple2.Tools.Scheduler;

namespace Maple2.Server.Tests.Tools;

public class EventQueueTests {
    [Test]
    public void Schedule_ImmediateTask_Executes() {
        var queue = new EventQueue();
        queue.Start();
        bool called = false;
        queue.Schedule(() => called = true);
        queue.InvokeAll();
        Assert.That(called, Is.True);
    }

    [Test]
    public void Schedule_DelayedTask_ExecutesAfterDelay() {
        var queue = new EventQueue();
        queue.Start();
        bool called = false;
        queue.Schedule(() => called = true, TimeSpan.FromMilliseconds(50));
        queue.InvokeAll();
        Assert.That(called, Is.False);
        Thread.Sleep(60);
        queue.InvokeAll();
        Assert.That(called, Is.True);
    }

    [Test]
    public void ScheduleRepeated_ExecutesMultipleTimes() {
        var queue = new EventQueue();
        queue.Start();
        int count = 0;
        queue.ScheduleRepeated(() => count++, TimeSpan.FromMilliseconds(30));
        for (int i = 0; i < 3; i++) {
            Thread.Sleep(35);
            queue.InvokeAll();
        }
        Assert.That(count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void ScheduleRepeated_StrictMode_ExecutesAtFixedIntervals() {
        var queue = new EventQueue();
        queue.Start();
        int count = 0;
        queue.ScheduleRepeated(() => count++, TimeSpan.FromMilliseconds(20), strict: true);
        Thread.Sleep(25);
        queue.InvokeAll();
        Thread.Sleep(25);
        queue.InvokeAll();
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void ScheduleRepeated_SkipFirst_SkipsInitialExecution() {
        var queue = new EventQueue();
        queue.Start();
        int count = 0;
        queue.ScheduleRepeated(() => count++, TimeSpan.FromMilliseconds(20), skipFirst: true);
        queue.InvokeAll();
        Assert.That(count, Is.EqualTo(0));
        Thread.Sleep(25);
        queue.InvokeAll();
        Assert.That(count, Is.EqualTo(1));
    }
}
