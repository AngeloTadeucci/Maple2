using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Collision;
using Maple2.Tools.Scheduler;

namespace Maple2.Server.Game.Model;

public class FieldPlayer : Actor<Player> {
    public readonly GameSession Session;
    public Vector3 LastGroundPosition;

    public override Stats Stats => Session.Stats.Values;
    public override IPrism Shape => new Prism(new Circle(new Vector2(Position.X, Position.Y), 10), Position.Z, 100);
    public ActorState State { get; set; }
    public ActorSubState SubState { get; set; }

    private long battleTick;
    private bool inBattle;

    public int TagId = 1;

    private readonly EventQueue scheduler;

    public FieldPlayer(GameSession session, Player player) : base(session.Field!, player.ObjectId, player) {
        Session = session;

        scheduler = new EventQueue();
        scheduler.ScheduleRepeated(() => Field.Broadcast(ProxyObjectPacket.UpdatePlayer(this, 66)), 2000);
        scheduler.Start();
    }

    protected override void Dispose(bool disposing) {
        scheduler.Stop();
    }

    public static implicit operator Player(FieldPlayer fieldPlayer) => fieldPlayer.Value;

    public bool InBattle {
        get => inBattle;
        set {
            if (value != inBattle) {
                inBattle = value;
                Session.Field?.Broadcast(SkillPacket.InBattle(this));
            }

            if (inBattle) {
                battleTick = Environment.TickCount64;
            }
        }
    }

    public override void Update(long tickCount) {
        if (InBattle && tickCount - battleTick > 2000) {
            InBattle = false;
        }

        Buffs.Update(tickCount);
    }

    protected override void OnDeath() {
        throw new NotImplementedException();
    }
}
