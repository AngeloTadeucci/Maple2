using Maple2.Model.Game;
using CubePortalSettings = Maple2.Database.Model.CubePortalSettings;
using CubeSettings = Maple2.Database.Model.CubeSettings;
using InteractCube = Maple2.Database.Model.InteractCube;

namespace Maple2.Database.Extensions;

internal static class CubeHelper {
    public static CubeSettings? GetCubeSettings(PlotCube cube) {
        if (cube.CubePortalSettings is not null) {
            return (CubePortalSettings) cube.CubePortalSettings;
        }

        if (cube.Interact is not null) {
            return (InteractCube) cube.Interact;
        }

        // Other settings

        return null;
    }
}
