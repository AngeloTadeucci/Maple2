using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Game.DebugGraphics;
using Maple2.Server.Game.Manager.Items;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util;
using Maple2.Tools.Collision;

namespace Maple2.Server.Game.Manager.Field;

public interface IField : IDisposable {
    public int RoomId { get; }
    public int DungeonId { get; }
    public bool Disposed { get; }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public GameStorage GameStorage { get; init; }
    public ItemMetadataStorage ItemMetadata { get; init; }
    public MapMetadataStorage MapMetadata { get; init; }
    public MapDataStorage MapData { get; init; }
    public NpcMetadataStorage NpcMetadata { get; init; }
    public AiMetadataStorage AiMetadata { get; init; }
    public SkillMetadataStorage SkillMetadata { get; init; }
    public TableMetadataStorage TableMetadata { get; init; }
    public FunctionCubeMetadataStorage FunctionCubeMetadata { get; init; }
    public ServerTableMetadataStorage ServerTableMetadata { get; init; }
    public ItemStatsCalculator ItemStatsCalc { get; init; }
    public Lua.Lua Lua { get; init; }
    public IGraphicsContext DebugGraphicsContext { get; init; }
    // ReSharper restore All
    #endregion

    public ItemDropManager ItemDrop { get; }

    public Navigation Navigation { get; }
    public MapEntityMetadata Entities { get; init; }

    public ConcurrentDictionary<int, FieldPlayer> Players { get; }
    public ConcurrentDictionary<int, FieldNpc> Npcs { get; }
    public ConcurrentDictionary<int, FieldNpc> Mobs { get; }
    public ConcurrentDictionary<int, FieldPet> Pets { get; }

    public RoomTimer? RoomTimer { get; }
    public InstanceFieldMetadata FieldInstance { get; }
    public FieldType FieldType { get; }
    public int Size { get; init; }

    public int MapId { get; init; }
    public long FieldTick { get; }
    public virtual void Init() { }

    /// <summary>
/// Adds a skill effect to the field at the specified position and rotation, with a given interval.
/// </summary>
/// <param name="metadata">Metadata describing the skill to add.</param>
/// <param name="interval">The interval, in milliseconds, for the skill's effect or duration.</param>
/// <param name="position">The world position where the skill effect is placed.</param>
/// <param name="rotation">The rotation of the skill effect. Defaults to no rotation if not specified.</param>
public void AddSkill(SkillMetadata metadata, int interval, in Vector3 position, in Vector3 rotation = default);
    /// <summary>
/// Adds a skill to the field using the specified skill record.
/// </summary>
/// <param name="record">The skill record containing information about the skill to add.</param>
public void AddSkill(SkillRecord record);
    /// <summary>
/// Adds a skill effect to the field, originating from the specified caster, using the given effect metadata, points, and rotation.
/// </summary>
/// <param name="caster">The actor casting the skill.</param>
/// <param name="effect">Metadata describing the skill effect to apply.</param>
/// <param name="points">An array of points defining the effect's area or trajectory.</param>
/// <param name="rotation">The rotation to apply to the skill effect. Defaults to no rotation.</param>
public void AddSkill(IActor caster, SkillEffectMetadata effect, Vector3[] points, in Vector3 rotation = default);
    /// <summary>
/// Returns a collection of actors within the specified prisms that match the given target type, up to the specified limit.
/// </summary>
/// <param name="prisms">Geometric prisms used to define the target search area.</param>
/// <param name="targetType">The type of targets to include (e.g., enemies, allies).</param>
/// <param name="limit">The maximum number of actors to return.</param>
/// <param name="ignore">Optional collection of actors to exclude from the results.</param>
/// <returns>An enumerable of actors matching the criteria.</returns>
public IEnumerable<IActor> GetTargets(Prism[] prisms, ApplyTargetType targetType, int limit, ICollection<IActor>? ignore = null);
    /// <summary>
/// Removes a skill effect or instance from the field by its object ID.
/// </summary>
/// <param name="objectId">The unique identifier of the skill to remove.</param>
public void RemoveSkill(int objectId);
    /// <summary>
/// Sends a packet to all players in the field, optionally excluding the specified sender.
/// </summary>
public void Broadcast(ByteWriter packet, GameSession? sender = null);
    /// <summary>
/// Broadcasts an AI-related message packet to all relevant entities in the field.
/// </summary>
public void BroadcastAiMessage(ByteWriter packet);
    public void BroadcastAiType(GameSession requester);

    public void SetRoomTimer(RoomTimerType type, int duration); //TODO: MOVE THIS TO RANDOM ONLY


    public FieldItem SpawnItem(IActor owner, Item item);
    public FieldItem SpawnItem(Vector3 position, Vector3 rotation, Item item, long characterId = 0, bool fixedPosition = false);
    public FieldItem SpawnItem(IFieldEntity owner, Vector3 position, Vector3 rotation, Item item, long characterId);

    public void EnsurePlayerPosition(FieldPlayer player);
    public bool TryGetPlayerById(long characterId, [NotNullWhen(true)] out FieldPlayer? player);
    public bool TryGetActor(int objectId, [NotNullWhen(true)] out IActor? actor);
    public bool TryGetPlayer(int objectId, [NotNullWhen(true)] out FieldPlayer? player);
    public bool TryGetPlayer(string name, [NotNullWhen(true)] out FieldPlayer? player);
    public bool TryGetPortal(int portalId, [NotNullWhen(true)] out FieldPortal? portal);
    public bool TryGetItem(int objectId, [NotNullWhen(true)] out FieldItem? fieldItem);
    public bool TryGetBreakable(string entityId, [NotNullWhen(true)] out FieldBreakable? fieldBreakable);
    public bool TryGetBreakable(int triggerId, [NotNullWhen(true)] out FieldBreakable? fieldBreakable);
    public bool TryGetLiftable(string entityId, [NotNullWhen(true)] out FieldLiftable? fieldLiftable);
    public ICollection<FieldInteract> EnumerateInteract();
    public ICollection<FieldLiftable> EnumerateLiftables();
    public bool TryGetInteract(string entityId, [NotNullWhen(true)] out FieldInteract? fieldInteract);
    public bool UsePortal(GameSession session, int portalId, string password);
    public bool LiftupCube(in Vector3B coordinates, [NotNullWhen(true)] out LiftupWeapon? liftupWeapon);
    public void MovePlayerAlongPath(string pathName);

    public bool RemoveNpc(int objectId, int removeDelay = 0);
    public bool RemovePet(int objectId, int removeDelay = 0);
}
