using System;
using System.Collections.Generic;
using System.Threading;

namespace Maple2.Tools.Scheduler;

public class EventQueue {
    public bool Running { get; private set; }
    public int Queued => nextEvents.Count;
    public int Count => nextEvents.Count + timedEvents.Count;

    private readonly List<ScheduledEvent> timedEvents;
    private Queue<ScheduledEvent> nextEvents;
    private long nextTime;

    private readonly object mutex = new object();

    public EventQueue() {
        timedEvents = new List<ScheduledEvent>();
        nextEvents = new Queue<ScheduledEvent>();
        nextTime = long.MaxValue;
    }

    public void Start() {
        Running = true;
    }

    public void Stop() {
        Running = false;
    }

    public void Clear() {
        lock (mutex) {
            timedEvents.Clear();
            nextEvents.Clear();
        }
    }

    /// <summary>
    /// Schedule a task to be run once
    /// </summary>
    /// <param name="task">task to be run</param>
    public void Schedule(Action task) {
        lock (mutex) {
            nextEvents.Enqueue(new ScheduledEvent(task));
        }

        Interlocked.Exchange(ref nextTime, 0);
    }

    /// <summary>
    /// Schedule a task to be run once after a specified delay
    /// </summary>
    /// <param name="task">task to be run</param>
    /// <param name="delay">delay after which the task will be run</param>
    public void Schedule(Action task, int delay) {
        if (delay <= 0) {
            Schedule(task);
            return;
        }

        lock (mutex) {
            long executionTime = Environment.TickCount64 + delay;
            timedEvents.Add(new ScheduledEvent(task, executionTime));

            Interlocked.Exchange(ref nextTime, Math.Min(nextTime, executionTime));
        }
    }

    /// <summary>
    /// Schedule a task to be executed repeatedly
    /// </summary>
    /// <param name="task">task to be run</param>
    /// <param name="interval">interval for this task to be run at</param>
    /// <param name="strict">if true, any delays will not affect future task scheduling</param>
    /// <param name="skipFirst">if true, the first execution will be skipped</param>
    public void ScheduleRepeated(Action task, int interval, bool strict = false, bool skipFirst = false) {
        if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be positive");
        lock (mutex) {
            long executionTime = Environment.TickCount64;
            if (skipFirst) {
                executionTime += interval;
            }
            timedEvents.Add(new ScheduledEvent(task, executionTime, interval, strict));

            Interlocked.Exchange(ref nextTime, Math.Min(nextTime, executionTime));
        }
    }

    public void InvokeAll() {
        // Avoid processing queue if we know nothing is available
        if (!Running || Environment.TickCount64 < nextTime) {
            return;
        }

        Queue<ScheduledEvent> events = nextEvents;
        lock (mutex) {
            nextEvents = new Queue<ScheduledEvent>();
            Interlocked.Exchange(ref nextTime, long.MaxValue);

            long timeNow = Environment.TickCount64;
            for (int i = timedEvents.Count - 1; i >= 0; i--) {
                if (timedEvents[i].Completed) {
                    timedEvents.RemoveAt(i);
                } else if (timedEvents[i].IsReady(timeNow)) {
                    events.Enqueue(timedEvents[i]);
                } else {
                    Interlocked.Exchange(ref nextTime, Math.Min(nextTime, timedEvents[i].ExecutionTime));
                }
            }
        }

        foreach (ScheduledEvent scheduledEvent in events) {
            long result = scheduledEvent.Invoke();
            if (result >= 0) {
                Interlocked.Exchange(ref nextTime, Math.Min(nextTime, result));
            }
        }
    }
}
