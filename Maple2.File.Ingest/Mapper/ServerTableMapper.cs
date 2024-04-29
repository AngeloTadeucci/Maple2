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
        var results = new Dictionary<int, InstanceType>();
        foreach ((int InstanceId, Parser.Xml.Table.Server.InstanceField InstanceField) in parser.ParseInstanceField()) {
            foreach (int fieldId in InstanceField.fieldIDs) {

                results.Add(fieldId, (InstanceType) InstanceField.instanceType);
            }
        }

        return new InstanceFieldTable(results);
    }
}