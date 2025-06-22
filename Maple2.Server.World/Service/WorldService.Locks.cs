using System.Collections.Concurrent;
using Grpc.Core;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);
    private static readonly ConcurrentDictionary<long, DateTime> Locks = new ConcurrentDictionary<long, DateTime>();

    public override Task<LockResponse> AcquireLock(LockRequest request, ServerCallContext context) {
        DateTime now = DateTime.UtcNow;

        // Remove expired lock if present
        if (Locks.TryGetValue(request.AccountId, out DateTime timestamp)) {
            if (now - timestamp > LockTimeout) {
                Locks.TryRemove(request.AccountId, out _);
            }
        }

        // Try to acquire lock
        bool acquired = Locks.TryAdd(request.AccountId, now);
        if (acquired) {
            return Task.FromResult(new LockResponse {
                Success = true,
            });
        }
        return Task.FromResult(new LockResponse {
            Success = false,
            Error = "Lock already held.",
        });
    }

    public override Task<LockResponse> ReleaseLock(LockRequest request, ServerCallContext context) {
        bool removed = Locks.TryRemove(request.AccountId, out _);
        if (removed) {
            return Task.FromResult(new LockResponse {
                Success = true,
            });
        }
        return Task.FromResult(new LockResponse {
            Success = false,
            Error = "Lock not held.",
        });
    }
}
