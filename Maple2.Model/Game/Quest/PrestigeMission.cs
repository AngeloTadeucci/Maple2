using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class PrestigeMission : IByteSerializable {
    public long Id { get; init; }
    public long GainedLevels;
    public bool Awarded;

    public PrestigeMission(long id) {
        Id = id;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteLong(GainedLevels);
        writer.WriteBool(Awarded);
    }
}
