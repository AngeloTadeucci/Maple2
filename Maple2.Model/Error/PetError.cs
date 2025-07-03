// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum PetError {
    [Description("Not enough mesos.")]
    s_err_lack_meso = 27,
    [Description("Not enough merets.")]
    s_err_lack_merat_ask = 28,
    [Description("Storage is full.")]
    s_item_err_store_full = 30,
    [Description("You can feed your pet up to a year's worth of food at once.\nIt won't be healthy if you feed them more.")]
    s_pet_extension_period_limit = 32,
    [Description("You cannot summon a pet from this location.")]
    s_pet_error_summon_potion = 33,
    [Description("")]
    none = 39,

    [Description("An unknown error has occurred.\nPlease try again later.")]
    s_common_error_unknown = byte.MaxValue,
}
