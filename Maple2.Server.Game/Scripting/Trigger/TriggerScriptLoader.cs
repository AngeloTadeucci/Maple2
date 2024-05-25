using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using IronPython.Hosting;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Trigger;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;
using Serilog;
using Serilog.Core;

namespace Maple2.Server.Game.Scripting.Trigger;

public class TriggerScriptLoader {
    private readonly ScriptEngine engine;
    private readonly ConcurrentDictionary<(string XBlock, string Name), ScriptEntry> scriptSources;
    private readonly ILogger logger = Log.Logger.ForContext<TriggerScriptLoader>();

    private struct ScriptEntry(ScriptSource script, List<ScriptSource> sharedScripts) {
        public ScriptSource Script { get; set; } = script;
        public List<ScriptSource> SharedScripts { get; set; } = sharedScripts;
    };

    public TriggerScriptLoader() {
        engine = Python.CreateEngine();
        ICollection<string> paths = engine.GetSearchPaths();
        foreach (string dir in Directory.GetDirectories("Scripts/")) {
            paths.Add(dir);
        }
        engine.SetSearchPaths(paths);

        scriptSources = new ConcurrentDictionary<(string, string), ScriptEntry>();
    }

    // Initializes a script for the specified trigger. If the script has not yet been loaded, also loads it to the cache.
    public bool TryInitScript(TriggerContext context, string xBlock, string name, [NotNullWhen(true)] out TriggerState? state) {
        string scriptPath = $"Scripts/Trigger/{xBlock}/{name}.py";
        if (!File.Exists(scriptPath)) {
            state = null;
            return false;
        }

        if (!scriptSources.TryGetValue((xBlock, name), out ScriptEntry scriptEntry)) {
            scriptEntry = new ScriptEntry(engine.CreateScriptSourceFromFile(scriptPath), new List<ScriptSource>());
            string code = scriptEntry.Script.GetCode();

            string[] lines = code.Split(["\n", "\r\n"], StringSplitOptions.None);

            bool didUpdate = false;
            foreach (string line in lines) {
                // Match "from dungeon_common.checkusercount import *"
                Match import = Regex.Match(line, @"^from\s+(\w.+)\s+import\s+.*$");
                if (import.Success) {
                    string[] parts = import.Groups[1].Value.Split('.');
                    string path = $"Scripts/Trigger/{string.Join("/", parts)}.py";
                    if (!File.Exists(path)) {
                        logger.Error("Invalid shared script import: ", line);
                        continue;
                    }
                    ScriptSource sharedScript = engine.CreateScriptSourceFromFile(path);
                    scriptEntry.SharedScripts.Add(sharedScript);

                    int index = lines.FindIndex(x => x == line);
                    lines[index] = string.Empty;
                    didUpdate = true;
                }
            }
            if (didUpdate) {
                scriptEntry.Script = engine.CreateScriptSourceFromString(string.Join("\n", lines));
            }
            scriptSources[(xBlock, name)] = scriptEntry;
        }

        foreach (ScriptSource sharedScript in scriptEntry.SharedScripts) {
            sharedScript.Execute(context.Scope);
        }

        scriptEntry.Script.Execute(context.Scope);

        dynamic? initialStateClass = context.Scope.GetVariable("initial_state");
        if (initialStateClass == null) {
            state = null;
            return false;
        }

        dynamic? initialState = engine.Operations.CreateInstance(initialStateClass, context);
        if (initialState == null) {
            state = null;
            return false;
        }

        state = new TriggerState(initialState);
        return true;
    }

    public TriggerContext CreateContext(FieldTrigger owner) {
        return new TriggerContext(engine, owner);
    }
}
