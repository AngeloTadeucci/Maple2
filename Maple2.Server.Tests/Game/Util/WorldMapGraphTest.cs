using System;
using System.Globalization;
using Maple2.Database.Context;
using Maple2.Database.Storage;
using Maple2.Server.Game.Util;
using Maple2.Tools;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Server.Tests.Game.Util;

public class WorldMapGraphStorageTest {
    private WorldMapGraphStorage? worldMapGraphStorage;

    private static bool IsDatabaseConfigured() {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_IP")) &&
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_PORT")) &&
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATA_DB_NAME")) &&
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_USER")) &&
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_PASSWORD"));
    }

    private void AssumeDatabaseIsConfigured() {
        if (!IsDatabaseConfigured()) {
            Assert.Ignore("Database configuration not found - skipping test");
        }

        Assert.That(worldMapGraphStorage, Is.Not.Null, "Database setup failed");
    }

    [OneTimeSetUp]
    public void ClassInitialize() {
        // Force Globalization to en-US because we use periods instead of commas for decimals
        CultureInfo.CurrentCulture = new("en-US");

        DotEnv.Load();

        // Skip setup if database not configured
        if (!IsDatabaseConfigured()) {
            Assert.Ignore("Database configuration not found - skipping database tests");
            return;
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
        DbContextOptions options = new DbContextOptionsBuilder()
            .UseMySql(dataDbConnection, ServerVersion.AutoDetect(dataDbConnection)).Options;

        var context = new MetadataContext(options);
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var tableMetadataStorage = new TableMetadataStorage(context);
        var mapMetadataStorage = new MapMetadataStorage(context);
        worldMapGraphStorage = new WorldMapGraphStorage(tableMetadataStorage, mapMetadataStorage);
    }

    [Test]
    public void CanPathFindTriaToLithHarbor() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000062;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(6));
    }

    [Test]
    public void CanPathFindTriaToEllinia() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000023;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(5));
    }

    [Test]
    public void CanPathFindTriaToKerning() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000100;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(7));
    }

    [Test]
    public void CanPathFindTriaToTaliskar() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000270;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(9));
    }

    [Test]
    public void CanPathFindTriaToPerion() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000051;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(10));
    }

    [Test]
    public void CanPathFindTriaToHenesys() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000076;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(6));
    }

    [Test]
    public void CanPathFindTriaToIglooHill() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000264;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(17));
    }

    [Test]
    public void CanPathFindLithHarborToCocoIsland() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2000062;
        int mapDestination = 2000377;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(15));
    }

    [Test]
    public void CantPathFindTriaToLudari() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2010002;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(mapCount, Is.EqualTo(0));
    }

    [Test]
    public void CanPathFindLudariToMoonlightDesert() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2010002;
        int mapDestination = 2010033;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(7));
    }

    [Test]
    public void CanPathFindRizabIslandToMinar() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2010043;
        int mapDestination = 2010063;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(8));
    }

    [Test]
    public void CantPathFindTriaToSafehold() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2020041;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(mapCount, Is.EqualTo(0));
    }

    [Test]
    public void CantPathFindSafeholdToLudari() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2020041;
        int mapDestination = 2010002;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(mapCount, Is.EqualTo(0));
    }

    [Test]
    public void CanPathFindSafeholdToForainForest() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2020041;
        int mapDestination = 2020029;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(4));
    }

    [Test]
    public void CanPathFindAuroraLakeToForainForest() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2020001;
        int mapDestination = 2020029;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(6));
    }

    [Test]
    public void CanPathFindSafeholdToTairenRobotFactory() {
        AssumeDatabaseIsConfigured();

        // Arrange
        int mapOrigin = 2020041;
        int mapDestination = 2020035;

        // Act
        bool result = worldMapGraphStorage!.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(5));
    }
}
