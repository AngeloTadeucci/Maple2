using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model.Shop;

internal class BeautyShop {
    public int Id { get; set; }
    public byte Unknown1 { get; set; }
    public byte Unknown2 { get; set; }
    public BeautyShopCategory Category { get; set; }
    public BeautyShopType ShopType { get; set; }
    public int ShopSubType { get; set; }
    public int VoucherId { get; set; }
    public int ServiceRewardItemId { get; set; }
    public ShopCurrencyType ServiceCostCurrencyType { get; set; }
    public int ServiceCostItemId { get; set; }
    public int ServiceCostAmount { get; set; }
    public ShopCurrencyType RecolorCostCurrencyType { get; set; }
    public int RecolorCostItemId { get; set; }
    public int RecolorCostAmount { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Shop.BeautyShop?(BeautyShop? other) {
        return other == null ? null : new Maple2.Model.Game.Shop.BeautyShop(other.Id) {
            Unknown1 = other.Unknown1,
            Unknown2 = other.Unknown2,
            Category = other.Category,
            ShopType = other.ShopType,
            ShopSubType = other.ShopSubType,
            VoucherId = other.VoucherId,
            ServiceRewardItemId = other.ServiceRewardItemId,
            ServiceCost = new Maple2.Model.Game.Shop.BeautyShopCost(other.ServiceCostCurrencyType, other.ServiceCostItemId, other.ServiceCostAmount),
            RecolorCost = new Maple2.Model.Game.Shop.BeautyShopCost(other.RecolorCostCurrencyType, other.RecolorCostItemId, other.RecolorCostAmount),
        };
    }

    public static void Configure(EntityTypeBuilder<BeautyShop> builder) {
        builder.ToTable("beauty-shop");
        builder.HasKey(shop => shop.Id);
    }
}
