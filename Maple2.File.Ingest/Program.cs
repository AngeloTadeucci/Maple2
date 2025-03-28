using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Maple2.Database.Context;
using Maple2.Database.Extensions;
using Maple2.Database.Model.Metadata;
using Maple2.File.Ingest;
using Maple2.File.Ingest.Helpers;
using Maple2.File.Ingest.Mapper;
using Maple2.File.IO;
using Maple2.File.IO.Nif;
using Maple2.File.Parser.Flat;
using Maple2.File.Parser.MapXBlock;
using Maple2.File.Parser.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;
using Microsoft.EntityFrameworkCore;

const string locale = "NA";
const string env = "Live";

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool runNavmesh = false;
bool dropData = false;

foreach (string? arg in args) {
    switch (arg) {
        case "--run-navmesh":
            runNavmesh = true;
            break;
        case "--drop-data":
            dropData = true;
            break;
    }
}

// Force Globalization to en-US because we use periods instead of commas for decimals
CultureInfo.CurrentCulture = new("en-US");

DotEnv.Load();

string? ms2Root = Environment.GetEnvironmentVariable("MS2_DATA_FOLDER");
if (ms2Root == null) {
    throw new ArgumentException("MS2_DATA_FOLDER environment variable was not set");
}

string xmlPath = Path.Combine(ms2Root, "Xml.m2d");
string exportedPath = Path.Combine(ms2Root, "Resource/Exported.m2d");
string serverPath = Path.Combine(ms2Root, "Server.m2d");

if (!File.Exists(xmlPath)) {
    throw new FileNotFoundException($"Could not find Xml.m2d file at path: {xmlPath}");
}

if (!File.Exists(exportedPath)) {
    throw new FileNotFoundException($"Could not find Exported.m2d file at path: {exportedPath}");
}

if (!File.Exists(serverPath)) {
    throw new FileNotFoundException($"Could not find Server.m2d file at path: {serverPath}\n" +
                                    "You can download this file from here: https://github.com/Zintixx/MapleStory2-XML/releases/latest");
}

string? server = Environment.GetEnvironmentVariable("DB_IP");
string? port = Environment.GetEnvironmentVariable("DB_PORT");
string? database = Environment.GetEnvironmentVariable("DATA_DB_NAME");
string? user = Environment.GetEnvironmentVariable("DB_USER");
string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");

if (server == null || port == null || database == null || user == null || password == null) {
    throw new ArgumentException("Database connection information was not set");
}

string worldServerDir = Path.Combine(Paths.SOLUTION_DIR, "Maple2.Server.World");

bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
bool isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

// check if dotnet ef is installed
Process processCheck;
if (isWindows) {
    processCheck = Process.Start("CMD.exe", "/C dotnet ef");
} else if (isLinux || isMac) {
    processCheck = Process.Start("bash", "-c \"dotnet ef\"");
} else {
    throw new PlatformNotSupportedException("Unsupported OS platform");
}
processCheck.WaitForExit();

if (processCheck.ExitCode != 0) {

    Process installEf;
    if (isWindows) {
        installEf = Process.Start("CMD.exe", "/C dotnet tool install --global dotnet-ef");
    } else if (isLinux || isMac) {
        installEf = Process.Start("bash", "-c \"dotnet tool install --global dotnet-ef\"");
    } else {
        throw new PlatformNotSupportedException("Unsupported OS platform");
    }
    installEf.WaitForExit();
    if (installEf.ExitCode != 0) {
        throw new Exception("Failed to install dotnet-ef. Please install it manually by running 'dotnet tool install --global dotnet-ef'");
    }

    if (isWindows) {
        string dotnetToolsPath = Environment.GetEnvironmentVariable("USERPROFILE") + "/.dotnet/tools";
        string currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
        Environment.SetEnvironmentVariable("PATH", currentPath + ";" + dotnetToolsPath);
        Console.WriteLine($"Updated PATH to include {dotnetToolsPath}");
    } else if (isLinux || isMac) {
        string dotnetToolsPath = Environment.GetEnvironmentVariable("HOME") + "/.dotnet/tools";
        string currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
        Environment.SetEnvironmentVariable("PATH", currentPath + ":" + dotnetToolsPath);
        Console.WriteLine($"Updated PATH to include {dotnetToolsPath}");
    } else {
        throw new PlatformNotSupportedException("Unsupported OS platform");
    }
}

string cmdCommand = "cd " + worldServerDir + " && dotnet ef database update";

Console.WriteLine("Migrating game database...");

Process process;
if (isWindows) {
    process = Process.Start("CMD.exe", "/C " + cmdCommand);
} else if (isLinux || isMac) {
    process = Process.Start("bash", "-c \"" + cmdCommand + "\"");
} else {
    throw new PlatformNotSupportedException("Unsupported OS platform");
}

process.WaitForExit();

if (process.ExitCode != 0) {
    throw new Exception("Failed to migrate game database.");
}

Console.WriteLine("Game Migration complete!");

using var xmlReader = new M2dReader(xmlPath);
using var exportedReader = new M2dReader(exportedPath);
using var serverReader = new M2dReader(serverPath);

string dataDbConnection = $"Server={server};Port={port};Database={database};User={user};Password={password};oldguids=true";

