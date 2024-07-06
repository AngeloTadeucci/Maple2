using System.Collections.Generic;
using System.Linq;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore;
using GameEventUserValue = Maple2.Model.Game.GameEventUserValue;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Dictionary<int, Dictionary<GameEventUserValueType, GameEventUserValue>> GetEventUserValues(long characterId) {
            var results = new Dictionary<int, Dictionary<GameEventUserValueType, GameEventUserValue>>();
            foreach (GameEventUserValue value in Context.GameEventUserValue.Where(model => model.CharacterId == characterId)) {
                if (results.TryGetValue(value.EventId, out Dictionary<GameEventUserValueType, GameEventUserValue>? result)) {
                    result.Add(value.Type, value);
                } else {
                    results.Add(value.EventId, new Dictionary<GameEventUserValueType, GameEventUserValue> {
                        {value.Type, value}
                    });
                }
            }
            return results;
        }

        public bool SaveGameEventUserValues(long characterId, IList<GameEventUserValue> values) {
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            var existing = Context.GameEventUserValue.Where(model => model.CharacterId == characterId)
                .ToDictionary(model => model.Type, model => model);

            foreach (GameEventUserValue value in values) {
                if (existing.TryGetValue(value.Type, out Model.GameEventUserValue? model)) {
                    model.Value = value.Value;
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
