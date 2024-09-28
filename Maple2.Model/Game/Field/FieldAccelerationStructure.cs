using Maple2.Model.Common;
using Maple2.Model.Metadata.FieldEntities;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;
using Maple2.Tools.VectorMath;
using System.Collections.ObjectModel;
using System.Numerics;

namespace Maple2.Model.Game.Field;

internal enum FieldEntityMembers : byte {
    None = 0x0,
    Id = 0x1,
    Position = 0x2,
    Rotation = 0x4,
    Scale = 0x8,
    Bounds = 0x10,
    Llid = 0x20
}

/*
 * FieldAccelerationStructure is a helper class that specializes in various spatial queries for objects.
 * It's designed to maximize performance for finding objects in a 3D space with desired constraints.
 * 
 * Internally it uses specialized storage techniques with fast look up times, but knowledge on these
 * techniques isn't necessary for use. The Query functions allow you to find the objects you need without
 * knowing any of the implementation details.
 * 
 * You can do general queries or queries for specific entity types for all types of queries available.
 * Every entity matching the query constraints will be reported back through a callback, or in a list with
 * Query___List() methods.
 * 
 * Available query types include:
 * - Cell: Captures all entities in a specific grid cell, or range of cells. Doesn't capture freely floating entities.
 * - Point: Captures all entities intersecting with a specific point.
 * - CellAtPoint: Captures all entities in the grid cell intersecting with a specific point. Doesn't capture freely floating entities.
 * - BoundingBox: Captures all entities intersecting with a bounding box.
 * - CellsInBoundingBox: Captures all entities in grid cells intersecting with a bounding box. Doesn't capture freely floating entities.
 * - Sphere: Captures all entities intersecting with a sphere.
 * - CellsInSphere: Captures all entities in grid cells intersecting with a sphere. Doesn't capture freely floating entities.
 * - Ray: Captures all entities intersecting with a ray. Object results may not be in order.
 * - CellsOnRay: Captures all entities intersecting with a ray. Object results may not be in order. Doesn't capture freely floating entities.
 * - RayCast: Captures all boxes & meshes intersecting with a ray in order until false is returned by the callback.
 * - CellRayCast: Captures all boxes & meshes intersecting with a ray in order until -1 is returned by the callback. Doesn't capture freely floating entities.
 * 
 * Special purpose queries:
 * - Spawns: Captures all mob spawn candidates in a sphere.
 * - Fluids: Captures all fluids within a bounding box.
 * - VibrateObjects: Captures all vibrate objects within a bounding box.
*/
public class FieldAccelerationStructure : IByteSerializable, IByteDeserializable {
    public const int AXIS_TRIM_ENTITY_COUNT = 10;

    public Vector3S GridSize { get; private set; } = new Vector3S();
    public Vector3S MinIndex { get; private set; } = new Vector3S();
    public Vector3S MaxIndex { get; private set; } = new Vector3S();

    public ReadOnlyCollection<FieldEntity> AlignedEntities { get => alignedEntities.AsReadOnly(); }
    public ReadOnlyCollection<FieldEntity> UnalignedEntities { get => unalignedEntities.AsReadOnly(); }

    public List<FieldEntity> alignedEntities;
    public List<FieldEntity> unalignedEntities; // TODO: add AABB tree implementation for querying unaligned objects
    private int[,,] cellGrid;

    public ulong GridBytesWritten { get; private set; } = 0;

    public FieldAccelerationStructure() {
        alignedEntities = new();
        unalignedEntities = new();
        cellGrid = new int[0, 0, 0];
    }

    #region QueryApi


    public void QueryCell(Vector3 point, Action<FieldEntity> callback) {

    }

    public void QueryCells

    #endregion

    #region Initialization

    // used to guarantee deterministic output when parsing maps
    private void SortEntityList(List<FieldEntity> entityList) {
        if (entityList.Count <= 1) {
            return;
        }

        entityList.Sort((entity1, entity2) => {
            int comparison = entity1.Id.High.CompareTo(entity2.Id.High);

            if (comparison == 0) {
                return entity1.Id.Low.CompareTo(entity2.Id.Low);
            }

            return comparison;
        });
    }

