using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml;
using Serilog;

namespace Maple2.Server.Game.Trigger.Helpers;

public class TriggerState {
    private readonly XmlNode stateNode;
    private readonly TriggerContext triggerContext;

    public readonly string Name;

    public TriggerState(XmlNode state, TriggerContext context) {
        stateNode = state;
        triggerContext = context;
        Name = stateNode.Attributes?["name"]?.Value ?? throw new ArgumentException("State node must have a 'name' attribute");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TriggerState? OnEnter() {
        XmlNode? section = stateNode.SelectSingleNode("onEnter");
        if (section == null) return null;

        foreach (XmlNode actionNode in section.SelectNodes("action")!) {
            CallAction(actionNode);
        }

        if (!GetNextState(section, out XmlNode? nextState)) return null;

        return new TriggerState(nextState, triggerContext);
    }

    public TriggerState? OnTick() {
        XmlNodeList? section = stateNode.SelectNodes("condition");
        if (section == null) return null;

        foreach (XmlNode conditionNode in section) {
            if (!CallCondition(conditionNode)) continue;

            foreach (XmlNode actionNode in conditionNode.SelectNodes("action")!) {
                CallAction(actionNode);
            }

            if (!GetNextState(conditionNode, out XmlNode? nextState)) continue;

            return new TriggerState(nextState, triggerContext);
        }
        return null;
    }

    public void OnExit() {
        XmlNode? section = stateNode.SelectSingleNode("onExit");
        if (section == null) return;
        foreach (XmlNode actionNode in section.SelectNodes("action")!) {
            CallAction(actionNode);
        }
    }

    private bool GetNextState(XmlNode condition, [NotNullWhen(true)] out XmlNode? xmlNode) {
        XmlNode? nextStateNode = condition.SelectSingleNode("transition");
        if (nextStateNode == null) {
            xmlNode = null;
            return false;
        }

        string nextStateName = nextStateNode.Attributes?["state"]?.Value ?? "";
        if (string.IsNullOrEmpty(nextStateName)) {
            xmlNode = null;
            return false;
        }

        XmlNode? nextNode = triggerContext.Owner.GetStateNode(nextStateName);
        if (nextNode == null) {
            xmlNode = null;
            return false;
        }
        xmlNode = nextNode;
        return true;
    }

    private void CallAction(XmlNode actionNode) {
        string actionName = actionNode.Attributes?["name"]?.Value ?? "";
        TriggerFunctionMapping.ActionMap.TryGetValue(actionName, out Action<ITriggerContext, XmlAttributeCollection?>? actionFunc);
        if (actionFunc is not null) {
            try {
                actionFunc(triggerContext, actionNode.Attributes);
            } catch (Exception e) {
                Log.Logger.Error(e, "CallAction: error executing action '{ActionName}', Node: {Node}", actionName, actionNode.OuterXml);
            }
            return;
        }
        Log.Logger.Error("CallAction: action function not found for action '{ActionName}'", actionName);
    }

    private bool CallCondition(XmlNode conditionNode) {
        string conditionName = conditionNode.Attributes?["name"]?.Value ?? "";

        // Handle group conditions
        if (conditionName is "any_one" or "all_of" or "true" or "always") {
            XmlNode? groupNode = conditionNode.SelectSingleNode("group");
            if (groupNode == null) {
                Log.Logger.Error("CallCondition: group node not found for grouped condition '{ActionName}'", conditionName);
                return false;
            }
            XmlNodeList? childConditions = groupNode.SelectNodes("condition");
            if (childConditions == null || childConditions.Count == 0) {
                Log.Logger.Error("CallCondition: no child conditions in group for '{ActionName}'", conditionName);
                return false;
            }

            switch (conditionName) {
                case "any_one":
                    foreach (XmlNode child in childConditions) {
                        if (CallCondition(child)) return true;
                    }
                    return false;
                case "all_of":
                    foreach (XmlNode child in childConditions) {
                        if (!CallCondition(child)) return false;
                    }
                    return true;
                case "true":
                case "always":
                    return true;
            }
        }

        TriggerFunctionMapping.ConditionMap.TryGetValue(conditionName, out Func<ITriggerContext, XmlAttributeCollection?, bool>? conditionFunc);
        if (conditionFunc is not null) {
            try {
                bool result = conditionFunc(triggerContext, conditionNode.Attributes);
                return result;
            } catch (Exception e) {
                Log.Logger.Error(e, "CallCondition: error executing condition '{ConditionName}', Node: {Node}", conditionName, conditionNode.OuterXml);
                return false;
            }
        }

        Log.Logger.Error("CallCondition: condition function not found for action '{ConditionName}'", conditionName);
        return false;
    }
}
