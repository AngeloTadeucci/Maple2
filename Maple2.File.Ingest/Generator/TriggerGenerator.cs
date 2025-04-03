using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using M2dXmlGenerator;
using Maple2.File.Ingest.Utils;
using Maple2.File.Ingest.Utils.Trigger;
using Maple2.File.IO;
using Maple2.File.IO.Crypto.Common;
using Maple2.Tools;
using static System.Char;

namespace Maple2.File.Ingest.Generator;

public class TriggerGenerator {
    private readonly M2dReader reader;
    private readonly Dictionary<string, string> checkUserCountStates = new();
    private readonly Dictionary<string, string> checkUser10States = new();

    private static readonly HashSet<(string, bool, bool, bool)> ProcessedStrings = [];
    private static readonly SortedDictionary<string, (bool IsState, bool IsAction, bool IsCondition)> KoreanStrings = new();
    private static readonly TriggerApiScript ApiScript = new();

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

            Directory.CreateDirectory(Path.Combine(Paths.GAME_SCRIPTS_DIR, scriptDir));
            string pyName = Path.Combine(Paths.GAME_SCRIPTS_DIR, scriptDir, $"{scriptName}.py");
            using var stream = new StreamWriter(pyName);
            using var writer = new IndentedTextWriter(stream, "    ");
            writer.WriteLine(@$""""""" {entry.Name} """"""");

            var script = new TriggerScript {
                Shared = scriptDir == "dungeon_common",
            };

