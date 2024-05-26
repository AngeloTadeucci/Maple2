using Maple2.Server.Game.Model.Enum;

namespace Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;

public class TaskState {
    public FieldNpc Actor { get; init; }

    private PriorityQueue<NpcTask, NpcTaskPriority> taskQueue;
    private NpcTask?[] runningTasks;
    private bool isPendingStart = false;
    private NpcTask? pendingTask = null;

    public TaskState(FieldNpc actor) {
        Actor = actor;

        var comparer = Comparer<NpcTaskPriority>.Create((NpcTaskPriority item1, NpcTaskPriority item2) => 1 - item1.CompareTo(item2));

        taskQueue = new PriorityQueue<NpcTask, NpcTaskPriority>(comparer);
        runningTasks = new NpcTask?[(int) NpcTaskPriority.Count];
    }

    private NpcTaskStatus QueueTask(NpcTask task) {
        NpcTask? queued = runningTasks[task.PriorityValue];

        if (queued != null && !task.ShouldOverride(task)) {
            return NpcTaskStatus.Cancelled;
        }

        if (taskQueue.TryPeek(out NpcTask? currentTask, out NpcTaskPriority priority)) {
            bool cancelLowerPriority = currentTask.PriorityValue < task.PriorityValue && currentTask.CancelOnInterrupt;
            bool cancelEqualPriority = currentTask.PriorityValue == task.PriorityValue;

            if (cancelLowerPriority || cancelEqualPriority) {
                currentTask.Cancel();
            } else if (currentTask.PriorityValue < task.PriorityValue) {
                currentTask.Pause();
            }
        }

        runningTasks[task.PriorityValue] = task;
        taskQueue.Enqueue(task, task.Priority);

        if (taskQueue.Peek() == task) {
            isPendingStart = true;
            pendingTask = task;

            return NpcTaskStatus.Running;
        }

        return NpcTaskStatus.Pending;
    }

    private void FinishTask(NpcTask task) {
        if (runningTasks[task.PriorityValue] != task) {
            return;
        }

        runningTasks[task.PriorityValue] = null;

        if (taskQueue.TryPeek(out NpcTask? currentTask, out NpcTaskPriority priority) && currentTask == task) {
            taskQueue.Dequeue();

            if (taskQueue.TryPeek(out currentTask, out priority)) {
                isPendingStart = true;
                pendingTask = currentTask;
            }
        }
    }

    public void Update(long tickCount) {
        if (isPendingStart) {
            if (taskQueue.TryPeek(out NpcTask? task, out NpcTaskPriority priority) && task == pendingTask) {
                task.Resume();
            }
        }

        isPendingStart = false;
        pendingTask = null;
    }

    public abstract class NpcTask {
        protected TaskState queue { get; private set; }

        public NpcTaskPriority Priority { get; private init; }
        public int PriorityValue { get => (int) Priority; }
        public NpcTaskStatus Status {
            get => status;
            private set {
                status = value;
            }
        }
        public bool IsDone { get => Status == NpcTaskStatus.Cancelled || Status == NpcTaskStatus.Complete; }
        public virtual bool CancelOnInterrupt { get; }
        private NpcTaskStatus status;

        public NpcTask(TaskState queue, NpcTaskPriority priority) {
            this.queue = queue;
            Priority = priority;

            Status = queue.QueueTask(this);
        }

        internal void Resume() {
            Status = NpcTaskStatus.Running;

            TaskResumed();
        }

        protected virtual void TaskResumed() { }

        public virtual bool ShouldOverride(NpcTask task) {
            return (int) Priority >= (int) task.Priority;
        }

        public void Pause() {
            if (Status != NpcTaskStatus.Running) {
                return;
            }

            Status = NpcTaskStatus.Pending;

            TaskPaused();
        }

        protected virtual void TaskPaused() { }

        public void Finish(bool isCompleted) {
            if (IsDone) {
                return;
            }

            Status = isCompleted ? NpcTaskStatus.Complete : NpcTaskStatus.Cancelled;

            queue.FinishTask(this);
            TaskFinished(isCompleted);
        }

        protected virtual void TaskFinished(bool isCompleted) { }

        public void Cancel() {
            Finish(false);
        }

        public void Completed() {
            Finish(true);
        }
    }
}
