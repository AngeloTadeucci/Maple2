﻿using System.Numerics;
using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml.Table;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using JobTable = Maple2.Model.Metadata.JobTable;
using MagicPath = Maple2.Model.Metadata.MagicPath;

namespace Maple2.File.Ingest.Mapper;

public class TableMapper : TypeMapper<TableMetadata> {
    private readonly TableParser parser;

    public TableMapper(M2dReader xmlReader) {
        parser = new TableParser(xmlReader);
    }

    protected override IEnumerable<TableMetadata> Map() {
        yield return new TableMetadata {Name = "itembreakingredient.xml", Table = ParseItemBreakIngredient()};
        yield return new TableMetadata {Name = "itemgemstoneupgrade.xml", Table = ParseItemGemstoneUpgrade()};
        yield return new TableMetadata {Name = "job.xml", Table = ParseJobTable()};
        yield return new TableMetadata {Name = "magicpath.xml", Table = ParseMagicPath()};
        yield return new TableMetadata {Name = "instrumentcategoryinfo.xml", Table = ParseInstrument()};
    }

    private ItemBreakTable ParseItemBreakIngredient() {
        var results = new Dictionary<int, IReadOnlyList<ItemBreakTable.Ingredient>>();
        foreach ((int itemId, ItemBreakIngredient item) in parser.ParseItemBreakIngredient()) {
            var ingredients = new List<ItemBreakTable.Ingredient>();
            if (item.IngredientItemID1 > 0 && item.IngredientCount1 > 0) {
                ingredients.Add(new ItemBreakTable.Ingredient(item.IngredientItemID1, item.IngredientCount1));
            }
            if (item.IngredientItemID2 > 0 && item.IngredientCount2 > 0) {
                ingredients.Add(new ItemBreakTable.Ingredient(item.IngredientItemID2, item.IngredientCount2));
            }
            if (item.IngredientItemID3 > 0 && item.IngredientCount3 > 0) {
                ingredients.Add(new ItemBreakTable.Ingredient(item.IngredientItemID3, item.IngredientCount3));
            }

            results.Add(itemId, ingredients);
        }

        return new ItemBreakTable(results);
    }

    private GemstoneUpgradeTable ParseItemGemstoneUpgrade() {
        var results = new Dictionary<int, GemstoneUpgradeTable.Entry>();
        foreach ((int itemId, ItemGemstoneUpgrade upgrade) in parser.ParseItemGemstoneUpgrade()) {
            var ingredients = new List<GemstoneUpgradeTable.Ingredient>();
            if (upgrade.IngredientCount1 > 0 && upgrade.IngredientItemID1?.Length > 1) {
                ingredients.Add(new GemstoneUpgradeTable.Ingredient(upgrade.IngredientItemID1[1], upgrade.IngredientCount1));
            }
            if (upgrade.IngredientCount2 > 0 && upgrade.IngredientItemID2?.Length > 1) {
                ingredients.Add(new GemstoneUpgradeTable.Ingredient(upgrade.IngredientItemID2[1], upgrade.IngredientCount2));
            }
            if (upgrade.IngredientCount3 > 0 && upgrade.IngredientItemID3?.Length > 1) {
                ingredients.Add(new GemstoneUpgradeTable.Ingredient(upgrade.IngredientItemID3[1], upgrade.IngredientCount3));
            }
            if (upgrade.IngredientCount4 > 0 && upgrade.IngredientItemID4?.Length > 1) {
                ingredients.Add(new GemstoneUpgradeTable.Ingredient(upgrade.IngredientItemID4[1], upgrade.IngredientCount4));
            }

            results.Add(itemId, new GemstoneUpgradeTable.Entry(upgrade.GemLevel, upgrade.NextItemID, ingredients));
        }

        return new GemstoneUpgradeTable(results);
    }

    private JobTable ParseJobTable() {
        var results = new Dictionary<JobCode, JobTable.Entry>();
        foreach (File.Parser.Xml.Table.JobTable data in parser.ParseJobTable()) {
            var skills = new Dictionary<SkillRank, JobTable.Skill[]> {
                [SkillRank.Basic] = data.skills.skill
                    .Where(skill => skill.subJobCode <= data.code) // This is not actually correct, but works.
                    .Select(skill => new JobTable.Skill(skill.main, skill.sub, skill.maxLevel, skill.quickSlotPriority))
                    .ToArray(),
                [SkillRank.Awakening] = data.skills.skill
                    .Where(skill => skill.subJobCode > data.code) // This is not actually correct, but works.
                    .Select(skill => new JobTable.Skill(skill.main, skill.sub, skill.maxLevel, skill.quickSlotPriority))
                    .ToArray(),
            };

            results[(JobCode) data.code] = new JobTable.Entry(Tutorial: new JobTable.Tutorial(StartField: data.startField,
                    SkipField: data.tutorialSkipField.Length > 0 ? data.tutorialSkipField[0] : 0,
                    SkipItem: data.tutorialSkipItem,
                    OpenMaps: data.tutorialClearOpenMaps,
                    OpenTaxis: data.tutorialClearOpenTaxis,
                    StartItem: data.startInvenItem.item.Select(item => new JobTable.Item(item.itemID, item.grade, item.count)).ToArray(),
                    Reward: data.reward.item.Select(item => new JobTable.Item(item.itemID, item.grade, 1)).ToArray()),
                Skills: skills,
                BaseSkills: data.learn.SelectMany(learn => learn.skill)
                    .SelectMany(skill => skill.sub.Append(skill.id))
                    .OrderBy(id => id)
                    .ToArray());
        }

        return new JobTable(results);
    }

    private MagicPathTable ParseMagicPath() {
        var results = new Dictionary<long, IReadOnlyList<MagicPath>>();
        foreach ((long id, MagicType type) in parser.ParseMagicPath()) {
            // Dropping duplicates for now (60073021, 50000303, 5009, 5101)
            if (results.ContainsKey(id)) {
                continue;
            }

            List<MagicPath> moves = type.move.Select(move => new MagicPath(
                Align: move.align,
                AlignHeight: move.alignCubeHeight,
                Rotate: move.rotation,
                IgnoreAdjust: move.ignoreAdjustCubePosition,
                Direction: move.direction != default ? Vector3.Normalize(move.direction) : default,
                FireOffset: move.fireOffsetPosition,
                FireFixed: move.fireFixedPosition,
                TraceTargetOffset: move.traceTargetOffsetPos,
                Velocity: move.vel,
                Distance: move.distance,
                RotateZDegree: move.dirRotZDegree,
                LifeTime: move.lifeTime,
                DelayTime: move.delayTime,
                SpawnTime: move.spawnTime,
                DestroyTime: move.destroyTime
            )).ToList();
            results[id] = moves;
        }

        return new MagicPathTable(results);
    }

    private InstrumentTable ParseInstrument() {
        var categories = new Dictionary<int, (int MidiId, int PercussionId)>();
        foreach ((int _, InstrumentCategoryInfo info) in parser.ParseInstrumentCategoryInfo()) {
            categories[info.id] = (info.GMId, info.percussionId);
        }

        var results = new Dictionary<int, InstrumentMetadata>();
        foreach ((int id, InstrumentInfo info) in parser.ParseInstrumentInfo()) {
            (int midiId, int percussionId) = categories[info.category];
            results[id] = new InstrumentMetadata(
                Id: info.id,
                EquipId: info.equipItemId,
                ScoreCount: info.soloRelayScoreCount,
                Category: info.category,
                MidiId: midiId,
                PercussionId: percussionId);
        }

        return new InstrumentTable(results);
    }
}
