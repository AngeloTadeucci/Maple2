using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Shop;

public class RestrictedBuyData : IByteSerializable {

    public long StartTime { get; init; }
    public long EndTime { get; init; }
    public IList<BuyTimeOfDay> TimeRanges { get; init; } = new List<BuyTimeOfDay>();
    public IList<ShopBuyDay> Days { get; init; } = new List<ShopBuyDay>();

    public RestrictedBuyData Clone() {
        return new RestrictedBuyData() {
            StartTime = StartTime,
            EndTime = EndTime,
            TimeRanges = TimeRanges.Select(time => time.Clone()).ToList(),
            Days = Days.Select(day => day).ToList(),
        };
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteBool(StartTime > 0 && EndTime > 0);
        writer.WriteLong(StartTime);
        writer.WriteLong(EndTime);
        writer.WriteShort((short) TimeRanges.Count);

        foreach (BuyTimeOfDay time in TimeRanges) {
            writer.Write<BuyTimeOfDay>(time);
        }

        writer.WriteByte((byte) Days.Count);
        foreach (ShopBuyDay day in Days) {
            writer.Write<ShopBuyDay>(day);
        }
    }
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
[method: JsonConstructor]
public readonly struct BuyTimeOfDay(int startTime, int endTime) {
    public int StartTimeOfDay { get; } = startTime; // time begin in seconds. ex 1200 = 12:20 AM
    public int EndTimeOfDay { get; } = endTime; // time end in seconds. ex 10600 = 2:56 AM

    public BuyTimeOfDay Clone() {
        return new BuyTimeOfDay(StartTimeOfDay, EndTimeOfDay);
    }
}
