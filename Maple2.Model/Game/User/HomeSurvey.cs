namespace Maple2.Model.Game;

public class HomeSurvey {
    public readonly long Id;
    public readonly bool Public;

    public string Question;
    public long OwnerId;
    public bool Started;
    public bool Ended;

    public int Answers;
    public int MaxAnswers;

    // Dictionary<option , List of character names>
    public readonly Dictionary<string, List<string>> Options;
    public List<string> AvailableCharacters;

    public HomeSurvey(long id, string question, bool publicQuestion) {
        Question = question.Trim();
        Id = id;
        Started = false;
        Ended = false;
        Options = new Dictionary<string, List<string>>();
        AvailableCharacters = [];
        Answers = 0;
        MaxAnswers = 0;
        OwnerId = 0;
        Public = publicQuestion;
    }

    public void Start(long characterId, List<Character> players) {
        OwnerId = characterId;
        Started = true;
        MaxAnswers = players.Count;
        AvailableCharacters = players.Select(x => x.Name).ToList();
    }

    public void End() {
        Ended = true;
        Started = false;
        Question = string.Empty;
    }

    public bool AddOption(string option) {
        if (Options.ContainsKey(option)) {
            return false;
        }

        Options.Add(option, []);
        return true;
    }
}
