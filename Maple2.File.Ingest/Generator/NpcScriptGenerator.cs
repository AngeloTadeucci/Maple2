using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text;
using Maple2.File.Ingest.Utils;
using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml.Npc;
using Maple2.File.Parser.Xml.Quest;
using Maple2.File.Parser.Xml.Script;
using Maple2.Model.Enum;
using Maple2.Tools.Extensions;

namespace Maple2.File.Ingest.Generator;

public class NpcScriptGenerator {
    private const bool NO_DIALOGUE = false;

    private readonly Dictionary<int, (string Name, NpcData Data)> npcs;
    private readonly Dictionary<int, (string Name, QuestData Data)> quests;
    private readonly IDictionary<string, string?> strings;
    private readonly ScriptParser scriptParser;

    public NpcScriptGenerator(M2dReader xmlReader) {
        npcs = new NpcParser(xmlReader).Parse()
            .ToDictionary(entry => entry.Id, entry => (entry.Name, entry.Data));
        quests = new QuestParser(xmlReader).Parse()
            .ToDictionary(entry => entry.Id, entry => (entry.Name, entry.Data));

        scriptParser = new ScriptParser(xmlReader);
        strings = scriptParser.ParseStrings();
    }

    private class IndexedNpcScript {
        public readonly NpcScript Value;
        public readonly NpcData Npc;
        public IDictionary<int, TalkScript> Scripts;

        public IndexedNpcScript(NpcScript value, NpcData npc) {
            Value = value;
            Npc = npc;
            Scripts = new Dictionary<int, TalkScript>();

            foreach (TalkScript script in value.select) {
                Scripts[script.id] = script;
            }
            if (value.job != null) {
                Scripts[value.job.id] = value.job;
            }
            foreach (ConditionTalkScript script in value.script) {
                Scripts[script.id] = script;
            }
        }

        public NpcTalkButton GetResult(int id, int index) {
            if (!Scripts.TryGetValue(id, out TalkScript? script)) {
                return NpcTalkButton.None;
            }

            Debug.Assert(index < script.content.Count, $"{index} >= {script.content.Count}");
            CinematicContent content = script.content[index];
            bool isLast = index == script.content.Count - 1;

            // Roulette Npcs have this information in their scripts.
            if (content.buttonSet != 0) {
                return (NpcTalkButton) content.buttonSet;
            }

            // Job scripts can have special buttons
            if (isLast && id == Value.job?.id) {
                switch (Npc.basic.kind) {
                    case >= 30 and < 40:
                        return NpcTalkButton.SelectableBeauty;
                    case 80:
                        return NpcTalkButton.ChangeJob;
                    case 81:
                        return NpcTalkButton.PenaltyResolve;
                    case 82:
                        return NpcTalkButton.TakeBoat;
                }
            }

            if (content.distractor.Count > 0) {
                return NpcTalkButton.SelectableDistractor;
            }

            return isLast ? NpcTalkButton.Close : NpcTalkButton.Next;
        }
    }

