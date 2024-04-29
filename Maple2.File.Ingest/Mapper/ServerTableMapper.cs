using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class ServerTableMapper : TypeMapper<ServerTableMetadata> {
    private readonly ServerTableParser parser;

    public ServerTableMapper(M2dReader xmlReader) {
        parser = new ServerTableParser(xmlReader);
    }

    protected override IEnumerable<ServerTableMetadata> Map() {
        yield return new ServerTableMetadata { Name = "instancefield.xml", Table = ParseInstanceField() };

    }

    private InstanceFieldTable ParseInstanceField() {
        var results = new Dictionary<int, InstanceFieldMetadata>();
        foreach ((int InstanceId, Parser.Xml.Table.Server.InstanceField InstanceField) in parser.ParseInstanceField()) {
            foreach (int fieldId in InstanceField.fieldIDs) {

                InstanceFieldMetadata instanceFieldMetadata = new(
                    (InstanceType) InstanceField.instanceType,
                    InstanceId,
                    InstanceField.backupSourcePortal,
                    InstanceField.poolCount,
                    InstanceField.isSaveField,
                    InstanceField.npcStatFactorID,
                    InstanceField.maxCount,
                    InstanceField.openType,
                    InstanceField.openValue
                );

                results.Add(fieldId, instanceFieldMetadata);
            }
        }

        return new InstanceFieldTable(results);
    }
}