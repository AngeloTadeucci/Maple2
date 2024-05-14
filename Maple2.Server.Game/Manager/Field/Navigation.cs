using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Maple2.Model.Metadata;
using Maple2.PathEngine;
using Maple2.PathEngine.Interface;
using Maple2.PathEngine.Types;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Util;
using Serilog;

namespace Maple2.Server.Game.Manager.Field;

public sealed class Navigation : IDisposable {
    private static readonly ILogger Logger = Log.Logger.ForContext<Navigation>();

    // ErrorHandler must be initialized here to retain ownership.
    private static readonly LogErrorHandler ErrorHandler = new(Logger);
    private static readonly PathEngine.PathEngine PathEngine = new(ErrorHandler);

    public readonly string Name;
    private readonly Mesh mesh;
    private readonly CollisionContext context;

    private readonly ConcurrentDictionary<(int, int), Shape> shapeCache = new();

    public Navigation(string name, byte[]? data = null) {
        Name = name;
        if (data == null) {
            Logger.Error("No navigation mesh for: {XBlock}", name);
            mesh = PathEngine.buildMeshFromContent(Array.Empty<IFaceVertexMesh>());
        } else {
            mesh = PathEngine.loadMeshFromBuffer(FileFormat.tok, data);
        }
        context = mesh.newContext();
    }

    public AgentNavigation ForAgent(FieldNpc npc, Agent agent) {
        return new AgentNavigation(npc, agent, mesh, context);
    }

    public Agent? AddAgent(NpcMetadata metadata, Vector3 origin) {
        // Using radius for width for now
        Shape shape = GetShape((int) metadata.Property.Capsule.Radius, (int) metadata.Property.Capsule.Height);
        if (!TryFindPosition(shape, ToPosition(origin), metadata.Action.MoveArea, out Position? position)) {
            return null;
        }

        Agent agent = mesh.placeAgent(shape, (Position) position);
        context.addAgent(agent);
        return agent;
    }

    private bool TryFindPosition(Shape? shape, Position origin, int distance, [NotNullWhen(true)] out Position? position) {
        position = origin;
        if (!mesh.positionIsValid(origin)) {
            return false;
        }

        if (distance > 0) {
            try {
                // Unobstructed Position is required for pathfinding, attempt to find one.
                position = mesh.findClosestUnobstructedPosition(shape, context, (Position) position, distance);
            } catch { /* ignored */ }
        }

        if (!mesh.positionIsValid((Position) position)) {
            return false;
        }

        return true;
    }

    public Position ToPosition(Vector3 vector) {
        return mesh.positionNear3DPoint((int) vector.X, (int) vector.Y, (int) vector.Z, horizontalRange: 25, verticalRange: 5);
    }

    public Vector3 FromPosition(Position position) {
        if (!mesh.positionIsValid(position)) {
            return default;
        }

        float z = mesh.heightAtPositionF(position);
        return new Vector3(position.X, position.Y, z);
    }

    private Shape GetShape(int width, int height) {
        if (shapeCache.TryGetValue((width, height), out Shape? shape)) {
            return shape;
        }

        int halfWidth = Math.Max(width / 2, 1);
        int halfHeight = Math.Max(height / 2, 1);
        List<Point> vertices = [
            new Point(-halfWidth, -halfHeight),
            new Point(-halfWidth, halfHeight),
            new Point(halfWidth, halfHeight),
            new Point(halfWidth, -halfHeight),
        ];

        shape = PathEngine.newShape(vertices);
        mesh.generateUnobstructedSpaceFor(shape, true);
        mesh.generatePathfindPreprocessFor(shape);
        shapeCache.TryAdd((width, height), shape);
        return shape;
    }

    public void Dispose() {
        mesh.Dispose();
    }
}
