using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class SkillMacroPacket {
    private enum Command : byte {
        Update = 0,
        Load = 2,
    }

    public static ByteWriter Update(ICollection<SkillMacro> skillMacros) {
        var pWriter = Packet.Of(SendOp.SkillMacro);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteInt(skillMacros.Count);
        foreach (SkillMacro macro in skillMacros) {
            pWriter.WriteClass<SkillMacro>(macro);
        }

        return pWriter;
    }

    public static ByteWriter Load(ICollection<SkillMacro> skillMacros) {
        var pWriter = Packet.Of(SendOp.SkillMacro);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(skillMacros.Count);
        foreach (SkillMacro macro in skillMacros) {
            pWriter.WriteClass<SkillMacro>(macro);
        }

        return pWriter;
    }
}
