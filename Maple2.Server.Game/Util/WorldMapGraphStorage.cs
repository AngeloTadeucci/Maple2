using DijkstraAlgorithm.Graphing;
using DijkstraAlgorithm.Pathing;
using IronPython.Runtime;
using Maple2.Database.Storage;
using Maple2.Model.Metadata;
using Serilog;

namespace Maple2.Server.Game.Util;

public sealed class WorldMapGraphStorage {
    private readonly Graph worldMapGraph;
    private readonly MapMetadataStorage mapMetadata;

    public WorldMapGraphStorage(TableMetadataStorage TableMetadata, MapMetadataStorage MapMetadata) {
        mapMetadata = MapMetadata;
        GraphBuilder builder = new();

        // first add all the nodes
        List<WorldMapTable.Map> maps = TableMetadata.WorldMapTable.Entries;

        foreach (var entry in maps) {
            builder.AddNode(entry.Code.ToString());
        }

        // then add all the edges
        for (int i = 0; i < maps.Count; i++) {
            for (int j = i + 1; j < maps.Count; j++) {
                if (maps[i].Code == maps[j].Code) {
                    continue;
                }

                if (AreNeighbors(maps[i], maps[j])) {
                    builder.AddBidirectionalLink(maps[i].Code.ToString(), maps[j].Code.ToString(), 1);
                }
            }
        }

        worldMapGraph = builder.Build();
    }


    private bool AreNeighbors(WorldMapTable.Map a, WorldMapTable.Map b) {
        mapMetadata.TryGet(a.Code, out var aMetadata);
        mapMetadata.TryGet(b.Code, out var bMetadata);
        if (aMetadata is null || bMetadata is null) {
            return false;
        }

        // Check if the maps are on the same continent
        if (aMetadata.Property.Continent != bMetadata.Property.Continent) {
            return false;
        }

        // Check if z levels are adjacent
        bool sameZLevel = a.Z == b.Z || Math.Abs(a.Z - b.Z) == 1;

        // Define the boundaries of map a
        int aLeft = a.X;
        int aRight = a.X + a.Size - 1;
        int aTop = a.Y;
        int aBottom = a.Y + a.Size - 1;

        // Define the boundaries of map b
        int bLeft = b.X;
        int bRight = b.X + b.Size - 1;
        int bTop = b.Y;
        int bBottom = b.Y + b.Size - 1;

        // Check for horizontal adjacency
        bool horizontalAdjacent = (aLeft <= bRight + 1 && aRight >= bLeft - 1) &&
                                  (aTop <= bBottom && aBottom >= bTop);

        // Check for vertical adjacency
        bool verticalAdjacent = (aTop <= bBottom + 1 && aBottom >= bTop - 1) &&
                                (aLeft <= bRight && aRight >= bLeft);

        return sameZLevel && (horizontalAdjacent || verticalAdjacent);
    }

    /// <summary>
    /// Returns if the pathfinder is able to find a path to the given destination.
    /// </summary>
    /// <returns>The count of maps between the origin and destination.</returns>
    public bool CanPathFind(int mapOrigin, int mapDestination, out int mapCount) {
        mapCount = 0;
        PathFinder pathFinder = new(worldMapGraph);
        Node? originNode = worldMapGraph.Nodes.FirstOrDefault(x => x.Id == mapOrigin.ToString());
        Node? destinationNode = worldMapGraph.Nodes.FirstOrDefault(x => x.Id == mapDestination.ToString());

        if (originNode == default && destinationNode == default) {
            return false;
        }

        try {
            DijkstraAlgorithm.Pathing.Path path = pathFinder.FindShortestPath(originNode, destinationNode);
            if (path == null) {
                return false;
            }

            mapCount = path.Segments.Count;
            return true;
        } catch (Exception ex) {
            Log.Error(ex, "Error while trying to find a path between {Origin} and {Destination}", mapOrigin, mapDestination);
            return false;
        }
    }
}
