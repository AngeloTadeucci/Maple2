using System;
using System.IO;

namespace Maple2.Tools;
public static class DotEnv {
    public static void Load() {
        string? envOverride = Environment.GetEnvironmentVariable("DOTENV_PATH");
        string? resolved = ResolveEnvPath(envOverride);
        if (string.IsNullOrWhiteSpace(resolved) || !File.Exists(resolved)) {
            Console.WriteLine($"No .env file found. Checked DOTENV_PATH, solution dir, CWD, base dir.");
            return;
        }

        foreach (string line in File.ReadAllLines(resolved)) {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) {
                continue;
            }

            int index = line.IndexOf('=');
            if (index == -1) {
                Console.WriteLine($"Invalid .env line: {line}");
                continue;
            }

            string key = line[..index].Trim();
            string value = line[(index + 1)..].Trim();
            if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'"))) {
                value = value.Substring(1, value.Length - 2);
            }

            // Do not override values that are already set by the host/container
            string? existing = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(existing)) {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string? ResolveEnvPath(string? overridePath) {
        try {
            if (!string.IsNullOrWhiteSpace(overridePath)) {
                string full = Path.GetFullPath(overridePath);
                if (File.Exists(full)) return full;
            }

            // 1) Original behavior: solution directory
            string sol = Path.Combine(Paths.SOLUTION_DIR, ".env");
            if (File.Exists(sol)) return sol;

            // 2) Current working directory
            string cwd = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, ".env"));
            if (File.Exists(cwd)) return cwd;

            // 3) App base directory
            string baseDir = AppContext.BaseDirectory;
            string baseEnv = Path.Combine(baseDir, ".env");
            if (File.Exists(baseEnv)) return baseEnv;

            // 4) Walk up from CWD a few levels
            string? cur = Environment.CurrentDirectory;
            for (int i = 0; i < 6 && !string.IsNullOrEmpty(cur); i++) {
                string candidate = Path.Combine(cur, ".env");
                if (File.Exists(candidate)) return candidate;
                cur = Path.GetDirectoryName(cur);
            }

            // 5) Walk up from base dir a few levels
            cur = baseDir;
            for (int i = 0; i < 6 && !string.IsNullOrEmpty(cur); i++) {
                string candidate = Path.Combine(cur, ".env");
                if (File.Exists(candidate)) return candidate;
                cur = Path.GetDirectoryName(cur);
            }
        } catch {
            // ignore and fall through
        }
        return null;
    }
}
