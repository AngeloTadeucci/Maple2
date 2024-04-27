using System;
using System.Collections.Generic;
using System.Linq;
using Maple2.Model.Game.Event;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public IList<GameEvent> GetEvents() {
            var results = new List<GameEvent>();
            foreach (Model.Event.GameEvent @event in Context.GameEvent.Where(model => model.BeginTime <= DateTimeOffset.UtcNow && model.EndTime >= DateTimeOffset.UtcNow)) {
                GameEvent result = @event;
                result.EventInfo.Id = @event.Id;
                result.EventInfo.Name = @event.Name;
                results.Add(result);
            }
            return results;
        }

        public GameEvent? FindEvent(string name) {
            return Context.GameEvent.FirstOrDefault(@event => @event.BeginTime <= DateTimeOffset.UtcNow && @event.EndTime >= DateTimeOffset.UtcNow && @event.Name == name);
        }
    }
}
