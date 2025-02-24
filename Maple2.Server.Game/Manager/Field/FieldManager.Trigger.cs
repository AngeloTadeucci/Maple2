using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.Widget;
using Maple2.Server.Game.Scripting.Trigger;

namespace Maple2.Server.Game.Manager.Field;

public partial class FieldManager {
    public readonly TriggerCollection TriggerObjects;

    private readonly ConcurrentDictionary<int, FieldBreakable> triggerBreakable = new();
    private readonly ConcurrentDictionary<string, FieldTrigger> fieldTriggers = new();
    public readonly Dictionary<string, TickTimer> Timers = new();
    public readonly Dictionary<string, int> UserValues = new();
    public readonly Dictionary<string, Widget> Widgets = new();
    public readonly Dictionary<int, List<object>> States = new();

    public FieldTrigger? AddTrigger(TriggerModel trigger) {
        try {
            var fieldTrigger = new FieldTrigger(this, NextLocalId(), trigger) {
                Position = trigger.Position,
                Rotation = trigger.Rotation,
            };
            fieldTriggers[trigger.Name] = fieldTrigger;

            return fieldTrigger;
        } catch (ArgumentException ex) {
            logger.Warning("Invalid Trigger: {Exception}", ex.Message);
            logger.Warning(ex.StackTrace);
            return null;
        }
    }

    public ICollection<FieldTrigger> EnumerateTrigger() => fieldTriggers.Values;

    public bool TryGetTrigger(string name, [NotNullWhen(true)] out FieldTrigger? fieldTrigger) {
        return fieldTriggers.TryGetValue(name, out fieldTrigger);
    }

    // Used for debugging triggers
    public bool ReplaceTrigger(FieldTrigger oldFieldTrigger, FieldTrigger newFieldTrigger) {
        return fieldTriggers.TryUpdate(oldFieldTrigger.Value.Name, newFieldTrigger, oldFieldTrigger);
    }
}
