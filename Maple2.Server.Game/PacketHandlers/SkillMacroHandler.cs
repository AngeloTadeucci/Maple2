using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.PacketHandlers;

public class SkillMacroHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.SkillMacro;

    private const int TOTAL_SKILL_MACROS = 3;

    private enum Command : byte {
        Load = 0,
        Update = 1,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();

        switch (command) {
            case Command.Load:
                HandleLoad(session);
                return;
            case Command.Update:
                HandleUpdate(session, packet);
                return;
        }
    }

    private void HandleLoad(GameSession session) {
        session.Config.LoadMacros();
    }

    private void HandleUpdate(GameSession session, IByteReader packet) {
        int count = packet.ReadInt();
        if (count > TOTAL_SKILL_MACROS) {
            return;
        }

        var macros = new List<SkillMacro>();
        for (int i = 0; i < count; i++) {
            macros.Add(packet.ReadClass<SkillMacro>());
        }

        session.Config.UpdateMacros(macros);
    }
}
