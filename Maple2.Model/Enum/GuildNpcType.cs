using System.ComponentModel;

namespace Maple2.Model.Enum;

public enum GuildNpcType {
    [Description("unknown")]
    Unknown = 0,
    [Description("equip")]
    Equip = 1,
    [Description("goods")]
    Goods = 2,
    [Description("gemstone")]
    Gemstone = 3,
    [Description("itemMerge")]
    ItemMerge = 4,
    [Description("music")]
    Music = 5,
    [Description("quest")]
    Quest = 6,
}
