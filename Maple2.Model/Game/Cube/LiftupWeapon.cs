using Maple2.Model.Metadata;

namespace Maple2.Model.Game;

public class LiftupWeapon(ObjectWeapon @object, int itemId, int skillId, short level) {
    public readonly ObjectWeapon Object = @object;
    public readonly int ItemId = itemId;
    public readonly int SkillId = skillId;
    public readonly short Level = level;

}
