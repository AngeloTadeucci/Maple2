using Maple2.Model.Enum;

namespace Maple2.Server.Core.Config;

public sealed class ServerSettings {
    public RatesSection Rates { get; init; } = new();
    public EconomySection Economy { get; init; } = new();
    public LootSection Loot { get; init; } = new();
    public DifficultySection Difficulty { get; init; } = new();

    public sealed class RatesSection {
        public ExpRates Exp { get; init; } = new();
        public MesoRates Meso { get; init; } = new();

        public sealed class ExpRates {
            public float Global { get; init; } = 1.0f;
            public float Kill { get; init; } = 1.0f;
            public float Quest { get; init; } = 1.0f;
            public float Dungeon { get; init; } = 1.0f;
            public float Prestige { get; init; } = 1.0f;
            public float Mastery { get; init; } = 1.0f; // fishing/gathering/etc.
        }

        public sealed class MesoRates {
            public float Gain { get; init; } = 1.0f;
            public float Cost { get; init; } = 1.0f;
        }
    }

    public sealed class EconomySection {
        public float RepairCostRate { get; init; } = 1.0f;
        public float EnchantCostRate { get; init; } = 1.0f;
        public float TravelFeeRate { get; init; } = 1.0f;
        public float MarketTaxRate { get; init; } = 1.0f; // multiplier on Constant.MesoMarketTaxRate
    }

    public sealed class LootSection {
        public float GlobalDropRate { get; init; } = 1.0f;
        public float BossDropRate { get; init; } = 1.0f;
        public float RareDropRate { get; init; } = 1.0f;
        public float MesosDropRate { get; init; } = 1.0f;
    }

    public sealed class DifficultySection {
        public float DamageDealtRate { get; init; } = 1.0f;
        public float DamageTakenRate { get; init; } = 1.0f;
        public float EnemyHpScale { get; init; } = 1.0f;
        public int EnemyLevelOffset { get; init; } = 0;
    }
}

public static class ServerSettingsExtensions {
    public static float ExpMultiplier(this ServerSettings settings, ExpType type) {
        float g = settings.Rates.Exp.Global;
        return type switch {
            ExpType.monster or ExpType.monsterBoss or ExpType.monsterElite or ExpType.assist or ExpType.assistBonus => g * settings.Rates.Exp.Kill,
            ExpType.quest or ExpType.epicQuest or ExpType.mission or ExpType.questEtc => g * settings.Rates.Exp.Quest,
            ExpType.dungeonClear or ExpType.dungeonBoss or ExpType.dungeonRelative => g * settings.Rates.Exp.Dungeon,
            ExpType.fishing or ExpType.gathering or ExpType.manufacturing or ExpType.arcade or ExpType.miniGame or ExpType.userMiniGame or ExpType.musicMastery1 or ExpType.musicMastery2 or ExpType.musicMastery3 or ExpType.musicMastery4 => g * settings.Rates.Exp.Mastery,
            _ => g,
        };
    }
}
