using System;
using System.Runtime.InteropServices;
using Maple2.Model.Enum;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
public readonly struct IngredientInfo(ItemTag tag, int amount) {
    public readonly int Unknown;
    public readonly ItemTag Tag = tag;
    public readonly int Amount = amount;

    public static IngredientInfo operator *(in IngredientInfo self, double ratio) {
        return new IngredientInfo(self.Tag, (int) Math.Round(self.Amount * ratio));
    }
}
