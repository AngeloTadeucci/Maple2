using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class PremiumMarketPromoData : IByteSerializable {
    public string Name { get; init; }
    public long StartTime { get; init; }
    public long EndTime { get; init; }

    public PremiumMarketPromoData() {
        Name = string.Empty;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteString(Name);
        writer.WriteLong(StartTime);
        writer.WriteLong(EndTime);
    }
}