    private void GenerateSpawnLocations(Dictionary<Vector3S, List<FieldEntity>> gridAlignedEntities, Vector3S minIndex, Vector3S maxIndex, List<FieldEntity> unalignedEntities) {
        Dictionary<Vector3S, (bool isOccupied, bool isGround)> occupancyMap = new();
        List<(Vector3S index, FieldSpawnTile tile)> spawnTiles = new();

        foreach ((Vector3S coord, List<FieldEntity> entityList) in gridAlignedEntities) {
            if (!occupancyMap.TryGetValue(coord, out (bool isOccupied, bool isGround) occupancy)) {
                occupancy = (false, false);
            }

            foreach (FieldEntity entity in entityList) {
                if (entity is FieldBoxColliderEntity boxEntity && !boxEntity.IsWhiteBox) {
                    occupancyMap[coord] = (occupancy.isOccupied, true);

                    continue;
                }

                if (entity is FieldVibrateEntity) {
                    continue;
                }

                occupancyMap[coord] = (occupancy.isOccupied, true);
            }
        }

        foreach (FieldEntity entity in unalignedEntities) {
            Vector3 minPosition = (1 / 150.0f) * entity.Bounds.Min;
            Vector3 maxPosition = (1 / 150.0f) * entity.Bounds.Max;
            Vector3S minCubeIndex = new Vector3S((short) Math.Floor(minPosition.X + 0.5f), (short) Math.Floor(minPosition.Y + 0.5f), (short) Math.Floor(minPosition.Z + 0.5f));
            Vector3S maxCubeIndex = new Vector3S((short) Math.Floor(maxPosition.X + 0.5f), (short) Math.Floor(maxPosition.Y + 0.5f), (short) Math.Floor(maxPosition.Z + 0.5f));

            for (short x = minCubeIndex.X;  x <= maxCubeIndex.X; x++) {
                for (short y = minCubeIndex.Y; y <= maxCubeIndex.Y; y++) {
                    for (short z = minCubeIndex.Z; z <= maxCubeIndex.Z; z++) {
                        Vector3S coord = new Vector3S(x, y, z);

                        if (!occupancyMap.TryGetValue(coord, out (bool isOccupied, bool isGround) occupancy)) {
                            occupancy = (false, false);
                        }

                        occupancyMap[coord] = (true, occupancy.isGround);
                    }
                }
            }
        }

        for (short x = minIndex.X; x <= maxIndex.X; x++) {
            for (short y = minIndex.Y; y <= maxIndex.Y; y++) {
                for (short z = (short) (minIndex.Z + 1); z <= maxIndex.Z; z++) {
                    Vector3S coord = new Vector3S(x, y, z);
                    Vector3S groundCoord = new Vector3S(x, y, (short) (z - 1));

                    if (!occupancyMap.TryGetValue(coord, out (bool isOccupied, bool isGround) occupancy)) {
                        occupancy = (false, false);
                    }

                    if (!occupancyMap.TryGetValue(groundCoord, out (bool isOccupied, bool isGround) groundOccupancy)) {
                        groundOccupancy = (false, false);
                    }

                    if (!occupancy.isGround && !occupancy.isOccupied && groundOccupancy.isGround) {
                        if (!gridAlignedEntities.TryGetValue(coord, out List<FieldEntity>? entities)) {
                            entities = new();

                            gridAlignedEntities.Add(coord, entities);
                        }

                        Vector3 cellPosition = 150.0f * coord.Vector3;
                        BoundingBox3 bounds = new BoundingBox3(cellPosition - new Vector3(75, 75, 0), cellPosition + new Vector3(75, 75, 150));

                        entities.Add(new FieldSpawnTile(
                            Id: new FieldEntityId(0, 0),
                            Position: cellPosition,
                            Rotation: new Vector3(0, 0, 0),
                            Scale: 1,
                            Bounds: bounds));
                    }
                }
            }
        }
    }

