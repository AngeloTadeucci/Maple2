using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml.Riding;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class RideMapper : TypeMapper<RideMetadata> {
    private readonly RidingParser parser;

    public RideMapper(M2dReader xmlReader) {
        parser = new RidingParser(xmlReader);
    }

    protected override IEnumerable<RideMetadata> Map() {
        var passengers = new Dictionary<int, int>();
        foreach ((int id, IList<PassengerRiding> data) in parser.ParsePassenger()) {
            passengers[id] = data.Count;
        }

        foreach ((int id, Riding data) in parser.Parse()) {
            yield return new RideMetadata(
                Id: id,
                Model: data.basic.kfm,
                Basic: new RideMetadataBasic(
                    Type: Enum.TryParse(data.basic.type.ToString(), true, out RideOnType type) ? type : RideOnType.Default,
                    SkillSetId: data.basic.skillSetID,
                    SummonTime: data.basic.rideSummonCastTime,
                    RunXStamina: data.basic.runXConsumeEp,
                    EnableSwim: data.basic.enableSwim,
                    FallDamageDown: data.basic.fallDamageDown,
                    Passengers: passengers.GetValueOrDefault(id)),
                Speed: new RideMetadataSpeed(
                    WalkSpeed: data.basic.walkSpeed,
                    RunSpeed: data.basic.runSpeed,
                    RunXSpeed: data.basic.runXSpeed,
                    SwimSpeed: data.basic.swimSpeed),
                Stats: data.stat.ToDictionary()
            );
        }
    }
}
