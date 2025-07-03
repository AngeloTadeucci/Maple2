using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class GuildPoster : IByteSerializable {
    public int Id;
    public string Picture = string.Empty;
    public long OwnerId;
    public string OwnerName = string.Empty;
    public long ResourceId;

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteUnicodeString(Picture);
        writer.WriteLong(OwnerId);
        writer.WriteUnicodeString(OwnerName);
    }
}
