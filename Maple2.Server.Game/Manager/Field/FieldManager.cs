using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Database.Storage.Metadata;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Field;
using Maple2.Model.Game.Ugc;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.DebugGraphics;
using Maple2.Server.Game.Manager.Items;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util;
using Maple2.Tools.Extensions;
using Maple2.Tools.Scheduler;
using Serilog;

namespace Maple2.Server.Game.Manager.Field;

// FieldManager is instantiated by Autofac
// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class FieldManager : IDisposable {
    private static int _globalIdCounter = 10000000;
    private int localIdCounter = 50000000;
    private DateTime? fieldEmptySince;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public GameStorage GameStorage { get; init; } = null!;
    public ItemMetadataStorage ItemMetadata { get; init; } = null!;
    public MapMetadataStorage MapMetadata { get; init; } = null!;
    public MapDataStorage MapData { get; init; } = null!;
    public NpcMetadataStorage NpcMetadata { get; init; } = null!;
    public AiMetadataStorage AiMetadata { get; init; } = null!;
    public SkillMetadataStorage SkillMetadata { get; init; } = null!;
    public TableMetadataStorage TableMetadata { get; init; } = null!;
    public FunctionCubeMetadataStorage FunctionCubeMetadata { get; init; } = null!;
    public ServerTableMetadataStorage ServerTableMetadata { get; init; } = null!;
    public ItemStatsCalculator ItemStatsCalc { get; init; } = null!;
    public Lua.Lua Lua { get; init; } = null!;
    public Factory FieldFactory { get; init; } = null!;
    public IGraphicsContext DebugGraphicsContext { get; init; } = null!;
    // ReSharper restore All
    #endregion

    public readonly MapMetadata Metadata;
    public readonly MapEntityMetadata Entities;
    public readonly Navigation Navigation;
    public readonly PerformanceStageManager? PerformanceStage;
    public FieldAccelerationStructure? AccelerationStructure { get; private set; }
    private readonly UgcMapMetadata ugcMetadata;

    private readonly ConcurrentBag<SpawnPointNPC> npcSpawns = [];

    internal readonly EventQueue Scheduler;
    internal readonly FieldActor FieldActor;
    private readonly CancellationTokenSource cancel;
    private readonly Thread thread;
    private bool initialized;
    public bool Disposed { get; private set; }

    private readonly ILogger logger = Log.Logger.ForContext<FieldManager>();

    public ItemDropManager ItemDrop { get; }

    public int MapId => Metadata.Id;
    public readonly int InstanceId;
    public FieldInstance FieldInstance = FieldInstance.Default;
    public readonly AiManager Ai;
    public IFieldRenderer? DebugRenderer { get; private set; }

    public FieldManager(MapMetadata metadata, UgcMapMetadata ugcMetadata, MapEntityMetadata entities, NpcMetadataStorage npcMetadata, long ownerId = 0) {
        Metadata = metadata;
        this.ugcMetadata = ugcMetadata;
        this.Entities = entities;
        TriggerObjects = new TriggerCollection(entities);

        Scheduler = new EventQueue();
        FieldActor = new FieldActor(this, npcMetadata); // pulls from argument because member NpcMetadata is null here
        cancel = new CancellationTokenSource();
        thread = new Thread(UpdateLoop);
        Ai = new AiManager(this);
        OwnerId = ownerId;
        InstanceId = NextGlobalId();

        ItemDrop = new ItemDropManager(this);

        Navigation = new Navigation(metadata.XBlock);

        if (MapId is Constant.PerformanceMapId) {
            PerformanceStage = new PerformanceStageManager(this);
        }
    }

    // Init is separate from constructor to allow properties to be injected first.
    private void Init() {
        if (initialized) {
            return;
        }

        initialized = true;

        if (MapData.TryGet(Metadata.XBlock, out FieldAccelerationStructure? accelerationStructure)) {
            AccelerationStructure = accelerationStructure;
        } else {
            logger.Error("Failed to load acceleration structure for map {MapId}", MapId);
        }

        if (ServerTableMetadata.InstanceFieldTable.Entries.TryGetValue(Metadata.Id, out InstanceFieldMetadata? instanceField)) {
            FieldInstance = new FieldInstance(blockChangeChannel: true, instanceField.Type, instanceField.InstanceId);
        }

        if (ugcMetadata.Plots.Count > 0) {
            using GameStorage.Request db = GameStorage.Context();
            // Type 3 = 62000000_ugc and 62900000_ugd
            long plotOwnerId = Metadata.Property.Type == MapType.Home ? OwnerId : -1;
            foreach (Plot plot in db.LoadPlotsForMap(MapId, plotOwnerId)) {
                Plots[plot.Number] = plot;
            }
        }

        foreach (TriggerModel trigger in Entities.TriggerModels.Values) {
            AddTrigger(trigger);
        }
        foreach (Portal portal in Entities.Portals.Values) {
            SpawnPortal(portal);
        }

        if (MapId is Constant.DefaultHomeMapId) {
            List<PlotCube> cubePortals = Plots.FirstOrDefault().Value.Cubes.Values
                .Where(x => x.Interact?.PortalSettings is not null)
                .ToList();

            foreach (PlotCube cubePortal in cubePortals) {
                SpawnCubePortal(cubePortal);
            }

            List<PlotCube> lifeSkillCubes = Plots.FirstOrDefault().Value.Cubes.Values
                .Where(x => x.HousingCategory is HousingCategory.Ranching or HousingCategory.Farming)
                .ToList();

            foreach (PlotCube cube in lifeSkillCubes) {
                AddFieldFunctionInteract(cube);
            }
        }

        foreach ((Guid guid, BreakableActor breakable) in Entities.BreakableActors) {
            AddBreakable(guid.ToString("N"), breakable);
        }
        foreach ((Guid guid, Liftable liftable) in Entities.Liftables) {
            AddLiftable(guid.ToString("N"), liftable);
        }
        foreach ((Guid guid, InteractObject interact) in Entities.Interacts) {
            AddInteract(guid.ToString("N"), interact);
        }

        foreach (SpawnPointNPC spawnPointNpc in Entities.NpcSpawns) {
            if (spawnPointNpc.RegenCheckTime > 0) {
                npcSpawns.Add(spawnPointNpc);
            }

            if (spawnPointNpc.SpawnOnFieldCreate) {
                foreach (SpawnPointNPCListEntry spawn in spawnPointNpc.NpcList) {
                    if (!NpcMetadata.TryGet(spawn.NpcId, out NpcMetadata? npcMetadata)) {
                        logger.Warning("Npc {NpcId} failed to load for map {MapId}", spawn.NpcId, MapId);
                        continue;
                    }

                    for (int i = 0; i < spawn.Count; i++) {
                        SpawnNpc(npcMetadata, spawnPointNpc);
                    }
                }
            }
        }

        IList<MapMetadata> bonusMaps = MapMetadata.GetMapsByType(Metadata.Property.Continent, MapType.PocketRealm);
        foreach (MapMetadataSpawn spawn in Metadata.Spawns) {
            if (!Entities.RegionSpawns.TryGetValue(spawn.Id, out Ms2RegionSpawn? regionSpawn)) {
                continue;
            }

            var npcIds = new HashSet<int>();
            foreach (string tag in spawn.Tags) {
                if (NpcMetadata.TryLookupTag(tag, out IReadOnlyCollection<int>? tagNpcIds)) {
                    foreach (int tagNpcId in tagNpcIds) {
                        npcIds.Add(tagNpcId);
                    }
                }
            }

            if (npcIds.Count > 0 && spawn.Population > 0) {
                AddMobSpawn(spawn, regionSpawn, npcIds);
                continue;
            }

            if (spawn.Tags.Contains("보너스맵")) { // Bonus Map
                // Spawn a hat within a random range of 5 min to 8 hours
                int delay = Random.Shared.Next(1, 97) * (int) TimeSpan.FromMinutes(5).TotalMilliseconds;
                Scheduler.Schedule(() => SetBonusMapPortal(bonusMaps, regionSpawn), delay);
            }
        }

        foreach (Ms2RegionSkill regionSkill in Entities.RegionSkills) {
            if (!SkillMetadata.TryGet(regionSkill.SkillId, regionSkill.Level, out SkillMetadata? skill)) {
                continue;
            }

            AddSkill(skill, regionSkill.Interval, regionSkill.Position, regionSkill.Rotation);
        }

        foreach (BannerTable.Entry entry in TableMetadata.BannerTable.Entries) {
            if (entry.MapId != MapId) {
                continue;
            }

            using GameStorage.Request db = GameStorage.Context();

            FieldUgcBanner ugcBanner = new(this, entry.Id, MapId, db.FindBannerSlotsByBannerId(entry.Id));
            Banners[entry.Id] = ugcBanner;

            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;

            BannerSlot? slot = ugcBanner.Slots.FirstOrDefault(x => x.ActivateTime.Day == dateTimeOffset.Day && x.ActivateTime.Hour == dateTimeOffset.Hour);

            if (slot is null || slot.Expired || slot.Active) {
                continue;
            }

            slot.Active = true;
        }

        Scheduler.Start();
        thread.Start();
        DebugRenderer = DebugGraphicsContext.FieldAdded(this);
    }

    /// <summary>
    /// Generates an ObjectId unique across all map instances.
    /// </summary>
    /// <returns>Returns a globally unique ObjectId</returns>
    public static int NextGlobalId() => Interlocked.Increment(ref _globalIdCounter);

    /// <summary>
    /// Generates an ObjectId unique to this specific map instance.
    /// </summary>
    /// <returns>Returns a local ObjectId</returns>
    private int NextLocalId() => Interlocked.Increment(ref localIdCounter);

    // Use this to keep systems in sync. Do not use Environment.TickCount directly
    public long FieldTick { get; private set; }

    private void UpdateLoop() {
        while (!cancel.IsCancellationRequested) {
            if (!(DebugRenderer?.IsActive ?? false)) {
                Update();
            }

            // Environment.TickCount has ~16ms precision so sleep until next update
            Thread.Sleep(15);
        }
    }

    public void Update() {
        if (Players.IsEmpty) {
            return;
        }

        Scheduler.InvokeAll();

        FieldTick = Environment.TickCount64;
        foreach (FieldTrigger trigger in fieldTriggers.Values) trigger.Update(FieldTick);

        foreach (FieldPlayer player in Players.Values) player.Update(FieldTick);
        foreach (FieldNpc npc in Npcs.Values) npc.Update(FieldTick);
        foreach (FieldNpc mob in Mobs.Values) mob.Update(FieldTick);
        foreach (FieldPet pet in Pets.Values) pet.Update(FieldTick);
        foreach (FieldBreakable breakable in fieldBreakables.Values) breakable.Update(FieldTick);
        foreach (FieldLiftable liftable in fieldLiftables.Values) liftable.Update(FieldTick);
        foreach (FieldInteract interact in fieldInteracts.Values) interact.Update(FieldTick);
        foreach (FieldFunctionInteract interact in fieldFunctionInteracts.Values) interact.Update(FieldTick);
        foreach (FieldInteract interact in fieldAdBalloons.Values) interact.Update(FieldTick);
        foreach (FieldItem item in fieldItems.Values) item.Update(FieldTick);
        foreach (FieldMobSpawn mobSpawn in fieldMobSpawns.Values) mobSpawn.Update(FieldTick);
        foreach (FieldSkill skill in fieldSkills.Values) skill.Update(FieldTick);
        foreach (FieldPortal portal in fieldPortals.Values) portal.Update(FieldTick);
        UpdateBanners();

        RoomTimer?.Update(FieldTick);
    }

    public void EnsurePlayerPosition(FieldPlayer player) {
        if (Entities.BoundingBox.Contains(player.Position)) {
            return;
        }

        player.Session.Send(PortalPacket.MoveByPortal(player, player.LastGroundPosition.Align() + new Vector3(0, 0, 150f), default));
    }

    public bool TryGetPlayerById(long characterId, [NotNullWhen(true)] out FieldPlayer? player) {
        foreach (FieldPlayer fieldPlayer in Players.Values) {
            if (fieldPlayer.Value.Character.Id == characterId) {
                player = fieldPlayer;
                return true;
            }
        }

        player = null;
        return false;
    }

    public bool TryGetActor(int objectId, [NotNullWhen(true)] out IActor? actor) {
        if (Players.TryGetValue(objectId, out FieldPlayer? player)) {
            actor = player;
            return true;
        }

        if (Npcs.TryGetValue(objectId, out FieldNpc? npc)) {
            actor = npc;
            return true;
        }

        if (Mobs.TryGetValue(objectId, out FieldNpc? mob)) {
            actor = mob;
            return true;
        }

        if (Pets.TryGetValue(objectId, out FieldPet? pet)) {
            actor = pet;
            return true;
        }

        actor = null;

        return false;
    }

    public bool TryGetPlayer(int objectId, [NotNullWhen(true)] out FieldPlayer? player) {
        return Players.TryGetValue(objectId, out player);
    }

    public bool TryGetPlayer(string name, [NotNullWhen(true)] out FieldPlayer? player) {
        player = Players.Values.FirstOrDefault(p => p.Value.Character.Name == name);
        return player != null;
    }

    public bool TryGetPortal(int portalId, [NotNullWhen(true)] out FieldPortal? portal) {
        portal = fieldPortals.Values.FirstOrDefault(p => p.Value.Id == portalId);
        return portal != null;
    }

    public bool TryGetItem(int objectId, [NotNullWhen(true)] out FieldItem? fieldItem) {
        return fieldItems.TryGetValue(objectId, out fieldItem);
    }

    public bool TryGetBreakable(string entityId, [NotNullWhen(true)] out FieldBreakable? fieldBreakable) {
        return fieldBreakables.TryGetValue(entityId, out fieldBreakable);
    }

    public bool TryGetBreakable(int triggerId, [NotNullWhen(true)] out FieldBreakable? fieldBreakable) {
        return triggerBreakable.TryGetValue(triggerId, out fieldBreakable);
    }

    public bool TryGetLiftable(string entityId, [NotNullWhen(true)] out FieldLiftable? fieldLiftable) {
        return fieldLiftables.TryGetValue(entityId, out fieldLiftable);
    }

    public ICollection<FieldInteract> EnumerateInteract() => fieldInteracts.Values;
    public ICollection<FieldLiftable> EnumerateLiftables() => fieldLiftables.Values;
    public bool TryGetInteract(string entityId, [NotNullWhen(true)] out FieldInteract? fieldInteract) {
        return fieldInteracts.TryGetValue(entityId, out fieldInteract) || fieldAdBalloons.TryGetValue(entityId, out fieldInteract);
    }

    public bool MoveToPortal(GameSession session, int portalId) {
        if (!TryGetPortal(portalId, out FieldPortal? portal)) {
            return false;
        }

        session.Send(PortalPacket.MoveByPortal(session.Player, portal));
        return true;
    }

    public bool UsePortal(GameSession session, int portalId, string password) {
        if (!TryGetPortal(portalId, out FieldPortal? fieldPortal)) {
            return false;
        }

        if (!fieldPortal.Enabled) {
            session.Send(NoticePacket.MessageBox(new InterfaceText($"Cannot use disabled portal: {portalId}")));
            return false;
        }

        if (!string.IsNullOrEmpty(fieldPortal.Password) && password != fieldPortal.Password) {
            session.Send(NoticePacket.Message(StringCode.s_home_password_mismatch, NoticePacket.Flags.Alert));
            return false;
        }

        /* TODO: Remove portal once capacity is reached for Event portals
        if (fieldPortal.Value.Type == PortalType.Event) {
            RemovePortal(fieldPortal.ObjectId);
        }
        */

        // MoveByPortal (same map)
        Portal srcPortal = fieldPortal;
        if (srcPortal.Type is PortalType.InHome) {
            PlotCube? cubePortal = Plots.First().Value.Cubes.Values.FirstOrDefault(x => x.Interact?.PortalSettings is not null && x.Interact.PortalSettings.PortalObjectId == fieldPortal.ObjectId);
            if (cubePortal is null) {
                return false;
            }

            switch (cubePortal.Interact!.PortalSettings!.Destination) {
                case CubePortalDestination.PortalInHome:
                    PlotCube? destinationCube = Plots.First().Value.Cubes.Values.FirstOrDefault(x => x.Interact?.PortalSettings is not null && x.Interact.PortalSettings.PortalName == cubePortal.Interact.PortalSettings.DestinationTarget);
                    if (destinationCube is null) {
                        return false;
                    }

                    session.Player.MoveToPosition(destinationCube.Position, default);
                    return true;
                case CubePortalDestination.SelectedMap:
                    session.Send(session.PrepareField(srcPortal.TargetMapId, portalId: srcPortal.TargetPortalId)
                        ? FieldEnterPacket.Request(session.Player)
                        : FieldEnterPacket.Error(MigrationError.s_move_err_default));
                    return true;
                case CubePortalDestination.FriendHome: {
                        using GameStorage.Request db = session.GameStorage.Context();
                        Home? home = db.GetHome(fieldPortal.HomeId);
                        if (home is null) {
                            session.Send(FieldEnterPacket.Error(MigrationError.s_move_err_no_server));
                            return false;
                        }

                        session.MigrateToInstance(home.Indoor.MapId, home.Indoor.OwnerId);
                        return true;
                    }
            }
            return false;
        }

        if (srcPortal.TargetMapId == MapId) {
            if (TryGetPortal(srcPortal.TargetPortalId, out FieldPortal? dstPortal)) {
                session.Send(PortalPacket.MoveByPortal(session.Player, dstPortal));
            }

            return true;
        }

        if (srcPortal.TargetMapId == 0) {
            session.ReturnField();
            return true;
        }

        session.Send(session.PrepareField(srcPortal.TargetMapId, portalId: srcPortal.TargetPortalId)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(MigrationError.s_move_err_default));
        return true;
    }

    public bool LiftupCube(in Vector3B coordinates, [NotNullWhen(true)] out LiftupWeapon? liftupWeapon) {
        if (!Entities.ObjectWeapons.TryGetValue(coordinates, out ObjectWeapon? objectWeapon)) {
            liftupWeapon = null;
            return false;
        }

        int itemId = objectWeapon.ItemIds[Environment.TickCount % objectWeapon.ItemIds.Length];
        if (!ItemMetadata.TryGet(itemId, out ItemMetadata? item) || !(item.Skill?.WeaponId > 0)) {
            liftupWeapon = null;
            return false;
        }

        liftupWeapon = new LiftupWeapon(objectWeapon, itemId, item.Skill.WeaponId, item.Skill.WeaponLevel);

        if (objectWeapon.SpawnNpcId != 0 && Random.Shared.NextSingle() < objectWeapon.SpawnNpcRate) {
            if (NpcMetadata.TryGet(objectWeapon.SpawnNpcId, out NpcMetadata? metadata)) {
                FieldNpc? fieldNpc = SpawnNpc(metadata, objectWeapon.Position, objectWeapon.Rotation);
                if (fieldNpc != null) {
                    Broadcast(FieldPacket.AddNpc(fieldNpc));
                    Broadcast(ProxyObjectPacket.AddNpc(fieldNpc));
                }
            }
        }

        return true;
    }

    public void MovePlayerAlongPath(string pathName) {
        MS2PatrolData? patrolData = Entities.Patrols.FirstOrDefault(p => p.Name == pathName);
        if (patrolData is null) {
            return;
        }

        foreach (FieldPlayer player in Players.Values) {
            int dummyNpcId = player.Value.Character.Gender is Gender.Male ? Constant.DummyNpcMale : Constant.DummyNpcFemale;

            if (!NpcMetadata.TryGet(dummyNpcId, out NpcMetadata? npcMetadata)) {
                continue;
            }

            FieldNpc? dummyNpc = SpawnNpc(npcMetadata, player.Position, player.Rotation);
            if (dummyNpc is null) {
                continue;
            }
            Broadcast(FieldPacket.AddNpc(dummyNpc));
            Broadcast(ProxyObjectPacket.AddNpc(dummyNpc));

            dummyNpc.SetPatrolData(patrolData);
            dummyNpc.MovementState.CleanupPatrolData();
            player.Session.Send(FollowNpcPacket.FollowNpc(dummyNpc.ObjectId));
        }
    }

    #region DebugUtils
    public void BroadcastAiMessage(ByteWriter packet) {
        foreach ((int objectId, FieldPlayer player) in Players) {
            if (player.DebugAi) {
                player.Session.Send(packet);
            }
        }
    }

    public void BroadcastAiType(GameSession requester) {
        foreach ((int objectId, FieldNpc npc) in Npcs) {
            npc.SendDebugAiInfo(requester);
        }

        foreach ((int objectId, FieldPet npc) in Pets) {
            npc.SendDebugAiInfo(requester);
        }
    }
    #endregion

    public void Broadcast(ByteWriter packet, GameSession? sender = null) {
        if (!initialized) {
            return;
        }

        foreach (FieldPlayer fieldPlayer in Players.Values) {
            if (fieldPlayer.Session == sender) continue;
            fieldPlayer.Session.Send(packet);
        }
    }

    public void Dispose() {
        if (Disposed) {
            return;
        }

        logger.Debug("Disposing FieldManager {MapId}", MapId);

        DebugGraphicsContext.FieldRemoved(this);
        try {
            cancel.Cancel();
        } catch (Exception e) {
            logger.Error(e, "Failed to cancel FieldManager thread, cancel.Cancel() threw an exception. Disposed: {Disposed}", Disposed);
        }
        cancel.Dispose();
        thread.Join();
        Navigation.Dispose();

        Disposed = true;
        logger.Debug("Disposed FieldManager {MapId}", MapId);
    }
}
