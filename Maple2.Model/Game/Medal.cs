using System.Runtime.InteropServices;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class Medal : IByteSerializable {
    public readonly int Id;
    public readonly MedalType Type;
    public short Slot = -1;
    public long ExpiryTime;

    public Medal(int id, MedalType type) {
        Id = id;
        Type = type;
    }
    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteLong(ExpiryTime);
    }
}
