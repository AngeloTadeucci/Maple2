using System.Numerics;
using Maple2.PathEngine;
using Maple2.PathEngine.Exception;
using Maple2.PathEngine.Types;
using Maple2.Server.Game.Model;
using Serilog;

namespace Maple2.Server.Game.Manager.Field;

public sealed class AgentNavigation : IDisposable {
    private static readonly ILogger Logger = Log.Logger.ForContext<AgentNavigation>();

    private readonly FieldNpc npc;
    private readonly Agent agent;
    private readonly Mesh mesh;
    private readonly CollisionContext context;

    private PathEngine.Path? currentPath;

    public AgentNavigation(FieldNpc npc, Agent agent, Mesh mesh, CollisionContext context) {
        this.npc = npc;
        this.agent = agent;
        this.mesh = mesh;
        this.context = context;

        context.temporarilyIgnoreAgent(agent);
    }

    public void Dispose() {
        // mesh+context are disposed by Navigation
        currentPath?.Dispose();
        currentPath = null;
        context.restoreTemporarilyIgnoredAgent(agent);
        context.removeAgent(agent);
    }

    public bool HasPath => currentPath != null && currentPath.size() >= 2;

    public bool UpdatePosition() {
        Position position = mesh.positionNear3DPoint((int) npc.Position.X, (int) npc.Position.Y, (int) npc.Position.Z, horizontalRange: 500, verticalRange: 50);
        if (!mesh.positionIsValid(position)) {
            Logger.Error("Failed to find valid position from {Source} => {Position}", npc.Position, position);
            return false;
        }

        agent.moveTo(position);
        npc.Position = FromPosition(position);
        return true;
    }

    public Vector3 GetAgentPosition() {
        return FromPosition(agent.getPosition());
    }

    public (Vector3 Start, Vector3 End) Advance(TimeSpan timeSpan, float speed, out bool followSegment) {
        followSegment = false;

        if (currentPath == null || currentPath.size() < 2) {
            return default;
        }

        Position startPosition = agent.getPosition();
        Vector3 start = FromPosition(startPosition);
        float distance = (float) timeSpan.TotalSeconds * speed;
        using CollisionInfo? info = agent.advanceAlongPath(currentPath, distance, null);
        Vector3 end = FromPosition(agent.getPosition());

        // TODO: Requires jump to reach next position.
        if (Math.Abs(start.Z - end.Z) > 75) {
            agent.moveTo(startPosition); // Revert agent position
            currentPath.Dispose();
            currentPath = null;
            return default;
        }

        if (info != null || currentPath.getLength() < 25) {
            currentPath.Dispose();
            currentPath = null;
        }

        followSegment = true;

        return (start, end);
    }

    public Vector3 GetRandomPatrolPoint() {
        Position origin = ToPosition(npc.Origin);
        if (!mesh.positionIsValid(origin)) {
            return npc.Position;
        }

        if (npc.Value.Metadata.Action.MoveArea == 0) {
            return FromPosition(origin);
        }

        Position end = mesh.generateRandomPositionLocally(origin, npc.Value.Metadata.Action.MoveArea);

        return FromPosition(end);
    }

    public bool RandomPatrol() {
        // Npc cannot move?
        if (npc.Value.Metadata.Action.MoveArea == 0) {
            return false;
        }

        if (!mesh.positionIsValid(agent.getPosition())) {
            return false;
        }

        Position origin = ToPosition(npc.Origin);
        if (!mesh.positionIsValid(origin)) {
            return false;
        }

        Position end = mesh.generateRandomPositionLocally(origin, npc.Value.Metadata.Action.MoveArea);
        return SetPathTo(end);
    }

    public Vector3 FindClosestPoint(Vector3 point, int maxDistance) {
        return FromPosition(mesh.findClosestUnobstructedPosition(agent.getShape(), context, ToPosition(point), maxDistance));
    }

    public bool PathTo(Vector3 goal) {
        if (!mesh.positionIsValid(agent.getPosition())) {
            return false;
        }

        Position end = ToPosition(goal);
        if (!mesh.positionIsValid(end)) {
            return false;
        }

        return SetPathTo(end);
    }

    private bool SetPathTo(Position target) {
        currentPath?.Dispose();
        currentPath = null;
        try {
            currentPath = agent.findShortestPathTo(context, target);
        } catch (PathEngineException) { /* ignored */ }

        return currentPath != null;
    }

    public bool PathAway(Vector3 goal, int distance) {
        if (!mesh.positionIsValid(agent.getPosition())) {
            return false;
        }

        Position end = ToPosition(goal);
        if (!mesh.positionIsValid(end)) {
            return false;
        }

        return SetPathAway(end, distance);
    }

    private bool SetPathAway(Position target, int distance) {
        currentPath?.Dispose();
        currentPath = null;
        try {
            currentPath = agent.findPathAway(context, target, distance);
        } catch (PathEngineException) { /* ignored */ }

        return currentPath != null;
    }

    #region Conversion
    private Position ToPosition(Vector3 vector) {
        return mesh.positionNear3DPoint((int) vector.X, (int) vector.Y, (int) vector.Z, horizontalRange: 25, verticalRange: 5);
    }

    private Vector3 FromPosition(Position position) {
        if (!mesh.positionIsValid(position)) {
            return default;
        }

        float z = mesh.heightAtPositionF(position);
        return new Vector3(position.X, position.Y, z);
    }
    #endregion
}
