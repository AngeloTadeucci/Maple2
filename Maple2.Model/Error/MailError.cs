// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum MailError : byte {
    none = 0,
    [Description("Character not found.")]
    s_mail_error_username = 1,
    [Description("The item amounts do not match.")]
    s_mail_error_attachcount = 2, // also 5
    [Description("This item cannot be sent.")]
    s_mail_error_cannot_attach_item = 3, // also 4
    [Description("Mail has not been sent.")]
    s_mail_error_sendmail = 12,
    [Description("This mail has been read already.")]
    s_mail_error_alreadyread = 16,
    [Description("The attached item on this mail has already been retrieved.")]
    s_mail_error_already_receive = 17, // also 21
    [Description("The item cannot be retrieved because your inventory is full.")]
    s_mail_error_receiveitem_to_inven = 20,
    [Description("The item cannot be retrieved because the mail has expired.")]
    s_mail_error_receive_expired = 21,
    [Description("The item amounts do not match.")]
    s_mail_error_attachcount_effect18 = 22, // + effect(18)
    [Description("The sale has ended.")]
    s_mail_error_ad_expired = 23,
    [Description("Mail creation failed.")]
    s_mail_error_createmail = 24,
    [Description("The sender and recipient are the same player.")]
    s_mail_error_recipient_equal_sender = 25,
    [Description("Not enough mesos.")]
    s_err_lack_meso = 26,
    [Description("The mail recipient has been blocked. Please unblock and try again.")]
    s_mail_error_block_from_me = 27,
    [Description("The mail recipient has blocked you. You will not be able to send them mail.")]
    s_mail_error_block_from_other = 28,
    [Description("Mail cannot be sent.")]
    s_mail_error_admin_character = 29,
    [Description("GMs cannot send mail directly to players. Please use the proper GM tool.")]
    s_mail_error_from_admin_to_user = 30,
    [Description("The title and/or text contains a forbidden word.")]
    s_mail_error_bancheck = 31,
    [Description("Your mail privileges have been suspended.")]
    s_mail_error_admin_block = 32,
    [Description("The game will not run due to fatigue time.")]
    s_anti_addiction_cannot_receive = 33,

    // Custom Error Codes
    mail_not_found = 44,

    [Description("System Error: Mail. p = {0}, code = {1}")]
    s_mail_error = byte.MaxValue,
}
