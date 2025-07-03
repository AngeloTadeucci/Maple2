﻿using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class HomeBankHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.RequestHomeBank;

    private enum Command : byte {
        Home = 1,
        Premium = 2,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Home:
                long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (session.Player.Value.Character.StorageCooldown + Constant.HomeBankCallCooldown > time) {
                    return;
                }

                session.Player.Value.Character.StorageCooldown = time;
                session.Send(HomeBank(time));
                return;
            case Command.Premium:
                session.Send(HomeBank());
                return;
        }
    }

    private static ByteWriter HomeBank(long time = 0) {
        var pWriter = Packet.Of(SendOp.HomeBank);
        pWriter.WriteLong(time);

        return pWriter;
    }
}
