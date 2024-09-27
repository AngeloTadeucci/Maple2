using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Database.Model;

internal static class CubeHelper {
    public static CubeSettings? GetCubeSettings(PlotCube cube) {
        if (cube.CubePortalSettings is not null) {
            return (CubePortalSettings) cube.CubePortalSettings;
        }

        // Other settings

        return null;
    }
}
