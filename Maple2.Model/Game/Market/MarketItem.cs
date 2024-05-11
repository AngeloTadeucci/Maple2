using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public abstract class MarketItem : IByteSerializable {
    public readonly ItemMetadata ItemMetadata;
    protected string Name => ItemMetadata.Name ?? string.Empty;
    public long Price { get; set; }
    public int SalesCount { get; set; }
    public required int TabId { get; init; }
    public long CreationTime { get; init; }

    protected MarketItem(ItemMetadata itemMetadata) {
        ItemMetadata = itemMetadata;
    }

    public virtual void WriteTo(IByteWriter writer) { }
}
