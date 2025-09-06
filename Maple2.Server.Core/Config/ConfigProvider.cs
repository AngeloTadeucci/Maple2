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
            Load();
            SetupWatcher();
        }
    }

    private static string ResolvePath(string? path) {
        if (!string.IsNullOrWhiteSpace(path)) return Path.GetFullPath(path);
        string? env = Environment.GetEnvironmentVariable("CONFIG_PATH");
        if (!string.IsNullOrWhiteSpace(env)) return Path.GetFullPath(env);
        return Path.GetFullPath("config.yaml");
    }

    private static void Load() {
        try {
            if (!File.Exists(configPath)) {
                // No file: keep defaults
                return;
            }
            string text = File.ReadAllText(configPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            ServerSettings? loaded = deserializer.Deserialize<ServerSettings>(text);
            if (loaded != null) {
                Settings = MergeWithDefaults(loaded);
            }
        } catch {
            // Keep previous settings on failure
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

