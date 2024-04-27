using System;
using System.Linq;
using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Game.PacketHandlers;

public class UserChatHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.UserChat;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required WorldClient World { private get; init; }
    public required GameStorage GameStorage { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var type = packet.Read<ChatType>();
        string message = packet.ReadUnicodeString();
        string recipient = packet.ReadUnicodeString();
        long clubId = packet.ReadLong();

        // Handle ChatCommands
        if (message.StartsWith('/')) {
            session.CommandHandler.Invoke(message[1..]);
            return;
        }

        switch (type) {
            case ChatType.Normal:
                HandleNormal(session, message);
                return;
            case ChatType.WhisperTo:
                HandleWhisper(session, message, recipient);
                return;
            case ChatType.Party:
                HandleParty(session, message);
                return;
            case ChatType.Guild:
                HandleGuild(session, message);
                return;
            case ChatType.World:
                HandleWorld(session, message);
                return;
            case ChatType.Channel:
                HandleChannel(session, message);
                return;
            case ChatType.Super:
                HandleSuper(session, message);
                return;
            case ChatType.Club:
                HandleClub(session, message, clubId);
                return;
            case ChatType.Wedding:
                HandleWedding(session, message);
                return;
        }
    }

    private static void HandleNormal(GameSession session, string message) {
        session.Field?.Broadcast(ChatPacket.Message(session.Player.Value, ChatType.Normal, message));
    }

    private void HandleWhisper(GameSession session, string message, string recipient) {
        if (recipient == session.PlayerName) {
            session.Send(ChatPacket.Alert(StringCode.s_whisper_err_myself));
            return;
        }

        using GameStorage.Request db = GameStorage.Context();
        long characterId = db.GetCharacterId(recipient);
        if (!session.PlayerInfo.GetOrFetch(characterId, out PlayerInfo? info)) {
            session.Send(ChatPacket.Alert(StringCode.s_whisper_err_target));
            return;
        }

        var request = new ChatRequest {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Name = session.PlayerName,
            Message = message,
            Whisper = new ChatRequest.Types.Whisper { RecipientId = info.CharacterId, RecipientName = recipient },
        };

        try {
            World.Chat(request);
            session.Send(ChatPacket.Whisper(info.AccountId, info.CharacterId, info.Name, request.Message));
        } catch (RpcException ex) {
            switch (ex.StatusCode) {
                case StatusCode.NotFound:
                    session.Send(ChatPacket.Alert(StringCode.s_err_cannot_find_user));
                    return;
                case StatusCode.PermissionDenied:
                    session.Send(ChatPacket.WhisperReject(recipient));
                    return;
                case StatusCode.Unavailable:
                    return;
            }
        }
    }

    private void HandleParty(GameSession session, string message) {
        var request = new ChatRequest {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Name = session.PlayerName,
            Message = message,
            Party = new ChatRequest.Types.Party {
                PartyId = session.Party.Id,
            },
        };

        try {
            World.Chat(request);
        } catch (RpcException) { }
    }

    private void HandleGuild(GameSession session, string message) {
        var request = new ChatRequest {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Name = session.PlayerName,
            Message = message,
            Guild = new ChatRequest.Types.Guild { GuildId = 0 },
        };

        try {
            World.Chat(request);
        } catch (RpcException) { }
    }

    private void HandleWorld(GameSession session, string message) {
        var voucher = session.Item.Inventory.Filter(item => item.Metadata.Property.Tag == ItemTag.FreeWorldChatCoupon).FirstOrDefault();
        if (voucher != null) {
            if (!session.Item.Inventory.Consume(voucher.Uid, 1)) {
                session.Send(NoticePacket.Notice(NoticePacket.Flags.Alert | NoticePacket.Flags.Message, StringCode.s_err_invalid_item));
                return;
            }
            session.Send(NoticePacket.Notice(NoticePacket.Flags.Alert | NoticePacket.Flags.Message, StringCode.s_worldchat_use_coupon));
        } else {
            int meretCost = Constant.MeretConsumeWorldChat;
            if (session.FindEvent<SaleChat>()?.EventInfo is SaleChat gameEvent) {
                meretCost -= (int) (meretCost * Convert.ToSingle(gameEvent.WorldChatDiscount) / 10000);
            }

            if (session.Currency.Meret < meretCost) {
                session.Send(ChatPacket.Alert(StringCode.s_err_lack_merat));
                return;
            }
            session.Currency.Meret -= meretCost;
        }

        var request = new ChatRequest {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Name = session.PlayerName,
            Message = message,
            World = new ChatRequest.Types.World(),
        };

        try {
            World.Chat(request);
        } catch (RpcException) { }
    }

    private void HandleChannel(GameSession session, string message) {
        var voucher = session.Item.Inventory.Filter(item => item.Metadata.Property.Tag == ItemTag.FreeChannelChatCoupon).FirstOrDefault();
        if (voucher != null) {
            if (!session.Item.Inventory.Consume(voucher.Uid, 1)) {
                session.Send(NoticePacket.Notice(NoticePacket.Flags.Alert | NoticePacket.Flags.Message, StringCode.s_err_invalid_item));
                return;
            }
            session.Send(NoticePacket.Notice(NoticePacket.Flags.Alert | NoticePacket.Flags.Message, StringCode.s_channelchat_use_coupon));
        } else {
            int meretCost = Constant.MeretConsumeChannelChat;
            if (session.FindEvent<SaleChat>()?.EventInfo is SaleChat gameEvent) {
                meretCost -= (int) (meretCost * Convert.ToSingle(gameEvent.ChannelChatDiscount) / 10000);
            }

            if (session.Currency.Meret < meretCost) {
                session.Send(ChatPacket.Alert(StringCode.s_err_lack_merat));
                return;
            }
            session.Currency.Meret -= meretCost;
        }

        session.ChannelBroadcast(ChatPacket.Message(session.AccountId, session.CharacterId, session.PlayerName, ChatType.Channel, message));
    }

    private void HandleSuper(GameSession session, string message) {
        if (session.SuperChatId == 0) {
            return;
        }

        Item? superChatItem = session.Item.Inventory.Find(session.SuperChatItemId).FirstOrDefault();
        if (superChatItem == null) {
            session.SuperChatId = 0;
            session.SuperChatItemId = 0;
            session.Send(SuperChatPacket.Deselect(session.Player.ObjectId));
            session.Send(ChatPacket.Alert(StringCode.s_err_lack_super_coupon));
            return;
        }

        var request = new ChatRequest {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Name = session.PlayerName,
            Message = message,
            Super = new ChatRequest.Types.Super { ItemId = session.SuperChatId },
        };

        if (session.Item.Inventory.Consume(superChatItem.Uid, 1)) {
            try {
                session.SuperChatId = 0;
                session.SuperChatItemId = 0;
                session.Send(SuperChatPacket.Deselect(session.Player.ObjectId));
                World.Chat(request);
            } catch (RpcException) { }
        }

    }

    private void HandleClub(GameSession session, string message, long clubId) {
        var request = new ChatRequest {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Name = session.PlayerName,
            Message = message,
            Club = new ChatRequest.Types.Club { ClubId = clubId },
        };

        try {
            World.Chat(request);
        } catch (RpcException) { }
    }

    private void HandleWedding(GameSession session, string message) {
        var request = new ChatRequest {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Name = session.PlayerName,
            Message = message,
            Wedding = new ChatRequest.Types.Wedding { ItemId = 0 },
        };

        try {
            World.Chat(request);
        } catch (RpcException) { }
    }
}
