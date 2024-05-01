using System.Diagnostics;
using System.Globalization;
using Maple2.Database.Context;
using Maple2.Database.Extensions;
using Maple2.Database.Model.Metadata;
using Maple2.File.Ingest.Mapper;
using Maple2.File.IO;
using Maple2.File.Parser.Tools;
using Maple2.Tools;
using Microsoft.EntityFrameworkCore;

const string locale = "NA";
const string env = "Live";

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Force Globalization to en-US because we use periods instead of commas for decimals
CultureInfo.CurrentCulture = new("en-US");

DotEnv.Load();

string? ms2Root = Environment.GetEnvironmentVariable("MS2_DATA_FOLDER");
if (ms2Root == null) {
    throw new ArgumentException("MS2_DATA_FOLDER environment variable was not set");
}

string xmlPath = Path.Combine(ms2Root, "Xml.m2d");
string exportedPath = Path.Combine(ms2Root, "Resource/Exported.m2d");
string terrainPath = Path.Combine(ms2Root, "Resource/PrecomputedTerrain.m2d");
string serverPath = Path.Combine(ms2Root, "Server.m2d");

if (!File.Exists(xmlPath)) {
    throw new FileNotFoundException("Could not find Xml.m2d file");
}

if (!File.Exists(exportedPath)) {
    throw new FileNotFoundException("Could not find Exported.m2d file");
}

if (!File.Exists(terrainPath)) {
    throw new FileNotFoundException("Could not find PrecomputedTerrain.m2d file");
}

if (!File.Exists(serverPath)) {
    throw new FileNotFoundException("Could not find Server.m2d file, check discord for this file. Link in README.md");
}

string? server = Environment.GetEnvironmentVariable("DB_IP");
string? port = Environment.GetEnvironmentVariable("DB_PORT");
string? database = Environment.GetEnvironmentVariable("DATA_DB_NAME");
string? user = Environment.GetEnvironmentVariable("DB_USER");
string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");

if (server == null || port == null || database == null || user == null || password == null) {
    throw new ArgumentException("Database connection information was not set");
}

string dataDbConnection = $"Server={server};Port={port};Database={database};User={user};Password={password};oldguids=true";

using var xmlReader = new M2dReader(xmlPath);
using var exportedReader = new M2dReader(exportedPath);
using var terrainReader = new M2dReader(terrainPath);
using var serverReader = new M2dReader(serverPath);

DbContextOptions options = new DbContextOptionsBuilder()
    .UseMySql(dataDbConnection, ServerVersion.AutoDetect(dataDbConnection)).Options;

using var metadataContext = new MetadataContext(options);
metadataContext.Database.EnsureCreated();
metadataContext.Database.ExecuteSqlRaw(@"SET GLOBAL max_allowed_packet=268435456"); // 256MB

// Filter Xml results based on feature settings.
Filter.Load(xmlReader, locale, env);

// new TriggerGenerator(xmlReader).Generate();

UpdateDatabase(metadataContext, new AdditionalEffectMapper(xmlReader));
UpdateDatabase(metadataContext, new AnimationMapper(xmlReader));
UpdateDatabase(metadataContext, new ItemMapper(xmlReader));
UpdateDatabase(metadataContext, new NpcMapper(xmlReader));
UpdateDatabase(metadataContext, new PetMapper(xmlReader));
UpdateDatabase(metadataContext, new MapMapper(xmlReader));
UpdateDatabase(metadataContext, new UgcMapMapper(xmlReader));
UpdateDatabase(metadataContext, new QuestMapper(xmlReader));
UpdateDatabase(metadataContext, new RideMapper(xmlReader));
UpdateDatabase(metadataContext, new ScriptMapper(xmlReader));
UpdateDatabase(metadataContext, new SkillMapper(xmlReader));
UpdateDatabase(metadataContext, new TableMapper(xmlReader));
UpdateDatabase(metadataContext, new AchievementMapper(xmlReader));

UpdateDatabase(metadataContext, new MapEntityMapper(metadataContext, exportedReader));
UpdateDatabase(metadataContext, new NavMeshMapper(terrainReader));

UpdateDatabase(metadataContext, new ServerTableMapper(serverReader));

// new MusicScoreParser(xmlReader).Parse().ToList();
// new ScriptParser(xmlReader).ParseNpc().ToList();
// new ScriptParser(xmlReader).ParseQuest().ToList();
// new PetParser(xmlReader).Parse().ToList();
// new PetParser(xmlReader).ParseProperty().ToList();
//
// new AchieveParser(xmlReader).Parse().ToList();
// new AdditionalEffectParser(xmlReader).Parse().ToList();
// new QuestParser(xmlReader).Parse().ToList();

Console.WriteLine("Done!");

void UpdateDatabase<T>(DbContext context, TypeMapper<T> mapper) where T : class {
    string? tableName = context.GetTableName<T>();
    Debug.Assert(!string.IsNullOrEmpty(tableName), $"Invalid table name: {tableName}");

    uint crc32C = mapper.Process();

    var checksum = context.Find<TableChecksum>(tableName);
    if (checksum != null) {
        if (checksum.Crc32C == crc32C) {
            Console.WriteLine($"Table '{tableName}' is up-to-date");
            return;
        }

        checksum.Crc32C = crc32C;
        Console.WriteLine($"Table '{tableName}' outdated");
        int result = context.Database.ExecuteSqlRaw(@$"DELETE FROM `{tableName}`");
        Console.WriteLine($"Removed Table '{tableName}' rows: {result}");
    }

    // Write entries to table
    foreach (T result in mapper.Results) {
        context.Add(result);
    }

    // Write checksum to table
    if (checksum == null) {
        context.Add(new TableChecksum {
            TableName = tableName,
            Crc32C = crc32C,
        });
    } else {
        context.Update(checksum);
    }

    context.SaveChanges();
}
