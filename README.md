[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/AngeloTadeucci/Maple2)

# MapleStory2 Server Emulator

This is an open source MapleStory2 server emulation project created in C#. It is being developed completely for educational purposes only. This software is being provided "as-is", completely for free. As a result, we, nor anybody who chooses to contribute, are not responsible for any damages or other liability as a result of this software.

Setup Guide: https://github.com/AngeloTadeucci/Maple2/wiki/Prerequisites

Join the [community discord](https://discord.gg/r78CXkUmuj)! - Updated as of 9/28/2024

## Server Configuration (Rates)

Use `config.yaml` in the repo root (or set `CONFIG_PATH` to point elsewhere). All keys are optional; defaults are 1.0.

- Rates:
  - EXP: `rates.exp.global`, `rates.exp.kill`, `rates.exp.quest`, `rates.exp.dungeon`, `rates.exp.prestige`, `rates.exp.mastery`
  - Meso: `rates.meso.gain`, `rates.meso.cost`
- Loot: `loot.global_drop_rate`, `loot.boss_drop_rate`, `loot.rare_drop_rate`, `loot.mesos_drop_rate`
- Economy: `economy.repair_cost_rate`, `economy.enchant_cost_rate`, `economy.travel_fee_rate`, `economy.market_tax_rate`
- Difficulty: `difficulty.damage_dealt_rate`, `difficulty.damage_taken_rate`, `difficulty.enemy_hp_scale`, `difficulty.enemy_level_offset`

Example `config.yaml` snippet:

```
rates:
  exp:
    global: 2.0
    kill: 2.0
    quest: 1.5
  meso:
    gain: 2.5
    cost: 1.0
loot:
  global_drop_rate: 1.5
```

Notes:
- EXP: per-type multipliers apply and include the global multiplier.
- Meso: positive gains use `rates.meso.gain`; costs use `rates.meso.cost`.
- The server watches the YAML file and reloads on changes.
