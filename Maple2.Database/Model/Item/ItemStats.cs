using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Database.Model;

internal record ItemStats(Dictionary<BasicAttribute, BasicOption>[] BasicOption,
        Dictionary<SpecialAttribute, SpecialOption>[] SpecialOption) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemStats?(Maple2.Model.Game.ItemStats? other) {
        if (other == null) {
            return null;
        }

        Maple2.Model.Game.ItemStats.Type[] values = Enum.GetValues<Maple2.Model.Game.ItemStats.Type>();
        var basicOption = new Dictionary<BasicAttribute, BasicOption>[values.Length];
        var specialOption = new Dictionary<SpecialAttribute, SpecialOption>[values.Length];
        foreach (Maple2.Model.Game.ItemStats.Type type in values) {
            basicOption[(int) type] = other[type].Basic;
            specialOption[(int) type] = other[type].Special;
        }

        return new ItemStats(basicOption, specialOption);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemStats?(ItemStats? other) {
        return other == null ? null : new Maple2.Model.Game.ItemStats(other.BasicOption, other.SpecialOption);
    }
}

internal record ItemEnchant(int Enchants, int EnchantExp, byte EnchantCharges, bool Tradeable, int Charges,
        Dictionary<BasicAttribute, BasicOption> BasicOptions) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemEnchant?(Maple2.Model.Game.ItemEnchant? other) {
        return other == null ? null : new ItemEnchant(other.Enchants, other.EnchantExp, other.EnchantCharges,
            other.Tradeable, other.Charges, other.BasicOptions);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemEnchant?(ItemEnchant? other) {
        return other == null ? null : new Maple2.Model.Game.ItemEnchant(other.Enchants, other.EnchantExp,
            other.EnchantCharges, other.Tradeable, other.Charges, other.BasicOptions);
    }
}

internal record ItemLimitBreak(int Level, IDictionary<BasicAttribute, BasicOption> BasicOptions,
        IDictionary<SpecialAttribute, SpecialOption> SpecialOptions) {
    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ItemLimitBreak?(Maple2.Model.Game.ItemLimitBreak? other) {
        return other == null ? null : new ItemLimitBreak(other.Level, other.BasicOptions, other.SpecialOptions);
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.ItemLimitBreak?(ItemLimitBreak? other) {
        return other == null ? null :
            new Maple2.Model.Game.ItemLimitBreak(other.Level, other.BasicOptions, other.SpecialOptions);
    }
}
