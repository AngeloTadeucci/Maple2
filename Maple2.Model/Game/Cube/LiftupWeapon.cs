using Maple2.Model.Metadata;

namespace Maple2.Model.Game;

public class LiftupWeapon {
    public readonly ObjectWeapon Object;
    public readonly int ItemId;
    public readonly int SkillId;
    public readonly short Level;

    public LiftupWeapon(ObjectWeapon @object, int itemId, int skillId, short level) {
        Object = @object;
        ItemId = itemId;
        SkillId = skillId;
        Level = level;
    }
}