    public void TrimGridSize(Dictionary<Vector3S, List<FieldEntity>> gridAlignedEntities, ref Vector3S minIndex, ref Vector3S maxIndex, List<FieldEntity> unalignedEntities) {
        Vector3S gridSize = maxIndex - minIndex + new Vector3S(1, 1, 1);
        int cellCount = gridSize.X * gridSize.Y * (gridSize.Z - 1);
        int[] axisXCount = new int[gridSize.X];
        int[] axisYCount = new int[gridSize.Y];
        int[] axisZCount = new int[gridSize.Z];

        foreach ((Vector3S coord, List<FieldEntity> entityList) in gridAlignedEntities) {
            Vector3S index = coord - minIndex;

            axisXCount[index.X] += entityList.Count;
            axisYCount[index.Y] += entityList.Count;
            axisZCount[index.Z] += entityList.Count;
        }

        int trimmedCount = 0;
        short trimmedMinX = 0;
        short trimmedMinY = 0;
        short trimmedMinZ = 0;
        short trimmedMaxX = (short) (maxIndex.X - minIndex.X);
        short trimmedMaxY = (short) (maxIndex.Y - minIndex.Y);
        short trimmedMaxZ = (short) (maxIndex.Z - minIndex.Z);

        for (int i = 0, cumulative = 0; i < axisXCount.Length; i++) {
            int currentAxisCount = axisXCount[i];
            int remaining = gridAlignedEntities.Count - cumulative;
            cumulative += currentAxisCount;

            // only cull isolated cells
            if (currentAxisCount != 0) {
                continue;
            }

            if (cumulative < AXIS_TRIM_ENTITY_COUNT) {
                trimmedMinX = (short) i;
            }

            if (remaining < AXIS_TRIM_ENTITY_COUNT) {
                trimmedMaxX = (short) (i - 1);

                break;
            }
        }

        for (int i = 0, cumulative = 0; i < axisYCount.Length; i++) {
            int currentAxisCount = axisYCount[i];
            int remaining = gridAlignedEntities.Count - cumulative;
            cumulative += currentAxisCount;

            // only cull isolated cells
            if (currentAxisCount != 0) {
                continue;
            }

            if (cumulative < AXIS_TRIM_ENTITY_COUNT) {
                trimmedMinY = (short) i;
            }

            if (remaining < AXIS_TRIM_ENTITY_COUNT) {
                trimmedMaxY = (short) (i - 1);

                break;
            }
        }

        for (int i = 0, cumulative = 0; i < axisZCount.Length; i++) {
            int currentAxisCount = axisZCount[i];
            int remaining = gridAlignedEntities.Count - cumulative;
            cumulative += currentAxisCount;

            // only cull isolated cells
            if (currentAxisCount != 0) {
                continue;
            }

            if (cumulative < AXIS_TRIM_ENTITY_COUNT) {
                trimmedMinZ = (short) i;
            }

            if (remaining < AXIS_TRIM_ENTITY_COUNT) {
                trimmedMaxZ = (short) (i - 1);

                break;
            }
        }

        Vector3S newMinIndex = new Vector3S(trimmedMinX, trimmedMinY, trimmedMinZ) + minIndex;
        Vector3S newMaxIndex = new Vector3S(trimmedMaxX, trimmedMaxY, trimmedMaxZ) + minIndex;
        Vector3S newGridSize = newMaxIndex - newMinIndex + new Vector3S(1, 1, 1);
        int newCellCount = newGridSize.X * newGridSize.Y * newGridSize.Z;
        int culledCells = cellCount - newCellCount;

        foreach ((Vector3S coord, List<FieldEntity> entityList) in gridAlignedEntities) {
            Vector3S index = coord - minIndex;

            bool isTrimmed = index.X < trimmedMinX;
            isTrimmed |= index.X > trimmedMaxX;
            isTrimmed |= index.Y < trimmedMinY;
            isTrimmed |= index.Y > trimmedMaxY;
            isTrimmed |= index.Z < trimmedMinZ;
            isTrimmed |= index.Z > trimmedMaxZ;

            if (isTrimmed && entityList.Count > 0) {
                trimmedCount += entityList.Count;

                Vector3 cellPosition = 150.0f * coord.Vector3;
                BoundingBox3 bounds = entityList.First().Bounds;

                foreach (FieldEntity entity in entityList) {
                    bounds = bounds.Expand(entity.Bounds);
                }

                if (entityList.Count == 1) {
                    unalignedEntities.Add(entityList.First());

                    continue;
                }

                SortEntityList(entityList);

                // limit the number of nodes in the AABB tree by bundling cells together
                FieldCellEntities cell = new FieldCellEntities(
                    Id: new FieldEntityId(0, 0),
                    Position: cellPosition,
                    Rotation: new Vector3(0, 0, 0),
                    Scale: 1,
                    Bounds: bounds,
                    Entities: entityList);

                unalignedEntities.Add(cell);
            }
        }
    }

