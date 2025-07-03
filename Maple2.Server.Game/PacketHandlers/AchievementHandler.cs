using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class AchievementHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Achieve;

    private enum Command : byte {
        Reward = 3,
        ToggleFavorite = 4,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Reward:
                HandleReward(session, packet);
                break;
            case Command.ToggleFavorite:
                HandleToggleFavorite(session, packet);
                break;
        }
    }

    private void HandleReward(GameSession session, IByteReader packet) {
        int achievementId = packet.ReadInt();

        session.Achievement.ClaimReward(achievementId);
    }

    private void HandleToggleFavorite(GameSession session, IByteReader packet) {
        int achievementId = packet.ReadInt();
        bool favorite = packet.ReadBool();

        if (!session.Achievement.TryGetAchievement(achievementId, out Achievement? achievement)) {
            return;
        }

        achievement.Favorite = favorite;
    }
}
