using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class MeretMarketSearch : IByteDeserializable {
    public int TabId { get; private set; }
    public GenderFilterFlag Gender { get; private set; }
    public JobFilterFlag Job { get; private set; }
    public MeretMarketSort SortBy { get; private set; }
    public string SearchString { get; private set; } = "";
    public int StartPage { get; private set; }
    public byte ItemsPerPage { get; set; }

    public void ReadFrom(IByteReader packet) {
        TabId = packet.ReadInt();
        Gender = packet.Read<GenderFilterFlag>();
        Job = packet.Read<JobFilterFlag>();
        SortBy = packet.Read<MeretMarketSort>();
        SearchString = packet.ReadUnicodeString();
        StartPage = packet.ReadInt(); // 1
        packet.ReadInt(); // 1
        packet.ReadByte(); // 1 on premium, 0 on design menu
        packet.ReadByte();
        ItemsPerPage = packet.ReadByte();
    }
}
