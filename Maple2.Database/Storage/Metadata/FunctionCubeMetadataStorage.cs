using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Context;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class FunctionCubeMetadataStorage : MetadataStorage<int, FunctionCubeMetadata> {
    protected readonly Dictionary<int, FunctionCubeMetadata> FunctionCubeMetadata;

    public FunctionCubeMetadataStorage(MetadataContext context) : base(context, capacity: 500) {
        FunctionCubeMetadata = new Dictionary<int, FunctionCubeMetadata>();

        foreach (FunctionCubeMetadata functionCube in context.FunctionCubeMetadata) {
            FunctionCubeMetadata.Add(functionCube.Id, functionCube);
        }
    }

    public bool TryGet(int id, [NotNullWhen(true)] out FunctionCubeMetadata? functionCube) {
        return FunctionCubeMetadata.TryGetValue(id, out functionCube);
    }
}
