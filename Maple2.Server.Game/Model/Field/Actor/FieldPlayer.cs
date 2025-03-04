using System.Numerics;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Collision;
using Maple2.Tools.Scheduler;

namespace Maple2.Server.Game.Model;

public class FieldPlayer : Actor<Player> {
    public readonly GameSession Session;
    public Vector3 LastGroundPosition;

    public override StatsManager Stats => Session.Stats;
    public override BuffManager Buffs => Session.Buffs;
    public override IPrism Shape => new Prism(new Circle(new Vector2(Position.X, Position.Y), 10), Position.Z, 100);
    public ActorState State { get; set; }
    public ActorSubState SubState { get; set; }
    public PlayerObjectFlag Flag { get; set; }
    private long flagTick;

    private long battleTick;
    private bool inBattle;

    // Key: Attribute, Value: RegenAttribute, RegenInterval
    private readonly Dictionary<BasicAttribute, Tuple<BasicAttribute, BasicAttribute>> regenStats;
    private readonly Dictionary<BasicAttribute, long> lastRegenTime;

    private readonly Dictionary<ActorState, float> stateSyncDistanceTracking;
    private long stateSyncTimeTracking { get; set; }
    private long stateSyncTrackingTick { get; set; }

    #region DebugFlags
    private bool debugAi = false;
    public bool DebugSkills = false;
    #endregion

    public int TagId = 1;

    private readonly EventQueue scheduler;

    public FieldPlayer(GameSession session, Player player) : base(session.Field!, player.ObjectId, player, GetPlayerModel(player.Character.Gender)) {
        Session = session;


        regenStats = new Dictionary<BasicAttribute, Tuple<BasicAttribute, BasicAttribute>>();
        lastRegenTime = new Dictionary<BasicAttribute, long>();

        stateSyncDistanceTracking = new Dictionary<ActorState, float>();
        stateSyncTimeTracking = 0;
        stateSyncTrackingTick = Environment.TickCount64;

        scheduler = new EventQueue();
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

        if (Flag != PlayerObjectFlag.None && tickCount > flagTick) {
            Field.Broadcast(ProxyObjectPacket.UpdatePlayer(this, Flag));
            Flag = PlayerObjectFlag.None;
            flagTick = (long) (tickCount + TimeSpan.FromSeconds(2).TotalMilliseconds);
        }

        // Loops through each registered regen stat and applies regen
        var statsToRemove = new List<BasicAttribute>();
        foreach (BasicAttribute attribute in regenStats.Keys) {
            Stat stat = Stats.Values[attribute];
            Stat regen = Stats.Values[regenStats[attribute].Item1];
            Stat interval = Stats.Values[regenStats[attribute].Item2];

            if (stat.Current >= stat.Total) {
                // Removes stat from regen stats so it won't be listened for
                statsToRemove.Add(attribute);
                continue;
            }

            lastRegenTime.TryGetValue(attribute, out long regenTime);

            if (tickCount - regenTime > interval.Total) {
                lastRegenTime[attribute] = tickCount;
                Stats.Values[attribute].Add(regen.Total);
                Session.Send(StatsPacket.Update(this, attribute));
            }
        }
        foreach (BasicAttribute attribute in statsToRemove) {
            regenStats.Remove(attribute);
        }

        Session.GameEvent.Update(tickCount);

        base.Update(tickCount);
    }

