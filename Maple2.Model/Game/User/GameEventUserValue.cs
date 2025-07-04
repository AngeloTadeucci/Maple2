using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class GameEventUserValue : IByteSerializable {
    public GameEventUserValueType Type { get; init; }
    public string Value { get; private set; }
    public int EventId { get; init; }
    public long ExpirationTime { get; init; }

    public GameEventUserValue(string value = "") {
        Value = value;
    }

    public GameEventUserValue(GameEventUserValueType type, long expirationTime, int eventId) {
        Value = string.Empty;
        Type = type;
        ExpirationTime = expirationTime;
        EventId = eventId;
    }

    public void SetValue(string value) {
        Value = value;
    }

    public int Int() => int.TryParse(Value, out int result) ? result : 0;

    public long Long() => long.TryParse(Value, out long result) ? result : 0;

    public string String() => Value;

    public bool Bool() => bool.TryParse(Value, out bool result) && result;

    public void WriteTo(IByteWriter writer) {
        writer.Write<GameEventUserValueType>(Type);
        writer.WriteInt(EventId);
        writer.WriteUnicodeString(Value);
        writer.WriteLong(ExpirationTime);
    }
}
