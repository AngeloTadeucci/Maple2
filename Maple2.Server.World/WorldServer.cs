using System;
using Maple2.Database.Storage;
using Maple2.Tools.Scheduler;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.World;

public class WorldServer {
    private readonly GameStorage gameStorage;
    private readonly EventQueue scheduler;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required WorldClient World { get; init; }
    // ReSharper restore All
    #endregion

    public WorldServer(GameStorage gameStorage) {
        this.gameStorage = gameStorage;
        scheduler = new EventQueue();

        SetDailyReset();
    }

    public void SetDailyReset() {
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
        scheduler.Schedule(DailyReset, (int) timeUntilMidnight.TotalMilliseconds);
    }

    public void DailyReset() {
        using GameStorage.Request db = gameStorage.Context();
        db.DailyReset();
        GameResetResponse? response = World.GameReset(new GameResetRequest {
            Daily = new GameResetRequest.Types.Daily(),
        });
        scheduler.ScheduleRepeated(DailyReset, (int) TimeSpan.FromDays(1).TotalMilliseconds, true);
    }
}
