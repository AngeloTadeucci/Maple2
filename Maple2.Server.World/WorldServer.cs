using System.Collections.Concurrent;
using Grpc.Core;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.Server.Channel.Service;
using Maple2.Server.World.Containers;
using Maple2.Tools.Scheduler;
using Serilog;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;
using LoginClient = Maple2.Server.Login.Service.Login.LoginClient;


namespace Maple2.Server.World;

public class WorldServer {
    private readonly GameStorage gameStorage;
    private readonly ChannelClientLookup channelClients;
    private readonly ServerTableMetadataStorage serverTableMetadata;
    private readonly GlobalPortalLookup globalPortalLookup;
    private readonly PlayerInfoLookup playerInfoLookup;
    private readonly Thread thread;
    private readonly Thread heartbeatThread;
    private readonly EventQueue scheduler;
    private readonly CancellationTokenSource tokenSource = new();
    private readonly ConcurrentDictionary<int, string> memoryStringBoards;
    private static int _globalIdCounter;

    private readonly ILogger logger = Log.ForContext<WorldServer>();

    private readonly LoginClient login;

    public WorldServer(GameStorage gameStorage, ChannelClientLookup channelClients, ServerTableMetadataStorage serverTableMetadata, GlobalPortalLookup globalPortalLookup, PlayerInfoLookup playerInfoLookup, LoginClient login) {
        this.gameStorage = gameStorage;
        this.channelClients = channelClients;
        this.serverTableMetadata = serverTableMetadata;
        this.globalPortalLookup = globalPortalLookup;
        this.playerInfoLookup = playerInfoLookup;
        this.login = login;
        scheduler = new EventQueue();
        scheduler.Start();
        memoryStringBoards = [];

        SetAllCharacterToOffline();

        StartDailyReset();
        StartWorldEvents();
        ScheduleGameEvents();
        thread = new Thread(Loop);
        thread.Start();

        heartbeatThread = new Thread(Heartbeat);
        heartbeatThread.Start();
    }

    private void SetAllCharacterToOffline() {
        using GameStorage.Request db = gameStorage.Context();
        db.SetAllCharacterToOffline();
    }

    private void Heartbeat() {
        while (!tokenSource.Token.IsCancellationRequested) {
            try {
                Task.Delay(TimeSpan.FromSeconds(30), tokenSource.Token).Wait(tokenSource.Token);

                login.Heartbeat(new HeartbeatRequest(), cancellationToken: tokenSource.Token);

                foreach (PlayerInfo playerInfo in playerInfoLookup.GetOnlinePlayerInfos()) {
                    if (playerInfo.CharacterId == 0) continue;
                    if (!channelClients.TryGetClient(playerInfo.Channel, out ChannelClient? channel)) continue;

                    try {
                        HeartbeatResponse? response = channel.Heartbeat(new HeartbeatRequest {
                            CharacterId = playerInfo.CharacterId,
                        }, cancellationToken: tokenSource.Token);
                        if (response is { Success: false }) {
                            playerInfoLookup.Update(new PlayerUpdateRequest {
                                AccountId = playerInfo.AccountId,
                                CharacterId = playerInfo.CharacterId,
                                LastOnlineTime = DateTime.UtcNow.ToEpochSeconds(),
                                Channel = -1,
                                Async = true,
                            });
                            continue;
                        }
                        playerInfo.RetryHeartbeat = 3;
                    } catch (RpcException) {
                        if (playerInfo.RetryHeartbeat >= 0) {
                            playerInfo.RetryHeartbeat--;
                            continue;
                        }
                        playerInfoLookup.Update(new PlayerUpdateRequest {
                            AccountId = playerInfo.AccountId,
                            CharacterId = playerInfo.CharacterId,
                            LastOnlineTime = DateTime.UtcNow.ToEpochSeconds(),
                            Channel = -1,
                            Async = true,
                        });
                    }

                }
            } catch (OperationCanceledException) {
                break;
            } catch (Exception ex) {
                logger.Warning(ex, "Heartbeat loop error");
            }
        }
    }

    private void Loop() {
        while (!tokenSource.Token.IsCancellationRequested) {
            try {
                scheduler.InvokeAll();
            } catch (Exception e) {
                logger.Error(e, "Error in world server loop");
            }
            try {
                Task.Delay(TimeSpan.FromMinutes(1), tokenSource.Token).Wait();
            } catch { /* do nothing */
            }
        }
    }

    public void Stop() {
        tokenSource.Cancel();
        thread.Join();
        scheduler.Stop();
    }

    #region Daily Reset
    private void StartDailyReset() {
        // Daily reset
        using GameStorage.Request db = gameStorage.Context();
        DateTime lastReset = db.GetLastDailyReset();

        // Get last midnight.
        DateTime now = DateTime.Now;
        var lastMidnight = new DateTime(now.Year, now.Month, now.Day);
        if (lastReset < lastMidnight) {
            db.DailyReset();
        }

        DateTime nextMidnight = lastMidnight.AddDays(1);
        TimeSpan timeUntilMidnight = nextMidnight - now;
        scheduler.Schedule(ScheduleDailyReset, timeUntilMidnight);
    }

