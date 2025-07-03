// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum AttendanceError : byte {
    [Description("Not enough mesos.")]
    s_attendGift_payAttend_result_lackMoney = 1,
    [Description("Not enough merets.")]
    s_attendGift_payAttend_result_lackMerat = 2,
    [Description("No vouchers.")]
    s_attendGift_payAttend_result_hasNotCoupon = 3,
    [Description("This event has already been completed.")]
    s_attendGift_item_attend_already_used = 5,
    [Description("Event not found.")]
    s_attendGift_item_attend_not_found_evnet = 6,
}
