using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class GuildApplication : IByteSerializable {
    public required long Id { get; init; }
    public required Guild Guild;
    public required PlayerInfo Applicant;
    public long CreationTime;

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteLong(Guild.Id);
        writer.WriteLong(Applicant.CharacterId);
        writer.WriteLong(Applicant.AccountId);
        writer.WriteUnicodeString(Applicant.Name);
        writer.WriteUnicodeString(Applicant.Picture);
        writer.Write<Job>(Applicant.Job);
        writer.WriteInt((int) Applicant.Job.Code());
        writer.WriteInt(Applicant.Level);
        writer.Write<AchievementInfo>(Applicant.AchievementInfo);
        writer.WriteLong(CreationTime);
    }
}
