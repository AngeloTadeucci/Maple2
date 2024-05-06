using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class FishEntry(int id) : IByteSerializable {

    public int Id = id;
    public int TotalCaught;
    public int TotalPrizeFish;
    public int LargestSize;

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteInt(TotalCaught);
        writer.WriteInt(TotalPrizeFish);
        writer.WriteInt(LargestSize);
    }
}
