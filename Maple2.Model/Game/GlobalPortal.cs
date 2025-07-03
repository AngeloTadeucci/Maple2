using Maple2.Model.Metadata;

namespace Maple2.Model.Game;

public class GlobalPortal {
    public int MetadataId => Metadata.Id;
    public int Id;
    public GlobalPortalMetadata Metadata;
    public long EndTick;

    public GlobalPortal(GlobalPortalMetadata metadata, int id) {
        Metadata = metadata;
        Id = id;
    }
}
