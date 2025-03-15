using Maple2.Database.Extensions;
using Maple2.Database.Model;
using Microsoft.EntityFrameworkCore;
using HomeLayout = Maple2.Model.Game.HomeLayout;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public HomeLayout? SaveHomeLayout(HomeLayout layout) {
            Model.HomeLayout homeLayout = layout;
            Context.HomeLayout.Add(homeLayout);
            foreach (HomeLayoutCube cubes in homeLayout.Cubes) {
                Context.UgcCubeLayout.Add(cubes);
            }
            bool success = Context.TrySaveChanges();

            return success ? ToHomeLayout(homeLayout) : null;
        }

        public void RemoveHomeLayout(HomeLayout layout) {
            Model.HomeLayout homeLayout = layout;
            Context.HomeLayout.Remove(homeLayout);
            Context.TrySaveChanges();
        }

        public HomeLayout? GetHomeLayout(long layoutUid) {
            HomeLayout? layout = Context.HomeLayout
                .Where(homeLayout => homeLayout.Uid == layoutUid)
                .Include(homeLayout => homeLayout.Cubes)
                .Select(ToHomeLayout)
                .FirstOrDefault();

            return layout;
        }
    }
}
