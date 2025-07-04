using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Serilog;

namespace Maple2.Server.Game.Model;

public class RoomTimer : IUpdatable {
    private readonly FieldManager field;
    public int StartTick { get; private set; }
    public readonly RoomTimerType Type;
    public int Duration;
    private bool started;

    private readonly ILogger logger = Log.Logger.ForContext<RoomTimer>();

    public RoomTimer(FieldManager field, RoomTimerType type, int duration) {
        this.field = field;
        Type = type;
        Duration = duration;
    }

    public void Modify(int tick) {
        int originalDuration = Duration;
        Duration = Math.Max(0, Duration + tick);
        int delta = Duration - originalDuration;
        field.Broadcast(RoomTimerPacket.Modify(this, delta));
    }

    public void Update(long tickCount) {
        if (!started) {
            StartTick = (int) tickCount;
            field.Broadcast(RoomTimerPacket.Start(this));
            started = true;
        }

        if (tickCount > StartTick + Duration) {
            foreach ((int objectId, FieldPlayer player) in field.Players) {
                int returnMapId = player.Value.Character.ReturnMaps.Peek();
                player.Session.Send(player.Session.PrepareField(returnMapId, returnMapId)
                    ? FieldEnterPacket.Request(player)
                    : FieldEnterPacket.Error(MigrationError.s_move_err_default));
            }

            logger.Debug("Room timer expired, disposing field {FieldId}", field.MapId);
            field.Dispose();
        }
    }

    public bool Expired(long tickCount) => tickCount > StartTick + Duration;
}
