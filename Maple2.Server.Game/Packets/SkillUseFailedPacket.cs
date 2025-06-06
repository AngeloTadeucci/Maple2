﻿using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model.Skill;

namespace Maple2.Server.Game.Packets;

public static class SkillUseFailedPacket {
    private enum Command : byte {
        Fail = 5,
        Unknown = 6,
    }

    public static ByteWriter Fail(SkillRecord record) {
        var pWriter = Packet.Of(SendOp.SkillUseFailed);
        pWriter.Write<Command>(Command.Fail);
        pWriter.WriteLong(record.CastUid);
        pWriter.WriteInt(record.Caster.ObjectId);
        pWriter.WriteByte(); // Maybe motion point?
        pWriter.WriteInt(record.ServerTick);
        pWriter.WriteShort();
        pWriter.WriteInt();

        return pWriter;
    }
}
