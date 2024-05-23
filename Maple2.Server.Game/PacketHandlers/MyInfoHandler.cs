using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class MyInfoHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.MyInfo;

    private enum Command : byte {
        SetMotto = 0,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.SetMotto:
                HandleSetMotto(session, packet);
                return;
        }
    }

    private void HandleSetMotto(GameSession session, IByteReader packet) {
        string motto = packet.ReadUnicodeString();
        if (motto.Length > Constant.MaxMottoLength) {
            session.Send(MyInfoPacket.Error($"The motto can only be up to {Constant.MaxMottoLength} letters long."));
            return;
        }

        session.Player.Value.Character.Motto = motto;
        session.Field?.Broadcast(MyInfoPacket.UpdateMotto(session.Player));

        session.PlayerInfo.SendUpdate(new PlayerUpdateRequest {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Motto = motto,
            Async = true,
        });
        session.Player.Flag |= PlayerObjectFlag.Motto;
    }
}
