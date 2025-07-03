namespace Maple2.Database.Extensions;

public static class DateTimeExtensions {
    public static long ToEpochSeconds(this DateTime dateTime) {
        if (dateTime <= DateTime.UnixEpoch) {
            return DateTime.UnixEpoch.Second;
        }

        return (long) (dateTime.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
    }

    public static DateTime FromEpochSeconds(this long epochSeconds) {
        return DateTimeOffset.FromUnixTimeSeconds(epochSeconds).LocalDateTime;
    }
}
