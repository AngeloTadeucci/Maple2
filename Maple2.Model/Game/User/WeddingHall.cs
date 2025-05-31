using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class WeddingHall : IByteSerializable, IByteDeserializable {
    public static WeddingHall Default => new() {
        Id = 0,
        MarriageId = 0,
        ReserverAccountId = 0,
        ReserverCharacterId = 0,
        ReserverName = string.Empty,
        PartnerName = string.Empty,
        CeremonyTime = 0,
        PackageId = 0,
        PackageHallId = 0,
        Public = false,
    };
    public required long Id { get; set; }
    public required long MarriageId { get; init; }
    public long CeremonyTime { get; set; }
    public int PackageId { get; set; }
    public int PackageHallId { get; set; }
    public bool Public { get; set; }
    public required long ReserverAccountId { get; set; }
    public required long ReserverCharacterId { get; set; }
    public required string ReserverName { get; set; } = string.Empty;
    public required string PartnerName { get; set; } = string.Empty;
    public Dictionary<long, long> GuestList { get; set; } = new(); // CharacterId, AccountId
    public long CreationTime { get; set; }

    public void ReadFrom(IByteReader reader) {
        Id = reader.ReadLong();
        PackageId = reader.ReadInt();
        PackageHallId = reader.ReadInt();
        CeremonyTime = reader.ReadLong();
        Public = reader.ReadBool();
        ReserverAccountId = reader.ReadLong();
        ReserverCharacterId = reader.ReadLong();
        ReserverName = reader.ReadUnicodeString();
        PartnerName = reader.ReadUnicodeString();
        reader.ReadLong();
        reader.ReadLong();
        reader.ReadLong();
        reader.ReadLong();
        reader.ReadLong();
        reader.ReadInt();

        ReserverName = string.Empty;
        PartnerName = string.Empty;
        GuestList = [];
        /*int guestCount = reader.ReadInt();
        for (int i = 0; i < guestCount; i++) {
            long characterId = reader.ReadLong();
            long accountId = reader.ReadLong();
            GuestList[characterId] = accountId;
        }*/
    }
    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteInt(PackageId);
        writer.WriteInt(PackageHallId);
        writer.WriteLong(CeremonyTime);
        writer.WriteBool(Public);
        writer.WriteLong(ReserverAccountId);
        writer.WriteLong(ReserverCharacterId);
        writer.WriteUnicodeString(ReserverName);
        writer.WriteUnicodeString(PartnerName);

        // WeddingHallReservationFee
        writer.WriteLong();
        writer.WriteLong();
        writer.WriteLong();
        writer.WriteLong();

        writer.WriteLong();
        writer.WriteInt();

        writer.WriteInt(GuestList.Count);
        foreach ((long characterId, long accountId) in GuestList) {
            writer.WriteLong(accountId);
            writer.WriteLong(characterId);
        }
    }
}
