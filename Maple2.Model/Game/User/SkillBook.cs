using System.Collections.Generic;

namespace Maple2.Model.Game;

public class SkillBook {
    public int MaxSkillTabs;
    public long ActiveSkillTabId;

    public List<SkillTab> SkillTabs;

    public SkillBook() {
        MaxSkillTabs = 1;
        ActiveSkillTabId = 0;
        SkillTabs = [];
    }
}
