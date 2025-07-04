// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum MasteryError : short {
    [Description("Not enough mastery.")]
    s_mastery_error_lack_mastery = 1,
    [Description("Not enough mesos.")]
    s_mastery_error_lack_meso = 2,
    [Description("You have not completed the required quest.")]
    s_mastery_error_lack_quest = 3,
    [Description("Not enough items.")]
    s_mastery_error_lack_item = 4,
    [Description("System error")]
    s_mastery_error_unknown = 5,
    [Description("Insufficient level.")]
    s_mastery_error_invalid_level = 7,
    [Description("The game will not run due to fatigue time.")]
    s_anti_addiction_cannot_receive = 12,
}
