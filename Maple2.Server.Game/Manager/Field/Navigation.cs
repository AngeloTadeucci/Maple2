using System.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using DotRecast.Detour.Io;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Builder;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Tools;
using Maple2.Tools.DotRecast;
using Serilog;

namespace Maple2.Server.Game.Manager.Field;

public sealed class Navigation : IDisposable {
    private static readonly ILogger Logger = Log.Logger.ForContext<Navigation>();

    public readonly string Name;
    private readonly DtNavMesh navMesh;
    private readonly DtNavMeshQuery navMeshQuery;
    public DtCrowd Crowd { get; private set; }
    private readonly DtCrowdAgentConfig crowdAgentConfig = new DtCrowdAgentConfig();

    public Navigation(string name) {
        Name = name;
        navMesh = LoadNavMesh();
        navMeshQuery = new DtNavMeshQuery(navMesh);

        Crowd = new DtCrowd(new DtCrowdConfig(maxAgentRadius: 0.3f), navMesh, __ => new DtQueryDefaultFilter(
            SampleAreaModifications.SAMPLE_POLYFLAGS_ALL,
            SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED,
            [1f, 10f, 1f, 1f, 2f, 1.5f]) // TODO: understand what actually these values are
        );
    }

    private DtNavMesh LoadNavMesh() {
        FileStream fs = new FileStream(System.IO.Path.Combine(Paths.NAVMESH_DIR, $"{Name}.navmesh"), FileMode.Open, FileAccess.Read);
        BinaryReader br = new BinaryReader(fs);
        DtMeshSetReader reader = new DtMeshSetReader();

        DtNavMesh dtNavMesh = reader.Read(br, DotRecastHelper.VERTS_PER_POLY);
        br.Close();
        fs.Close();
        return dtNavMesh;
    }

    public AgentNavigation ForAgent(FieldNpc npc, DtCrowdAgent agent) {
        return new AgentNavigation(npc, agent, Crowd);
    }

    public DtCrowdAgent AddAgent(NpcMetadata metadata, Vector3 origin) {
        RcNavMeshBuildSettings settings = DotRecastHelper.NavMeshBuildSettings;
        // use metadata speed instead of settings?
        DtCrowdAgentParams agentParams = CreateAgentParams(0.3f, 1.4f, settings.agentMaxAcceleration, settings.agentMaxSpeed);
        return Crowd.AddAgent(DotRecastHelper.ToNavMeshSpace(origin), agentParams);
    }

    private DtCrowdAgentParams CreateAgentParams(float agentRadius, float agentHeight, float agentMaxAcceleration, float agentMaxSpeed) {
        DtCrowdAgentParams ap = new() {
            radius = agentRadius,
            height = agentHeight,
            maxAcceleration = agentMaxAcceleration,
            maxSpeed = agentMaxSpeed,
            updateFlags = crowdAgentConfig.GetUpdateFlags(),
            obstacleAvoidanceType = crowdAgentConfig.obstacleAvoidanceType,
            separationWeight = crowdAgentConfig.separationWeight
        };
        ap.collisionQueryRange = ap.radius * 12.0f;
        ap.pathOptimizationRange = ap.radius * 30.0f;
        return ap;
    }

    public void Dispose() {

    }
}
