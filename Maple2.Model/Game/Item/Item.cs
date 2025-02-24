using System;
using System.Linq;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class Item : IByteSerializable, IByteDeserializable {
    public readonly ItemMetadata Metadata;
    public readonly InventoryType Inventory;
    public readonly ItemType Type;

    public long Uid { get; init; }
    public int Rarity { get; init; }
    public short Slot = -1;
    public ItemGroup Group = ItemGroup.Default;

    public int Id => Metadata.Id;
    public int Amount;

    public long CreationTime;
    public long ExpiryTime;

    public int TimeChangedOption;
    public int RemainUses;
    public bool IsLocked;
    public long UnlockTime;
    public short GlamorForges;
    public int GachaDismantleId;

    public ItemAppearance? Appearance;
    public ItemStats? Stats;
    public ItemEnchant? Enchant;
    public ItemLimitBreak? LimitBreak;

    public ItemTransfer? Transfer;
    public ItemSocket? Socket;
    public ItemCoupleInfo? CoupleInfo;
    public ItemBinding? Binding;

    #region Special Types
    public UgcItemLook? Template;
    public ItemBlueprint? Blueprint;
    public ItemPet? Pet;
    public ItemCustomMusicScore? Music;
    public ItemBadge? Badge;
    #endregion

    public Item(ItemMetadata metadata, int rarity = 1, int amount = 1, bool initialize = true) {
        Metadata = metadata;
        Rarity = rarity;
        Amount = amount;
        Inventory = Metadata.Inventory();
        Type = new ItemType(metadata.Id);

        // Skip initialization of fields, this is done if we will initialize separately.
        if (!initialize) {
            return;
        }

        GlamorForges = (short) Metadata.Limit.GlamorForgeCount;
        Appearance = Metadata.SlotNames.FirstOrDefault(EquipSlot.Unknown) switch {
            EquipSlot.HR => new HairAppearance(default),
            EquipSlot.FD => new DecalAppearance(default),
            EquipSlot.CP => new CapAppearance(default),
            _ => new ItemAppearance(default),
        };

        Transfer = new ItemTransfer(GetTransferFlag(), Metadata.Property.TradableCount);
        Enchant = new ItemEnchant(tradeable: Transfer.Flag.HasFlag(TransferFlag.Trade) || Transfer.Flag.HasFlag(TransferFlag.LimitTrade));

        ExpiryTime = GetExpiryTime();

        if (Metadata.Music != null) {
            RemainUses = Metadata.Music.PlayCount;
        }

        // Template? or Blueprint
        if (Metadata.Mesh != string.Empty || Metadata.Property.Type == 22) {
            Template = new UgcItemLook();
            Blueprint = new ItemBlueprint();
        } else if (Inventory == InventoryType.Pets) {
            Pet = new ItemPet();
        } else if (Metadata.Music?.IsCustomNote == true) {
            Music = new ItemCustomMusicScore();
        } else if (Inventory == InventoryType.Badge) {
            Badge = new ItemBadge(Id);
        }
    }

    public Item Clone() {
        return new Item(Metadata, Rarity, Amount, false) {
            Uid = Uid,
            CreationTime = CreationTime,
            ExpiryTime = ExpiryTime,
            TimeChangedOption = TimeChangedOption,
            RemainUses = RemainUses,
            IsLocked = IsLocked,
            UnlockTime = UnlockTime,
            GlamorForges = GlamorForges,
            GachaDismantleId = GachaDismantleId,
            Appearance = Appearance?.Clone(),
            Stats = Stats?.Clone(),
            Enchant = Enchant?.Clone(),
            LimitBreak = LimitBreak?.Clone(),
            Transfer = Transfer?.Clone(),
            Socket = Socket?.Clone(),
            CoupleInfo = CoupleInfo?.Clone(),
            Binding = Binding?.Clone(),
            Template = Template?.Clone(),
            Blueprint = Blueprint?.Clone(),
            Pet = Pet?.Clone(),
            Music = Music?.Clone(),
            Badge = Badge?.Clone(),
        };
    }

    public Item Mutate(ItemMetadata metadata, int? rarity = null) {
        return new Item(metadata, rarity ?? Rarity, Amount, false) {
            Uid = Uid,
            CreationTime = CreationTime,
            ExpiryTime = ExpiryTime,
            TimeChangedOption = TimeChangedOption,
            RemainUses = RemainUses,
            IsLocked = IsLocked,
            UnlockTime = UnlockTime,
            GlamorForges = GlamorForges,
            GachaDismantleId = GachaDismantleId,
            Appearance = Appearance,
            Stats = Stats,
            Enchant = Enchant,
            LimitBreak = LimitBreak,
            Transfer = Transfer,
            Socket = Socket,
            CoupleInfo = CoupleInfo,
            Binding = Binding,
            Template = Template,
            Blueprint = Blueprint,
            Pet = Pet,
            Music = Music,
            Badge = Badge,
        };
    }

    private TransferFlag GetTransferFlag() {
        bool zeroTrades = Metadata.Property.TradableCount <= 0;
        bool belowRarity = Rarity < Metadata.Limit.TradeMaxRarity;
        switch (Metadata.Limit.TransferType) {
            case TransferType.Tradable:
                if (belowRarity) {
                    return TransferFlag.Trade | TransferFlag.Split;
                }
                return zeroTrades ? TransferFlag.None : TransferFlag.LimitTrade;
            case TransferType.Untradeable:
                return zeroTrades ? TransferFlag.None : TransferFlag.LimitTrade;
            case TransferType.BindOnLoot:
            case TransferType.BindOnEquip:
            case TransferType.BindOnUse:
            case TransferType.BindOnTrade:
            case TransferType.BindPet: // summon/enchant/reroll
                var result = TransferFlag.Bind;
                if (zeroTrades) {
                    if (belowRarity) {
                        result |= TransferFlag.Trade | TransferFlag.Split;
                    }
                } else {
                    result |= TransferFlag.LimitTrade;
                }
                return result;
            case TransferType.BlackMarketOnly:
                if (!zeroTrades || belowRarity) {
                    return TransferFlag.Trade;
                }
                return zeroTrades ? TransferFlag.None : TransferFlag.LimitTrade;
            default:
                return TransferFlag.None;
        }
    }

    private long GetExpiryTime() {
        if (Metadata.Life.ExpirationTimestamp > 0) {
#if DEBUG
            return (long) (DateTime.Now.AddYears(1).ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
#else
            return Metadata.Life.ExpirationTimestamp;
#endif
        }

        if (Metadata.Life.ExpirationDuration > 0) {
            return (long) (DateTime.Now.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds + Metadata.Life.ExpirationDuration;
        }
        return 0;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Amount);
        writer.WriteInt(-1);
        writer.WriteLong(CreationTime);
        writer.WriteLong(ExpiryTime);
        writer.WriteLong();
        writer.WriteInt(TimeChangedOption);
        writer.WriteInt(RemainUses);
        writer.WriteBool(IsLocked);
        writer.WriteLong(UnlockTime);
        writer.WriteShort(GlamorForges);
        writer.WriteBool(false);
        writer.WriteInt(GachaDismantleId);

        writer.WriteClass<ItemAppearance>(Appearance ?? ItemAppearance.Default);
        writer.WriteClass<ItemStats>(Stats ?? ItemStats.Default);
        writer.WriteClass<ItemEnchant>(Enchant ?? ItemEnchant.Default);
        writer.WriteClass<ItemLimitBreak>(LimitBreak ?? ItemLimitBreak.Default);

        if (Template != null && Blueprint != null) {
            writer.WriteClass<UgcItemLook>(Template);
            writer.WriteClass<ItemBlueprint>(Blueprint);
        } else if (Pet != null) {
            writer.WriteClass<ItemPet>(Pet);
        } else if (Music != null) {
            writer.WriteClass<ItemCustomMusicScore>(Music);
        } else if (Badge != null) {
            writer.WriteClass<ItemBadge>(Badge); // TODO
        }

        writer.WriteClass<ItemTransfer>(Transfer ?? ItemTransfer.Default);
        writer.WriteClass<ItemSocket>(Socket ?? ItemSocket.Default);
        writer.WriteClass<ItemCoupleInfo>(CoupleInfo ?? ItemCoupleInfo.Default);
        writer.WriteClass<ItemBinding>(Binding ?? ItemBinding.Default);
    }

    public void ReadFrom(IByteReader reader) {
        Amount = reader.ReadInt();
        reader.ReadInt();
        CreationTime = reader.ReadLong();
        ExpiryTime = reader.ReadLong();
        reader.ReadLong();
        TimeChangedOption = reader.ReadInt();
        RemainUses = reader.ReadInt();
        IsLocked = reader.ReadBool();
        UnlockTime = reader.ReadLong();
        GlamorForges = reader.ReadShort();
        reader.ReadBool();
        reader.ReadInt();

        Appearance = reader.ReadClass<ItemAppearance>();
        Stats = reader.ReadClass<ItemStats>();
        Enchant = reader.ReadClass<ItemEnchant>();
        LimitBreak = reader.ReadClass<ItemLimitBreak>();

        if (Template != null && Blueprint != null) {
            Template = reader.ReadClass<UgcItemLook>();
            Blueprint = reader.ReadClass<ItemBlueprint>();
        } else if (Pet != null) {
            Pet = reader.ReadClass<ItemPet>();
        } else if (Music != null) {
            Music = reader.ReadClass<ItemCustomMusicScore>();
        } else if (Badge != null) {
            Badge = reader.ReadClass<ItemBadge>();
        }

        Transfer = reader.ReadClass<ItemTransfer>();
        Socket = reader.ReadClass<ItemSocket>();
        CoupleInfo = reader.ReadClass<ItemCoupleInfo>();
        Binding = reader.ReadClass<ItemBinding>();
    }
}
