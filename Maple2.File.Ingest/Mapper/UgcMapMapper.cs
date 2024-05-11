using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class UgcMapMapper : TypeMapper<UgcMapMetadata> {
    private readonly UgcMapParser parser;

    public UgcMapMapper(M2dReader xmlReader) {
        parser = new(xmlReader);
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
