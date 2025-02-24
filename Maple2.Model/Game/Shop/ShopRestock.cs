using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Shop;

public class ShopRestock : IByteSerializable {

    public ShopRestockData Metadata;
    public int RestockCount;
    public ShopRestock(ShopRestockData metadata) {
        Metadata = metadata;
    }

    public void WriteTo(IByteWriter writer) {
        writer.Write<ShopCurrencyType>(Metadata.CurrencyType);
        writer.Write<ShopCurrencyType>(Metadata.ExcessCurrencyType);
        writer.WriteInt();
        writer.WriteInt(Metadata.Price);
        writer.WriteBool(Metadata.EnablePriceMultiplier);
        writer.WriteInt(RestockCount);
        writer.Write<ResetType>(Metadata.ResetType);
        writer.WriteBool(Metadata.DisableInstantRestock);
        writer.WriteBool(Metadata.AccountWide);
    }
}
