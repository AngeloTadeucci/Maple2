using Maple2.Database.Extensions;
using Maple2.Database.Model;
using Microsoft.EntityFrameworkCore;
using HomeLayout = Maple2.Model.Game.HomeLayout;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public HomeLayout? SaveHomeLayout(HomeLayout layout) {
            Model.HomeLayout homeLayout = layout;
            Context.HomeLayouts.Add(homeLayout);
            foreach (HomeLayoutCube cubes in homeLayout.Cubes) {
                Context.UgcCubeLayouts.Add(cubes);
            }
            bool success = Context.TrySaveChanges();

            return success ? homeLayout : null;
        }

        public void RemoveHomeLayout(HomeLayout layout) {
            Model.HomeLayout homeLayout = layout;
            Context.HomeLayouts.Remove(homeLayout);
            Context.TrySaveChanges();
        }

        public HomeLayout? GetHomeLayout(long layoutUid) {
            HomeLayout? layout = Context.HomeLayouts
                .Where(homeLayout => homeLayout.Uid == layoutUid)
                .Include(homeLayout => homeLayout.Cubes)
                .FirstOrDefault();

            return layout;
        }
    }
}
