using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Maple2.File.Ingest.Utils;
using Maple2.File.IO;
using Maple2.File.IO.Crypto.Common;
using static System.Char;

namespace Maple2.File.Ingest.Generator;

public class TriggerGenerator {
    private readonly M2dReader reader;
    private readonly Dictionary<string, string> checkUserCountStates = new();
    private readonly Dictionary<string, string> checkUser10States = new();

    private static readonly HashSet<(string, bool, bool, bool)> ProcessedStrings = [];
    private static readonly SortedDictionary<string, (bool IsState, bool IsAction, bool IsCondition)> KoreanStrings = new();
    private static readonly TriggerScriptCommon ApiScript = new();

    public TriggerGenerator(M2dReader xmlReader) {
        reader = xmlReader;

        XmlDocument checkUserCountDocument = reader.GetXmlDocument(reader.Files.First(entry =>
            entry.Name.StartsWith("trigger/dungeon_common/checkusercount.xml")));
        foreach (XmlNode stateNode in checkUserCountDocument.SelectNodes("ms2/state")!) {
            string stateName = stateNode.Attributes["name"].Value;
            checkUserCountStates.Add(stateName, stateName);
        }

        XmlDocument guildCheckUserDocument = reader.GetXmlDocument(reader.Files.First(entry =>
            entry.Name.StartsWith("trigger/dungeon_common/checkuser10_guildraid.xml")));
        foreach (XmlNode stateNode in guildCheckUserDocument.SelectNodes("ms2/state")!) {
            string stateName = stateNode.Attributes["name"].Value;
            checkUser10States.Add(stateName, stateName);
        }
    }

