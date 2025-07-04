using System;

namespace Maple2.Tools.Extensions;

public static class DateTimeExtension {
    public static DateTime NextDayOfWeek(this DateTime from, DayOfWeek dayOfWeek) {
        int start = (int) from.DayOfWeek;
        int target = (int) dayOfWeek;
        if (target <= start)
            target += 7;
        return from.AddDays(target - start);
    }
}
