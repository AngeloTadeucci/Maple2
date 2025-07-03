
using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Web.Packet;

public static class MentorPacket {
    public static ByteWriter MenteeList(IList<PlayerInfo> mentees) {
        var pWriter = new ByteWriter();
        pWriter.WriteInt(0); // 0
        pWriter.WriteInt(mentees.Count);

        foreach (PlayerInfo mentee in mentees) {
            pWriter.WriteLong(mentee.CharacterId);
            pWriter.WriteLong(mentee.AccountId);
            pWriter.WriteUnicodeStringWithLength(mentee.Name);
            pWriter.WriteUnicodeStringWithLength(mentee.Picture);
            pWriter.WriteInt(mentee.Level);
            pWriter.Write<Job>(mentee.Job);
            pWriter.WriteInt(mentee.Online ? 1 : 0);
        }

        return pWriter;
    }
}
