using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Ugc;

public class UgcBanner {
    public readonly long Id;
    public readonly int MapId;
    public readonly List<BannerSlot> Slots;

    public UgcBanner(long id, int mapId, List<BannerSlot> slots) {
        Id = id;
        MapId = mapId;
        Slots = slots;
    }
}

public class BannerSlot : IByteSerializable {
    public long Id;
    public readonly int Date;
    public readonly int Hour;
    public readonly long BannerId;
    public bool Active;
    public UgcItemLook? Template;
    public bool Expired;

    public readonly DateTimeOffset ActivateTime;

    public BannerSlot() { }

    public BannerSlot(long bannerId, int date, int hour) {
        BannerId = bannerId;
        Date = date;
        Hour = hour;

        int year = date / 10000;
        int month = date % 10000 / 100;
        int day = date % 100;

        ActivateTime = new(year, month, day, hour, 0, 0, TimeSpan.Zero);
    }

    public BannerSlot(long id, DateTimeOffset dateInUnixSeconds, long bannerId, UgcItemLook? template) {
        Id = id;
        ActivateTime = dateInUnixSeconds;

        Date = int.Parse(ActivateTime.ToString("yyyyMMdd"));
        Hour = ActivateTime.Hour;

        BannerId = bannerId;
        Template = template;
    }

    public void WriteTo(IByteWriter pWriter) {
        pWriter.WriteLong(Id);
        pWriter.WriteInt(Active ? 2 : 1); // idk
        pWriter.WriteLong(BannerId);
        pWriter.WriteInt(Date);
        pWriter.WriteInt(Hour);
        pWriter.WriteLong();
    }
}
