using System.Diagnostics;
using System.Xml;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Trigger;
using Maple2.Server.Game.Trigger.Helpers;
using Serilog;

namespace Maple2.Server.Game.Model;

public class FieldTrigger : FieldEntity<TriggerModel> {
    public readonly TriggerContext Context;
    private readonly XmlDocument triggerDocument;

    private long nextTick;
    private TriggerState? state;
    private TriggerState? nextState;

    public FieldTrigger(FieldManager field, int objectId, TriggerModel value) : base(field, objectId, value) {
        Context = new TriggerContext(this);

        field.TriggerMetadata.TryGet(Field.Metadata.XBlock, Value.Name.ToLower(), out TriggerMetadata? metadata);
        if (metadata == null) {
            throw new ArgumentException($"Trigger {Value.Name} not found in {Field.Metadata.XBlock}");
        }
        Stopwatch stopwatch = Stopwatch.StartNew();

        var document = new XmlDocument();
        document.LoadXml(metadata.Xml);

        stopwatch.Stop();
        Log.Logger.Information("[Trigger] {Name} loaded in {Elapsed}ms", Value.Name, stopwatch.ElapsedMilliseconds);

        triggerDocument = document;

        XmlElement? root = document.DocumentElement;
        if (root is not { Name: "ms2" }) {
            throw new ArgumentException($"Trigger {Value.Name} has no <ms2> root element in {Field.Metadata.XBlock}");
        }

        XmlNode? initialState = root.ChildNodes[0];
        if (initialState is not { Name: "state" }) {
            throw new ArgumentException($"Trigger {Value.Name} has no initial_state in {Field.Metadata.XBlock}");
        }

        nextState = new TriggerState(initialState, Context);
        nextTick = field.FieldTick;
    }

    public List<TriggerState> GetStates(string[] names) {
        if (names.Length == 0) {
            throw new ArgumentException("At least one state name must be provided.");
        }

        List<TriggerState> states = [];
        foreach (XmlNode stateNode in triggerDocument.SelectNodes("//state")!) {
            if (stateNode is not XmlElement stateElement || !names.Contains(stateElement.GetAttribute("name"))) {
                continue;
            }
            states.Add(new TriggerState(stateNode, Context));
        }

        return states;
    }

    public TriggerState? GetState(string name) {
        XmlNode? stateNode = triggerDocument.SelectSingleNode($"//state[@name='{name}']");
        if (stateNode == null) {
            return null;
        }

        return new TriggerState(stateNode, Context);
    }

    public XmlNode? GetStateNode(string name) {
        return triggerDocument.SelectSingleNode($"//state[@name='{name}']");
    }

    public List<string> GetStateNames() {
        List<string> stateNames = [];
        foreach (XmlNode stateNode in triggerDocument.SelectNodes("//state")!) {
            if (stateNode is XmlElement stateElement) {
                stateNames.Add(stateElement.GetAttribute("name"));
            }
        }
        return stateNames;
    }

    public bool Skip() {
        if (Context.TryGetSkip(out TriggerState? skip)) {
            nextState = skip;
            Field.Broadcast(CinematicPacket.StartSkip());
            return true;
        }

        return false;
    }

    public override void Update(long tickCount) {
        Context.Events.InvokeAll();

        if (tickCount < nextTick) {
            return;
        }

        nextTick += Constant.NextStateTriggerDefaultTick;

        if (nextState != null) {
            Context.DebugLog("[OnExit] {State}", state?.Name ?? "null");
            state?.OnExit();
            state = nextState;
            Context.StartTick = Field.FieldTick;
            Context.DebugLog("[OnEnter] {State}", state.Name);
            nextState = state.OnEnter();

            // If OnEnter transitions to nextState, we skip OnTick.
            if (nextState != null) {
                return;
            }
        }

        nextState = state?.OnTick();
    }

    /// <summary>
    /// Should only be used for debugging
    /// </summary>
    public bool SetNextState(string next) {
        TriggerState? stateClass = GetState(next);
        if (stateClass == null) {
            return false;
        }

        nextState = stateClass;
        return nextState != null;
    }
}
