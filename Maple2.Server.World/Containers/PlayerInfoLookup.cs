using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Server.Channel.Service;
using Maple2.Server.Core.Sync;
using Serilog;

namespace Maple2.Server.World.Containers;

public class PlayerInfoLookup : IPlayerInfoProvider, IDisposable {
    private readonly TimeSpan syncInterval = TimeSpan.FromSeconds(2);

    private readonly GameStorage gameStorage;
    private readonly ChannelClientLookup channels;
    private readonly CancellationTokenSource tokenSource;

    // TODO: Just using dictionary for now, might need eviction at some point (LRUCache)
    private readonly ConcurrentDictionary<long, PlayerInfo> cache;
    private readonly ConcurrentQueue<PlayerInfoUpdateEvent> events;

    private readonly ILogger logger = Log.ForContext<PlayerInfoLookup>();

    public PlayerInfoLookup(GameStorage gameStorage, ChannelClientLookup channels) {
        this.gameStorage = gameStorage;
        this.channels = channels;
        tokenSource = new CancellationTokenSource();

        cache = new ConcurrentDictionary<long, PlayerInfo>();
        events = new ConcurrentQueue<PlayerInfoUpdateEvent>();

        Task.Factory.StartNew(() => {
            while (!tokenSource.Token.IsCancellationRequested) {
                Thread.Sleep(syncInterval);
                Sync();
            }
        }, tokenSource.Token);
    }

    public void Dispose() {
        tokenSource.Cancel();
        tokenSource.Dispose();
    }

    public bool TryGet(long characterId, [NotNullWhen(true)] out PlayerInfo? info) {
        if (cache.TryGetValue(characterId, out info)) {
            return true;
        }

        // If data is not cached, fetch from database.
        using GameStorage.Request db = gameStorage.Context();
        info = db.GetPlayerInfo(characterId);
        return info != null && cache.TryAdd(characterId, info);
    }

    // Try to get a cached player by account id and that's online, since we need a ChannelClient to notify.
    // If the player is not cached just return false since it was never online.
    // This is a very specific use case, and should be used with caution, mainly used for account wide mail notifications.
    public bool TryGetByAccountId(long accountId, [NotNullWhen(true)] out PlayerInfo? info) {
        info = cache.Values.FirstOrDefault(player => player.AccountId == accountId && player.Online);
        return info is not null;
    }

    public bool Update(PlayerUpdateRequest request) {
        PlayerInfoUpdateEvent @event = cache.TryGetValue(request.CharacterId, out PlayerInfo? info)
            ? new PlayerInfoUpdateEvent(info, request)
            : new PlayerInfoUpdateEvent(request);

        // TODO: This support for both sync and async can cause ordering issues where older updates overwrite newer ones.
        if (request.Async) {
            events.Enqueue(@event);
            return true;
        }

        info = Update(@event);
        return info != null && Notify(info, @event.Type);
    }

    private void Sync() {
        if (events.IsEmpty) {
            return;
        }

        var updated = new Dictionary<long, Notification>();
        while (events.TryDequeue(out PlayerInfoUpdateEvent? @event)) {
            PlayerInfo? info = Update(@event);
            if (info == null) {
                continue;
            }

            if (!updated.ContainsKey(@event.Request.CharacterId)) {
                updated[@event.Request.CharacterId] = new Notification(info);
            }
            updated[@event.Request.CharacterId].Type |= @event.Type;
        }

        foreach ((long _, Notification notification) in updated) {
            Notify(notification.Info, notification.Type);
        }
    }

    private PlayerInfo? Update(PlayerInfoUpdateEvent @event) {
        if (@event.Type == UpdateField.None) {
            return null; // No fields changed
        }

        if (!TryGet(@event.Request.CharacterId, out PlayerInfo? info)) {
            return null; // No entry to update
        }

        info.Update(@event);
        return info;
    }

    private bool Notify(PlayerInfo info, UpdateField type) {
        var request = new PlayerUpdateRequest {
            AccountId = info.AccountId,
            CharacterId = info.CharacterId,
        };
        request.SetFields(type, info);

        // Forward all updates to channels for caching.
        Parallel.ForEach(channels, entry => {
            try {
                entry.Item2.UpdatePlayer(request);
            } catch (RpcException ex) {
                logger.Warning("[{Error}] Failed to notify channel {Channel} with events: {CharacterId}|{Type}", ex.StatusCode, entry.Item1, info.CharacterId, type);
            }
        });

        return true;
    }

    private record Notification(PlayerInfo Info) {
        public UpdateField Type;
    }

    public PlayerInfo? GetPlayerInfo(long id) {
        TryGet(id, out PlayerInfo? info);
        return info;
    }

    public List<PlayerInfo> GetOnlinePlayerInfos() {
        return cache.Values
            .Where(player => player.Online)
            .ToList();
    }
}
