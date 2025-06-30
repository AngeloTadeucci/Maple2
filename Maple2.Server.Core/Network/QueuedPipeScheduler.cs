using System.IO.Pipelines;
using System.Threading.Tasks.Dataflow;

namespace Maple2.Server.Core.Network;

public class QueuedPipeScheduler : PipeScheduler {
    private readonly BufferBlock<(Action<object?> Action, object? State)> queue;

    public QueuedPipeScheduler() {
        queue = new BufferBlock<(Action<object?> Action, object? State)>();
    }

    public Task<bool> OutputAvailableAsync() => queue.OutputAvailableAsync();
    public void Complete() => queue.Complete();
    public Task Completion => queue.Completion;

    public override void Schedule(Action<object?> action, object? state) {
        queue.Post((action, state));
    }

    public void ProcessQueue() {
        while (queue.TryReceive(out (Action<object?> Action, object? State) item)) {
            item.Action(item.State);
        }
    }
}