DbContextOptions options = new DbContextOptionsBuilder()
    .UseMySql(dataDbConnection, ServerVersion.AutoDetect(dataDbConnection)).Options;

Console.WriteLine("Connecting to metadata database...");
using var metadataContext = new MetadataContext(options);

bool schemaChanged = SchemaVersionManager.ShouldRecreateDatabase(metadataContext);

if (dropData || schemaChanged) {
    Console.WriteLine("Dropping metadata database...");
    metadataContext.Database.EnsureDeleted();
    metadataContext.ChangeTracker.Clear();
}
Console.WriteLine("Ensuring metadata database is created...");
metadataContext.Database.EnsureCreated();
metadataContext.Database.ExecuteSqlRaw(@"SET GLOBAL max_allowed_packet=268435456"); // 256MB

// Store schema version after creation
SchemaVersionManager.StoreSchemaVersion(metadataContext);

Console.WriteLine("Starting data ingestion...");

// Filter Xml results based on feature settings.
Filter.Load(xmlReader, locale, env);

// new TriggerGenerator(xmlReader).Generate();

var modelReaders = new List<PrefixedM2dReader> {
    new("/library/", Path.Combine(ms2Root, "Resource/Library.m2d")),
    new("/model/map/", Path.Combine(ms2Root, "Resource/Model/Map.m2d")),
    new("/model/effect/", Path.Combine(ms2Root, "Resource/Model/Effect.m2d")),
    new("/model/camera/", Path.Combine(ms2Root, "Resource/Model/Camera.m2d")),
    new("/model/tool/", Path.Combine(ms2Root, "Resource/Model/Tool.m2d")),
    new("/model/item/", Path.Combine(ms2Root, "Resource/Model/Item.m2d")),
    new("/model/npc/", Path.Combine(ms2Root, "Resource/Model/Npc.m2d")),
    new("/model/path/", Path.Combine(ms2Root, "Resource/Model/Path.m2d")),
    new("/model/character/", Path.Combine(ms2Root, "Resource/Model/Character.m2d")),
    new("/model/textures/", Path.Combine(ms2Root, "Resource/Model/Textures.m2d")),
};

UpdateDatabase(metadataContext, new ServerTableMapper(serverReader));
UpdateDatabase(metadataContext, new AiMapper(serverReader));

UpdateDatabase(metadataContext, new AdditionalEffectMapper(xmlReader));
UpdateDatabase(metadataContext, new AnimationMapper(xmlReader));
UpdateDatabase(metadataContext, new ItemMapper(xmlReader));
UpdateDatabase(metadataContext, new NpcMapper(xmlReader));
UpdateDatabase(metadataContext, new PetMapper(xmlReader));
UpdateDatabase(metadataContext, new MapMapper(xmlReader));
UpdateDatabase(metadataContext, new UgcMapMapper(xmlReader));
UpdateDatabase(metadataContext, new ExportedUgcMapMapper(xmlReader));
UpdateDatabase(metadataContext, new QuestMapper(xmlReader));
UpdateDatabase(metadataContext, new RideMapper(xmlReader));
UpdateDatabase(metadataContext, new ScriptMapper(xmlReader));
UpdateDatabase(metadataContext, new SkillMapper(xmlReader));
UpdateDatabase(metadataContext, new TableMapper(xmlReader));
UpdateDatabase(metadataContext, new AchievementMapper(xmlReader));
UpdateDatabase(metadataContext, new FunctionCubeMapper(xmlReader));

NifParserHelper.ParseNif(modelReaders);

UpdateDatabase(metadataContext, new NifMapper());
UpdateDatabase(metadataContext, new NxsMeshMapper());

var index = new FlatTypeIndex(exportedReader);

XBlockParser parser = new XBlockParser(exportedReader, index);

UpdateDatabase(metadataContext, new MapEntityMapper(metadataContext, parser));

MapDataMapper mapDataMapper = new MapDataMapper(metadataContext, parser);

UpdateDatabase(metadataContext, mapDataMapper);

mapDataMapper.ReportStats();

if (runNavmesh) {
    _ = new NavMeshMapper(metadataContext, exportedReader);
}

Console.WriteLine("Done!".ColorGreen());

void UpdateDatabase<T>(DbContext context, TypeMapper<T> mapper) where T : class {
    string? tableName = context.GetTableName<T>();
    Debug.Assert(!string.IsNullOrEmpty(tableName), $"Invalid table name: {tableName}");

    Console.Write($"Processing {tableName}... ");
    uint crc32C = mapper.Process();
    Console.Write($"Finished in {mapper.ElapsedMilliseconds}ms");
    Console.WriteLine();

    var checksum = context.Find<TableChecksum>(tableName);
    if (checksum != null) {
        if (checksum.Crc32C == crc32C) {
            Console.WriteLine($"Table {tableName} is up-to-date".ColorGreen());
            return;
        }

        checksum.Crc32C = crc32C;
        Console.WriteLine($"Table {tableName} outdated".ColorRed());
        int result = context.Database.ExecuteSqlRaw(@$"DELETE FROM `{tableName}`");
        Console.WriteLine($"Removed table {tableName} rows: {result}");
    }

    Stopwatch stopwatch = Stopwatch.StartNew();
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

    stopwatch.Stop();
    Console.WriteLine($"Wrote {mapper.Results.Count} entries to {tableName} in {stopwatch.ElapsedMilliseconds}ms");
}
