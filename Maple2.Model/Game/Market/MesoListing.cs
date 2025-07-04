using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class MesoListing : IByteSerializable {
    public long Id { get; init; }
    public long AccountId { get; init; }
    public long CharacterId { get; init; }

    public long CreationTime;
    public long ExpiryTime;
    public long Price;
    public long Amount;

    public MesoListing(TimeSpan duration = default) {
        if (duration != default) {
            ExpiryTime = (DateTimeOffset.UtcNow + duration).ToUnixTimeSeconds();
        }
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteLong(Amount);
        writer.WriteLong(Price);
        writer.WriteLong(CreationTime);
        writer.WriteLong(ExpiryTime);
    }
}
