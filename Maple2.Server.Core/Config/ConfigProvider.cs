using System.Globalization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Maple2.Server.Core.Config;

public static class ConfigProvider {
    private static readonly object LockObj = new();
    private static FileSystemWatcher? watcher;
    private static string configPath = string.Empty;

    public static ServerSettings Settings { get; private set; } = new();

    public static void Initialize(string? path = null) {
        lock (LockObj) {
            CultureInfo.CurrentCulture = new("en-US");
            configPath = ResolvePath(path);
            Console.WriteLine($"Config: resolved path {configPath}; exists={(File.Exists(configPath) ? "yes" : "no")}");
            Load();
            SetupWatcher();
        }
    }

    private static string ResolvePath(string? path) {
        // 1) Explicit path argument
        if (!string.IsNullOrWhiteSpace(path)) {
            string full = Path.GetFullPath(path);
            if (File.Exists(full)) return full;
        }

        // 2) Environment variable
        string? env = Environment.GetEnvironmentVariable("CONFIG_PATH");
        if (!string.IsNullOrWhiteSpace(env)) {
            string full = Path.GetFullPath(env);
            if (File.Exists(full)) return full;
        }

        // 3) Current working directory
        string cwdPath = Path.GetFullPath("config.yaml");
        if (File.Exists(cwdPath)) return cwdPath;

        // 4) Assembly base directory
        string baseDir = AppContext.BaseDirectory;
        string baseDirPath = Path.Combine(baseDir, "config.yaml");
        if (File.Exists(baseDirPath)) return baseDirPath;

        // 5) Walk up from base directory a few levels to catch repo-root configs
        string? cur = baseDir;
        for (int i = 0; i < 6 && !string.IsNullOrEmpty(cur); i++) {
            string candidate = Path.Combine(cur, "config.yaml");
            if (File.Exists(candidate)) return candidate;
            cur = Path.GetDirectoryName(cur);
        }

        // 6) As a last resort, return resolved path in CWD (may not exist)
        return cwdPath;
    }

    private static void Load() {
        try {
            if (!File.Exists(configPath)) {
                // No file: keep defaults
                Console.WriteLine($"Config: not found at {configPath}; using defaults.");
                return;
            }
            string text = File.ReadAllText(configPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            ServerSettings? loaded = deserializer.Deserialize<ServerSettings>(text);
            if (loaded != null) {
                Settings = MergeWithDefaults(loaded);
                Console.WriteLine($"Config: loaded settings from {configPath}.");
            }
        } catch (Exception ex) {
            // Keep previous settings on failure
            Console.WriteLine($"Config: failed to load {configPath}: {ex.Message}");
        }
    }

    private static ServerSettings MergeWithDefaults(ServerSettings incoming) {
        // This relies on init defaults in POCOs
        return incoming;
    }

    private static void SetupWatcher() {
        watcher?.Dispose();
        string dir = Path.GetDirectoryName(configPath)!;
        string file = Path.GetFileName(configPath);
        watcher = new FileSystemWatcher(dir, file) {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Attributes
        };
        watcher.Changed += (_, __) => {
            try { Load(); } catch { /* ignore */ }
        };
        watcher.EnableRaisingEvents = true;
    }
}
