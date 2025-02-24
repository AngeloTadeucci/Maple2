using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml.Pet;
using Maple2.File.Parser.Xml.Table;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class PetMapper : TypeMapper<PetMetadata> {
    private readonly PetParser parser;

    public PetMapper(M2dReader xmlReader) {
        parser = new PetParser(xmlReader);
    }

    protected override IEnumerable<PetMetadata> Map() {
        var petNames = new Dictionary<int, string>();
        var petData = new Dictionary<int, PetData>();
        foreach ((int id, string name, PetData data) in parser.Parse()) {
            petNames[id] = name;
            petData[id] = data;
        }

        foreach (PetProperty property in parser.ParseProperty()) {
            if (!petData.TryGetValue(property.code, out PetData? data)) {
                // Defaults
                data = new PetData {
                    code = property.code,
                    slotNum = property.slotNum,
                    skill = new Skill(),
                    distance = new Distance {
                        pick = 1050,
                        warp = property.warpDistance,
                        trace = property.traceDistance,
                        battleTrace = property.battleTraceDistance,
                    },
                    time = new Time {
                        bore = 120000,
                        idle = 70000,
                        skill = 13000,
                        tired = 10000,
                        summonCast = 700,
                    },
                };
            }
            //Debug.Assert(property.slotNum == data.slotNum, $"{id} inventory slots mismatch: {property.slotNum} != {data.slotNum}");
            // 60000026
            yield return new PetMetadata(
                Id: property.code,
                Name: petNames.GetValueOrDefault(property.code),
                Type: property.type,
                AiPresets: property.tamingAiPresets.ToArray(),
                NpcId: property.npcID,
                ItemSlots: property.slotNum,
                EnableExtraction: property.enablePetExtraction,
                OptionLevel: property.optionLevel,
                OptionFactor: property.constantOptionFactor,
                Skill: data.skill.id == 0 ? null : new PetMetadataSkill(data.skill.id, data.skill.level),
                Effect: property.additionalEffectID == null ? Array.Empty<PetMetadataEffect>()
                    : property.additionalEffectID.Zip(property.additionalEffectLevel,
                        (effectId, level) => new PetMetadataEffect(effectId, level)).ToArray(),
                Distance: new PetMetadataDistance(
                    Warp: property.warpDistance,
                    Trace: property.traceDistance,
                    BattleTrace: property.battleTraceDistance),
                Time: new PetMetadataTime(
                    Idle: data.time.idle,
                    Bore: data.time.bore,
                    Summon: data.time.summonCast,
                    Tired: data.time.tired,
                    Skill: data.time.skill)
            );
        }
    }
}
