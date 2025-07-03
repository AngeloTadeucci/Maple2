using Maple2.Database.Context;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UgcResource = Maple2.Model.Game.UgcResource;

namespace Maple2.Database.Storage;

public partial class WebStorage {
    private readonly ILogger logger;
    private readonly DbContextOptions options;

    public WebStorage(DbContextOptions options, ILogger<WebStorage> logger) {
        this.options = options;
        this.logger = logger;
    }

    public Request Context() {
        // We use NoTracking by default since most requests are Read or Overwrite.
        // If we need tracking for modifying data, we can set it individually as needed.
        var context = new WebContext(options);
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return new Request(context, logger);
    }

    public partial class Request(WebContext context, ILogger logger) : DatabaseRequest<WebContext>(context, logger) {

        public UgcResource? CreateUgc(UgcType type, long ownerId) {
            var model = new Model.UgcResource {
                Type = type,
                OwnerId = ownerId,
            };
            Context.UgcResource.Add(model);

            return Context.TrySaveChanges() ? model : null;
        }

        public bool SaveUgc(UgcResource ugc, long ownerId) {
            Model.UgcResource model = ugc;
            model.OwnerId = ownerId;
            Context.UgcResource.Update(model);

            return SaveChanges();
        }

        public bool UpdatePath(long id, string path) {
            Model.UgcResource? model = Context.UgcResource.Find(id);
            if (model == null) {
                return false;
            }

            model.Path = path;
            Context.UgcResource.Update(model);
            return SaveChanges();
        }

        public UgcResource? GetUgc(long id) {
            return Context.UgcResource.Find(id);
        }
    }
}
