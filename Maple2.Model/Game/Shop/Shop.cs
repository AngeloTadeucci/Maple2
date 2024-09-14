using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Shop;

public class Shop : IByteSerializable {
    public int Id => Metadata.Id;
    public ShopMetadata Metadata;
    public ShopRestockData RestockData => Metadata.RestockData;
    public long RestockTime;
    public int RestockCount;
    public SortedDictionary<int, ShopItem> Items;

    public Shop(ShopMetadata metadata) {
        Metadata = metadata;
        RestockTime = metadata.RestockTime;
        Items = new SortedDictionary<int, ShopItem>();
    }

    public virtual void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteLong(RestockTime);
        writer.WriteInt();
        writer.WriteShort((short) Items.Count);
        writer.WriteInt(Metadata.CategoryId);
        writer.WriteBool(Metadata.OpenWallet);
        writer.WriteBool(Metadata.IsOnlySell);
        writer.WriteBool(Metadata.EnableReset);
        writer.WriteBool(Metadata.DisableDisplayOrderSort);
        writer.Write<ShopFrameType>(Metadata.FrameType);
        writer.WriteBool(Metadata.DisplayOnlyUsable);
        writer.WriteBool(Metadata.HideStats);
        writer.WriteBool(false);
        writer.WriteBool(Metadata.DisplayNew);
        writer.WriteString(Metadata.Name);
        if (Metadata.EnableReset) {
            writer.Write<ShopCurrencyType>(RestockData.CurrencyType);
            writer.Write<ShopCurrencyType>(RestockData.ExcessCurrencyType);
            writer.WriteInt();
            writer.WriteInt(RestockData.Price);
            writer.WriteBool(RestockData.EnablePriceMultiplier);
            writer.WriteInt(RestockCount);
            writer.Write<ResetType>(RestockData.ResetType);
            writer.WriteBool(RestockData.DisableInstantRestock);
            writer.WriteBool(RestockData.AccountWide);;
        }
    }
}
