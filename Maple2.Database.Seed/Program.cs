using Maple2.Database.Context;
using Maple2.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

DotEnv.Load();

string? server = Environment.GetEnvironmentVariable("DB_IP");
string? port = Environment.GetEnvironmentVariable("DB_PORT");
string? database = Environment.GetEnvironmentVariable("GAME_DB_NAME");
string? user = Environment.GetEnvironmentVariable("DB_USER");
string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");

if (server == null || port == null || database == null || user == null || password == null) {
    throw new ArgumentException("Database connection information was not set");
}

string dataDbConnection = $"Server={server};Port={port};Database={database};User={user};Password={password};oldguids=true";

string worldServerDir = Path.Combine(Paths.SOLUTION_DIR, "Maple2.Server.World");

string cmdCommand = "cd " + worldServerDir + " && dotnet ef database update";

Console.WriteLine("Migrating...");

Process process = Process.Start("CMD.exe", "/C " + cmdCommand);

process.WaitForExit();

Console.WriteLine("Migration complete!");

DbContextOptions options = new DbContextOptionsBuilder()
    .UseMySql(dataDbConnection, ServerVersion.AutoDetect(dataDbConnection)).Options;

using var ms2Context = new Ms2Context(options);

string[] seeds =
        {
            "shops", "shop_items", "beauty_shops", "beauty_shop_items", "game_event", "system_banner", "premium_market_item"
        };

Console.WriteLine("Seeding...");

foreach (string seed in seeds) {
    Seed(seed);
}

Console.WriteLine("Seeding complete!");


void Seed(string type) {
    Stopwatch stopwatch = new();

    Console.Write($"Seeding {type}... ");

    string fileLines = File.ReadAllText(Path.Combine(Paths.DB_SEEDS_DIR, $"{type}.sql"));
    if (ExecuteSqlFile(fileLines)) {
        Console.Write($"finished in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine();
    } else {
        Console.WriteLine("Failed to seed {type}");
    }
}

bool ExecuteSqlFile(string fileLines) {
    fileLines = fileLines.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("{", "{{").Replace("}", "}}");

    try {
        ms2Context.Database.ExecuteSqlRaw(fileLines);
        return true;
    } catch (Exception e) {
        Console.Error.WriteLine(e.Message);
        return false;
    }
}

