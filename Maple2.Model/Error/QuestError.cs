// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum QuestError {
    none = 0,
    [Description("Clear out some space in your inventory.")]
    s_quest_error_inventory_full = 1,
    [Description("Failed to complete the quest.")]
    s_quest_error_consume_fail = 2,
    [Description("Failed to accept the quest.")]
    s_quest_error_accept_fail = 3,
    [Description("This quest has expired.")]
    s_quest_error_invalid_date = 4,
    [Description("You cannot receive rewards while tombstoned.")]
    s_quest_error_fail_complete_by_dead = 5,
    [Description("")]
    s_alliance_quest_completion_ticket_error = 7,
}
