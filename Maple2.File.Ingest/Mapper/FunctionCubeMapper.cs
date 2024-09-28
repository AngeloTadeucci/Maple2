using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml.Object;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class FunctionCubeMapper : TypeMapper<FunctionCubeMetadata> {
    private readonly FunctionCubeParser parser;

    public FunctionCubeMapper(M2dReader xmlReader) {
        parser = new FunctionCubeParser(xmlReader);
    }

    protected override IEnumerable<FunctionCubeMetadata> Map() {
        foreach ((int id, FunctionCube functionCube) in parser.Parse()) {
            yield return new FunctionCubeMetadata(
                Id: id,
                DefaultState: (InteractCubeState) functionCube.DefaultState,
                AutoStateChange: functionCube.AutoStateChange,
                AutoStateChangeTime: functionCube.AutoStateChangeTime
            );
        }
    }
}
