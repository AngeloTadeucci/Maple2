using System.Collections.Concurrent;
using Grpc.Core;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    private static readonly ConcurrentDictionary<long, bool> Locks = new ConcurrentDictionary<long, bool>();

    public override Task<LockResponse> AcquireLock(LockRequest request, ServerCallContext context) {
        bool acquired = Locks.TryAdd(request.AccountId, true);
        if (acquired) {
            return Task.FromResult(new LockResponse { Success = true });
        }
        return Task.FromResult(new LockResponse { Success = false, Error = "Lock already held." });
    }

    public override Task<LockResponse> ReleaseLock(LockRequest request, ServerCallContext context) {
        bool removed = Locks.TryRemove(request.AccountId, out _);
        if (removed) {
            return Task.FromResult(new LockResponse { Success = true });
        }
        return Task.FromResult(new LockResponse { Success = false, Error = "Lock not held." });
    }
}
