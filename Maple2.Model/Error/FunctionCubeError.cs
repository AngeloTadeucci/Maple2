// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum FunctionCubeError {
    ok = 0,
    [Description("This block has not been placed.")]
    s_function_cube_error_invalid_cube = 1, // and 2
    [Description("Too far to operate.")]
    s_function_cube_error_invalid_pos = 3,
    [Description("Only characters currently in the indoor space can be summoned.")]
    s_function_cube_error_invalid_summon_user = 4,
}
