using System;
using System.IO;

namespace Maple2.Tools;
public static class DotEnv {
    public static void Load() {
        string dotenv = Path.Combine(Paths.SOLUTION_DIR, ".env");

        if (!File.Exists(dotenv)) {
            return;
        }

        foreach (string line in File.ReadAllLines(dotenv)) {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) {
                continue;
            }

            string[] parts = line.Split('=', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2) {
                continue;
            }

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}
