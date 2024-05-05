using System.Collections.Immutable;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Manager;

public sealed class GameEventUserValueManager {
    private const int BATCH_SIZE = 10;
    private readonly GameSession session;
    private readonly Dictionary<int, Dictionary<GameEventUserValueType, GameEventUserValue>> eventValues;

    public GameEventUserValueManager(GameSession session) {
        this.session = session;

        using GameStorage.Request db = session.GameStorage.Context();
        eventValues = db.GetEventUserValues(session.CharacterId);
    }

    public void Set(int gameEventId, GameEventUserValueType type, object value) {
        if (!eventValues.TryGetValue(gameEventId, out Dictionary<GameEventUserValueType, GameEventUserValue>? eventDictionary)) {
            throw new ArgumentOutOfRangeException(nameof(gameEventId), gameEventId, "Invalid game event id.");
        }
        if (!eventDictionary.TryGetValue(type, out GameEventUserValue? gameEventUserValue)) {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid game event type.");
        }

        gameEventUserValue.Value = value.ToString() ?? throw new ArgumentException("Invalid new value");
        session.Send(GameEventUserValuePacket.Update(gameEventUserValue));
    }

    public void Load() {
        foreach (ImmutableList<GameEventUserValue> batch in eventValues.Values.SelectMany(dict => dict.Values).Batch(BATCH_SIZE)) {
            session.Send(GameEventUserValuePacket.Load(batch));
        }
    }

    public GameEventUserValue Get(GameEventUserValueType type, int eventId, long expirationTime) {
        if (!eventValues.TryGetValue(eventId, out Dictionary<GameEventUserValueType, GameEventUserValue>? valueDict)) {
            eventValues.Add(eventId, new Dictionary<GameEventUserValueType, GameEventUserValue> {
                {type, new GameEventUserValue(type, expirationTime, eventId)},
            });
        } else if (!valueDict.ContainsKey(type)) {
            valueDict.Add(type, new GameEventUserValue(type, expirationTime, eventId));
        }

        if (eventValues[eventId][type].ExpirationTime < DateTime.Now.ToEpochSeconds()) {
            eventValues[eventId][type] = new GameEventUserValue(type, expirationTime, eventId);
        }

        return eventValues[eventId][type];
    }

    public void Save(GameStorage.Request db) {
        // Update certain values upon log off
        // TODO: Maybe update this to handle a list of other types that need to be updated upon logoff?
        IEnumerable<GameEventUserValue> accumulatedTimeValues =
            eventValues.Values.SelectMany(dict => dict.Values.Where(value => value.Type == GameEventUserValueType.AttendanceAccumulatedTime));
        foreach (GameEventUserValue userValue in accumulatedTimeValues) {
            userValue.Value = (DateTime.Now.AddSeconds(userValue.Long()) - session.Player.Value.Character.LastModified).Seconds.ToString();
        }
        db.SaveGameEventUserValues(session.CharacterId, eventValues.Values.SelectMany(value => value.Values).ToList());
    }
}
