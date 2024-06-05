using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Scripting.Trigger;
using Maple2.Tools.Scheduler;
using Microsoft.Scripting.Hosting;
using Serilog;
using Serilog.Core;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext : ITriggerContext {
    private readonly ScriptEngine engine;
    private readonly FieldTrigger owner;
    private readonly ILogger logger = Log.Logger.ForContext<TriggerContext>();

    public readonly ScriptScope Scope;
    private FieldManager Field => owner.Field;
    private TriggerCollection Objects => owner.Field.TriggerObjects;

    private float currentRandom = float.MaxValue;

    // Skip state class reference, must instantiate before using.
    private dynamic? skipState;
    public readonly EventQueue Events;
    public long StartTick;

    public TriggerContext(ScriptEngine engine, FieldTrigger owner) {
        this.engine = engine;
        this.owner = owner;

        Scope = engine.CreateScope();
        Events = new EventQueue();
        Events.Start();
        StartTick = Environment.TickCount64;
    }

    public bool TryGetSkip([NotNullWhen(true)] out TriggerState? state) {
        if (skipState == null) {
            state = null;
            return false;
        }

        state = CreateState(skipState);
        return true;
    }

    public TriggerState? CreateState(dynamic stateClass) {
        dynamic? state = engine.Operations.CreateInstance(stateClass, this);
        return state == null ? null : new TriggerState(state);
    }

    private void Broadcast(ByteWriter packet) => Field.Broadcast(packet);

    private string lastDebugKey = "";

    [Conditional("TRIGGER_DEBUG")]
    [MessageTemplateFormatMethod("messageTemplate")]
    internal void DebugLog(string messageTemplate, params object[] args) {
        LogOnce(logger.Debug, messageTemplate, args);
    }

    [Conditional("TRIGGER_DEBUG")]
    [MessageTemplateFormatMethod("messageTemplate")]
    internal void WarnLog(string messageTemplate, params object[] args) {
        LogOnce(logger.Warning, messageTemplate, args);
    }

    [Conditional("TRIGGER_DEBUG")]
    [MessageTemplateFormatMethod("messageTemplate")]
    internal void ErrorLog(string messageTemplate, params object[] args) {
        LogOnce(logger.Error, messageTemplate, args);
    }

    private void LogOnce(Action<string, object[]> logAction, string messageTemplate, params object[] args) {
        string key = messageTemplate + string.Join(", ", args);
        if (key == lastDebugKey) {
            return;
        }

        logAction($"{owner.Value.Name} {messageTemplate}", args);
        lastDebugKey = key;
    }

    // Accessors
    public int ShadowExpeditionPoints() {
        ErrorLog("[GetShadowExpeditionPoints]");
        return 0;
    }

    public int DungeonVariable(int id) {
        ErrorLog("[GetDungeonVariable] id:{Id}", id);
        return 0;
    }

    public float NpcDamage(int spawnPointId) {
        ErrorLog("[GetNpcDamageRate] spawnPointId:{Id}", spawnPointId);
        return 1.0f;
    }

    public int NpcHp(int spawnPointId, bool isRelative) {
        ErrorLog("[GetNpcHpRate] spawnPointId:{Id}", spawnPointId);
        return 100;
    }

    public int DungeonId() {
        ErrorLog("[GetDungeonId]");
        return 0;
    }

    public int DungeonLevel() {
        ErrorLog("[GetDungeonLevel]");
        return 3;
    }

    public int DungeonMaxUserCount() {
        ErrorLog("[GetDungeonMaxUserCount]");
        return 1;
    }

    public int DungeonRound() {
        ErrorLog("[GetDungeonRoundsRequired]");
        return int.MaxValue;
    }

    public bool CheckUser() {
        return !Field.Players.IsEmpty;
    }

    public int UserCount() {
        return Field.Players.Count;
    }

    public int CountUsers(int boxId, int userTagId) {
        DebugLog("[GetUserCount] boxId:{BoxId}, userTagId:{TagId}", boxId, userTagId);
        if (!Objects.Boxes.TryGetValue(boxId, out TriggerBox? box)) {
            return 0;
        }

        if (userTagId > 0) {
            return Field.Players.Values.Count(player => player.TagId == userTagId && box.Contains(player.Position));
        }

        return Field.Players.Values.Count(player => box.Contains(player.Position));
    }

    public int NpcExtraData(int spawnId, string extraDataKey) {
        WarnLog("[GetNpcExtraData] spawnId:{SpawnId}, extraDataKey:{Key}", spawnId, extraDataKey);
        var npc = Field.EnumerateNpcs().FirstOrDefault(npc => npc.SpawnPointId == spawnId);
        if (npc is null) {
            return 0;
        }

        return npc.AiExtraData.GetValueOrDefault(extraDataKey, 0);
    }

    public int DungeonPlayTime() {
        ErrorLog("[GetDungeonPlayTime]");
        return 0;
    }

    // Scripts seem to just check if this is "Fail"
    public string DungeonState() {
        ErrorLog("[GetDungeonState]");
        return "";
    }

    public int DungeonFirstUserMissionScore() {
        ErrorLog("[GetDungeonFirstUserMissionScore]");
        return 0;
    }

    public int ScoreBoardScore() {
        ErrorLog("[GetScoreBoardScore]");
        return 0;
    }

    public int UserValue(string key) {
        WarnLog("[GetUserValue] key:{Key}", key);
        return Field.UserValues.GetValueOrDefault(key, 0);
    }

    public void DebugString(string value, string feature) {
        logger.Debug("{Value} [{Feature}]", value, feature);
    }

    public void WriteLog(string logName, string @event, int triggerId, string subEvent, int level) {
        logger.Information("{Log}: {Event}, {TriggerId}, {SubEvent}, {Level}", logName, @event, triggerId, subEvent, level);
    }

    #region Conditions
    public int DayOfWeek(string description) {
        return (int) DateTime.UtcNow.DayOfWeek + 1;
    }

    public bool RandomCondition(float rate, string description) {
        if (rate < 0f || rate > 100f) {
            LogOnce(logger.Error, "[RandomCondition] Invalid rate: {Rate}", rate);
            return false;
        }

        if (currentRandom >= 100f) {
            currentRandom = Random.Shared.NextSingle() * 100;
        }

        currentRandom -= rate;
        if (currentRandom > rate) {
            return false;
        }

        currentRandom = float.MaxValue; // Reset
        return true;
    }

    public bool WaitAndResetTick(int waitTick) {
        long tickNow = Environment.TickCount64;
        if (tickNow <= StartTick + waitTick) {
            return false;
        }

        StartTick = tickNow;
        return true;
    }

    public bool WaitTick(int waitTick) {
        return Environment.TickCount64 > StartTick + waitTick;
    }
    #endregion
}
