using System.Collections.Generic;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record BonusGameTable(
    IReadOnlyDictionary<int, BonusGameTable.Game> Games,
    IReadOnlyDictionary<int, BonusGameTable.Drop> Drops) : ServerTable {


    public record Game(
        int Id,
        ItemComponent ConsumeItem,
        BonusGameTable.Game.Slot[] Slots) {

        public record Slot(
            int MinProp,
            int MaxProp);
    }

    public record Drop(
        int Id,
        BonusGameTable.Drop.Item[] Items) {

        public record Item(
            ItemComponent ItemComponent,
            int Probability,
            bool Notice);
    }
}
