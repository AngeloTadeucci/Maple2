using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Server.Game.Model.Skill;

public class TargetRecord : IByteSerializable {
    public long PrevUid;
    public long Uid;
    public int TargetId;
    public byte Unknown;
    public byte Index;

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(PrevUid);
        writer.WriteLong(Uid);
        writer.WriteInt(TargetId);
        writer.WriteByte(Unknown);
        writer.WriteByte(Index);
    }
}
