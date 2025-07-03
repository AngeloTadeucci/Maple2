// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum WeddingError : short {
    none = 0,
    [Description("You are now engaged to {0}")]
    s_wedding_propose_ok = 1,
    [Description("{0} declined your proposal.")]
    s_wedding_result_decline_propose = 2,
    [Description("You have proposed to {0}. \\nIf they accept, you both will be engaged.")]
    s_wedding_msg_box_propose_to = 3,
    [Description("You cancelled the proposal.")]
    s_wedding_result_cancel_propose_to = 4,
    [Description("{0} cancelled the proposal.")]
    s_wedding_result_cancel_propose_from = 5,
    [Description("The proposal has been cancelled because the waiting time has expired.")]
    s_wedding_result_cancel_propose_expire = 6,
    confirm_reservation = 7, // Unknown what string this is supposed to be
    decline_reservation = 8, // Unknown what string this is supposed to be
    accept_reservation_cancel = 9, // Unknown what string this is supposed to be
    decline_reservation_cancel = 10, // Unknown what string this is supposed to be
    accept_reservation_change = 11, // Unknown what string this is supposed to be
    decline_reservation_change = 12, // Unknown what string this is supposed to be
    [Description("You sent a request to your fiancee to confirm the wedding hall reservation. The reservation will be complete when they confirm.")]
    s_wedding_result_send_wedding_hall_reservation = 15,
    [Description("You have sent a request to your fiancee to change the wedding reservation time.\\nThe reservation will be complete when they confirm.")]
    s_wedding_result_send_wedding_hall_reservation_change = 16,
    [Description("You have sent a request to your fiancee to cancel the wedding reservation time.\\nOnce confirmed, the cancellation will be complete and a free reservation coupon will be given to book a wedding venue of the same grade in the future.")]
    s_wedding_result_send_wedding_hall_reservation_cancel = 17,
    s_wedding_result_err_wedding_block_age_to = 19,
    s_wedding_result_err_wedding_block_age_from = 20,
    [Description("You cannot propose to a character of the same gender.")]
    s_wedding_result_err_wedding_block_gender = 21,
    [Description("You cannot propose to {0} because you are not friends.\\nBecome friends in order to propose.")]
    s_wedding_result_err_not_buddy = 22,
    [Description("You are currently engaged and cannot propose to {0}.")]
    s_wedding_result_err_propose_state_to = 23,
    [Description("You are currently married and cannot propose to {0}.")]
    s_wedding_result_err_marriage_state_to = 24,
    [Description("You currently have a divorce in progress and cannot propose to {0}.")]
    s_wedding_result_err_coolingoff_state_to = 25,
    [Description("You have to wait 48 hours after a divorce to remarry.")]
    s_wedding_result_err_prpose_cooltime_state_to = 26,
    [Description("You cannot propose to a character who is engaged.")]
    s_wedding_result_err_propose_state_from = 27,
    [Description("You cannot propose to a married character.")]
    s_wedding_result_err_mareriage_state_from = 28,
    [Description("You cannot propose to a character with a divorce in progress.")]
    s_wedding_result_err_coolingoff_state_from = 29,
    [Description("You cannot propose to a character with less than 48 hours after divorce.")]
    s_wedding_result_err_propose_cooltime_state_from = 30,
    [Description("Both characters must have {0} in order to propose.")]
    s_wedding_result_err_not_find_propose_item = 31,
    [Description("Distance is too far to propose.")]
    s_wedding_result_err_propose_distance = 32,
    [Description("You have already been proposed.")]
    s_wedding_result_err_already_propose_request = 33,
    [Description("In order to be engaged, reserve a wedding hall, change reservation, or settle a divorce, you must be together in the same map as the other individual.")]
    s_wedding_result_err_same_field = 34,
    [Description("Only available when engaged.")]
    s_wedding_result_err_only_promise_user = 35,
    [Description("This is an unknown wedding hall.")]
    s_wedding_result_err_invalid_wedding_hall = 36,
    [Description("Insufficent merets.")]
    s_wedding_result_err_lack_meratal = 37,
    [Description("There is a maintenance soon. Please make a reservation at a different time.")]
    s_wedding_result_err_maintenance_time = 38,
    [Description("Could not find the wedding hall.")]
    s_wedding_result_not_find_weddinghall = 39,
    [Description("The wedding invitation has already been sent.")]
    s_wedding_result_already_send_invitation = 40,
    [Description("The engagement has expired.")]
    s_wedding_result_late_promise_time = 41,
    [Description("It is too late to modify the reservation.")]
    s_wedding_result_late_wedding_reserve_modify_time = 42,
    [Description("You already have a wedding reservation.")]
    s_wedding_result_already_reservation = 43,
    [Description("There is a wedding already booked for this time.")]
    s_wedding_result_already_same_reservation = 44,
    [Description("You can only divorce if you are married.")]
    s_wedding_result_invaild_state_divorce = 45,
    [Description("Your marriage is too new and cannot file for divorce.")]
    s_wedding_result_lack_marriage_date = 46,
    [Description("You do not have enough mesos to divorce.")]
    s_wedding_result_lack_meso_divorce = 47,
    [Description("The divorce has been completed.")]
    s_wedding_result_late_divorce_agree = 48,
    [Description("Divorce consent can only be made by the person who asked for a divorce.")]
    s_wedding_result_invalid_divorce_agree_user = 49,
    [Description("Can only be used by married users.")]
    s_wedding_result_only_marriage_user = 50,
    [Description("You have exceeded the number of invitations you can send at one time.")]
    s_wedding_result_wedding_invitation_count = 51,
    [Description("You do not have enough mesos to send a wedding invitation.")]
    s_wedding_result_lack_meso_invitation = 52,
    [Description("Cannot send reminder not too long after a wedding.")]
    s_wedding_result_early_remind_wedding = 53,
    [Description("The reservation time is too early.")]
    s_wedding_result_early_wedding_reserve = 54,
    [Description("Forced divorce request cannot be canceled after the divorce meditation period has passed.")]
    s_wedding_result_divorcecancel = 55,
    [Description("You cannot enter the wedding at this time.")]
    s_wedding_visit_failed_common = 56,
    [Description("You cannot enter the wedding ceremony in this map.")]
    s_wedding_visit_failed_disablemap = 57,
    [Description("You cannot attend a wedding while you are tombstoned.")]
    s_wedding_visit_failed_dead = 58,
    [Description("You have already entered the wedding")]
    s_wedding_visit_failed_same_place = 59,
    [Description("You cannot enter the wedding ceremony while in a dungeon.")]
    s_wedding_visit_failed_solo_instance = 60,
    [Description("It is not possible to enter the wedding at this time.")]
    s_wedding_visit_failed_invalid_time = 61,
    [Description("Only the character who reserved the wedding hall can modify or cancel the reservation.")]
    s_wedding_result_only_booking_character = 62,
    [Description("A wedding hall has already been booked.")]
    s_wedding_result_already_weddinghall_reserve = 63,
    [Description("Not enough coupons")]
    s_wedding_result_lack_coupon = 64,
    [Description("System error. [{0}]")]
    s_wedding_result_err_system = short.MaxValue,

}
