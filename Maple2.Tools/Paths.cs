using System;
using System.IO;

namespace Maple2.Tools;
public static class Paths {
    public static readonly string SOLUTION_DIR = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));

    public static readonly string GAME_SCRIPTS_DIR = Path.Combine(SOLUTION_DIR, "Maple2.Server.Game", "Scripts");

    public static readonly string DB_SEEDS_DIR = Path.Combine(SOLUTION_DIR, "Maple2.Database.Seed", "Seeds");
}