    public void OnStateSync(StateSync stateSync) {
        if (Position != stateSync.Position) {
            Flag |= PlayerObjectFlag.Position;
        }

        float syncDistance = Vector3.Distance(Position, stateSync.Position); // distance between old player position and new state sync position
        long syncTick = Field.FieldTick - stateSyncTrackingTick; // time elapsed since last state sync
        stateSyncTrackingTick = Field.FieldTick;

        Position = stateSync.Position;
        Rotation = new Vector3(0, 0, stateSync.Rotation / 10f);

        if (State != stateSync.State) {
            Flag |= PlayerObjectFlag.State;
            stateSyncTimeTracking = 0; // Reset time tracking on state change
        }
        State = stateSync.State;
        SubState = stateSync.SubState;

        if (stateSync.SyncNumber != int.MaxValue) {
            LastGroundPosition = stateSync.Position;
        }

        bool UpdateStateSyncTracking(ActorState state) {
            if (stateSyncDistanceTracking.TryGetValue(state, out float totalDistance)) {
                totalDistance += syncDistance;
                // 150f = BLOCK_SIZE = 1 meter
                if (totalDistance >= 150F) {
                    stateSyncDistanceTracking[state] = 0f;
                    return true;
                }
                stateSyncDistanceTracking[state] = totalDistance;
                return false;
            }
            stateSyncDistanceTracking[state] = syncDistance;
            return false;
        }

        bool UpdateStateSyncTimeTracking() {
            stateSyncTimeTracking += syncTick;
            if (stateSyncTimeTracking >= 1000) {
                stateSyncTimeTracking = 0;
                return true;
            }
            return false;
        }

        // Condition updates
        // Distance conditions are in increments of 1 meter, while time conditions are 1 second.
        switch (stateSync.State) {
            case ActorState.Fall:
                if (UpdateStateSyncTracking(ActorState.Fall)) {
                    Session.ConditionUpdate(ConditionType.fall, codeLong: Value.Character.MapId);
                }
                break;
            case ActorState.SwimDash:
            case ActorState.Swim:
                if (UpdateStateSyncTracking(ActorState.Swim)) {
                    Session.ConditionUpdate(ConditionType.swim, codeLong: Value.Character.MapId);
                }

                if (UpdateStateSyncTimeTracking()) {
                    Session.ConditionUpdate(ConditionType.swimtime, targetLong: Value.Character.MapId);
                }
                break;
            case ActorState.Walk:
                if (UpdateStateSyncTracking(ActorState.Walk)) {
                    Session.ConditionUpdate(ConditionType.run, codeLong: Value.Character.MapId);
                }
                break;
            case ActorState.Crawl:
                if (UpdateStateSyncTracking(ActorState.Crawl)) {
                    Session.ConditionUpdate(ConditionType.crawl, codeLong: Value.Character.MapId);
                }
                break;
            case ActorState.Glide:
                if (UpdateStateSyncTracking(ActorState.Glide)) {
                    Session.ConditionUpdate(ConditionType.glide, codeLong: Value.Character.MapId);
                }
                break;
            case ActorState.Climb:
                if (UpdateStateSyncTracking(ActorState.Climb)) {
                    Session.ConditionUpdate(ConditionType.climb, codeLong: Value.Character.MapId);
                }
                break;
            case ActorState.Rope:
                if (UpdateStateSyncTimeTracking()) {
                    Session.ConditionUpdate(ConditionType.ropetime, targetLong: Value.Character.MapId);
                }
                break;
            case ActorState.Ladder:
                if (UpdateStateSyncTimeTracking()) {
                    Session.ConditionUpdate(ConditionType.laddertime, targetLong: Value.Character.MapId);
                }
                break;
            case ActorState.Hold:
                if (UpdateStateSyncTimeTracking()) {
                    Session.ConditionUpdate(ConditionType.holdtime, targetLong: Value.Character.MapId);
                }
                break;
            case ActorState.Ride:
                if (UpdateStateSyncTracking(ActorState.Ride)) {
                    Session.ConditionUpdate(ConditionType.riding, codeLong: Value.Character.MapId);
                }
                break;
            case ActorState.EmotionIdle:
                if (UpdateStateSyncTimeTracking()) {
                    Field.SkillMetadata.TryGet(stateSync.EmotionId, 1, out var emote);
                    if (emote == null) {
                        break;
                    }
                    Session.ConditionUpdate(ConditionType.emotiontime, codeString: emote.Property.Emotion);
                }
                break;
                // TODO: Any more condition states?
        }

        Field?.EnsurePlayerPosition(this);
    }

    protected override void OnDeath() {
        Flag |= PlayerObjectFlag.Dead; // TODO: Need to also send this flag upon revival
    }

    /// <summary>
    /// Adds health to player, and sends update packet.
    /// </summary>
    /// <param name="amount"></param>
    public void RecoverHp(int amount) {
        if (amount <= 0) {
            return;
        }

        Stat stat = Stats.Values[BasicAttribute.Health];
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

        Stat stat = Stats.Values[BasicAttribute.Health];
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

        Stat stat = Stats.Values[BasicAttribute.Spirit];
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

        Stats.Values[BasicAttribute.Spirit].Add(-amount);

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

        Stat stat = Stats.Values[BasicAttribute.Stamina];
        if (stat.Total < stat.Base) {
            Stats.Values[BasicAttribute.Stamina].Add(amount);
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

        Stats.Values[BasicAttribute.Stamina].Add(-amount);

        if (!regenStats.ContainsKey(BasicAttribute.Stamina) && !noRegen) {
            regenStats.Add(BasicAttribute.Stamina, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.StaminaRegen, BasicAttribute.StaminaRegenInterval));
        }
    }

    public void CheckRegen() {
        // Health
        var health = Stats.Values[BasicAttribute.Health];
        if (health.Current < health.Total && !regenStats.ContainsKey(BasicAttribute.Health)) {
            regenStats.Add(BasicAttribute.Health, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.HpRegen, BasicAttribute.HpRegenInterval));
        }

        // Spirit
        var spirit = Stats.Values[BasicAttribute.Spirit];
        if (spirit.Current < spirit.Total && !regenStats.ContainsKey(BasicAttribute.Spirit)) {
            regenStats.Add(BasicAttribute.Spirit, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.SpRegen, BasicAttribute.SpRegenInterval));
        }

        // Stamina
        var stamina = Stats.Values[BasicAttribute.Stamina];
        if (stamina.Current < stamina.Total && !regenStats.ContainsKey(BasicAttribute.Stamina)) {
            regenStats.Add(BasicAttribute.Stamina, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.StaminaRegen, BasicAttribute.StaminaRegenInterval));
        }
    }

    public override void KeyframeEvent(string keyName) {

    }

    public void MoveToPosition(Vector3 position, Vector3 rotation) {
        Session.Send(PortalPacket.MoveByPortal(this, position, rotation));
    }
}

