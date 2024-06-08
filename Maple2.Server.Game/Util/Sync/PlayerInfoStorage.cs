using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Core.Sync;
using Serilog;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Game.Util.Sync;

public class PlayerInfoStorage {
    private readonly WorldClient world;
    // TODO: Just using dictionary for now, might need eviction at some point (LRUCache)
    private readonly ConcurrentDictionary<long, PlayerInfo> cache;
    private readonly ConcurrentDictionary<long, IDictionary<int, PlayerInfoListener>> listeners;

    private readonly ILogger logger = Log.ForContext<PlayerInfoStorage>();

    // private readonly ConcurrentDictionary<Key, IDictionary<PlayerInfoUpdateEvent.Type, PlayerSubscriber>> subscribers;

    public PlayerInfoStorage(WorldClient world) {
        this.world = world;

        cache = new ConcurrentDictionary<long, PlayerInfo>();
        listeners = new ConcurrentDictionary<long, IDictionary<int, PlayerInfoListener>>();
    }

    public bool GetOrFetch(long characterId, [NotNullWhen(true)] out PlayerInfo? info) {
        if (cache.TryGetValue(characterId, out info)) {
            return true;
        }

        // Fetch PlayerInfo from World server.
        try {
            PlayerInfoResponse response = world.PlayerInfo(new PlayerInfoRequest {
                CharacterId = characterId,
            });
            var characterInfo = new CharacterInfo(response.AccountId, response.CharacterId, response.Name, response.Motto, response.Picture, (Gender) response.Gender, (Job) response.Job, (short) response.Level) {
                GearScore = response.GearScore,
                CurrentHp = response.Health.CurrentHp,
                TotalHp = response.Health.TotalHp,
                MapId = response.MapId,
                Channel = (short) response.Channel,
                UpdateTime = response.UpdateTime,
            };
            var trophy = new AchievementInfo {
                Adventure = response.Trophy.Adventure,
                Combat = response.Trophy.Combat,
                Lifestyle = response.Trophy.Lifestyle,
            };

            info = new PlayerInfo(characterInfo, response.Home.Name, trophy, response.Clubs.Id.ToList()) {
                PlotMapId = response.Home.MapId,
                PlotNumber = response.Home.PlotNumber,
                ApartmentNumber = response.Home.ApartmentNumber,
                PlotExpiryTime = response.Home.ExpiryTime.Seconds,
                PremiumTime = response.PremiumTime,
            };

            cache[characterId] = info;
            return true;
        } catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound) {
            return false;
        } catch (RpcException ex) {
            logger.Error("Failed to fetch PlayerInfo: {Status}", ex.Status);
            return false;
        }
    }

    public void Listen(long characterId, PlayerInfoListener listener) {
        if (!listeners.ContainsKey(characterId)) {
            listeners[characterId] = new ConcurrentDictionary<int, PlayerInfoListener>();
        }

        listeners[characterId][listener.GetHashCode()] = listener;
    }

    public void SendUpdate(PlayerUpdateRequest request) {
        try {
            //PlayerInfoCache
            world.UpdatePlayer(request);
        } catch (RpcException) { /* ignored */ }
    }

    public bool ReceiveUpdate(PlayerUpdateRequest request) {
        PlayerInfoUpdateEvent @event = cache.TryGetValue(request.CharacterId, out PlayerInfo? info)
            ? new PlayerInfoUpdateEvent(info, request)
            : new PlayerInfoUpdateEvent(request);

        if (@event.Type == UpdateField.None) {
            return false; // No fields changed
        }

        if (!GetOrFetch(@event.Request.CharacterId, out info)) {
            return false; // Character does not exist
        }

        info.Update(@event);
        return Notify(info, @event.Type);
    }

    private bool Notify(PlayerInfo info, UpdateField type) {
        if (!listeners.TryGetValue(info.CharacterId, out IDictionary<int, PlayerInfoListener>? dict)) {
            return false;
        }

        foreach ((int id, PlayerInfoListener listener) in dict) {
            if ((listener.Type & type) == default) {
                continue;
            }

            bool completed = listener.Callback(type, info);
            if (completed) {
                dict.Remove(id, out _);
            }
        }
        return true;
    }
}