    // cell grid contains ints that contain both list start index & entity count for the cell
    // top byte is used for entity count, the 3 least significant bytes are used for list start index: CC II II II
    // compare cell data with 0 to check if it is empty: 00 00 00 00
    public static (byte count, int startIndex) GetCellInfo(int cellData) {
        byte count = (byte) (cellData >> 24);
        int startIndex = cellData & 0xFFFFFF;

        return (count, startIndex);
    }

    public static int WriteCellInfo(int count, int startIndex) {
        return ((count & 0xFF) << 24) | (startIndex & 0xFFFFFF);
    }

    public void AddEntities(Dictionary<Vector3S, List<FieldEntity>> gridAlignedEntities, Vector3S minIndex, Vector3S maxIndex, List<FieldEntity> unalignedEntities) {
        if (minIndex.X == short.MaxValue) {
            minIndex = new Vector3S(0, 0, 0);
            maxIndex = new Vector3S(0, 0, 0);
        }

        GenerateSpawnLocations(gridAlignedEntities, minIndex, maxIndex, unalignedEntities);
        TrimGridSize(gridAlignedEntities, ref minIndex, ref maxIndex, unalignedEntities);

        GridSize = maxIndex - minIndex + new Vector3S(1, 1, 1);
        MinIndex = minIndex;
        MaxIndex = maxIndex;
        alignedEntities.Clear();
        this.unalignedEntities = unalignedEntities;
        cellGrid = new int[GridSize.X, GridSize.Y, GridSize.Z];

        // opting to order list entries by z index first, then by y, and last by x, the same way the memory will be laid out
        // this will dramatically speed up both load times and cell access times by storing them in a defragmented format from the start
        // the reason why is to reduce cache misses
        for (short x = minIndex.X; x <= maxIndex.X; x++) {
            for (short y = minIndex.Y; y <= maxIndex.Y; y++) {
                for (short z = minIndex.Z; z <= maxIndex.Z; z++) {
                    Vector3S coord = new Vector3S(x, y, z);

                    if (!gridAlignedEntities.TryGetValue(coord, out List<FieldEntity>? entities) || entities.Count == 0) {
                        continue;
                    }

                    SortEntityList(entities);

                    Vector3S index = coord - minIndex;

                    cellGrid[index.X, index.Y, index.Z] = WriteCellInfo(entities.Count, alignedEntities.Count);
                    alignedEntities.AddRange(entities);
                }
            }
        }

        SortEntityList(unalignedEntities);

        GenerateAabbTree();
    }

    public void GenerateAabbTree() {
        // TODO: generate AABB tree from unaligned objects list
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteShort(GridSize.X);
        writer.WriteShort(GridSize.Y);
        writer.WriteShort(GridSize.Z);

        writer.WriteShort(MinIndex.X);
        writer.WriteShort(MinIndex.Y);
        writer.WriteShort(MinIndex.Z);
        
        for (short x = 0; x < GridSize.X; x++) {
            for (short y = 0; y < GridSize.Y; y++) {
                for (short z = 0; z < GridSize.Z; z++) {
                    if (cellGrid[x, y, z] != 0) {
                        writer.WriteInt(cellGrid[x, y, z]);

                        continue;
                    }

                    int emptyCount = 0;

                    while (z < GridSize.Z && cellGrid[x, y, z] == 0) {
                        ++emptyCount;
                        ++z;
                    }

                    // use list start index as empty count for byte streams
                    writer.WriteInt(WriteCellInfo(0, emptyCount));

                    --z; // don't skip first occupied cell
                }
            }
        }

        if (writer is ByteWriter byteWriter) {
            GridBytesWritten = (ulong)byteWriter.Length;
        }

        writer.WriteInt(alignedEntities.Count);

        foreach (FieldEntity entity in alignedEntities) {
            WriteTo(entity, writer);
        }

        writer.WriteInt(unalignedEntities.Count);

        foreach (FieldEntity entity in unalignedEntities) {
            WriteTo(entity, writer);
        }
    }

#endregion

    #region Serialization

    private Vector3S GetWorldGridIndex(Vector3 position) {
        int x = (int) Math.Round(position.X) / 150;
        int y = (int) Math.Round(position.Y) / 150;
        int z = (int) Math.Round(position.Z) / 150;

       return new Vector3S((short)x, (short)y, (short)z);
    }

