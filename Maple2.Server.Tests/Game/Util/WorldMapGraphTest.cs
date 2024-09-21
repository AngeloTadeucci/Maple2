using System;
using System.Globalization;
using Maple2.Database.Context;
using Maple2.Database.Storage;
using Maple2.Server.Game.Util;
using Maple2.Tools;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Server.Tests.Game.Util;

public class WorldMapGraphStorageTest {
    private WorldMapGraphStorage worldMapGraphStorage = null!;

    [OneTimeSetUp]
    public void ClassInitialize() {
        // Force Globalization to en-US because we use periods instead of commas for decimals
        CultureInfo.CurrentCulture = new("en-US");

        DotEnv.Load();

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

        using var context = new MetadataContext(options);
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var tableMetadataStorage = new TableMetadataStorage(context);
        var mapMetadataStorage = new MapMetadataStorage(context);
        worldMapGraphStorage = new WorldMapGraphStorage(tableMetadataStorage, mapMetadataStorage);
    }

    [Test]
    public void CanPathFindTriaToLithHarbor() {
        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000062;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(6));
    }

    [Test]
    public void CanPathFindTriaToEllinia() {
        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000023;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(5));
    }

    [Test]
    public void CanPathFindTriaToKerning() {
        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000100;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(7));
    }

    [Test]
    public void CanPathFindTriaToTaliskar() {
        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000270;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(9));
    }

    [Test]
    public void CanPathFindTriaToPerion() {
        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000051;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(10));
    }

    [Test]
    public void CanPathFindTriaToHenesys() {
        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000076;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(6));
    }

    [Test]
    public void CanPathFindTriaToIglooHill() {
        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2000264;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(17));
    }

    [Test]
    public void CanPathFindLithHarborToCocoIsland() {
        // Arrange
        int mapOrigin = 2000062;
        int mapDestination = 2000377;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(15));
    }

    [Test]
    public void CantPathFindTriaToLudari() {
        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2010002;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(mapCount, Is.EqualTo(0));
    }

    [Test]
    public void CanPathFindLudariToMoonlightDesert() {
        // Arrange
        int mapOrigin = 2010002;
        int mapDestination = 2010033;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(7));
    }

    [Test]
    public void CanPathFindRizabIslandToMinar() {
        // Arrange
        int mapOrigin = 2010043;
        int mapDestination = 2010063;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(8));
    }

    [Test]
    public void CantPathFindTriaToSafehold() {
        // Arrange
        int mapOrigin = 2000001;
        int mapDestination = 2020041;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(mapCount, Is.EqualTo(0));
    }

    [Test]
    public void CantPathFindSafeholdToLudari() {
        // Arrange
        int mapOrigin = 2020041;
        int mapDestination = 2010002;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(mapCount, Is.EqualTo(0));
    }

    [Test]
    public void CanPathFindSafeholdToForainForest() {
        // Arrange
        int mapOrigin = 2020041;
        int mapDestination = 2020029;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(4));
    }

    [Test]
    public void CanPathFindAuroraLakeToForainForest() {
        // Arrange
        int mapOrigin = 2020001;
        int mapDestination = 2020029;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(6));
    }

    [Test]
    public void CanPathFindSafeholdToTairenRobotFactory() {
        // Arrange
        int mapOrigin = 2020041;
        int mapDestination = 2020035;

        // Act
        bool result = worldMapGraphStorage.CanPathFind(mapOrigin, mapDestination, out int mapCount);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(mapCount, Is.EqualTo(5));
    }
}
