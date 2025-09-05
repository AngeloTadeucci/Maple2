using System;
using System.IO;

namespace Maple2.Tools;
public static class DotEnv {
    public static void Load() {
        string dotenv = Path.Combine(Paths.SOLUTION_DIR, ".env");

        if (!File.Exists(dotenv)) {
            Console.WriteLine($"No .env file found at path: {dotenv}");
            return;
        }

        foreach (string line in File.ReadAllLines(dotenv)) {
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
}