    private bool IsGridAligned(Vector3 position) {
        int x = (int) Math.Round(position.X) / 150;
        int y = (int) Math.Round(position.Y) / 150;
        int z = (int) Math.Round(position.Z) / 150;

        return position.IsNearlyEqual(150 * new Vector3(x, y, z), 0.1f);
    }

    private bool IsCellBounds(Vector3 position, BoundingBox3 bounds) {
        if (!IsGridAligned(position)) {
            return false;
        }

        bool isMinOnCell = bounds.Min.IsNearlyEqual(position - new Vector3(75, 75, 0), 0.1f);
        bool isMaxOnCell = bounds.Max.IsNearlyEqual(position + new Vector3(75, 75, 150), 0.1f);

        return isMinOnCell && isMaxOnCell;
    }

    public void WriteTo(FieldEntity entity, IByteWriter writer) {
        FieldEntityType type = entity switch {
            FieldVibrateEntity => FieldEntityType.Vibrate,
            FieldSpawnTile => FieldEntityType.SpawnTile,
            FieldBoxColliderEntity => FieldEntityType.BoxCollider,
            FieldFluidEntity => FieldEntityType.Fluid,
            FieldMeshColliderEntity => FieldEntityType.MeshCollider,
            FieldCellEntities => FieldEntityType.Cell,
            _ => FieldEntityType.Unknown
        };

        FieldEntityMembers memberFlags = FieldEntityMembers.None;

        memberFlags |= (entity.Id.High == 0 && entity.Id.Low == 0) ? 0 : FieldEntityMembers.Id;
        memberFlags |= IsGridAligned(entity.Position) ? 0 : FieldEntityMembers.Position;
        memberFlags |= entity.Rotation.IsNearlyEqual(new Vector3(0, 0, 0), 1e-3f) ? 0 : FieldEntityMembers.Rotation;
        memberFlags |= entity.Scale.IsNearlyEqual(1, 1e-3f) ? 0 : FieldEntityMembers.Scale;
        memberFlags |= IsCellBounds(entity.Position, entity.Bounds) ? 0 : FieldEntityMembers.Bounds;

        switch (entity) {
            case FieldMeshColliderEntity meshCollider:
                memberFlags |= (meshCollider.MeshLlid == 0) ? 0 : FieldEntityMembers.Llid;
                break;
            default:
                break;
        }

        writer.Write(type);
        writer.Write(memberFlags);

        if ((memberFlags & FieldEntityMembers.Id) != 0) {
            writer.Write(entity.Id.High);
            writer.Write(entity.Id.Low);
        }

        if ((memberFlags & FieldEntityMembers.Position) != 0) {
            writer.Write(entity.Position);
        } else {
            writer.Write(GetWorldGridIndex(entity.Position));
        }

        if ((memberFlags & FieldEntityMembers.Rotation) != 0) {
            writer.Write(entity.Rotation);
        }

        if ((memberFlags & FieldEntityMembers.Scale) != 0) {
            writer.Write(entity.Scale);
        }

        if ((memberFlags & FieldEntityMembers.Bounds) != 0) {
            writer.Write(entity.Bounds.Min);
            writer.Write(entity.Bounds.Max);
        }

        switch(entity) {
            case FieldVibrateEntity vibrateEntity:
                break;
            case FieldSpawnTile spawnTile:
                break;
            case FieldBoxColliderEntity boxCollider:
                writer.Write(boxCollider.Size);
                writer.Write(boxCollider.IsWhiteBox);
                break;
            case FieldMeshColliderEntity meshCollider:
                writer.Write(meshCollider.MeshLlid);
                break;
            case FieldCellEntities cell:
                writer.WriteInt(cell.Entities.Count);
                foreach(FieldEntity childEntity in  cell.Entities) {
                    WriteTo(childEntity, writer);
                }
                break;
            default:
                throw new InvalidDataException($"Writing unhandled field entity type: {entity.GetType().FullName}");
        }
    }

