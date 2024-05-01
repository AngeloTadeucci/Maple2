using System;
using System.Linq;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class RevivalHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Revival;

    private enum Command : byte {
        SafeRevive = 0,
        InstantRevive = 2,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.SafeRevive:
                HandleSafeRevive(session);
                return;
            case Command.InstantRevive:
                HandleInstantRevive(session, packet);
                return;
        }
    }

    private void HandleSafeRevive(GameSession session) {
        session.Config.UpdateDeathPenalty((int) (Environment.TickCount64 + TimeSpan.FromMilliseconds(Constant.UserRevivalPaneltyTick).TotalMilliseconds));
        // send invincible buff?
        // send player to a spawn point
    }

    private void HandleInstantRevive(GameSession session, IByteReader packet) {
        bool useVoucher = packet.ReadBool();
    }
}
