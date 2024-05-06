using System;

namespace Maple2.Tools.Scheduler;

internal class ScheduledEvent(Action task, long executionTime = 0, int interval = -1, bool strict = false) {
    public bool Completed { get; private set; }
    public long ExecutionTime { get; private set; } = executionTime;

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
