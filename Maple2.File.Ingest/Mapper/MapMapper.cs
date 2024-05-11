using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml.Map;
using Maple2.File.Parser.Xml.Table;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class MapMapper : TypeMapper<MapMetadata> {
    private readonly TableParser spawnParser;
    private readonly MapParser parser;

    public MapMapper(M2dReader xmlReader) {
        spawnParser = new(xmlReader);
        parser = new(xmlReader);
    }

    protected override IEnumerable<MapMetadata> Map() {
        var pets = new Dictionary<int, Dictionary<int, int>>();
        foreach ((int mapId, IEnumerable<PetSpawnInfo> infos) in spawnParser.ParsePetSpawnInfo()) {
            pets[mapId] = infos.ToDictionary(info => info.npcID, info => info.petID);
        }

        var spawns = new Dictionary<int, IReadOnlyList<MapMetadataSpawn>>();
        foreach ((int mapId, IEnumerable<MapSpawnTag.Region> regions) in spawnParser.ParseMapSpawnTag()) {
            spawns[mapId] = regions.Select(region => new MapMetadataSpawn(
                Id: region.spawnPointID,
                MinDifficulty: region.difficultyMin,
                MaxDifficulty: region.difficulty,
                Population: region.population,
                Cooldown: region.coolTime,
                Tags: region.tag,
                PetPopulation: region.petPopulation,
                PetSpawnRate: region.petSpawnProbability,
                PetIds: pets.GetValueOrDefault(mapId, new Dictionary<int, int>()))
            ).ToList();
        }

        foreach ((int id, string name, MapData data) in parser.Parse()) {
            yield return new MapMetadata(
                Id: id,
                Name: name,
                XBlock: data.xblock.name.ToLower(),
                Property: new MapMetadataProperty(
                    Continent: (Continent) data.property.continentCode,
                    Region: (MapRegion) data.property.regionCode,
                    Category: data.property.mapCategoryCode,
                    Type: (MapType) data.property.mapType,
                    BigCity: data.property.bigCity,
                    ExploreType: data.property.exploreType,
                    TutorialType: data.property.tutorialType,
                    RevivalReturnId: data.property.revivalreturnid,
                    EnterReturnId: data.property.enterreturnid,
                    AutoRevivalType: data.property.autoRevivalType,
                    AutoRevivalTime: data.property.autoRevivalTime,
                    InfiniteMeretRevival: data.property.infinityMeratRevival,
                    NoRevivalHere: data.property.doNotRevivalHere,
                    ReviveFullHp: data.property.recoveryFullHP,
                    UseTimeEvent: data.property.useTimeEvent,
                    HomeReturnable: data.property.homeReturnable,
                    DeathPenalty: data.property.deathPenalty,
                    OnlyDarkTomb: data.property.onlyDarkTomb,
                    PkMode: data.property.pkMode,
                    CanFly: data.property.checkFly,
                    CanClimb: data.property.checkClimb,
                    IndoorType: data.indoor.type),
                Limit: new MapMetadataLimit(
                    Capacity: data.property.capacity,
                    MinLevel: data.property.enterMinLevel,
                    MaxLevel: data.property.enterMaxLevel,
                    RequireQuest: data.property.requireQuest,
                    DisableSkills: data.property.skillUseDisable,
                    Climb: data.property.checkClimb,
                    Fly: data.property.checkFly,
                    Move: data.property.limitMove,
                    FallDamage: data.ui.fallDamage,
                    Dash: data.ui.useEPSkill,
                    Ride: data.ui.useRidee,
                    Pet: data.ui.usePet),
                Drop: new MapMetadataDrop(
                    Level: data.drop.maplevel,
                    DropRank: data.drop.droprank,
                    GlobalDropBoxId: data.drop.globalDropBoxID),
                Spawns: spawns.GetValueOrDefault(id, new List<MapMetadataSpawn>()),
                CashCall: new MapMetadataCashCall(
                    TaxiDeparture: !data.cashCall.cashTaxiNotDeparture,
                    TaxiDestination: !data.cashCall.cashTaxiNotDestination,
                    Medic: !data.cashCall.cashCallMedicProhibit,
                    Market: !data.cashCall.cashCallMarketProhibit,
                    Recall: !data.cashCall.RecallOtherUserProhibit),
                // TODO: There are also EntranceBuffs for Survival
                EntranceBuffs: data.property.enteranceBuffIDs.Zip(data.property.enteranceBuffLevels,
                        (skillId, level) => new MapEntranceBuff(skillId, level)).ToArray());
        }
    }
}