    public void Generate() {
        foreach ((int id, NpcScript data) in scriptParser.ParseNpc()) {
            // No scripts for this npc
            if (data.job == null && data.select.Count == 0 && data.script.Count == 0) {
                //Console.WriteLine($"Skipping: {id}");
                continue;
            }

            //Console.WriteLine($"Processing: {id}");
            (string npcName, NpcData? npc) = npcs.GetValueOrDefault(id);
            if (npc == null) {
                // The Npc is not included in FeatureLocale, so skip generating the script.
                continue;
            }

            bool hasChoice = false;
            bool hasMultipleSelects = data.select.Count > 1;
            bool hasJobScript = data.job != null;
            int picks = data.script.Count(script => script.randomPick);
            bool multiplePicks = data.script.Count(script => script.randomPick) > 1;
            int[] functions = data.script.SelectMany(script =>
                    script.content.Where(content => content.functionID != 0).Select(content => content.functionID))
                .Distinct()
                .ToArray();
            if (data.job != null) {
                functions = functions.Concat(data.job.content.Where(content => content.functionID > 0).Select(content => content.functionID))
                    .Distinct()
                    .ToArray();
            }
            foreach (ConditionTalkScript talkScript in data.script) {
                // Distractors have "goto" which may need scripting.
                hasChoice = talkScript.content.Any(content =>
                    content.distractor.Any(distractor => distractor.@goto.Length + distractor.gotoFail.Length > 1));
            }

            if (!hasChoice && !hasMultipleSelects && !hasJobScript && !multiplePicks && functions.Length == 0) {
                Console.WriteLine($"Skipping: {id} due to no data");
                continue;
            }


            var index = new IndexedNpcScript(data, npc);
            var stream = new StreamWriter($"Scripts/Npc/{id}.py");
            var writer = new IndentedTextWriter(stream, "    ");
            writer.WriteLine(string.IsNullOrWhiteSpace(npcName) ? $@""""""" {id} """"""" : $@""""""" {id}: {npcName} """"""");

            writer.WriteLine("from npc_api import Script");
            writer.WriteLine("import random");
            BlankLine(writer);
            BlankLine(writer);

            writer.WriteLine("class Main(Script):");
            writer.Indent++;

            GenerateFirst(writer, index);
            BlankLine(writer);

            GenerateSelect(writer, data);
            BlankLine(writer);

            // foreach (TalkScript talkScript in data.select) {
            //     GenerateCase(talkScript.id, writer, index, talkScript);
            //     BlankLine(writer);
            // }

            if (data.job != null &&
                data.job.content.Any(content => content.distractor.Any(distractor => distractor.@goto.Length + distractor.gotoFail.Length > 1))) {
                writer.WriteLine("# Job");
                GenerateCase(data.job.id, writer, data.job);
                BlankLine(writer);
            }

            foreach (ConditionTalkScript talkScript in data.script) {
                if (talkScript.gotoConditionTalkID.Length > 0) {
                    GenerateCase(talkScript.id, writer, talkScript);
                    BlankLine(writer);
                    continue;
                }

                // Distractors have "goto" which may need scripting.
                hasChoice = talkScript.content.Any(content =>
                    content.distractor.Any(distractor => distractor.@goto.Length + distractor.gotoFail.Length > 1));
                if (!hasChoice) {
                    Console.WriteLine($"Skipping: {id} due to no data");
                    continue;
                }

                GenerateCase(talkScript.id, writer, talkScript);
                BlankLine(writer);
            }


            if (functions.Length > 0) {
                GenerateEnterExitStates(writer, functions);
            }

            // GenerateButton(writer, index);
            writer.Indent--;

            writer.Flush();
            stream.Flush();
        }


        foreach ((int id, QuestScript data) in scriptParser.ParseQuest()) {
            // No scripts for this quest
            if (data.script.Count == 0) {
                // Console.WriteLine($"Skipping: {id}");
                continue;
            }

            bool hasData = false;
            int[] functions = data.script.SelectMany(script =>
                    script.content.Where(content => content.functionID != 0).Select(content => content.functionID))
                .Distinct()
                .ToArray();
            foreach (QuestTalkScript talkScript in data.script) {
                // Distractors have "goto" which may need scripting.
                bool hasChoice = talkScript.content.Any(content =>
                    content.distractor.Any(distractor => distractor.@goto.Length + distractor.gotoFail.Length > 1));
                bool hasJobCondition = talkScript.jobCondition > 0;
                if (hasChoice || hasJobCondition || functions.Length > 0) {
                    hasData = true;
                }
            }

            if (!hasData) {
                Console.WriteLine($"Skipping: {id} due to no data");
                continue;
            }

            //Console.WriteLine($"Processing: {id}");
            (string npcName, QuestData? quest) = quests.GetValueOrDefault(id);
            if (quest == null) {
                // The Quest is not included in FeatureLocale, so skip generating the script.
                continue;
            }

            var stream = new StreamWriter($"Scripts/Quest/{id}.py");
            var writer = new IndentedTextWriter(stream, "    ");
            writer.WriteLine(string.IsNullOrWhiteSpace(npcName) ? $@""""""" {id} """"""" : $@""""""" {id}: {npcName} """"""");

            writer.WriteLine("from npc_api import Script");
            writer.WriteLine("import random");
            BlankLine(writer);
            BlankLine(writer);

            writer.WriteLine("class Main(Script):");
            writer.Indent++;

            foreach (QuestTalkScript talkScript in data.script) {
                // Distractors have "goto" which may need scripting.
                bool hasChoice = talkScript.content.Any(content =>
                    content.distractor.Any(distractor => distractor.@goto.Length + distractor.gotoFail.Length > 1));
                bool hasJobCondition = talkScript.jobCondition > 0;
                if (hasChoice || hasJobCondition) {
                    GenerateCase(talkScript.id, writer, talkScript);
                    BlankLine(writer);
                }
            }

            if (functions.Length > 0) {
                GenerateEnterExitStates(writer, functions);
            }

            writer.Indent--;

            writer.Flush();
            stream.Flush();
        }
    }

