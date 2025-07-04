using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml;
using Maple2.Model.Common;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class UgcMapMapper : TypeMapper<UgcMapMetadata> {
    private readonly UgcMapParser parser;

    public UgcMapMapper(M2dReader xmlReader) {
        parser = new UgcMapParser(xmlReader);
    }

    protected override IEnumerable<UgcMapMetadata> Map() {
        foreach ((int id, UgcMap data) in parser.Parse()) {
            yield return new UgcMapMetadata(
                Id: id,
                Plots: data.group.ToDictionary(
                    group => group.no,
                    group => new UgcMapGroup(
                        Number: group.no,
                        ApartmentNumber: 0,
                        Type: group.builingType,
                        ContractCost: new UgcMapGroup.Cost(
                            Amount: group.contractPrice,
                            ItemId: group.contractPriceItemCode,
                            Days: group.ugcHomeContractDate),
                        ExtensionCost: new UgcMapGroup.Cost(
                            Amount: group.extensionPrice,
                            ItemId: group.extensionPriceItemCode,
                            Days: group.ugcHomeExtensionDate),
                        Limit: new UgcMapGroup.Limits(
                            Height: group.heightLimit,
                            Area: group.area,
                            Maid: group.maidCount,
                            Trigger: group.triggerCount,
                            InstallNpc: group.installNpcCount,
                            InstallBuilding: group.installableBuildingCount)
                    )
                )
            );
        }
    }
}

public class ExportedUgcMapMapper : TypeMapper<ExportedUgcMapMetadata> {
    private readonly UgcMapParser parser;

    public ExportedUgcMapMapper(M2dReader xmlReader) {
        parser = new UgcMapParser(xmlReader);
    }

    protected override IEnumerable<ExportedUgcMapMetadata> Map() {
        foreach ((string id, ExportedUgcMap data) in parser.ParseExported()) {
            yield return new ExportedUgcMapMetadata(
                Id: id,
                BaseCubePosition: new Vector3B(data.baseCubePoint3[0], data.baseCubePoint3[1], data.baseCubePoint3[2]),
                IndoorSize: data.indoorSizeType.Select(x => (byte) x).ToArray(),
                Cubes: data.cube.Select(x =>
                 new ExportedUgcMapMetadata.Cube(ItemId: x.itemID,
                                                OffsetPosition: new Vector3B(x.offsetCubePoint3[0], x.offsetCubePoint3[1], x.offsetCubePoint3[2]),
                                                Rotation: x.rotation,
                                                WallDirection: (byte) x.wallDir)).ToList()
            );
        }
    }
}
