using System.Collections.Generic;

namespace Maple2.Model.Game;

public class SkillBook {
    public int MaxSkillTabs = 1;
    public long ActiveSkillTabId = 0;

    public List<SkillTab> SkillTabs = [];

}
