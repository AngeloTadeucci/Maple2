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

        DateTime now = DateTime.Now;
        DateTime midnight = new DateTime(now.Year, now.Month, now.Day).AddDays(1);
        TimeSpan timeUntilMidnight = midnight - now;
        scheduler.Schedule(DailyReset, (int) timeUntilMidnight.TotalMilliseconds);
    }

    public void DailyReset() {

        scheduler.ScheduleRepeated(DailyReset, (int) TimeSpan.FromDays(1).TotalMilliseconds, true);
    }
}
