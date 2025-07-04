using System.ComponentModel;

namespace Maple2.Model.Enum;

public enum CaughtFishType : short {
    [Description("Fishing mastery increased by {0}.")]
    Default = 1,
    [Description("You caught your first {0}, increasing your fishing mastery by {1}.")]
    FirstKind = 2,
    [Description("You caught a prize {0}, increasing your fishing mastery by {1}.")]
    Prize = 3,
}
