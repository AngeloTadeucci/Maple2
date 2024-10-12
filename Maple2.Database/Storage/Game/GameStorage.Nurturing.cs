using Maple2.Database.Extensions;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Nurturing = Maple2.Database.Model.Nurturing;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Maple2.Model.Game.Nurturing? GetNurturing(long characterId, int itemId) {
            Nurturing? result = Context.Nurturing.Find(characterId, itemId);
            if (result is null || !game.itemMetadata.TryGet(itemId, out ItemMetadata? itemMetadata) || !game.functionCubeMetadata.TryGet(itemMetadata.Install!.InteractId, out FunctionCubeMetadata? metadata)) {
                return null;
            }

            return new Maple2.Model.Game.Nurturing(result.Exp, result.ClaimedGiftForStage, result.PetBy, result.CreationTime, result.LastFeedTime, metadata.Nurturing);
        }

        public Maple2.Model.Game.Nurturing? CreateNurturing(long accountId, PlotCube plotCube) {
            var nurturing = new Nurturing {
                AccountId = accountId,
                ItemId = plotCube.ItemId,
                Exp = 0,
                ClaimedGiftForStage = 1,
                CreationTime = DateTime.Now,
                LastFeedTime = null,
                PetBy = [],
            };
            Context.Nurturing.Add(nurturing);
            bool success = Context.TrySaveChanges();
            if (!success) {
                return null;
            }

            return new Maple2.Model.Game.Nurturing(nurturing.Exp, nurturing.ClaimedGiftForStage, nurturing.PetBy, nurturing.CreationTime, nurturing.LastFeedTime, plotCube.Interact!.Nurturing!.NurturingMetadata);
        }

        public void UpdateNurturing(long accountId, PlotCube plotCube) {
            Nurturing? result = Context.Nurturing.Find(accountId, plotCube.ItemId);
            if (result is null) {
                return;
            }
            Maple2.Model.Game.Nurturing? interactNurturing = plotCube.Interact?.Nurturing;
            if (interactNurturing is null) {
                return;
            }
            result.Exp = interactNurturing.Exp;
            result.ClaimedGiftForStage = interactNurturing.ClaimedGiftForStage;
            result.PetBy = interactNurturing.PetBy.ToArray();
            if (interactNurturing.LastFeedTime is not null) {
                result.LastFeedTime = interactNurturing.LastFeedTime.Value.DateTime;
            }
            Context.Nurturing.Update(result);
            Context.TrySaveChanges();
        }

        // Count the number of nurturing items for the given account ID in petBy
        public int CountNurturingForAccount(int itemId, long accountId) {
            return Context.Nurturing.AsEnumerable().Count(x => x.ItemId == itemId && x.PetBy.Contains(accountId));
        }
    }
}
