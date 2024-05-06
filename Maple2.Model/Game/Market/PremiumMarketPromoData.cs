using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class PremiumMarketPromoData : IByteSerializable {
    public string Name { get; init; } = string.Empty;
    public long StartTime { get; init; }
    public long EndTime { get; init; }

    public void WriteTo(IByteWriter writer) {
        writer.WriteString(Name);
        writer.WriteLong(StartTime);
        writer.WriteLong(EndTime);
    }
}
