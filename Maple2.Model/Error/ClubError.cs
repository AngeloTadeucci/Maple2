// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ClubError : int {
    none = 0,
    [Description("Unknown club error")]
    s_club_err_unknown = 1,
    [Description("Cannot invite to the club.")]
    s_club_err_null_user = 11,
    [Description("Cannot invite to the club.")]
    s_club_err_null_user_2 = 12,
    [Description("Cannot find the club member.")]
    s_club_err_null_member = 13,
    [Description("Cannot find the club.")]
    s_club_err_null_club = 14,
    [Description("This character was not invited.")]
    s_club_err_null_invite_member = 15,
    // 10 ~ 50 - Undefined club error
    [Description("Only club leaders can do this.")]
    s_club_err_no_master = 51,
    [Description("Not a registered club member.")]
    s_club_err_not_join_member = 52,
    [Description("Club leaders cannot leave their own clubs.")]
    s_club_err_cannot_leave_master = 53,
    [Description("Failed to deliver club invitation.")]
    s_club_err_blocked = 54,
    [Description("Failed to invite club member.")]
    s_club_err_fail_addmember = 55,
    [Description("The club is full.")]
    s_club_err_full_member = 57,
    [Description("That character cannot join any more clubs.")]
    s_club_err_full_club_member = 58,
    [Description("A club with this name already exists.")]
    s_club_err_name_exist = 60,
    [Description("Contains a forbidden word.")]
    s_club_err_name_value = 61,
    [Description("Clubs cannot be disbanded while they still have members.")]
    s_club_err_exist_member = 62,
    [Description("That character is already a member of the club.")]
    s_club_err_already_exist = 63,
    [Description("Some of your party members cannot be invited to the club.")]
    s_club_err_notparty_alllogin = 67,
    [Description("Clubs can be renamed only once every hour.\nPlease try again later.")]
    s_club_err_remain_time = 72,
    [Description("This is the club's current name.")]
    s_club_err_same_club_name = 73,
    [Description("You cannot use spaces in club names.")]
    s_club_err_clubname_has_blank = 74



}
