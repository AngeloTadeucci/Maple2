using System.ComponentModel;

namespace Maple2.Model.Enum;

// These could be flags but not sure
public enum EnchantResult : byte {
    None = 0,
    Success = 1,
    Fail = 2,
    LevelDown = 3,
    FailDamage = 4,
    FailWithProtect = 5,
}

public enum EnchantFailType : short {
    [Description("s_enchant_fail_desc0 - ''")]
    None = 0,
    [Description("s_enchant_fail_desc1 - Failure will reduce this item's enchantment level.")]
    LevelDown = 1,
    [Description("s_enchant_fail_desc2 - Failure will render this item Unstable.\\nUnstable gear can no longer be enchanted.")]
    Unstabilize = 2,
}
