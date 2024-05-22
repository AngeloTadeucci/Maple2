using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using IronPython.Hosting;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Trigger;
using Microsoft.Scripting.Hosting;

namespace Maple2.Server.Game.Scripting.Trigger;

public class TriggerScriptLoader {
    private readonly ScriptEngine engine;
    private readonly ConcurrentDictionary<(string XBlock, string Name), ScriptSource> scriptSources;

    public TriggerScriptLoader() {
        engine = Python.CreateEngine();
        ICollection<string> paths = engine.GetSearchPaths();
        foreach (string dir in Directory.GetDirectories("Scripts/")) {
            paths.Add(dir);
        }
        engine.SetSearchPaths(paths);

        scriptSources = new ConcurrentDictionary<(string, string), ScriptSource>();
    }

    // Initializes a script for the specified trigger. If the script has not yet been loaded, also loads it to the cache.
    public bool TryInitScript(TriggerContext context, string xBlock, string name, [NotNullWhen(true)] out TriggerState? state) {
        string scriptPath = $"Scripts/Trigger/{xBlock}/{name}.py";
        if (!File.Exists(scriptPath)) {
            state = null;
            return false;
        }

        if (!scriptSources.TryGetValue((xBlock, name), out ScriptSource? script)) {
            script = engine.CreateScriptSourceFromFile(scriptPath);
            scriptSources[(xBlock, name)] = script;
        }

        ScriptSource dungeonCommonScript = engine.CreateScriptSourceFromFile("Scripts/Trigger/dungeon_common/checkusercount.py");
        dungeonCommonScript.Execute(context.Scope);

        ScriptSource guildRaidScript = engine.CreateScriptSourceFromFile("Scripts/Trigger/guild_raid/checkuser10_guildraid.py");
        guildRaidScript.Execute(context.Scope);

        script.Execute(context.Scope);
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