    public void GenerateEvent() {
        foreach ((int id, NpcScript data) in scriptParser.ParseNpc()) {
            // No scripts for this npc
            if (data.script.Count == 0 && data.job == null) {
                continue;
            }
            (string npcName, NpcData? npc) = npcs.GetValueOrDefault(id);
            if (npc == null) {
                // The Npc is not included in FeatureLocale, so skip generating the script.
                continue;
            }

            var index = new IndexedNpcScript(data, npc);
            var str = new StringWriter();
            var writer = new IndentedTextWriter(str, "    ");
            writer.WriteLine($@""""""" {id}: {npcName} """"""");
            writer.WriteLine("from npc_api import Script");

            string className = string.IsNullOrWhiteSpace(npcName) ? $"Npc{id}" : TriggerTranslate.ToPascalCase(npcName);
            writer.WriteLine($"class {className}Event(Script):");
            writer.Indent++;
            bool generated = GenerateEvent(writer, index);
            if (!generated) {
                continue;
            }
            writer.Indent--;

            writer.Flush();
            str.Flush();

            var stream = new StreamWriter($"Scripts/NpcEvent/{id}.py");
            stream.Write(str);
            stream.Flush();
        }
    }

    private void GenerateFirst(IndentedTextWriter writer, IndexedNpcScript index) {
        Dictionary<int, ConditionTalkScript> randomPicks = index.Value.script.Where(script => script.randomPick)
            .ToDictionary(entry => entry.id, entry => entry);

        writer.WriteLine("def first(self) -> int:");
        writer.Indent++;
        if (index.Value.job != null && randomPicks.Count == 0) {
            writer.WriteLine($"return {index.Value.job.id}");
        } else if (index.Value.job == null && randomPicks.Count >= 1) {
            writer.WriteLine(randomPicks.Count > 1
                ? $"return random.choice([{string.Join(", ", randomPicks.Keys)}])"
                : $"return {randomPicks.First().Key}");
        } else {
            if (index.Value.job != null) {
                writer.WriteLine($"# TODO: Job {index.Value.job.id}");
            }
            if (randomPicks.Count > 0) {
                writer.WriteLine($"return random.choice([{string.Join(", ", randomPicks.Keys)}])");
            } else {
                writer.WriteLine("return -1 # No dialogue");
            }
        }
        writer.Indent--;
    }

    private void GenerateSelect(IndentedTextWriter writer, NpcScript script) {
        int[] ids = script.select.Select(select => select.id).ToArray();
        if (ids.Length <= 1) {
            return;
        }
        // foreach (TalkScript talk in script.select) {
        //     writer.WriteLine($"# Select {talk.id}:");
        //     foreach (Content content in talk.content) {
        //         WriteScriptText(writer, content.text);
        //     }
        // }

        writer.WriteLine("def select(self) -> int:");
        writer.Indent++;
        writer.WriteLine(script.select.Count > 1
            ? $"return random.choice([{string.Join(", ", ids)}])"
            : $"return {ids.FirstOrDefault()}");
        writer.Indent--;
    }

