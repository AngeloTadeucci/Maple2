using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using IronPython.Hosting;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Trigger;
using Microsoft.Scripting.Hosting;
using Serilog;

namespace Maple2.Server.Game.Scripting.Trigger;

public partial class TriggerScriptLoader {
    private string rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "Trigger");

    private readonly ScriptEngine engine;
    private readonly ConcurrentDictionary<string, List<ScriptSource>> scriptSources;
    private readonly ILogger logger = Log.Logger.ForContext<TriggerScriptLoader>();

    [GeneratedRegex(@"^from\s+(\w.+)\s+import\s+\*\s*$")]
    private static partial Regex ScriptImportRegex();

    public TriggerScriptLoader() {
        engine = Python.CreateEngine();
        ICollection<string> paths = engine.GetSearchPaths();
        paths.Add(rootDir);
        engine.SetSearchPaths(paths);

        scriptSources = new ConcurrentDictionary<string, List<ScriptSource>>();
    }

    // Initializes a script for the specified trigger. If the script has not yet been loaded, also loads it to the cache.
    public bool TryInitScript(TriggerContext context, string xBlock, string name, [NotNullWhen(true)] out TriggerState? state) {
        if (!TryGetScript(Path.Combine(xBlock, name), out List<ScriptSource>? scripts)) {
            state = null;
            return false;
        }

        // Script "compilation" needs to be synchronized.
        lock (engine) {
            foreach (ScriptSource script in scripts) {
                try {
                    script.Execute(context.Scope);
                } catch (Exception e) {
                    logger.Error(e, "Error executing script {Script} in context {Context}", script.GetCode(), context);
                    state = null;
                    return false;
                }
            }
        }

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

    /// <summary>
    /// Retrieve a previously cached ScriptSource or build a new one. Imports are also resolved from the cache.
    /// </summary>
    /// <param name="key">Path without root directory and file extension.</param>
    /// <param name="scripts">Resulting scripts (and imports) to execute in order.</param>
    /// <returns></returns>
    private bool TryGetScript(string key, [NotNullWhen(true)] out List<ScriptSource>? scripts) {
        if (scriptSources.TryGetValue(key, out scripts)) {
            return true;
        }

        string scriptPath = Path.Combine(rootDir, $"{key}.py");
        if (!File.Exists(scriptPath)) {
            return false;
        }

        ScriptSource scriptSource = engine.CreateScriptSourceFromFile(scriptPath);
        string code = scriptSource.GetCode();
        string[] lines = code.Split(["\n", "\r\n"], StringSplitOptions.None);
        scripts = new List<ScriptSource>();
        for (int i = 0; i < lines.Length; i++) {
            // Match "from dungeon_common.checkusercount import *"
            Match import = ScriptImportRegex().Match(lines[i]);
            if (!import.Success) {
                continue;
            }

            string[] parts = import.Groups[1].Value.Split('.');
            if (!TryGetScript(string.Join("/", parts), out List<ScriptSource>? importScripts)) {
                logger.Error("Invalid shared script import: L{Num} {Line}", i, lines[i]);
                continue;
            }

            scripts.AddRange(importScripts);
            lines[i] = string.Empty;
        }

        // If there were any valid imports, we need to regenerate source with them removed.
        // Otherwise, we can just add the original source.
        scripts.Add(scripts.Count > 0 ? engine.CreateScriptSourceFromString(string.Join("\n", lines)) : scriptSource);
        scriptSources[key] = scripts;
        return true;
    }

    public TriggerContext CreateContext(FieldTrigger owner) {
        return new TriggerContext(engine, owner);
    }
}
