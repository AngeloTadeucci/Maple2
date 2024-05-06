using System.Collections.Generic;
using System.Linq;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;

namespace Maple2.Model.Game.Event;

/// <summary>
/// Attendance Event
/// </summary>
public class AttendGift : GameEventInfo {
    public string AttendanceName { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public long BeginTime { get; init; }
    public long EndTime { get; init; }
    public bool DisableClaimButton { get; init; }
    public int TimeRequired { get; init; }
    public AttendGiftCurrencyType SkipDayCurrencyType { get; init; }
    public int SkipDaysAllowed { get; init; }
    public long SkipDayCost { get; init; }
    public IDictionary<int, RewardItem> Rewards { get; init; } = new Dictionary<int, RewardItem>();

    public override void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteLong(BeginTime);
        writer.WriteLong(EndTime);
        writer.WriteUnicodeString(AttendanceName);
        writer.WriteString(Url);
        writer.WriteBool(false);
        writer.WriteBool(DisableClaimButton);
        writer.WriteInt(TimeRequired);
        writer.WriteByte();
        writer.WriteInt();
        writer.Write<AttendGiftCurrencyType>(SkipDayCurrencyType);

        if (SkipDayCurrencyType != AttendGiftCurrencyType.None) {
            writer.WriteInt(SkipDaysAllowed);
            writer.WriteLong(SkipDayCost);
            writer.WriteInt();
        }

        writer.WriteInt(Rewards.Count);
        foreach ((_, RewardItem reward) in Rewards.OrderBy(entry => entry.Key)) {
            writer.Write<RewardItem>(reward);
        }
    }
}
