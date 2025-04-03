using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class RequestReportHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RequestReport;

    public override void Handle(GameSession session, IByteReader packet) {
        if (session.ReportCooldown > 0 && session.ReportCooldown.FromEpochSeconds().AddMinutes(1) > DateTime.Now) {
            return;
        }

        string playerName = packet.ReadUnicodeString();
        using GameStorage.Request db = session.GameStorage.Context();
        long characterId = db.GetCharacterId(playerName);
        if (characterId == 0) {
            Logger.Error($"Failed to find character '{playerName}'");
            return;
        }
        string reason = packet.ReadUnicodeString();
        var reportCategory = packet.Read<ReportCategory>();
        switch (reportCategory) {
            case ReportCategory.Player:
                var playerFlag = packet.Read<PlayerReportFlag>();
                db.ReportPlayer(characterId, playerName, session.CharacterId, session.PlayerName, reason, playerFlag);
                break;
            case ReportCategory.Chat:
                var chatFlag = packet.Read<ChatReportFlag>();
                string chatMessage = packet.ReadUnicodeString();
                db.ReportChat(characterId, playerName, session.CharacterId, session.PlayerName, reason, chatMessage, chatFlag);
                break;
            case ReportCategory.Poster:
                var posterFlag = packet.Read<PosterReportFlag>();
                int posterId = packet.ReadInt();
                string templateId = packet.ReadUnicodeString();
                db.ReportPoster(characterId, playerName, session.CharacterId, session.PlayerName, reason, posterId, templateId, posterFlag);
                break;
            case ReportCategory.ItemDesign:
                var designItemFlag = packet.Read<DesignItemReportFlag>();
                long listingId = packet.ReadLong();
                db.ReportDesignItem(characterId, playerName, session.CharacterId, session.PlayerName, reason, listingId, designItemFlag);
                break;
            case ReportCategory.Home:
                var homeFlag = packet.Read<HomeReportFlag>();
                long homeId = packet.ReadLong();
                int mapId = packet.ReadInt();
                int plotId = packet.ReadInt();
                db.ReportHome(characterId, playerName, session.CharacterId, session.PlayerName, reason, homeId, mapId, plotId, homeFlag);
                break;
            case ReportCategory.Pet:
                var petFlag = packet.Read<PetReportFlag>();
                string petName = packet.ReadUnicodeString();
                db.ReportPet(characterId, playerName, session.CharacterId, session.PlayerName, reason, petName, petFlag);
                break;
            default:
                Logger.Error($"Unknown report category: {reportCategory}");
                return;
        }

        session.ReportCooldown = DateTime.Now.ToEpochSeconds();
    }
}
