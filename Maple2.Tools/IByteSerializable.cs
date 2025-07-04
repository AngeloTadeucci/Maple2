using Maple2.PacketLib.Tools;

namespace Maple2.Tools;

public interface IByteSerializable {
    public void WriteTo(IByteWriter writer);
}

public interface IByteDeserializable {
    public void ReadFrom(IByteReader reader);
}