    public void ReadFrom(IByteReader reader) {
        GridSize = reader.Read<Vector3S>();
        MinIndex = reader.Read<Vector3S>();
        MaxIndex = MinIndex + GridSize - new Vector3S(1, 1, 1);
        cellGrid = new int[GridSize.X, GridSize.Y, GridSize.Z];

        if (alignedEntities is null || unalignedEntities is null) {
            alignedEntities = new();
            unalignedEntities = new();
        }

        alignedEntities.Clear();
        unalignedEntities.Clear();

        for (short x = 0; x < GridSize.X; x++) {
            for (short y = 0; y < GridSize.Y; y++) {
                for (short z = 0; z < GridSize.Z; z++) {
                    int cellData = reader.ReadInt();
                    (byte count, int startIndex) cell = GetCellInfo(cellData);

                    if (cell.count == 0) {
                        // use list start index as empty count for byte streams
                        z += (short)(cell.startIndex - 1);

                        continue;
                    }

                    cellGrid[x, y, z] = cellData;
                }
            }
        }

        int alignedEntityCount = reader.ReadInt();

        for (int i = 0; i < alignedEntityCount; ++i) {
            alignedEntities.Add(ReadEntity(reader));
        }

        int unalignedEntityCount = reader.ReadInt();

        for (int i = 0; i < unalignedEntityCount; ++i) {
            unalignedEntities.Add(ReadEntity(reader));
        }

        GenerateAabbTree();
    }

    public FieldEntity ReadEntity(IByteReader reader) {
        FieldEntityType type = reader.Read<FieldEntityType>();
        FieldEntityMembers memberFlags = reader.Read<FieldEntityMembers>();

        FieldEntityId id = new FieldEntityId(0, 0);
        Vector3 position;
        Vector3 rotation = new Vector3(0, 0, 0);
        float scale = 1;
        BoundingBox3 bounds;
        uint llid = 0;

        if ((memberFlags & FieldEntityMembers.Id) != 0) {
            id = new FieldEntityId(reader.Read<ulong>(), reader.Read<ulong>());
        }

        if ((memberFlags & FieldEntityMembers.Id) != 0) {
            position = reader.Read<Vector3>();
        } else {
            position = 150 * reader.Read<Vector3S>().Vector3;
        }

        if ((memberFlags & FieldEntityMembers.Rotation) != 0) {
            rotation = reader.Read<Vector3>();
        }

        if ((memberFlags & FieldEntityMembers.Rotation) != 0) {
            scale = reader.ReadFloat();
        }

        if ((memberFlags & FieldEntityMembers.Bounds) != 0) {
            bounds = new BoundingBox3(
                min: reader.Read<Vector3>(),
                max: reader.Read<Vector3>());
        } else {
            bounds = new BoundingBox3(
                min: position - new Vector3(75, 75, 0),
                max: position + new Vector3(75, 75, 150));
        }

        switch (type) {
            case FieldEntityType.Vibrate:
                return new FieldVibrateEntity(
                    Id: id,
                    Position: position,
                    Rotation: rotation,
                    Scale: scale,
                    Bounds: bounds);
            case FieldEntityType.SpawnTile:
                return new FieldSpawnTile(
                    Id: id,
                    Position: position,
                    Rotation: rotation,
                    Scale: scale,
                    Bounds: bounds);
            case FieldEntityType.BoxCollider:
                return new FieldBoxColliderEntity(
                    Id: id,
                    Position: position,
                    Rotation: rotation,
                    Scale: scale,
                    Bounds: bounds,
                    Size: reader.Read<Vector3>(),
                    IsWhiteBox: reader.Read<bool>());
            case FieldEntityType.MeshCollider:
                if ((memberFlags & FieldEntityMembers.Llid) != 0) {
                    llid = reader.Read<uint>();
                }

                return new FieldMeshColliderEntity(
                    Id: id,
                    Position: position,
                    Rotation: rotation,
                    Scale: scale,
                    Bounds: bounds,
                    MeshLlid: llid);
            case FieldEntityType.Fluid:
                if ((memberFlags & FieldEntityMembers.Llid) != 0) {
                    llid = reader.Read<uint>();
                }

                return new FieldFluidEntity(
                    Id: id,
                    Position: position,
                    Rotation: rotation,
                    Scale: scale,
                    Bounds: bounds,
                    MeshLlid: llid);
            case FieldEntityType.Cell:
                int childCount = reader.ReadInt();
                List<FieldEntity> children = new List<FieldEntity>();
                for (int i = 0; i < childCount; ++i) {
                    children.Add(ReadEntity(reader));
                }
                return new FieldCellEntities(
                    Id: id,
                    Position: position,
                    Rotation: rotation,
                    Scale: scale,
                    Bounds: bounds,
                    Entities: children);
            default:
                throw new InvalidDataException($"Reading unhandled field entity type: {type}");
        }
    }

    #endregion

}
