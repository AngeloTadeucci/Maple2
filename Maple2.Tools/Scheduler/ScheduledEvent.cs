using System;

namespace Maple2.Tools.Scheduler;

internal class ScheduledEvent {
    public bool Completed { get; private set; }
    public long ExecutionTime { get; private set; }

    private readonly Action task;
    private readonly int interval;
    private readonly bool strict;

    public ScheduledEvent(Action task, long executionTime = 0, int interval = -1, bool strict = false) {
        this.task = task;
        ExecutionTime = executionTime;
        this.interval = interval;
        this.strict = strict;
    }

    public bool IsReady(long time) => ExecutionTime <= time;

    // Invokes the task and returns the next execution time
    public long Invoke() {
        if (Completed) return -1;

        task.Invoke();

        if (interval < 0) {
            Completed = true;
            return -1;
        }

        if (strict) {
            ExecutionTime += interval;
        } else {
            ExecutionTime = Environment.TickCount64 + interval;
        }

        return ExecutionTime;
    }
}
