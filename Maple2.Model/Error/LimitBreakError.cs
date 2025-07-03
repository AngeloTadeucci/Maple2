// ReSharper disable InconsistentNaming, IdentifierTypo

namespace Maple2.Model.Error;

public enum LimitBreakError : short {
    none = 0,
    s_unlimited_enchant_err_invalid_item = 1,
    s_unlimited_enchant_err_lack_ingredient = 2,
    s_unlimited_enchant_err_lack_meso = 3,
    s_unlimited_enchant_err_max_unlimited_grade = 5,
}
