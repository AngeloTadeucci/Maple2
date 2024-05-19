using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Core.Packets;
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

    // Key: Attribute, Value: RegenAttribute, RegenInterval
    private readonly Dictionary<BasicAttribute, Tuple<BasicAttribute, BasicAttribute>> regenStats;
    private readonly Dictionary<BasicAttribute, long> lastRegenTime;

    #region DebugFlags
    private bool debugAi = false;
    public bool DebugSkills = false;
    #endregion

    public int TagId = 1;

    private readonly EventQueue scheduler;

    public FieldPlayer(GameSession session, Player player, NpcMetadataStorage npcMetadata) : base(session.Field!, player.ObjectId, player, GetPlayerModel(player.Character.Gender), npcMetadata) {
        Session = session;

        regenStats = new Dictionary<BasicAttribute, Tuple<BasicAttribute, BasicAttribute>>();
        lastRegenTime = new Dictionary<BasicAttribute, long>();

        scheduler = new EventQueue();
        scheduler.ScheduleRepeated(() => Field.Broadcast(ProxyObjectPacket.UpdatePlayer(this, PlayerObjectFlag.Position | PlayerObjectFlag.State)), 2000);
        scheduler.Start();
    }

    private static string GetPlayerModel(Gender gender) {
        return gender switch {
            Gender.Male => "male",
            Gender.Female => "female",
            _ => "male"
        };
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

    public bool DebugAi {
        get => debugAi;
        set {
            if (value) {
                Field.BroadcastAiType(Session);
            }

            debugAi = value;
        }
    }

    public override void Update(long tickCount) {
        if (InBattle && tickCount - battleTick > 2000) {
            InBattle = false;
        }

        // Loops through each registered regen stat and applies regen
        foreach (var attribute in regenStats.Keys) {
            var stat = Stats[attribute];
            var regen = Stats[regenStats[attribute].Item1];
            var interval = Stats[regenStats[attribute].Item2];

            if (stat.Current >= stat.Total) {
                // Removes stat from regen stats so it won't be listened for
                regenStats.Remove(attribute);
                continue;
            }

            lastRegenTime.TryGetValue(attribute, out long regenTime);

            if (tickCount - regenTime > interval.Total) {
                lastRegenTime[attribute] = tickCount;
                Stats[attribute].Add(regen.Total);
                Session.Send(StatsPacket.Update(this, attribute));
            }
        }

        base.Update(tickCount);
    }

    protected override void OnDeath() {
        throw new NotImplementedException();
    }

    public override void KeyframeEvent(long tickCount, long keyTick, string keyName) {

    }

    /// <summary>
    /// Adds health to player, and sends update packet.
    /// </summary>
    /// <param name="amount"></param>
    public void RecoverHp(int amount) {
        if (amount <= 0) {
            return;
        }

        Stat stat = Stats[BasicAttribute.Health];
        if (stat.Current < stat.Total) {
            stat.Add(amount);
            Session.Send(StatsPacket.Update(this, BasicAttribute.Health));
        }
    }

    /// <summary>
    /// Consumes health and starts regen if not already started.
    /// </summary>
    /// <param name="amount"></param>
    public void ConsumeHp(int amount) {
        if (amount <= 0) {
            return;
        }

        Stat stat = Stats[BasicAttribute.Health];
        stat.Add(-amount);

        if (!regenStats.ContainsKey(BasicAttribute.Health)) {
            regenStats.Add(BasicAttribute.Health, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.HpRegen, BasicAttribute.HpRegenInterval));
        }
    }

    /// <summary>
    /// Adds spirit to player, and sends update packet.
    /// </summary>
    /// <param name="amount"></param>
    public void RecoverSp(int amount) {
        if (amount <= 0) {
            return;
        }

        Stat stat = Stats[BasicAttribute.Spirit];
        if (stat.Current < stat.Total) {
            stat.Add(amount);
            Session.Send(StatsPacket.Update(this, BasicAttribute.Spirit));
        }
    }

    /// <summary>
    /// Consumes spirit and starts regen if not already started.
    /// </summary>
    /// <param name="amount"></param>
    public void ConsumeSp(int amount) {
        if (amount <= 0) {
            return;
        }

        Stats[BasicAttribute.Spirit].Add(-amount);

        if (!regenStats.ContainsKey(BasicAttribute.Spirit)) {
            regenStats.Add(BasicAttribute.Spirit, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.SpRegen, BasicAttribute.SpRegenInterval));
        }
    }

    /// <summary>
    /// Adds stamina to player, and sends update packet.
    /// </summary>
    /// <param name="amount"></param>
    public void RecoverStamina(int amount) {
        if (amount <= 0) {
            return;
        }

        Stat stat = Stats[BasicAttribute.Stamina];
        if (stat.Total < stat.Base) {
            Stats[BasicAttribute.Stamina].Add(amount);
            Session.Send(StatsPacket.Update(this, BasicAttribute.Stamina));
        }
    }

    /// <summary>
    /// Consumes stamina.
    /// </summary>
    /// <param name="amount">The amount</param>
    /// <param name="noRegen">If regen shouldn't be started</param>
    public void ConsumeStamina(int amount, bool noRegen = false) {
        if (amount <= 0) {
            return;
        }

        Stats[BasicAttribute.Stamina].Add(-amount);

        if (!regenStats.ContainsKey(BasicAttribute.Stamina) && !noRegen) {
            regenStats.Add(BasicAttribute.Stamina, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.StaminaRegen, BasicAttribute.StaminaRegenInterval));
        }
    }

    public void CheckRegen() {
        // Health
        var health = Stats[BasicAttribute.Health];
        if (health.Current < health.Total && !regenStats.ContainsKey(BasicAttribute.Health)) {
            regenStats.Add(BasicAttribute.Health, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.HpRegen, BasicAttribute.HpRegenInterval));
        }

        // Spirit
        var spirit = Stats[BasicAttribute.Spirit];
        if (spirit.Current < spirit.Total && !regenStats.ContainsKey(BasicAttribute.Spirit)) {
            regenStats.Add(BasicAttribute.Spirit, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.SpRegen, BasicAttribute.SpRegenInterval));
        }

        // Stamina
        var stamina = Stats[BasicAttribute.Stamina];
        if (stamina.Current < stamina.Total && !regenStats.ContainsKey(BasicAttribute.Stamina)) {
            regenStats.Add(BasicAttribute.Stamina, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.StaminaRegen, BasicAttribute.StaminaRegenInterval));
        }
    }
}
