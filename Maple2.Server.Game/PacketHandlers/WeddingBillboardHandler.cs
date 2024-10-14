using System.Reflection.Metadata;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;
using Constant = Maple2.Model.Metadata.Constant;

namespace Maple2.Server.Game.PacketHandlers;

public class WeddingBillboardHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.WeddingBillboard;

    private enum Command : byte {
        Load = 4,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();

        switch (command) {
            case Command.Load:
                HandleLoad(session, packet);
                return;
        }
    }

    private static void HandleLoad(GameSession session, IByteReader packet) {
        int unknown = packet.ReadInt(); // 0

        using GameStorage.Request db = session.GameStorage.Context();
        IList<WeddingHall> halls = db.GetWeddingHalls().ToList();

        session.Send(WeddingBillboardPacket.Load(halls));
    }
}