            try {
                Dictionary<string, string> stateIndex = [];
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
                    if (stateNode.Attributes["feature"] != null && !FeatureLocaleFilter.FeatureEnabled(stateNode.Attributes["feature"].Value)) {
                        continue;
                    }
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
                    if (stateNode.Attributes["feature"] != null && !FeatureLocaleFilter.FeatureEnabled(stateNode.Attributes["feature"].Value)) {
                        continue;
                    }
                    TriggerScript.State scriptState = ParseState(stateNode, stateIndex, entry.Name);

                    IList<string> comments = SiblingComments(stateNode);
                    // Join all comments and attempt to parse as XML
                    if (TryParseXml(string.Join("\n", comments).Trim(), "state", out List<XmlNode> commentNodes)) {
                        foreach (XmlNode commentNode in commentNodes) {
                            switch (commentNode.Name) {
                                case "state":
                                    script.States.Add(new CommentWrapper(ParseState(commentNode, stateIndex, entry.Name)));
                                    break;
                                default:
                                    Debug.Assert(commentNode.Value != null);
                                    script.States.Add(new Comment(commentNode.Value));
                                    break;
                            }
                        }
                    } else {
                        // If unable to parse full block comment, attempt individual blocks
                        foreach (string comment in comments) {
                            if (TryParseXml(comment, "state", out commentNodes)) {
                                foreach (XmlNode commentNode in commentNodes) {
                                    switch (commentNode.Name) {
                                        case "state":
                                            script.States.Add(new CommentWrapper(ParseState(commentNode, stateIndex, entry.Name)));
                                            break;
                                        default:
                                            Debug.Assert(commentNode.Value != null);
                                            script.States.Add(new Comment(commentNode.Value));
                                            break;
                                    }
                                }
                                continue;
                            }

                            // Anything unparsed is added back as a comment
                            scriptState.Comments.Add(comment);
                        }
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
        System.IO.File.Create(Path.Combine(Paths.GAME_SCRIPTS_DIR, "dungeon_common", "__init__.py"));
        using var apiStream = new StreamWriter(Path.Combine(Paths.GAME_SCRIPTS_DIR, "trigger_api.py"));
        using var apiWriter = new IndentedTextWriter(apiStream, "    ");
        ApiScript.WriteTo(apiWriter);

        using var csStream = new StreamWriter(Path.Combine(Paths.TRIGGER_CONTEXT_DIR, "TriggerContext.cs"));
        using var csWriter = new IndentedTextWriter(csStream, "    ");
        ApiScript.WriteInterface(csWriter);

        using var csEnumStream = new StreamWriter(Path.Combine(Paths.TRIGGER_CONTEXT_DIR, "TriggerEnums.cs"));
        using var csEnumWriter = new IndentedTextWriter(csEnumStream, "    ");
        ApiScript.WriteEnums(csEnumWriter);
    }

    private static TriggerScript.State ParseState(XmlNode node, Dictionary<string, string> stateIndex, string filePath) {
        string name = FixClassName(node.Attributes["name"].Value);
        // IndexStrings(name, isState: true);

        List<IScriptBlock> onEnter = [];
        TriggerScript.Transition? onEnterTransition = null;
        XmlNode? onEnterNode = node.SelectSingleNode("onEnter");
        if (onEnterNode != null) {
            IEnumerator<XmlNode> it = onEnterNode.ChildNodes.Cast<XmlNode>().GetEnumerator();
            while (it.MoveNext()) {
                switch (it.Current.Name) {
                    case "action":
                        onEnter.Add(ParseAction(it, stateIndex));
                        break;
                    case "transition":
                        onEnterTransition = ParseTransition(it, stateIndex);
                        break;
                    case "#comment":
                        if (it.Current.Value == null) {
                            break;
                        }
                        if (TryParseXml(it.Current.Value, "action", out List<XmlNode> commentActionNodes)) {
                            foreach (XmlNode commentNode in commentActionNodes) {
                                switch (commentNode.Name) {
                                    case "action":
                                        onEnter.Add(new CommentWrapper(ParseAction(commentNode, stateIndex)));
                                        break;
                                    default:
                                        Debug.Assert(commentNode.Value != null);
                                        AppendCommentOrAdd(onEnter, commentNode);
                                        break;
                                }
                            }
                        } else {
                            AppendCommentOrAdd(onEnter, it.Current);
                        }
                        break;
                    case "#text":
                        if (!string.IsNullOrWhiteSpace(node.Value)) {
                            Console.WriteLine($"Unexpected text: {node.Value}");
                        }
                        break;
                    default:
                        throw new ArgumentException($"[{filePath}] Unexpected node <onEnter><{it.Current.Name}>: {onEnterNode.InnerXml}");
                }
            }
        }

        List<IScriptBlock> conditions = [];
        foreach (XmlNode conditionNode in node.SelectNodes("condition")!) {
            TriggerScript.Condition condition = ParseCondition(conditionNode, stateIndex, filePath);
            if (conditionNode.NextSibling is XmlComment comment) {
                if (TryParseXml(comment.Value, "condition", out List<XmlNode> commentNodes, allowComments: false)) {
                    foreach (TriggerScript.Condition commentedCondition in commentNodes.Select(commentNode => ParseCondition(commentNode, stateIndex, filePath))) {
                        conditions.Add(new CommentWrapper(commentedCondition));
                    }
                } else {
                    condition.LineComment = comment.Value;
                }
            }
            conditions.Add(condition);
        }

        List<IScriptBlock> onExit = [];
        XmlNode? onExitNode = node.SelectSingleNode("onExit");
        if (onExitNode != null) {
            foreach (XmlNode child in onExitNode.ChildNodes) {
                switch (child.Name) {
                    case "action":
                        onExit.Add(ParseAction(child, stateIndex));
                        break;
                    case "condition":
                        Console.WriteLine($"[{filePath}] Moving onExit <condition> to onTick");
                        conditions.Add(ParseCondition(child, stateIndex, filePath));
                        break;
                    case "#comment":
                        if (child.Value == null) {
                            break;
                        }
                        if (TryParseXml(child.Value, "action", out List<XmlNode> commentNodes)) {
                            foreach (TriggerScript.Action commentedAction in commentNodes.Select(commentNode => ParseAction(commentNode, stateIndex))) {
                                onExit.Add(new CommentWrapper(commentedAction));
                            }
                        } else {
                            AppendCommentOrAdd(onExit, child);
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unexpected node in onExit: {child.Name}\n{child.OuterXml}");
                }
            }
        }

        return new TriggerScript.State(name) {
            OnEnter = onEnter,
            OnEnterTransition = onEnterTransition,
            Conditions = conditions,
            OnExit = onExit,
        };
    }

    private static TriggerScript.Action ParseAction(IEnumerator<XmlNode> it, Dictionary<string, string> stateIndex) {
        TriggerScript.Action action = ParseAction(it.Current, stateIndex);
        if (it.Current.NextSibling is XmlComment { Value: not null } lineComment) {
            string trimmed = lineComment.Value.Trim();
            if (trimmed.StartsWith('<') || trimmed.StartsWith("action name=")) {
                return action;
            }

            if (action.LineComment != null) {
                action.LineComment += ", " + lineComment.Value;
            } else {
                action.LineComment = lineComment.Value;
            }

            it.MoveNext(); // Advance iterator
        }

        return action;
    }

    private static TriggerScript.Action ParseAction(XmlNode node, Dictionary<string, string> stateIndex) {
        Debug.Assert(node.Name == "action", $"ParseAction(node) where node is not <action>: {node.OuterXml}");

        List<(string, string)> strArgs = [];
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
        string? extraDescription = null;
        string? splitArgValue = null;
        string? splitName = null;
        if (TriggerDefinitionOverride.ActionOverride.TryGetValue(name, out TriggerDefinitionOverride? @override) && @override.FunctionSplitter != null) {
            string? splitArgName;
            if (strArgs.Any(e => e.Item1 == @override.FunctionSplitter)) {
                (splitArgName, splitArgValue) = strArgs.Single(e => e.Item1 == @override.FunctionSplitter);
            } else {
                splitArgName = @override.FunctionSplitter;
                splitArgValue = @override.Types[@override.FunctionSplitter].Default!;
            }

            Debug.Assert(@override.FunctionLookup.ContainsKey(splitArgValue), $"Unknown function split in {name} for {splitArgValue}");
            splitName = @override.FunctionLookup[splitArgValue].Name;
            extraDescription = $"{splitArgName}={splitArgValue}";
        }

        if (!ApiScript.Actions.TryGetValue((name, splitName), out TriggerApiScript.Function? function)) {
            function = new TriggerApiScript.Function(name, splitArgValue, false) {
                Description = extraDescription != null ? $"{origName}: {extraDescription}" : origName,
            };
            ApiScript.Actions.Add((name, splitName), function);
        }

        List<PyParameter> args = [];
        foreach ((string argName, string argValue) in strArgs) {
            if (name == "reset_camera" && argValue == "interpolationTime") {
                continue;
            }

            (ScriptType Type, string Name) param = function.AddParameter(ScriptType.Str, argName);
            if (param.Type != ScriptType.None) {
                args.Add(new PyParameter(param.Type, param.Name, argValue));
            }
        }

        var action = new TriggerScript.Action(name, splitArgValue) {
            Args = args,
        };

        // Fix state names referenced in args
        if (name is "set_skip" or "set_scene_skip") {
            PyParameter? result = action.Args.FirstOrDefault(arg => arg.Name == "state");
            if (result != null) {
                string? fixedName = FixClassName(result.Value);
                if (fixedName != null && stateIndex.ContainsKey(fixedName)) {
                    result.Value = fixedName;
                } else {
                    if (!string.IsNullOrWhiteSpace(fixedName)) {
                        action.LineComment = $"Missing State: {fixedName}";
                    }
                    result.Value = null;
                }
            }
        }

        return action;
    }

    private static TriggerScript.Transition ParseTransition(IEnumerator<XmlNode> it, Dictionary<string, string> stateIndex) {
        string? transition = FixClassName(it.Current.Attributes?["state"]?.Value);
        bool isValid = false;
        if (transition != null) {
            isValid = stateIndex.ContainsKey(transition);
            // if (!isValid) {
            //     Console.WriteLine($"Script {filePath} Missing transition: {transition}");
            //     Console.WriteLine($"- {string.Join(",", stateIndex.Keys)}");
            // }
        }
        string? transitionComment = null;
        if (it.Current.NextSibling is XmlComment { Value: not null } comment) {
            if (!comment.Value.StartsWith('<') && !comment.Value.StartsWith("action name=")) {
                transitionComment = comment.Value;
                it.MoveNext(); // Advance iterator
            }
        }

        return new TriggerScript.Transition(transition, isValid, transitionComment);
    }

    private static TriggerScript.Condition ParseCondition(XmlNode node, Dictionary<string, string> stateIndex, string filePath) {
        List<(string, string)> strArgs = [];
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
        if (!ApiScript.Conditions.TryGetValue(name, out TriggerApiScript.Function? function)) {
            function = new TriggerApiScript.Function(name, null, true) {
                Description = origName,
            };
            ApiScript.Conditions.Add(name, function);
        }

        List<PyParameter> args = [];
        foreach ((string argName, string argValue) in strArgs) {
            (ScriptType Type, string Name) param = function.AddParameter(ScriptType.Str, argName);
            if (param.Type != ScriptType.None) {
                args.Add(new PyParameter(param.Type, param.Name, argValue));
            }
        }
        // Negative boxId matching
        if (name is "user_detected") {
            PyParameter? result = args.Find(arg => arg.Name == "box_ids");
            if (result != null && result.Value?.StartsWith("!") == true) {
                result.Value = result.Value.TrimStart('!');
                negated = !negated;
            }
        }

        var condition = new TriggerScript.Condition(name) {
            Negated = negated,
            Args = args,
        };
        IEnumerator<XmlNode> it = node.ChildNodes.Cast<XmlNode>().GetEnumerator();
        while (it.MoveNext()) {
            switch (it.Current.Name) {
                case "action":
                    condition.Actions.Add(ParseAction(it, stateIndex));
                    break;
                case "transition":
                    condition.Transition = ParseTransition(it, stateIndex);
                    break;
                case "group":
                    foreach (XmlNode child in it.Current.ChildNodes) {
                        switch (child.Name) {
                            case "condition":
                                condition.Group.Add(ParseCondition(child, stateIndex, filePath));
                                break;
                            case "#comment":
                                condition.Comments.Add($"{name}: {child.Value}");
                                break;
                            default:
                                Console.WriteLine($"[{filePath}] Unknown <condition><group>: {child.OuterXml}");
                                break;
                        }
                    }
                    break;
                case "#comment":
                    if (it.Current.Value == null) {
                        break;
                    }
                    if (TryParseXml(it.Current.Value, "action", out List<XmlNode> commentNodes)) {
                        foreach (XmlNode commentNode in commentNodes) {
                            switch (commentNode.Name) {
                                case "action":
                                    condition.Actions.Add(new CommentWrapper(ParseAction(commentNode, stateIndex)));
                                    break;
                                default:
                                    Debug.Assert(commentNode.Value != null);
                                    AppendCommentOrAdd(condition.Actions, commentNode);
                                    break;
                            }
                        }
                    } else {
                        AppendCommentOrAdd(condition.Actions, it.Current);
                    }
                    break;
                case "#text":
                    if (!string.IsNullOrWhiteSpace(it.Current.Value) && it.Current.Value != ">") {
                        if (condition.LineComment == null) {
                            condition.LineComment = it.Current.Value.Trim();
                        } else {
                            Console.WriteLine($"Unexpected text: {it.Current.Value}");
                        }
                    }
                    break;
                default:
                    Console.WriteLine($"[{filePath}] Unknown <condition>: {it.Current.OuterXml}");
                    break;
            }
        }

        return condition;
    }

    private static readonly Dictionary<string, string> SubStart = new() {
        { "1st", "First" },
        { "2nd", "Second" },
        { "3rd", "Third" },
        { "4th", "Fourth" },
        { "5th", "Fifth" },
        { "6th", "Sixth" },
        { "7th", "Seventh" },
    };

    [return: NotNullIfNotNull(nameof(name))]
    private static string? FixClassName(string? name) {
        if (name == null) {
            return null;
        }
        if (string.IsNullOrWhiteSpace(name)) {
            return "State";
        }

        // Reserved Keywords
        switch (name) {
            case "None":
                return "StateNone";
            case "True":
                return "StateTrue";
            case "False":
                return "StateFalse";
            case "del":
                return "StateDelete";
        }

        name = name.Replace("-", "To").Replace(" ", "_").Replace(".", "_");
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

    private static IList<string> SiblingComments(XmlNode? node, bool before = true, bool after = false) {
        List<string> comments = [];
        if (node == null) {
            return comments;
        }

        XmlNode? sibling = node.PreviousSibling;
        while (before && sibling is XmlComment { Value: not null } comment) {
            comments.Insert(0, comment.Value);
            sibling = sibling.PreviousSibling;
        }

        List<string> afterComments = [];
        sibling = node.NextSibling;
        while (after && sibling is XmlComment { Value: not null } comment) {
            afterComments.Add(comment.Value);
            sibling = sibling.NextSibling;
        }

        if (comments.Count > 0 && afterComments.Count > 0) {
            comments.Add(""); // Linebreak between comment groups
        }
        comments.AddRange(afterComments);

        return comments;
    }

    private static bool TryParseXml(string? xml, string name, out List<XmlNode> nodes, bool allowComments = true) {
        if (xml == null || !Regex.Match(xml, $"{name} +name=").Success) {
            nodes = [];
            return false;
        }
        // Attempt to close some unclosed xml elements
        xml = xml.Trim();
        if (!xml.Contains("\n")) {
            if (xml.Contains($"<{name}") && !xml.Contains("/>")) {
                xml = xml.Replace(">", "/>");
            }
        }

        // Always first try without allowing comments
        if (allowComments && TryParseXml(xml, name, out nodes, false)) {
            return true;
        }

        // Many cases of valid xml missing first and last <>
        foreach (string tryXml in new[] {
                xml,
                $"<{xml.Trim()}>",
                $"<{xml.Trim()}/>",
            }) {
            try {
                var stateDocument = new XmlDocument();
                stateDocument.LoadXml($"<root>{tryXml}</root>");
                if (stateDocument.DocumentElement != null) {
                    List<XmlNode> children = stateDocument.DocumentElement.ChildNodes.Cast<XmlNode>().ToList();
                    if (children.All(node => node.Name == name || (allowComments && node.Name is "#comment" or "#text"))) {
                        nodes = children.ToList();
                        return true;
                    }
                }
            } catch (XmlException) { /* ignored */
            }
        }

        nodes = [];
        return false;
    }

    // Appends a LineComment to the previous Action block if possible
    // otherwise, just adds a separate Comment block
    private static void AppendCommentOrAdd(IList<IScriptBlock> list, XmlNode comment) {
        Debug.Assert(comment is XmlComment or XmlText && comment.Value != null);
        if (list.Count == 0) {
            list.Add(new Comment(comment.Value));
            return;
        }

        IScriptBlock prevBlock = list[^1];
        if (prevBlock is CommentWrapper wrapper) {
            prevBlock = wrapper.Child;
        }

        if (prevBlock is TriggerScript.Action { LineComment: null } prevAction) {
            prevAction.LineComment = comment.Value;
        } else {
            list.Add(new Comment(comment.Value));
        }
    }
}
