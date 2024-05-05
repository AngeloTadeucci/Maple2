using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ChatStickerHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.ChatSticker;

    private enum Command : byte {
        Load = 0,
        Use = 3,
        GroupChat = 4,
        Favorite = 5,
        Unfavorite = 6,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Use:
                HandleUse(session, packet);
                break;
            case Command.GroupChat:
                HandleGroupChat(session, packet);
                break;
            case Command.Favorite:
                HandleFavorite(session, packet);
                break;
            case Command.Unfavorite:
                HandleUnfavorite(session, packet);
                break;
        }
    }

    private void HandleUse(GameSession session, IByteReader packet) {
        int stickerId = packet.ReadInt();
        string html = packet.ReadUnicodeString();

        if (!TryUseSticker(session.Player.Value.Unlock, stickerId)) {
            return;
        }

        session.Send(ChatStickerPacket.Use(stickerId, html));
    }

    private void HandleGroupChat(GameSession session, IByteReader packet) {
        int stickerId = packet.ReadInt();
        string groupChatName = packet.ReadUnicodeString();

        if (!TryUseSticker(session.Player.Value.Unlock, stickerId)) {
            return;
        }

        session.Send(ChatStickerPacket.GroupChat(stickerId, groupChatName));
    }

    private void HandleFavorite(GameSession session, IByteReader packet) {
        int stickerId = packet.ReadInt();

        if (!session.Config.TryFavoriteChatSticker(stickerId)) {
            return;
        }

        session.Send(ChatStickerPacket.Favorite(stickerId));
    }

    private void HandleUnfavorite(GameSession session, IByteReader packet) {
        int stickerId = packet.ReadInt();

        if (!session.Config.TryUnfavoriteChatSticker(stickerId)) {
            return;
        }

        session.Send(ChatStickerPacket.Unfavorite(stickerId));
    }

    private bool TryUseSticker(Unlock unlock, int stickerId) {
        if (!TableMetadata.ChatStickerTable.Entries.TryGetValue(stickerId, out ChatStickerMetadata? metadata)) {
            return false;
        }

        return unlock.StickerSets.ContainsKey(metadata.GroupId) && unlock.StickerSets[metadata.GroupId] >= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
