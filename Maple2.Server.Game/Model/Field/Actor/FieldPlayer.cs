using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
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
    private ActorState state;
    public ActorState State {
        get => state;
        set {
            if (state == value) return;
            state = value;
            Flag |= PlayerObjectFlag.State;
            stateSyncTimeTracking = 0; // Reset time tracking on state change
        }
    }
    private bool isDead;
    public override bool IsDead {
        get => isDead;
        protected set {
            if (value == isDead) return;
            isDead = value;
            Flag |= PlayerObjectFlag.Dead;
            if (!isDead) {
                DeathState = DeathState.Alive;
            } else {
                if (Session.Field.Metadata.Property.OnlyDarkTomb) {
                    DeathState = DeathState.Metal;
                } else {
                    DeathState = Session.Config.DeathCount == 0 ? DeathState.FirstDeath : DeathState.Metal;
                }
            }
        }
    }
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

    public Tombstone? Tombstone { get; set; }
    public DeathState DeathState {
        get => Value.Character.DeathState;
        set {
            if (value != Value.Character.DeathState) {
                Session.PlayerInfo.SendUpdate(new PlayerUpdateRequest {
                    AccountId = Session.AccountId,
                    CharacterId = Session.CharacterId,
                    DeathState = (int) value,
                    Async = true,
                });
            }
            Value.Character.DeathState = value;
        }
    }

    #region DebugFlags
    private bool debugAi = false;
    public bool DebugSkills = false;
    #endregion

    public int TagId = 1;
    public AdminPermissions AdminPermissions {
        get => Value.Account.AdminPermissions;
        set => Value.Account.AdminPermissions = value;
    }

    private readonly EventQueue scheduler;

    public FieldPlayer(GameSession session, Player player) : base(session.Field, player.ObjectId, player, session.NpcMetadata) {
        Session = session;
        Animation = Session.Animation;

        regenStats = new Dictionary<BasicAttribute, Tuple<BasicAttribute, BasicAttribute>>();
        lastRegenTime = new Dictionary<BasicAttribute, long>();

        stateSyncDistanceTracking = new Dictionary<ActorState, float>();
        stateSyncTimeTracking = 0;
        stateSyncTrackingTick = Environment.TickCount64;

        scheduler = new EventQueue();
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
        base.Update(tickCount);

        if (Flag != PlayerObjectFlag.None && tickCount > flagTick) {
            Field.Broadcast(ProxyObjectPacket.UpdatePlayer(this, Flag));
            Flag = PlayerObjectFlag.None;
            flagTick = (long) (tickCount + TimeSpan.FromSeconds(2).TotalMilliseconds);
        }

        if (IsDead) {
            if (Tombstone == null) {
                return;
            }
            if (Tombstone.HitsRemaining == 0) {
                Revive();
            }
            return;
        }

        if (InBattle && tickCount - battleTick > 2000) {
            InBattle = false;
        }

        if (!IsDead) {
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

                if (tickCount - regenTime > interval.Base) {
                    lastRegenTime[attribute] = tickCount;
                    switch (attribute) {
                        case BasicAttribute.Health:
                            RecoverHp((int) regen.Total);
                            continue;
                        case BasicAttribute.Spirit:
                            RecoverSp((int) regen.Total);
                            continue;
                        case BasicAttribute.Stamina:
                            RecoverStamina((int) regen.Total);
                            continue;
                    }
                    Session.Send(StatsPacket.Update(this, attribute));
                }
            }
            foreach (BasicAttribute attribute in statsToRemove) {
                regenStats.Remove(attribute);
            }
        }

        Session.GameEvent.Update(tickCount);
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
        Field.Broadcast(SetCraftModePacket.Stop(ObjectId));

        Session.HeldCube = null;
        InBattle = false;
        Tombstone = new Tombstone(this, Session.Config.DeathCount + 1);
        bool darkTomb = Session.Field.Metadata.Property.OnlyDarkTomb || Session.Config.DeathCount > 0;
        Field.Broadcast(DeadUserPacket.Dead(ObjectId, darkTomb));

        Buffs.OnDeath();
    }

    /// <summary>
    /// Revives the player from death state.
    /// </summary>
    /// <param name="instant">If true, performs an instant revival without moving to spawn point.</param>
    public bool Revive(bool instant = false) {
        // Check if player can be revived
        if (!IsDead || Tombstone == null) {
            return false;
        }

        // Restore health and update state
        RecoverHp((int) Stats.Values[BasicAttribute.Health].Total);
        IsDead = false;

        // Apply death penalty if field requires it
        if (Field.Metadata.Property.DeathPenalty) {
            Session.Config.UpdateDeathPenalty(Field.FieldTick + Constant.UserRevivalPaneltyTick);
        }

        // Update revival condition
        Session.ConditionUpdate(ConditionType.revival);

        // Mark tombstone as cleared
        Tombstone.HitsRemaining = 0;

        // Handle revival return to different map
        if (!instant && Field.Metadata.Property.RevivalReturnId != 0 && Field.Metadata.Property.RevivalReturnId != Field.Metadata.Id) {
            Session.Send(Session.PrepareField(Field.Metadata.Property.RevivalReturnId)
                ? FieldEnterPacket.Request(Session.Player)
                : FieldEnterPacket.Error(MigrationError.s_move_err_default));
            return true;
        }

        // Send revival packets
        Session.Send(StatsPacket.Init(this));
        if (instant) {
            Session.Send(RevivalPacket.RevivalCount(Session.Config.InstantReviveCount));
        }
        Field.Broadcast(RevivalPacket.Revive(ObjectId));

        // Move to spawn point if not instant revival
        if (!instant && Field.TryGetPlayerSpawn(-1, out FieldPlayerSpawnPoint? point)) {
            MoveToPosition(point.Position, point.Rotation);
        }

        Buffs.UpdateEnabled();
        CheckRegen();
        return true;
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

        Session.PlayerInfo.SendUpdate(new PlayerUpdateRequest {
            AccountId = Session.AccountId,
            CharacterId = Session.CharacterId,
            Health = new HealthUpdate {
                CurrentHp = Stats.Values[BasicAttribute.Health].Current,
                TotalHp = Stats.Values[BasicAttribute.Health].Total,
            },
            Async = true,
        });
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
        Session.Send(StatsPacket.Update(this, BasicAttribute.Health));

        if (!IsDead) {
            if (!regenStats.ContainsKey(BasicAttribute.Health)) {
                regenStats.Add(BasicAttribute.Health, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.HpRegen, BasicAttribute.HpRegenInterval));
            }
            lastRegenTime[BasicAttribute.Health] = Field.FieldTick + Constant.RecoveryHPWaitTick;
        }

        Session.PlayerInfo.SendUpdate(new PlayerUpdateRequest {
            AccountId = Session.AccountId,
            CharacterId = Session.CharacterId,
            Health = new HealthUpdate {
                CurrentHp = Stats.Values[BasicAttribute.Health].Current,
                TotalHp = Stats.Values[BasicAttribute.Health].Total,
            },
            Async = true,
        });
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

        if (!IsDead) {
            if (!regenStats.ContainsKey(BasicAttribute.Spirit)) {
                regenStats.Add(BasicAttribute.Spirit, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.SpRegen, BasicAttribute.SpRegenInterval));
            }
            // lastRegenTime[BasicAttribute.Spirit] = Field.FieldTick + Constant.RecoverySPWaitTick; - Not applicable for SP?
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
        if (stat.Current < stat.Total) {
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

        if (!IsDead) {
            if (!regenStats.ContainsKey(BasicAttribute.Stamina) && !noRegen) {
                regenStats.Add(BasicAttribute.Stamina, new Tuple<BasicAttribute, BasicAttribute>(BasicAttribute.StaminaRegen, BasicAttribute.StaminaRegenInterval));
            }
            lastRegenTime[BasicAttribute.Stamina] = Field.FieldTick + Constant.RecoveryEPWaitTick;
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
        if (!Field.ValidPosition(position)) {
            return;
        }

        Session.Send(PortalPacket.MoveByPortal(this, position, rotation));
    }

    public void MoveToPortal(FieldPortal portal) {
        if (!Field.ValidPosition(portal.Position)) {
            return;
        }

        Session.Send(PortalPacket.MoveByPortal(this, portal.Position, portal.Rotation));
    }

    public void FallDamage(float distance) {
        double distanceScalingFactor = 0.04813;      // base distance scaling factor
        double hpRatioExponent = 1.087;        // HP ratio exponent for diminishing returns
        double currentHp = Stats.Values[BasicAttribute.Health].Current;
        double maxHp = Stats.Values[BasicAttribute.Health].Total;
        double distanceFactor = distanceScalingFactor * Math.Exp(0.0046 * distance);
        double hpRatio = currentHp / maxHp;
        double hpScaling = Math.Pow(hpRatio, hpRatioExponent);

        double damageD = currentHp * distanceFactor * hpScaling;
        damageD = Math.Min(currentHp * 0.25, damageD);
        int damage = (int) damageD;
        if (damage > 0) {
            ConsumeHp(damage);
            Field.Broadcast(StatsPacket.Update(this, BasicAttribute.Health));
            Session.Send(FallDamagePacket.FallDamage(ObjectId, damage));
            Session.ConditionUpdate(ConditionType.fall_damage, targetLong: damage);
            if (!IsDead) {
                Session.ConditionUpdate(ConditionType.fall_survive);
            } else {
                Session.ConditionUpdate(ConditionType.fall_die);
            }
        }
    }
}