    private void GenerateButton(IndentedTextWriter writer, IndexedNpcScript index) {
        writer.WriteLine("def button(self) -> Option:");
        writer.Indent++;
        int count = 0;
        if (index.Value.job != null) {
            for (int i = 0; i < index.Value.job.content.Count; i++, count++) {
                int id = index.Value.job.id;
                if (count == 0) {
                    writer.WriteLine($"if (self.state, index) == ({id}, {i}):");
                } else {
                    writer.WriteLine($"elif (self.state, index) == ({id}, {i}):");
                }
                writer.Indent++;
                string option = TriggerTranslate.ToSnakeCase(index.GetResult(id, i).ToString()).ToUpper();
                writer.WriteLine($"return Option.{option}");
                writer.Indent--;
            }
        }
        foreach (ConditionTalkScript script in index.Value.script) {
            for (int i = 0; i < script.content.Count; i++, count++) {
                int id = script.id;
                if (count == 0) {
                    writer.WriteLine($"if (self.state, index) == ({id}, {i}):");
                } else {
                    writer.WriteLine($"elif (self.state, index) == ({id}, {i}):");
                }
                writer.Indent++;
                string option = TriggerTranslate.ToSnakeCase(index.GetResult(id, i).ToString()).ToUpper();
                writer.WriteLine($"return Option.{option}");
                writer.Indent--;
            }
        }
        writer.WriteLine("return Option.NONE");
        writer.Indent--;
    }

    private void GenerateCase(int id, IndentedTextWriter writer, TalkScript script) {
        Debug.Assert(script.content.Count > 0);

        var conditionScript = script as ConditionTalkScript;
        writer.WriteLine($"def __{id}(self, index: int, pick: int) -> int:");
        writer.Indent++;
        if (script is QuestTalkScript questTalkScript && questTalkScript.jobCondition > 0) {
            writer.WriteLine($"# TODO: Job Condition: {questTalkScript.jobCondition}");
        }
        if (script.content.Count == 1) {
            CinematicContent content = script.content[0];
            GenerateContent(writer, content);
            if (conditionScript != null && conditionScript.gotoConditionTalkID.Length > 0) {
                writer.WriteLine($"# TODO: gotoConditionTalkID {string.Join(", ", conditionScript.gotoConditionTalkID)}");
            }
        } else if (script.content.Count > 0) {
            for (int i = 0; i < script.content.Count; i++) {
                if (i == 0) {
                    writer.WriteLine($"if index == {i}:");
                } else {
                    writer.WriteLine($"elif index == {i}:");
                }
                writer.Indent++;

                CinematicContent content = script.content[i];
                GenerateContent(writer, content);

                // If it's the last content, we need to transition.
                if (i == script.content.Count - 1) {
                    if (conditionScript != null && conditionScript.gotoConditionTalkID.Length > 0) {
                        writer.WriteLine($"# TODO: gotoConditionTalkID {string.Join(", ", conditionScript.gotoConditionTalkID)}");
                    }
                    writer.WriteLine("return -1");
                } else {
                    writer.WriteLine($"return {script.id}");
                }


                writer.Indent--;
            }
        }

        writer.WriteLine("return -1");
        writer.Indent--;
    }

    private void GenerateContent(IndentedTextWriter writer, CinematicContent content) {
        var builder = new StringBuilder();
        if (content.functionID > 0) {
            builder.Append($"functionID={content.functionID} ");
        }
        // if (content.buttonSet > 0) {
        //     builder.Append($"buttonSet={content.buttonSet} ");
        // }
        if (content.openTalkReward) {
            builder.Append($"openTalkReward={content.openTalkReward} ");
        }
        if (builder.Length > 0) {
            writer.WriteLine($"# {builder}");
        }

        WriteScriptText(writer, content.text);

        if (content.distractor.Count > 0) {
            for (int j = 0; j < content.distractor.Count; j++) {
                CinematicDistractor distractor = content.distractor[j];
                if (j == 0) {
                    writer.WriteLine($"if pick == {j}:");
                } else {
                    writer.WriteLine($"elif pick == {j}:");
                }
                writer.Indent++;
                WriteScriptText(writer, distractor.text);

                int[] success = distractor.@goto;
                int[] fail = distractor.gotoFail;
                // No goto means script is complete

                if (success.Length == 0) {
                    // You can't fail if no success?
                    Debug.Assert(fail.Length == 0);
                    writer.WriteLine("return 0");
                } else {
                    if (fail.Length == 0 && success.Length == 1) {
                        writer.WriteLine($"return {success[0]}");
                    } else {
                        writer.WriteLine($"# TODO: goto {string.Join(", ", success)}");
                        if (fail.Length != 0) {
                            writer.WriteLine($"# TODO: gotoFail {string.Join(", ", fail)}");
                            writer.WriteLine($"return {string.Join(", ", fail)}");
                        } else {
                            writer.WriteLine("return -1");
                        }
                    }
                }
                writer.Indent--;
            }
        }
    }

