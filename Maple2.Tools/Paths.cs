using System;
using System.IO;

namespace Maple2.Tools;

public static class Paths {
    public static readonly string SOLUTION_DIR = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));

    public static readonly string GAME_SCRIPTS_DIR = Path.Combine(SOLUTION_DIR, "Maple2.Server.Game", "Scripts");

    public static readonly string WEB_DATA_DIR = Path.Combine(SOLUTION_DIR, "Maple2.Server.Web", "Data");

    public static readonly string NAVMESH_DIR = Path.Combine(SOLUTION_DIR, "Maple2.Server.Game", "Navmeshes");
    public static readonly string NAVMESH_HASH_DIR = Path.Combine(SOLUTION_DIR, "Maple2.Server.Game", "Navmeshes", "Hashes");

}
