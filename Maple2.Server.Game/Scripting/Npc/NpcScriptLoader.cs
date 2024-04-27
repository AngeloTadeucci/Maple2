using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using IronPython.Hosting;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;
using Maple2.Tools;
using Microsoft.Scripting.Hosting;

namespace Maple2.Server.Game.Scripting.Npc;

public class NpcScriptLoader {
    private readonly ScriptEngine engine;
    private readonly ConcurrentDictionary<int, ScriptSource> scriptSources;

    public NpcScriptLoader() {
        engine = Python.CreateEngine();
        ICollection<string> paths = engine.GetSearchPaths();

        if (!Directory.Exists(Paths.GAME_SCRIPTS_DIR)) {
            Directory.CreateDirectory(Paths.GAME_SCRIPTS_DIR);
        }

        foreach (string dir in Directory.GetDirectories(Paths.GAME_SCRIPTS_DIR)) {
            paths.Add(dir);
        }
        engine.SetSearchPaths(paths);

        scriptSources = new ConcurrentDictionary<int, ScriptSource>();
    }

    public NpcScript? GetNpc(GameSession session, NpcScriptContext context, FieldNpc npc, ScriptMetadata metadata) {
        Console.WriteLine($"Load script for: {npc.Value.Metadata.Id}");
        if (!scriptSources.TryGetValue(npc.Value.Metadata.Id, out ScriptSource? script)) {
            script = engine.CreateScriptSourceFromFile(Path.Combine(Paths.GAME_SCRIPTS_DIR, $"Npc/{npc.Value.Metadata.Id}.py"));
            scriptSources[npc.Value.Metadata.Id] = script;
        }

        if (!File.Exists(Path.Combine(Paths.GAME_SCRIPTS_DIR, $"Npc/{npc.Value.Metadata.Id}.py"))) {
            return new NpcScript(session, npc, metadata, null);
        }

        ScriptScope scope = engine.CreateScope();
        script.Execute(scope);

        dynamic? type = scope.GetVariable("Main");
        if (type == null) {
            return null;
        }

        return new NpcScript(session, npc, metadata, engine.Operations.CreateInstance(type, context));
    }

    public NpcScript? GetQuest(GameSession session, NpcScriptContext context, FieldNpc npc, ScriptMetadata metadata) {
        Console.WriteLine($"Load script for: {metadata.Id}");
        if (!scriptSources.TryGetValue(npc.Value.Metadata.Id, out ScriptSource? script)) {
            script = engine.CreateScriptSourceFromFile(Path.Combine(Paths.GAME_SCRIPTS_DIR, $"Quest/{metadata.Id}.py"));
            scriptSources[npc.Value.Metadata.Id] = script;
        }

        if (!File.Exists(Path.Combine(Paths.GAME_SCRIPTS_DIR, $"Quest/{metadata.Id}.py"))) {
            return new NpcScript(session, npc, metadata, null);
        }

        ScriptScope scope = engine.CreateScope();
        script.Execute(scope);

        dynamic? type = scope.GetVariable("Main");
        if (type == null) {
            return null;
        }

        return new NpcScript(session, npc, metadata, engine.Operations.CreateInstance(type, context));
    }
}
