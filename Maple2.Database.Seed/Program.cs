using Maple2.Database.Context;
using Maple2.Tools;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;

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

string[] seeds = [
    "shop",
    "shop-item",
    "beauty-shop",
    "beauty-shop-entry",
    "game-event",
    "system-banner",
    "premium-market-item",
];

Console.WriteLine("Seeding...");

foreach (string seed in seeds) {
    Seed(seed);
}

Console.WriteLine("Seeding complete!");

void Seed(string type) {
    Stopwatch stopwatch = new();

    Console.WriteLine($"Seeding {type}... ");

    if (IsTableEmpty(type)) {
        string fileLines = File.ReadAllText(Path.Combine(Paths.DB_SEEDS_DIR, $"{type}.sql"));
        if (ExecuteSqlFile(fileLines)) {
            Console.WriteLine($"{type} seeded in {stopwatch.ElapsedMilliseconds}ms");
        } else {
            Console.WriteLine($"Failed to seed {type}");
        }
        return;
    }

    Console.Write("Would you like to replace all data? (y/n): ");
    string? input = Console.ReadLine()?.Trim().ToLower();

    if (input == "y") {
        // Replace existing data
        var foreignKeys = GetForeignKeyConstraints(type);
        if (DropForeignKeyConstraints(foreignKeys)) {
            TruncateTable(type);
            string fileLines = File.ReadAllText(Path.Combine(Paths.DB_SEEDS_DIR, $"{type}.sql"));
            if (ExecuteSqlFile(fileLines)) {
                Console.WriteLine($"{type} seeded in {stopwatch.ElapsedMilliseconds}ms");
            } else {
                Console.WriteLine($"Failed to seed {type}");
            }
            RestoreForeignKeyConstraints(foreignKeys);
        }
    } else {
        // Skip seeding
        Console.WriteLine($"Skipping seeding of {type}");
    }
}

bool IsTableEmpty(string tableName) {
    try {
        var entityType = GetEntityType(tableName);
        if (entityType != null) {
            var method = typeof(DbContext).GetMethod(nameof(DbContext.Set), new Type[] { });
            var genericMethod = method?.MakeGenericMethod(entityType);
            var queryable = (IQueryable<object>?) genericMethod?.Invoke(ms2Context, null);

            bool isEmpty = !queryable?.Any() ?? true;
            Console.WriteLine($"Table {tableName} is empty.");
            return isEmpty;
        } else {
            Console.WriteLine($"Entity type for table {tableName} not found.");
            return false;
        }
    } catch (Exception e) {
        Console.Error.WriteLine($"Error checking if table {tableName} is empty: {e.Message}");
        return false;
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

void TruncateTable(string tableName) {
    try {
        string query = $"TRUNCATE TABLE `{tableName}`";
        ms2Context.Database.ExecuteSqlRaw(query);
        Console.WriteLine($"Table {tableName} truncated.");
    } catch (Exception e) {
        Console.Error.WriteLine($"Error truncating table {tableName}: {e.Message}");
    }
}

Type? GetEntityType(string tableName) {
    var entityTypes = ms2Context.Model.GetEntityTypes();
    var entityType = entityTypes.FirstOrDefault(t => t.GetTableName() == tableName);
    return entityType?.ClrType;
}

bool DropForeignKeyConstraints(List<ForeignKeyInfo> foreignKeys) {
    try {
        foreach (var foreignKey in foreignKeys) {
            ms2Context.Database.ExecuteSqlRaw($"ALTER TABLE `{foreignKey.TableName}` DROP FOREIGN KEY `{foreignKey.ConstraintName}`");
        }
        Console.WriteLine($"Dropped foreign key constraints.");
        return true;
    } catch (Exception e) {
        Console.Error.WriteLine($"Error dropping foreign key constraints: {e.Message}");
        return false;
    }
}

bool RestoreForeignKeyConstraints(List<ForeignKeyInfo> foreignKeys) {
    try {
        foreach (var foreignKey in foreignKeys) {
            ms2Context.Database.ExecuteSqlRaw($"ALTER TABLE `{foreignKey.TableName}` ADD CONSTRAINT `{foreignKey.ConstraintName}` FOREIGN KEY (`{foreignKey.ColumnName}`) REFERENCES `{foreignKey.ReferencedTableName}` (`{foreignKey.ReferencedColumnName}`) ON DELETE CASCADE");
        }
        Console.WriteLine($"Restored foreign key constraints.");
        return true;
    } catch (Exception e) {
        Console.Error.WriteLine($"Error restoring foreign key constraints: {e.Message}");
        return false;
    }
}

List<ForeignKeyInfo> GetForeignKeyConstraints(string tableName) {
    var foreignKeys = new List<ForeignKeyInfo>();
    string query = $@"
        SELECT
            CONSTRAINT_NAME,
            TABLE_NAME,
            COLUMN_NAME,
            REFERENCED_TABLE_NAME,
            REFERENCED_COLUMN_NAME
        FROM
            INFORMATION_SCHEMA.KEY_COLUMN_USAGE
        WHERE
            REFERENCED_TABLE_NAME = '{tableName}' AND
            TABLE_SCHEMA = '{database}'";
    using (var command = ms2Context.Database.GetDbConnection().CreateCommand()) {
        command.CommandText = query;
        ms2Context.Database.OpenConnection();
        using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
                foreignKeys.Add(new ForeignKeyInfo {
                    ConstraintName = reader.GetString(0),
                    TableName = reader.GetString(1),
                    ColumnName = reader.GetString(2),
                    ReferencedTableName = reader.GetString(3),
                    ReferencedColumnName = reader.GetString(4)
                });
            }
        }
    }
    return foreignKeys;
}

class ForeignKeyInfo {
    public string ConstraintName { get; set; }
    public string TableName { get; set; }
    public string ColumnName { get; set; }
    public string ReferencedTableName { get; set; }
    public string ReferencedColumnName { get; set; }
}
