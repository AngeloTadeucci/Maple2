using BannerSlot = Maple2.Model.Game.Ugc.BannerSlot;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public List<BannerSlot> FindBannerSlotsByBannerId(long bannerId) {
            return Context.BannerSlots
                .Where(slot => slot.BannerId == bannerId)
                .Select(slot => new BannerSlot(slot.Id, slot.ActivateTime, slot.BannerId, slot.Template))
                .ToList();
        }

        public BannerSlot AddBannerSlot(BannerSlot slot) {
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Model.BannerSlot> result = Context.BannerSlots.Add(new Model.BannerSlot {
                ActivateTime = slot.ActivateTime,
                BannerId = slot.BannerId,
                Template = slot.Template,
            });

            Context.SaveChanges();

            slot.Id = result.Entity.Id;

            return slot;
        }

        public void UpdateBannerSlot(BannerSlot slot) {
            Context.BannerSlots.Update(slot);

            Context.SaveChanges();
        }

        public void RemoveBannerSlots(IEnumerable<BannerSlot> slots) {
            foreach (BannerSlot slot in slots) {
                Context.BannerSlots.Remove(slot);
            }

            Context.SaveChanges();
        }
    }
}
