using Maple2.Model.Game;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public IList<SystemBanner> GetBanners() {
            return Context.SystemBanner
                .Where(banner => banner.EndTime > DateTime.Now)
                .Select<Model.SystemBanner, SystemBanner>(banner => banner)
                .ToList();
        }
    }
}
