using Maple2.Database.Extensions;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Nurturing = Maple2.Database.Model.Nurturing;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Maple2.Model.Game.Nurturing? GetNurturing(long characterId, int interactObjectCode, FunctionCubeMetadata.NurturingData metadata) {
            Nurturing? result = Context.Nurturing.Find(characterId, interactObjectCode);
            return result is null ? null : new Maple2.Model.Game.Nurturing(result.Exp, result.ClaimedGiftForStage, result.PlayedBy, result.CreationTime, result.LastFeedTime, metadata);
        }

        public Maple2.Model.Game.Nurturing? CreateNurturing(long accountId, FunctionCubeMetadata.NurturingData metadata, int interactId) {
            var nurturing = new Nurturing {
                AccountId = accountId,
                InteractId = interactId,
                Exp = 0,
                ClaimedGiftForStage = 1,
                CreationTime = DateTime.Now,
                LastFeedTime = DateTime.MinValue,
                PlayedBy = [],
            };
            Context.Nurturing.Add(nurturing);
            if (!Context.TrySaveChanges()) {
                return null;
            }

            return new Maple2.Model.Game.Nurturing(nurturing.Exp, nurturing.ClaimedGiftForStage, nurturing.PlayedBy, nurturing.CreationTime, nurturing.LastFeedTime, metadata);
        }

        public void UpdateNurturing(long accountId, InteractCube cube) {
            Nurturing? result = Context.Nurturing.Find(accountId, cube.Metadata.Id);
            if (result is null) {
                return;
            }
            Maple2.Model.Game.Nurturing? interactNurturing = cube.Nurturing;
            if (interactNurturing is null) {
                return;
            }
            result.Exp = interactNurturing.Exp;
            result.ClaimedGiftForStage = interactNurturing.ClaimedGiftForStage;
            result.PlayedBy = interactNurturing.PlayedBy.ToArray();
            result.LastFeedTime = interactNurturing.LastFeedTime.DateTime;

            Context.Nurturing.Update(result);
            Context.TrySaveChanges();
        }

        // Count the number of nurturing items for the given account ID in petBy
        public int CountNurturingForAccount(int itemId, long accountId) {
            return Context.Nurturing.AsEnumerable().Count(x => x.InteractId == itemId && x.PlayedBy.Contains(accountId));
        }
    }
}