    private bool GenerateEvent(IndentedTextWriter writer, IndexedNpcScript index) {
        List<ConditionTalkScript> eventScripts = index.Value.script.Where(script => {
            return script.content.Count > 0 && script.content.Any(content => content.@event.Count > 0);
        }).ToList();

        if (eventScripts.Count == 0) {
            return false;
        }

        writer.WriteLine("public static (string, string, string) Event(int scriptId, int eventId) {");
        writer.Indent++;
        writer.WriteLine("return (scriptId, eventId) switch {");
        writer.Indent++;
        foreach (ConditionTalkScript script in eventScripts) {
            CinematicContent content = script.content.First();
            foreach (CinematicEventScript @event in content.@event) {
                if (@event.content.Count == 1) {
                    ScriptContent eventContent = @event.content.First();
                    WriteScriptText(writer, eventContent.text, false);
                    writer.WriteLine($"({script.id}, {@event.id}) => (\"{eventContent.text}\", \"{eventContent.voiceID}\", \"{eventContent.illust}\"),");
                } else {
                    writer.WriteLine($"({script.id}, {@event.id}) => Random(");
                    writer.Indent++;
                    for (int i = 0; i < @event.content.Count; i++) {
                        ScriptContent eventContent = @event.content[i];
                        WriteScriptText(writer, eventContent.text, false);
                        writer.Write($"(\"{eventContent.text}\", \"{eventContent.voiceID}\", \"{eventContent.illust}\")");
                        writer.WriteLine((i < script.content.Count - 1) ? "," : "");
                    }
                    writer.Indent--;
                    writer.WriteLine("),");
                }
            }
        }
        writer.WriteLine("_ => (\"\", \"\", \"\"),");
        writer.Indent--;
        writer.WriteLine("}");
        writer.Indent--;
        writer.WriteLine("}");
        return true;
    }

    private void GenerateEnterExitStates(IndentedTextWriter writer, int[] functionIds) {
        Debug.Assert(functionIds.Length > 0);

        writer.WriteLine("def exit_state(self, functionId: int):");
        writer.Indent++;
        foreach (int functionId in functionIds) {
            writer.WriteLine($"if functionId == {functionId}:");
            writer.Indent++;
            writer.WriteLine($"# TODO: functionID {functionId}");
            writer.WriteLine("return");
            writer.Indent--;
        }
        writer.WriteLine("return");
        writer.Indent--;
        BlankLine(writer);

        writer.WriteLine("def enter_state(self, functionId: int):");
        writer.Indent++;
        foreach (int functionId in functionIds) {
            writer.WriteLine($"if functionId == {functionId}:");
            writer.Indent++;
            writer.WriteLine($"# TODO: functionID {functionId}");
            writer.WriteLine("return");
            writer.Indent--;
        }
        writer.WriteLine("return");
        writer.Indent--;
    }

    private void WriteScriptText(IndentedTextWriter writer, string scriptText, bool writeTextId = true) {
        if (NO_DIALOGUE) {
            return;
        }

        if (writeTextId) {
            writer.WriteLine($"# {scriptText}");
        }
        bool first = true;
        foreach (string text in GetScriptText(scriptText)) {
            if (first) {
                writer.WriteLine($"# - {text}");
                first = false;
            } else {
                writer.WriteLine($"#   {text}");
            }
        }
    }

    private string[] GetScriptText(string scriptText) {
        string id = scriptText[8..^1];
        string? result = strings.GetValueOrDefault(id);
        return result == null ? new[] { "" } : result.Split("\n", StringSplitOptions.RemoveEmptyEntries);
    }

    private static void BlankLine(IndentedTextWriter writer) {
        int indent = writer.Indent;
        writer.Indent = 0;
        writer.WriteLine();
        writer.Indent = indent;
    }
}
