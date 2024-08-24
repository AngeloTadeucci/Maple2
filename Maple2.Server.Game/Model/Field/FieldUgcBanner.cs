using Maple2.Model.Game.Ugc;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Field;

public class FieldUgcBanner : UgcBanner, IUpdatable {
    private readonly FieldManager field;
    public FieldUgcBanner(FieldManager field, long id, int mapId, List<BannerSlot> slots) : base(id, mapId, slots) {
        this.field = field;
    }

    public void Update(long tickCount) {
        DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;

        DeleteOldBannerSlots(dateTimeOffset);

        BannerSlot? slot = Slots.FirstOrDefault(x => x.ActivateTime.Day == dateTimeOffset.Day && x.ActivateTime.Hour == dateTimeOffset.Hour);

        if (slot is null || slot.Expired || slot.Active) {
            return;
        }

        slot.Active = true;
        field.Broadcast(UgcPacket.ActivateBanner(this));
    }

    private void DeleteOldBannerSlots(DateTimeOffset dateTimeOffset) {
        foreach (BannerSlot bannerSlot in Slots) {
            // check if the banner is expired
            DateTimeOffset expireTimeStamp = dateTimeOffset.Subtract(TimeSpan.FromHours(4));
            if (bannerSlot.ActivateTime >= expireTimeStamp) {
                continue;
            }

            bannerSlot.Expired = true;
        }
    }
}