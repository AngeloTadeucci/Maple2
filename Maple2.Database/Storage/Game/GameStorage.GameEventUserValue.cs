using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore;
using GameEventUserValue = Maple2.Model.Game.GameEventUserValue;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public IList<GameEventUserValue> GetEventUserValues(long characterId) {
            return Context.GameEventUserValue.Where(model => model.CharacterId == characterId)
                .Select<Model.GameEventUserValue, GameEventUserValue>(userValue => userValue)
                .ToList();
        }

        public void RemoveGameEventUserValue(long characterId, int eventId) {
            List<Model.GameEventUserValue> list = Context.GameEventUserValue
                .Where(model => model.CharacterId == characterId && model.EventId == eventId)
                .ToList();

            foreach (Model.GameEventUserValue model in list) {
                Context.GameEventUserValue.Remove(model);
            }
        }

        public bool RemoveGameEventUserValue(GameEventUserValue userValue, long characterId) {
            Model.GameEventUserValue model = userValue;
            model.CharacterId = characterId;
            Context.GameEventUserValue.Remove(model);
            return Context.TrySaveChanges();
        }

        public bool SaveGameEventUserValues(long characterId, IList<GameEventUserValue> values) {
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            Dictionary<int, Dictionary<GameEventUserValueType, Model.GameEventUserValue>> existing = Context.GameEventUserValue
                .Where(model => model.CharacterId == characterId)
                .GroupBy(model => model.EventId)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToDictionary(model => model.Type, model => model)
                );

            foreach (GameEventUserValue value in values) {
                if (existing.TryGetValue(value.EventId, out Dictionary<GameEventUserValueType, Model.GameEventUserValue>? modelDictionary) &&
                    modelDictionary.TryGetValue(value.Type, out Model.GameEventUserValue? model)) {
                    model.Value = value.Value;
                    model.ExpirationTime = value.ExpirationTime;
                    Context.GameEventUserValue.Update(model);
                } else {
                    model = value;
                    model.CharacterId = characterId;
                    Context.GameEventUserValue.Add(model);
                }
            }

            return Context.TrySaveChanges();
        }
    }
}
