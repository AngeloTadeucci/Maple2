using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class FishEntry : IByteSerializable {
    public int Id;
    public int TotalCaught;
    public int TotalPrizeFish;
    public int LargestSize;

    public FishEntry(int id) {
        Id = id;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteInt(TotalCaught);
        writer.WriteInt(TotalPrizeFish);
        writer.WriteInt(LargestSize);
    }
}