    private void ScheduleDailyReset() {
        DailyReset();
        // Schedule it to repeat every once a day.
        scheduler.ScheduleRepeated(DailyReset, TimeSpan.FromDays(1), strict: true);
    }

    private void DailyReset() {
        using GameStorage.Request db = gameStorage.Context();
        db.DailyReset();
        foreach ((int channelId, ChannelClient channelClient) in channelClients) {
            channelClient.GameReset(new GameResetRequest {
                Daily = new GameResetRequest.Types.Daily(),
            });
        }
    }
    #endregion

    private void StartWorldEvents() {
        // Global Portal
        IReadOnlyDictionary<int, GlobalPortalMetadata> globalEvents = serverTableMetadata.TimeEventTable.GlobalPortal;
        foreach ((int eventId, GlobalPortalMetadata eventData) in globalEvents) {
            if (eventData.EndTime < DateTime.Now) {
                continue;
            }

            // There is no cycle time, so we skip it.
            if (eventData.CycleTime == TimeSpan.Zero) {
                continue;
            }
            DateTime startTime = eventData.StartTime;
            if (DateTime.Now > startTime) {
                // catch up to a time after the start time
                while (startTime < DateTime.Now) {
                    startTime += eventData.CycleTime;
                }
                if (startTime > eventData.EndTime) {
                    continue;
                }
                scheduler.Schedule(() => GlobalPortal(eventData, startTime), startTime - DateTime.Now);
            }
        }
    }

    private void GlobalPortal(GlobalPortalMetadata data, DateTime startTime) {
        // check probability
        bool run = !(data.Probability < 100 && Random.Shared.Next(100) > data.Probability);

        if (run) {
            DateTime now = DateTime.Now;
            globalPortalLookup.Create(data, (long) (now.ToEpochSeconds() + data.LifeTime.TotalMilliseconds), out int eventId);
            if (!globalPortalLookup.TryGet(out GlobalPortalManager? manager)) {
                logger.Error("Failed to create global portal");
                return;
            }

            manager.CreateFields();

            Task.Factory.StartNew(() => {
                Thread.Sleep(data.LifeTime);
                if (globalPortalLookup.TryGet(out GlobalPortalManager? globalPortalManager) && globalPortalManager.Portal.Id == eventId) {
                    globalPortalLookup.Dispose();
                }
            });
        }

        DateTime nextRunTime = startTime + data.CycleTime;
        if (data.RandomTime > TimeSpan.Zero) {
            nextRunTime += TimeSpan.FromMilliseconds(Random.Shared.Next((int) data.RandomTime.TotalMilliseconds));
        }

        if (data.EndTime < nextRunTime) {
            return;
        }

        scheduler.Schedule(() => GlobalPortal(data, nextRunTime), nextRunTime - DateTime.Now);
    }

    private void ScheduleGameEvents() {
        IEnumerable<GameEvent> events = serverTableMetadata.GetGameEvents().ToList();
        // Add Events
        // Get only events that havent been started. Started events already get loaded on game/login servers on start up
        foreach (GameEvent data in events.Where(gameEvent => gameEvent.StartTime > DateTimeOffset.Now.ToUnixTimeSeconds())) {
            scheduler.Schedule(() => AddGameEvent(data.Id), TimeSpan.FromSeconds(data.StartTime - DateTimeOffset.Now.ToUnixTimeSeconds()));
        }

        // Remove Events
        foreach (GameEvent data in events.Where(gameEvent => gameEvent.EndTime > DateTimeOffset.Now.ToUnixTimeSeconds())) {
            scheduler.Schedule(() => RemoveGameEvent(data.Id), TimeSpan.FromSeconds(data.EndTime - DateTimeOffset.Now.ToUnixTimeSeconds()));
        }
    }

    private void AddGameEvent(int eventId) {
        foreach ((int channelId, ChannelClient channelClient) in channelClients) {
            channelClient.GameEvent(new GameEventRequest {
                Add = new GameEventRequest.Types.Add {
                    EventId = eventId,
                },
            });
        }
    }

    private void RemoveGameEvent(int eventId) {
        foreach ((int channelId, ChannelClient channelClient) in channelClients) {
            channelClient.GameEvent(new GameEventRequest {
                Remove = new GameEventRequest.Types.Remove {
                    EventId = eventId,
                },
            });
        }
    }

    private static int NextGlobalId() => Interlocked.Increment(ref _globalIdCounter);

    public int AddCustomStringBoard(string message) {
        if (string.IsNullOrEmpty(message)) {
            return -1;
        }

        int id = NextGlobalId();
        memoryStringBoards.TryAdd(id, message);
        return id;
    }

    public bool RemoveCustomStringBoard(int id) {
        return memoryStringBoards.TryRemove(id, out _);
    }

    public IReadOnlyDictionary<int, string> GetCustomStringBoards() {
        return memoryStringBoards;
    }
}
