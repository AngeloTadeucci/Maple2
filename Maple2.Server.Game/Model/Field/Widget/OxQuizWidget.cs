using System.Collections.Concurrent;
using System.Reflection;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;

namespace Maple2.Server.Game.Model.Widget;

public class OxQuizWidget : Widget {

    private bool isDevMode;
    private int round;
    private readonly Dictionary<int, OxQuizTable.Entry> questions; // round, question

    public OxQuizWidget(FieldManager field) : base(field) {
        Conditions = new ConcurrentDictionary<string, int>();
        questions = new Dictionary<int, OxQuizTable.Entry>();
    }

    public override void Action(string function, int argNum, string argStr) {
        MethodInfo? method = GetType().GetMethod(function, BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null) {
            method.Invoke(this, [argStr, argNum]);
        }
    }

    private void DevMode(string isOnStr, int argNum) {
        if (isOnStr == "1") {
            isDevMode = true;
        }
        #if DEBUG
        isDevMode = true;
        #endif
    }

    private void PickQuiz(string levelStr, int argNum) {
        round++;
        if (!int.TryParse(levelStr, out int level)) {
            level = 1;
        }

        List<OxQuizTable.Entry> matchingQuestions = Field.ServerTableMetadata.OxQuizTable.Entries.Values.Where(question => question.Level == level).ToList();
        if (matchingQuestions.Count > 0) {
            questions[round] = matchingQuestions[Random.Shared.Next(matchingQuestions.Count)];
        }
    }

    private void ShowQuiz(string durationStr, int argNum) {
        if (!questions.TryGetValue(round, out OxQuizTable.Entry? question)) {
            return;
        }
        string answer = string.Empty;
        if (isDevMode) {
            answer = question.IsTrue ? "TRUE" : "FALSE";
        }
        if (!int.TryParse(durationStr, out int duration)) {
            duration = 15;
        }
        Field.Broadcast(QuizEventPacket.Question(question.Category, question.Question, answer, duration));
    }

    private void ShowAnswer(string durationStr, int argNum) {
        if (!questions.TryGetValue(round, out OxQuizTable.Entry? question)) {
            return;
        }
        if (!int.TryParse(durationStr, out int duration)) {
            duration = 15;
        }
        Field.Broadcast(QuizEventPacket.Answer(question.IsTrue, question.Answer, duration));
    }

    private void PreJudge(string argStr, int argNum) {
        if (!questions.TryGetValue(round, out OxQuizTable.Entry? question)) {
            return;
        }
        Conditions["Correct"] = question.IsTrue ? 1 : 0;
        Conditions["Incorrect"] = question.IsTrue ? 0 : 1;
    }

    private void Judge(string argStr, int argNum) { }

    private void Winner(string argStr, int argNum) { }
}
