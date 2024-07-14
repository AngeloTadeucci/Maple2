using System;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class Item {
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public int ItemId { get; set; }
    public int Rarity { get; set; }
    public short Slot { get; set; } = -1;
    public ItemGroup Group { get; set; } = ItemGroup.Default;
    public int Amount { get; set; } = 1;
    public DateTime ExpiryTime { get; set; }
    public int TimeChangedOption { get; set; }
    public int RemainUses { get; set; }
    public bool IsLocked { get; set; }
    public long UnlockTime { get; set; }
    public short GlamorForges { get; set; }
    public int GachaDismantleId { get; set; }

    public ItemAppearance? Appearance { get; set; }
    public ItemStats? Stats { get; set; }
    public ItemEnchant? Enchant { get; set; }
    public ItemLimitBreak? LimitBreak { get; set; }

    public ItemTransfer? Transfer { get; set; }
    public ItemSocket? Socket { get; set; }
    public ItemCoupleInfo? CoupleInfo { get; set; }
    public ItemBinding? Binding { get; set; }

    public ItemSubType? SubType { get; set; }

    public DateTime CreationTime { get; set; }
    public DateTime LastModified { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Item?(Maple2.Model.Game.Item? other) {
        if (other == null) {
            return null;
        }

        var item = new Item {
            Id = other.Uid,
            ItemId = other.Id,
            Rarity = other.Rarity,
            Slot = other.Slot,
            Group = other.Group,
            Amount = other.Amount,
            CreationTime = other.CreationTime.FromEpochSeconds(),
            ExpiryTime = other.ExpiryTime.FromEpochSeconds(),
            TimeChangedOption = other.TimeChangedOption,
            RemainUses = other.RemainUses,
            IsLocked = other.IsLocked,
            UnlockTime = other.UnlockTime,
            GlamorForges = other.GlamorForges,
            GachaDismantleId = other.GachaDismantleId,
            Appearance = other.Appearance == null ? null : other.Appearance switch {
                Maple2.Model.Game.HairAppearance hair => (HairAppearance) hair,
                Maple2.Model.Game.DecalAppearance decal => (DecalAppearance) decal,
                Maple2.Model.Game.CapAppearance cap => (CapAppearance) cap,
                _ => (ColorAppearance) other.Appearance,
            },
            Stats = other.Stats,
            Enchant = other.Enchant,
            LimitBreak = other.LimitBreak,
            Transfer = other.Transfer,
            Socket = other.Socket,
            CoupleInfo = other.CoupleInfo,
            Binding = other.Binding,
        };

        if (other.Template != null && other.Blueprint != null) {
            item.SubType = new ItemUgc(other.Template, other.Blueprint);
        } else if (other.Pet != null) {
            item.SubType = (ItemPet) other.Pet;
        } else if (other.Music != null) {
            item.SubType = (ItemCustomMusicScore) other.Music;
        } else if (other.Badge != null) {
            item.SubType = (ItemBadge) other.Badge;
        }

        return item;
    }

    // Use explicit Convert() here because we need metadata to construct Item.
    public Maple2.Model.Game.Item Convert(ItemMetadata metadata) {
        var item = new Maple2.Model.Game.Item(metadata, Rarity, Amount, false) {
            Uid = Id,
            Slot = Slot,
            Group = Group,
            CreationTime = CreationTime.ToEpochSeconds(),
            ExpiryTime = ExpiryTime.ToEpochSeconds(),
            TimeChangedOption = TimeChangedOption,
            RemainUses = RemainUses,
            IsLocked = IsLocked,
            UnlockTime = UnlockTime,
            GlamorForges = GlamorForges,
            GachaDismantleId = GachaDismantleId,
            Appearance = Appearance switch {
                HairAppearance hair => hair,
                DecalAppearance decal => decal,
                CapAppearance cap => cap,
                ColorAppearance color => color,
                _ => new Maple2.Model.Game.ItemAppearance(default),
            },
            Stats = Stats,
            Enchant = Enchant,
            LimitBreak = LimitBreak,
            Transfer = Transfer,
            Socket = Socket,
            CoupleInfo = CoupleInfo,
            Binding = Binding,
        };

        switch (SubType) {
            case ItemUgc(var ugcItemLook, var itemBlueprint):
                item.Template = ugcItemLook;
                item.Blueprint = itemBlueprint;
                break;
            case ItemPet pet:
                item.Pet = pet;
                break;
            case ItemCustomMusicScore music:
                item.Music = music;
                break;
            case ItemBadge badge:
                item.Badge = badge;
                break;
        }

        return item;
    }

    public static void Configure(EntityTypeBuilder<Item> builder) {
        builder.ToTable("item");
        builder.HasKey(item => item.Id);
        builder.Property(character => character.CreationTime)
            .ValueGeneratedOnAdd();

        builder.Property(item => item.Appearance).HasJsonConversion().IsRequired();
        builder.Property(item => item.Stats).HasJsonConversion();
        builder.Property(item => item.Enchant).HasJsonConversion();
        builder.Property(item => item.LimitBreak).HasJsonConversion();
        builder.Property(item => item.Transfer).HasJsonConversion();
        builder.Property(item => item.Socket).HasJsonConversion();
        builder.Property(item => item.CoupleInfo).HasJsonConversion();
        builder.Property(item => item.Binding).HasJsonConversion();
        builder.Property(item => item.SubType).HasJsonConversion();

        builder.Property(item => item.LastModified).ValueGeneratedOnAdd();
        IMutableProperty creationTime = builder.Property(item => item.CreationTime)
            .ValueGeneratedOnAdd().Metadata;
        creationTime.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
