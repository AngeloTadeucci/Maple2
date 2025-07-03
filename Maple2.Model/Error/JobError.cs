// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum JobError : byte {
    [Description("Job advancement is only possible as a beginner.")]
    s_err_job_bad_job = 2,
    [Description("You have not completed the quest.")]
    s_err_job_not_complete_quest = 3, // 4
    [Description("")]
    s_err_job_privilege = 5,
    [Description("You do not have enough mesos to raise your job rank.")]
    s_err_job_not_enough_meso = 8,
    [Description("Your level is not high enough to raise your job rank.")]
    s_err_job_not_enough_level = 10,
    [Description("")]
    s_err_job_no_home = 11,
    [Description("You do not need treatment when healthy.")]
    s_err_job_no_penalty = 12,
    [Description("You must join a guild.")]
    s_err_job_guild = 14,
    [Description("Chat conditions do not apply.")]
    s_err_job_dayofweek = 15,
    [Description("Empty {0} spaces in your inventory.")]
    s_err_job_inventory_full = 18,

    [Description("Cannot be used due to conditions.")]
    s_err_job_unknown = byte.MaxValue,
}
