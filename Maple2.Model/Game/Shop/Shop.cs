using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game.Shop;

public class Shop : IByteSerializable {
    public readonly int Id;
    public int CategoryId { get; init; }
    public string Name { get; init; }
    public ShopSkin Skin { get; init; }
    public bool HideUnuseable { get; init; }
    public bool HideStats { get; init; }
    public bool DisableBuyback { get; init; }
    public bool OpenWallet { get; init; }
    public bool DisplayNew { get; init; }
    public bool RandomizeOrder { get; init; }
    public long RestockTime { get; set; }
    public ShopRestockData? RestockData { get; init; }
    public SortedDictionary<int, ShopItem> Items;

    public Shop(int id) {
        Id = id;
        Items = new SortedDictionary<int, ShopItem>();
    }

    /// <summary>
    /// Clones shops for instanced player shops.
    /// </summary>
    public Shop Clone() {
        return new Shop(Id) {
            CategoryId = CategoryId,
            Name = Name,
            Skin = Skin,
            HideUnuseable = HideUnuseable,
            HideStats = HideStats,
            DisableBuyback = DisableBuyback,
            OpenWallet = OpenWallet,
            DisplayNew = DisplayNew,
            RandomizeOrder = RandomizeOrder,
            RestockData = RestockData,
        };
    }

    public virtual void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteLong(RestockTime);
        writer.WriteInt();
        writer.WriteShort((short) Items.Count);
        writer.WriteInt(CategoryId);
        writer.WriteBool(OpenWallet);
        writer.WriteBool(DisableBuyback);
        writer.WriteBool(RestockTime > 0);
        writer.WriteBool(RandomizeOrder);
        writer.Write<ShopSkin>(Skin);
        writer.Write(HideUnuseable);
        writer.WriteBool(HideStats);
        writer.WriteBool(false);
        writer.WriteBool(DisplayNew);
        writer.WriteString(Name);
        if (RestockTime > 0 && RestockData != null) {
            writer.WriteClass<ShopRestockData>(RestockData);
        }
    }
}
