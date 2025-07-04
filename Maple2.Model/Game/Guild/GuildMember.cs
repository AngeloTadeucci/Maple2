using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class GuildMember : IByteSerializable, IDisposable {
    private const byte TYPE = 3;

    public long GuildId { get; init; }

    public required PlayerInfo Info;

    public string Message = string.Empty;
    public byte Rank;
    public long JoinTime;
    public long CheckinTime;
    public long DonationTime;
    public int WeeklyContribution;
    public int TotalContribution;
    public int DailyDonationCount;

    public long AccountId => Info.AccountId;
    public long CharacterId => Info.CharacterId;
    public string Name => Info.Name;

    public CancellationTokenSource? TokenSource;

    public void WriteTo(IByteWriter writer) {
        writer.WriteByte(TYPE);
        writer.WriteByte(Rank);
        writer.WriteLong(CharacterId);

        WriteInfo(writer, Info);

        writer.WriteUnicodeString(Message);
        writer.WriteLong(JoinTime);
        writer.WriteLong(Info.LastOnlineTime);
        writer.WriteLong(CheckinTime);
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteInt(WeeklyContribution);
        writer.WriteInt(TotalContribution);
        writer.WriteInt(DailyDonationCount);
        writer.WriteLong(DonationTime);
        writer.WriteInt();
        // for (int i = 0; i < count; i++) {
        //     writer.WriteInt();
        //     writer.WriteInt();
        // }
        writer.WriteBool(!Info.Online);
    }

    public static void WriteInfo(IByteWriter writer, PlayerInfo info) {
        writer.WriteLong(info.AccountId);
        writer.WriteLong(info.CharacterId);
        writer.WriteUnicodeString(info.Name);
        writer.Write<Gender>(info.Gender);
        writer.WriteInt((int) info.Job.Code());
        writer.Write<Job>(info.Job);
        writer.WriteShort(info.Level);
        writer.WriteInt(info.GearScore);
        writer.WriteInt(info.MapId);
        writer.WriteShort(info.Channel);
        writer.WriteUnicodeString(info.Picture);
        writer.WriteInt(info.PlotMapId);
        writer.WriteInt(info.PlotNumber);
        writer.WriteInt(info.ApartmentNumber);
        writer.WriteLong(info.PlotExpiryTime);
        writer.Write<AchievementInfo>(info.AchievementInfo);
    }

    public void Dispose() {
        TokenSource?.Cancel();
        TokenSource?.Dispose();
        TokenSource = null;
    }
}
