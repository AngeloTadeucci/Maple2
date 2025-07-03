using Maple2.Database.Extensions;
using Maple2.Database.Model;
using Maple2.Model.Enum;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public void ReportPlayer(long characterId, string playerName, long reporterCharacterId, string reporterName, string reason, PlayerReportFlag flag) {
            Context.PlayerReports.Add(new PlayerReport {
                ReporterCharacterId = reporterCharacterId,
                ReporterName = reporterName,
                CharacterId = characterId,
                PlayerName = playerName,
                Reason = reason,
                Category = ReportCategory.Player,
                CreateTime = DateTime.Now,
                ReportInfo = new PlayerReportInfo(flag.ToString()),
            });
            Context.TrySaveChanges();
        }

        public void ReportChat(long characterId, string playerName, long reporterCharacterId, string reporterName, string reason, string chatMessage, ChatReportFlag flag) {
            Context.PlayerReports.Add(new PlayerReport {
                ReporterCharacterId = reporterCharacterId,
                ReporterName = reporterName,
                CharacterId = characterId,
                PlayerName = playerName,
                Reason = reason,
                Category = ReportCategory.Chat,
                CreateTime = DateTime.Now,
                ReportInfo = new ChatReportInfo(flag.ToString(), chatMessage),
            });
            Context.TrySaveChanges();
        }

        public void ReportPoster(long characterId, string playerName, long reporterCharacterId, string reporterName, string reason, int posterId, string templateId, PosterReportFlag flag) {
            Context.PlayerReports.Add(new PlayerReport {
                ReporterCharacterId = reporterCharacterId,
                ReporterName = reporterName,
                CharacterId = characterId,
                PlayerName = playerName,
                Reason = reason,
                Category = ReportCategory.Poster,
                CreateTime = DateTime.Now,
                ReportInfo = new PosterReportInfo(flag.ToString(), posterId, templateId),
            });
            Context.TrySaveChanges();
        }

        public void ReportDesignItem(long characterId, string playerName, long reporterCharacterId, string reporterName, string reason, long listingId, DesignItemReportFlag flag) {
            Context.PlayerReports.Add(new PlayerReport {
                ReporterCharacterId = reporterCharacterId,
                ReporterName = reporterName,
                CharacterId = characterId,
                PlayerName = playerName,
                Reason = reason,
                Category = ReportCategory.ItemDesign,
                CreateTime = DateTime.Now,
                ReportInfo = new ItemReportInfo(flag.ToString(), listingId),
            });
            Context.TrySaveChanges();
        }

        public void ReportHome(long characterId, string playerName, long reporterCharacterId, string reporterName, string reason, long homeId, int mapId, int plotId, HomeReportFlag flag) {
            Context.PlayerReports.Add(new PlayerReport {
                ReporterCharacterId = reporterCharacterId,
                ReporterName = reporterName,
                CharacterId = characterId,
                PlayerName = playerName,
                Reason = reason,
                Category = ReportCategory.Home,
                CreateTime = DateTime.Now,
                ReportInfo = new HomeReportInfo(flag.ToString(), homeId, mapId, plotId),
            });
            Context.TrySaveChanges();
        }

        public void ReportPet(long characterId, string playerName, long reporterCharacterId, string reporterName, string reason, string petName, PetReportFlag flag) {
            Context.PlayerReports.Add(new PlayerReport {
                ReporterCharacterId = reporterCharacterId,
                ReporterName = reporterName,
                CharacterId = characterId,
                PlayerName = playerName,
                Reason = reason,
                Category = ReportCategory.Pet,
                CreateTime = DateTime.Now,
                ReportInfo = new PetReportInfo(flag.ToString(), petName),
            });
            Context.TrySaveChanges();
        }
    }
}
