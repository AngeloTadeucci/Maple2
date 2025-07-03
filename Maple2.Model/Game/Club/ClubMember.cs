using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Club;

public class ClubMember : IByteSerializable, IDisposable {
    public const byte TYPE = 2;

    public long ClubId { get; init; }
    public required PlayerInfo Info;
    public long AccountId => Info.AccountId;
    public long CharacterId => Info.CharacterId;
    public string Name => Info.Name;
    public long JoinTime;

    public CancellationTokenSource? TokenSource;


    public void Dispose() {
        TokenSource?.Cancel();
        TokenSource?.Dispose();
        TokenSource = null;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteByte(TYPE);
        writer.WriteLong(ClubId);

        WriteInfo(writer, this);
    }

    public static void WriteInfo(IByteWriter writer, ClubMember member) {
        PlayerInfo info = member.Info;
        writer.WriteLong(info.AccountId);
        writer.WriteLong(info.CharacterId);
        writer.WriteUnicodeString(info.Name);
        writer.Write<Gender>(info.Gender);
        writer.WriteInt((int) info.Job.Code());
        writer.Write<Job>(info.Job);
        writer.WriteShort(info.Level);
        writer.WriteInt(info.MapId);
        writer.WriteShort(info.Channel);
        writer.WriteUnicodeString(info.Picture);
        writer.WriteInt(info.PlotMapId);
        writer.WriteInt(info.PlotNumber);
        writer.WriteInt(info.ApartmentNumber);
        writer.WriteLong(info.PlotExpiryTime);
        writer.Write<AchievementInfo>(info.AchievementInfo);
        writer.WriteLong(member.JoinTime);
        writer.WriteLong(info.LastOnlineTime);
        writer.WriteBool(!info.Online);
    }
}
