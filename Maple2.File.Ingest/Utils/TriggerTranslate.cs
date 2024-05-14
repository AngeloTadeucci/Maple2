using System.Globalization;
using System.Text;

namespace Maple2.File.Ingest.Utils;

public static class TriggerTranslate {
    private static readonly Dictionary<string, string> ActionLookup = new() {
        {"대화를설정한다", "Set Dialogue"},
        {"랜덤메쉬를설정한다", "Set Random Mesh"},
        {"로그를남긴다", "Write Log"},
        {"로프를설정한다", "Set Rope"},
        {"AGENT를설정한다", "Set Agent"},
        {"NPC를이동시킨다", "Move NPC"},
        {"메쉬를설정한다", "Set Mesh"},
        {"메쉬애니를설정한다", "Set Mesh Animation"},
        {"몬스터를변경한다", "Change Monster"},
        {"몬스터를생성한다", "Spawn Monster"},
        {"몬스터소멸시킨다", "Destroy Monster"},
        {"무작위유저를이동시킨다", "Move Random User"},
        {"버프를걸어준다", "Add Buff"},
        {"버프를삭제한다", "Remove Buff"},
        {"사다리를설정한다", "Set Ladder"},
        {"사운드를설정한다", "Set Sound"},
        {"상태를사용한다", "Use State"},
        {"상태를설정한다", "Set State"},
        {"스킬을설정한다", "Set Skill"},
        {"스킵을설정한다", "Set Skip"},
        {"아이템을생성한다", "Create Item"},
        {"액터를설정한다", "Set Actor"},
        {"업적이벤트를발생시킨다", "Set Achievement"},
        {"연출를설정한다", "Set Direction"},
        {"오브젝트반응설정한다", "Set Interact Object"},
        {"움직이는발판을설정한다", "Set Breakable"},
        {"유저를경로이동시킨다", "Move User Path"},
        {"유저를이동시킨다", "Move User"},
        {"이벤트를설정한다", "Set Event"},
        {"이펙트를설정한다", "Set Effect"},
        {"PVP존을설정한다", "Set Pvp Zone"},
        {"카메라경로를선택한다", "Select Camera Path"},
        {"카메라를선택한다", "Select Camera"},
        {"카메라리셋", "Reset Camera"},
        {"타이머를설정한다", "Set Timer"},
        {"타이머를초기화한다", "Reset Timer"},
        {"포탈을설정한다", "Set Portal"},
        {"연출UI를설정한다", "Set Cinematic UI"},
        {"이벤트UI를설정한다", "Set Event UI"},
        {"공지를한다", "Announce"},
        {"전장점수를준다", "Allocate Battlefield Points"},
    };

    private static readonly Dictionary<string, string> ConditionLookup = new() {
        {"랜덤조건", "Random Condition"},
        {"NPC를감지했으면", "NPC Detected"},
        {"몬스터가전투상태면", "Monster In Combat"},
        {"몬스터가죽어있으면", "Monster Dead"},
        {"무조건", "Always"},
        {"보너스게임보상받은유저를감지했으면", "Bonus Game Reward Detected"},
        {"시간이경과했으면", "Time Expired"},
        {"여러명의유저를감지했으면", "Count Users"},
        {"오브젝트가반응했으면", "Object Interacted"},
        {"유저를감지했으면", "User Detected"},
        {"PVP존이종료했으면", "PVP Zone Ended"},
        {"퀘스트유저를감지하면", "Quest User Detected"},
    };

    public static string ToPascalCase(string text) {
        if (text == null) {
            throw new ArgumentNullException(nameof(text));
        }

        text = text.Replace("1st", "First")
            .Replace("2nd", "Second")
            .Replace("50 Meso", "Fifty Meso");
        var sb = new StringBuilder();
        foreach (char c in text) {
            if (!char.IsLetterOrDigit(c)) {
                sb.Append(" ");
            } else {
                sb.Append(c);
            }
        }

        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        return textInfo.ToTitleCase(sb.ToString().ToLower()).Replace(" ", "");
    }

    public static string ToCamelName(string text) {
        string pascal = ToPascalCase(text);
        return pascal[..1].ToLower() + pascal[1..];
    }

    public static string ToSnakeCase(string text) {
        if (text == null) {
            throw new ArgumentNullException(nameof(text));
        }
        text = text.Replace(" ", "");
        text = text.Replace("NPC", "Npc")
            .Replace("NPc", "Npc")
            .Replace("PVP", "Pvp")
            .Replace("ID", "Id")
            .Replace("PC", "Pc")
            .Replace("UI", "Ui")
            .Replace("Setpc", "SetPc")
            .Replace("UnSet", "Unset")
            .Replace("Emotionloop", "EmotionLoop");

        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(text[0]));
        for (int i = 1; i < text.Length; ++i) {
            char c = text[i];
            if (char.IsUpper(c)) {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            } else {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public static string TranslateAction(string input) {
        return ActionLookup.GetValueOrDefault(input, input);
    }

    public static string TranslateCondition(string input) {
        return ConditionLookup.GetValueOrDefault(input, input);
    }
}