    public void Generate() {
        foreach (PackFileEntry entry in reader.Files.Where(file => file.Name.StartsWith("trigger/"))) {
            XmlDocument document = reader.GetXmlDocument(entry);
            XmlNodeList stateNodeList = document.SelectNodes("ms2/state")!;
            if (stateNodeList.Count == 0) {
                Console.WriteLine($"Empty script: {entry.Name}");
                continue;
            }

            string scriptDir = entry.Name.Split('/', StringSplitOptions.RemoveEmptyEntries)[1];
            string scriptName = Path.GetFileNameWithoutExtension(entry.Name);
            Directory.CreateDirectory($"Scripts/Trigger/{scriptDir}");
            string pyName = $"Scripts/Trigger/{scriptDir}/{scriptName}.py";
            using var stream = new StreamWriter(pyName);
            using var writer = new IndentedTextWriter(stream, "    ");
            writer.WriteLine(@$""""""" {entry.Name} """"""");

            var script = new TriggerScript {
                Shared = scriptDir == "dungeon_common",
            };

            try {
                var stateIndex = new Dictionary<string, string>();
                foreach (XmlNode importNode in document.SelectNodes("ms2/import")!) {
                    string path = importNode.Attributes["path"].Value.ToLower();
                    string importModule = Directory.GetParent(path).Name;
                    string importName = Path.GetFileNameWithoutExtension(path);

                    script.Imports.Add($"{importModule}.{importName}");
                    switch (importName) {
                        case "checkusercount":
                            foreach (KeyValuePair<string, string> state in checkUserCountStates) {
                                stateIndex.Add(state.Key, state.Value);
                            }
                            break;
                        case "checkuser10_guildraid":
                            foreach (KeyValuePair<string, string> state in checkUser10States) {
                                stateIndex.Add(state.Key, state.Value);
                            }
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown import: {importModule}/{importName}");
                    }
                }
                stateIndex.Add("DungeonStart", "DungeonStart");

                // Copy node list so that we can remove duplicate states
                List<XmlNode> stateNodes = stateNodeList.Cast<XmlNode>().ToList();
                foreach (XmlNode stateNode in stateNodes.ToList()) {
                    string name = stateNode.Attributes["name"].Value;
                    if (name == "DungeonStart") continue; // Special case

                    name = FixClassName(name);
                    if (stateIndex.ContainsKey(name)) {
                        // Console.WriteLine($"Duplicate state in {entry.Name} ignored and removed: {name}");
                        stateNodes.Remove(stateNode);
                    } else {
                        stateIndex.Add(name, name);
                    }
                }

                foreach (XmlNode stateNode in stateNodes) {
                    TriggerScript.State scriptState = ParseState(stateNode, stateIndex, entry.Name);
                    XmlNode current = stateNode;
                    while (current.PreviousSibling is XmlComment comment && comment.Value != null) {
                        if (comment.Value.Contains("</state")) {
                            break;
                        }
                        scriptState.Comments.Insert(0, comment.Value);
                        current = comment;
                    }
                    script.States.Add(scriptState);
                }

                script.WriteTo(writer);
                //Console.WriteLine($"Generated {pyName}...");
            } catch (Exception ex) {
                Console.WriteLine($"Failed to parse file: {entry.Name} - {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        // Create module for dungeon_common
        System.IO.File.Create("Scripts/Trigger/dungeon_common/__init__.py");
        using var apiStream = new StreamWriter("Scripts/Trigger/trigger_api.py");
        using var apiWriter = new IndentedTextWriter(apiStream, "    ");
        ApiScript.WriteTo(apiWriter);
    }

    private static TriggerScript.State ParseState(XmlNode node, Dictionary<string, string> stateIndex, string filePath) {
        string name = FixClassName(node.Attributes["name"].Value);
        // IndexStrings(name, isState: true);

        var onEnter = new List<TriggerScript.Action>();
        foreach (XmlNode actionNode in node.SelectNodes("onEnter/action")!) {
            TriggerScript.Action action = ParseAction(actionNode, stateIndex);
            if (actionNode.NextSibling is XmlComment comment) {
                action.LineComment = comment.Value;
            }
            onEnter.Add(action);
        }

        var conditions = new List<TriggerScript.Condition>();
        foreach (XmlNode conditionNode in node.SelectNodes("condition")!) {
            TriggerScript.Condition condition = ParseCondition(conditionNode, stateIndex, filePath);
            if (conditionNode.NextSibling is XmlComment comment) {
                condition.LineComment = comment.Value;
            }
            conditions.Add(condition);
        }

        var onExit = new List<TriggerScript.Action>();
        foreach (XmlNode actionNode in node.SelectNodes("onExit/action")!) {
            TriggerScript.Action action = ParseAction(actionNode, stateIndex);
            if (actionNode.NextSibling is XmlComment comment) {
                action.LineComment = comment.Value;
            }
            onExit.Add(action);
        }

        return new TriggerScript.State {
            Name = name,
            OnEnter = onEnter,
            Conditions = conditions,
            OnExit = onExit,
        };
    }

    private static TriggerScript.Action ParseAction(XmlNode node, Dictionary<string, string> stateIndex) {
        var strArgs = new List<(string, string)>();
        string? origName = null;
        foreach (XmlAttribute attribute in node.Attributes) {
            if (attribute.Name == "name") {
                origName = attribute.Value;
                continue;
            }
            strArgs.Add((attribute.Name, attribute.Value));
        }
        Debug.Assert(origName != null, "Unable to find name param");

        // IndexStrings(name, isAction: true);
        string name = Translate(origName, TriggerTranslate.TranslateAction);
        if (!ApiScript.Actions.TryGetValue(name, out TriggerScriptCommon.Function? function)) {
            function = new TriggerScriptCommon.Function(name, false) {
                Description = origName,
            };
            ApiScript.Actions.Add(name, function);
        }

        var args = new List<Parameter>();
        foreach ((string argName, string argValue) in strArgs) {
            (ScriptType Type, string Name) param = function.AddParameter(ScriptType.Str, argName);
            if (param.Type != ScriptType.None) {
                args.Add(new Parameter(param.Type, param.Name, argValue));
            }
        }
        // Fix state names referenced in args
        string? comment = null;
        if (name is "set_skip" or "set_scene_skip") {
            Parameter? result = args.Find(arg => arg.Name == "state");
            if (result != null) {
                string? fixedName = FixClassName(result.Value);
                if (fixedName != null && stateIndex.ContainsKey(fixedName)) {
                    result.Value = fixedName;
                } else {
                    if (!string.IsNullOrWhiteSpace(fixedName)) {
                        comment = $"Missing State: {fixedName}";
                    }
                    result.Value = null;
                }
            }
        }

        var action = new TriggerScript.Action {
            Name = name,
            Args = args,
        };
        if (comment != null) {
            action.Comments.Insert(0, comment);
        }
        return action;
    }

    private static TriggerScript.Condition ParseCondition(XmlNode node, Dictionary<string, string> stateIndex, string filePath) {
        var strArgs = new List<(string, string)>();
        string? origName = null;
        foreach (XmlAttribute attribute in node.Attributes) {
            if (attribute.Name == "name") {
                origName = attribute.Value;
                continue;
            }
            strArgs.Add((attribute.Name, attribute.Value));
        }
        Debug.Assert(origName != null, "Unable to find name param");

        bool negated = origName.StartsWith('!');
        origName = origName.TrimStart('!');
        // IndexStrings(name, isCondition: true);
        string name = Translate(origName, TriggerTranslate.TranslateCondition);
        if (!ApiScript.Conditions.TryGetValue(name, out TriggerScriptCommon.Function? function)) {
            function = new TriggerScriptCommon.Function(name, true) {
                Description = origName,
                ReturnType = ScriptType.Bool,
            };
            ApiScript.Conditions.Add(name, function);
        }

        var args = new List<Parameter>();
        foreach ((string argName, string argValue) in strArgs) {
            (ScriptType Type, string Name) param = function.AddParameter(ScriptType.Str, argName);
            if (param.Type != ScriptType.None) {
                args.Add(new Parameter(param.Type, param.Name, argValue));
            }
        }
        // Negative boxId matching
        if (name is "user_detected") {
            Parameter? result = args.Find(arg => arg.Name == "boxIds");
            if (result != null && result.Value?.StartsWith("!") == true) {
                result.Value = result.Value.TrimStart('!');
                negated = !negated;
            }
        }

        var actions = new List<TriggerScript.Action>();
        foreach (XmlNode action in node.SelectNodes("action")!) {
            actions.Add(ParseAction(action, stateIndex));
        }

        string? transition = FixClassName(node.SelectSingleNode("transition")?.Attributes?["state"]?.Value);
        if (transition != null) {
            if (stateIndex.TryGetValue(transition, out string? result)) {
                transition = result;
            } else {
                // Console.WriteLine($"Script {filePath} Missing transition: {transition}");
                // Console.WriteLine($"- {string.Join(",", stateIndex.Keys)}");
                transition = null;
            }
        }

        return new TriggerScript.Condition {
            Name = name,
            Negated = negated,
            Args = args,
            Actions = actions,
            Transition = transition,
            TransitionComment = FixClassName(node.SelectSingleNode("transition")?.Attributes?["state"]?.Value),
        };
    }

    private static readonly Dictionary<string, string> SubStart = new() {
        {"1st", "First"},
        {"2nd", "Second"},
        {"3rd", "Third"},
        {"4th", "Fourth"},
        {"5th", "Fifth"},
        {"6th", "Sixth"},
        {"7th", "Seventh"},
    };
    [return: NotNullIfNotNull(nameof(name))]
    private static string? FixClassName(string? name) {
        if (name == null) {
            return null;
        }

        if (name == "None") {
            return "StateNone";
        }

        name = name.Replace("-", "To");
        foreach ((string key, string value) in SubStart) {
            if (name.StartsWith(key)) {
                name = name.Replace(key, value);
                break;
            }
        }

        string prefix = "";
        while (name.Length > 0 && !IsLetter(name[0])) {
            if (name[0] != '_') {
                prefix += name[0];
            }
            name = name[1..];
        }

        // name is already valid
        if (prefix.Length == 0) {
            return name;
        }
        if (name.Length == 0) {
            return $"State{prefix}";
        }

        return !IsLetter(name[^1]) ? $"{name}_{prefix}" : $"{name}{prefix}";
    }

    [return: NotNullIfNotNull(nameof(name))]
    private static string? Translate(string? name, Func<string, string> translator) {
        if (name == null) {
            return null;
        }

        var builder = new StringBuilder();
        foreach (string split in name.Split('_', ' ')) {
            builder.Append(translator(split));
        }

        return TriggerTranslate.ToSnakeCase(builder.ToString());
    }

    private static void IndexStrings(string? text, bool isState = false, bool isAction = false, bool isCondition = false) {
        if (text == null || ProcessedStrings.Contains((text, isState, isAction, isCondition))) {
            return;
        }

        var builder = new StringBuilder();
        foreach (string split in text.Split('_', ' ', 'U')) {
            string korean = Regex.Replace(split, "[0-9a-zA-Z]+", "");
            if (korean.Length == 0) {
                continue;
            }

            builder.Append($"{korean},");

            KoreanStrings.TryGetValue(korean, out (bool IsState, bool IsAction, bool IsCondition) value);
            value.IsState |= isState;
            value.IsAction |= isAction;
            value.IsCondition |= isCondition;
            KoreanStrings[korean] = value;
        }

        ProcessedStrings.Add((text, isState, isAction, isCondition));
        if (builder.Length > 0) {
            //Console.WriteLine($"{text} => {builder}");
        }
    }

    private static void WriteXmlComments(IndentedTextWriter writer, XmlNode? node) {
        if (node == null) {
            return;
        }

        foreach (XmlNode comment in node.ChildNodes) {
            if (comment is XmlComment) {
                writer.WriteLine($"# {comment.Value}");
            }
        }
    }
}
