using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Maple2.Model.Enum;

namespace Maple2.Database.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "!")]
[JsonDerivedType(typeof(ItemUgc), typeDiscriminator: "ugc")]
[JsonDerivedType(typeof(ItemPet), typeDiscriminator: "pet")]
[JsonDerivedType(typeof(ItemCustomMusicScore), typeDiscriminator: "music")]
[JsonDerivedType(typeof(ItemBadge), typeDiscriminator: "badge")]
internal abstract record ItemSubType;

internal record ItemUgc(UgcItemLook Template, ItemBlueprint Blueprint) : ItemSubType;

internal record UgcItemLook(
    long Id,
    string FileName,
    string Name,
    long AccountId,
    long CharacterId,
    string Author,
    long CreationTime,
    string Url) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator UgcItemLook?(Maple2.Model.Game.UgcItemLook? other) {
        return other == null ? null : new UgcItemLook(other.Id, other.FileName, other.Name, other.AccountId, other.CharacterId,
            other.Author, other.CreationTime, other.Url);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.UgcItemLook?(UgcItemLook? other) {
        return other == null ? null : new Maple2.Model.Game.UgcItemLook {
            Id = other.Id,
            FileName = other.FileName,
            Name = other.Name,
            AccountId = other.AccountId,
            CharacterId = other.CharacterId,
            Author = other.Author,
            CreationTime = other.CreationTime,
            Url = other.Url,
        };
    }
}

internal record ItemBlueprint(
    long BlueprintUid,
    int Length,
    int Width,
    int Height,
    DateTimeOffset CreationTime,
    int Type,
    long AccountId,
    long CharacterId,
    string CharacterName) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemBlueprint?(Maple2.Model.Game.ItemBlueprint? other) {
        return other == null ? null : new ItemBlueprint(other.BlueprintUid, other.Length, other.Width, other.Height, other.CreationTime, (int) other.Type, other.AccountId, other.CharacterId, other.CharacterName);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemBlueprint?(ItemBlueprint? other) {
        return other == null ? null : new Maple2.Model.Game.ItemBlueprint {
            BlueprintUid = other.BlueprintUid,
            Length = other.Length,
            Width = other.Width,
            Height = other.Height,
            CreationTime = other.CreationTime,
            Type = (BlueprintType) other.Type,
            AccountId = other.AccountId,
            CharacterId = other.CharacterId,
            CharacterName = other.CharacterName,
        };
    }
}

internal record ItemPet(string Name, long Exp, int EvolvePoints, short Level, short RenameRemaining) : ItemSubType {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemPet?(Maple2.Model.Game.ItemPet? other) {
        return other == null ? null : new ItemPet(other.Name, other.Exp, other.EvolvePoints, other.Level, other.RenameRemaining);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemPet?(ItemPet? other) {
        return other == null ? null : new Maple2.Model.Game.ItemPet {
            Name = other.Name,
            Exp = other.Exp,
            EvolvePoints = other.EvolvePoints,
            Level = other.Level,
            RenameRemaining = other.RenameRemaining,
        };
    }
}

internal record ItemCustomMusicScore(
    int Length,
    Instrument Instrument,
    string Title,
    string Author,
    long AuthorId,
    bool IsLocked,
    string Mml) : ItemSubType {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemCustomMusicScore?(Maple2.Model.Game.ItemCustomMusicScore? other) {
        return other == null ? null : new ItemCustomMusicScore(other.Length, other.Instrument, other.Title,
            other.Author, other.AuthorId, other.IsLocked, other.Mml);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemCustomMusicScore?(ItemCustomMusicScore? other) {
        return other == null ? null : new Maple2.Model.Game.ItemCustomMusicScore {
            Length = other.Length,
            Instrument = other.Instrument,
            Title = other.Title,
            Author = other.Author,
            AuthorId = other.AuthorId,
            IsLocked = other.IsLocked,
            Mml = other.Mml,
        };
    }
}

internal record ItemBadge(int Id, bool[] Transparency, int PetSkinId) : ItemSubType {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemBadge?(Maple2.Model.Game.ItemBadge? other) {
        return other == null ? null : new ItemBadge(other.Id, other.Transparency, other.PetSkinId);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemBadge?(ItemBadge? other) {
        return other == null ? null : new Maple2.Model.Game.ItemBadge(other.Id, other.Transparency) {
            PetSkinId = other.PetSkinId,
        };
    }
}
