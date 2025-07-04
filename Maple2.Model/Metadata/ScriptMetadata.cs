using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record ScriptMetadata(
    int Id,
    ScriptType Type,
    IReadOnlyDictionary<int, ScriptState> States);

public record ScriptState(
    int Id,
    ScriptStateType Type,
    bool Pick,
    JobCode? JobCondition,
    CinematicContent[] Contents);

public record CinematicContent(
    string Text,
    NpcTalkButton ButtonType,
    int FunctionId,
    CinematicDistractor[] Distractors,
    CinematicEventScript[] Events);

public record CinematicEventScript(int Id, ScriptContent[] Contents);

public record CinematicDistractor(int[] Goto, int[] GotoFail);

public record ScriptContent(string Text, string VoiceId, string Illustration);
