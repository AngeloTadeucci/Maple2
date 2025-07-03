using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class AnimationMapper : TypeMapper<AnimationMetadata> {
    private readonly AniKeyTextParser parser;

    public AnimationMapper(M2dReader xmlReader) {
        parser = new AniKeyTextParser(xmlReader);
    }

    protected override IEnumerable<AnimationMetadata> Map() {
        foreach (AnimationData data in parser.Parse()) {
            foreach (KeyFrameMotion kfm in data.kfm) {
                IEnumerable<(string Name, AnimationSequenceMetadata Sequence)> sequences = kfm.seq.Select(sequence => {
                    List<AnimationKey> keys = sequence.key.Select(key => new AnimationKey(key.name, (float) key.time)).ToList();
                    return (sequence.name,
                        new AnimationSequenceMetadata(
                            Name: sequence.name,
                            Id: (short) sequence.id,
                            Time: (float) (sequence.key.FirstOrDefault(key => key.name == "end")?.time ?? 0), keys)
                        );
                });

                var lookup = new Dictionary<string, AnimationSequenceMetadata>();
                foreach ((string name, AnimationSequenceMetadata sequence) in sequences) {
                    if (!lookup.TryAdd(name, sequence)) {
                        Console.WriteLine($"Ignore Duplicate: {name} for {kfm.name}");
                    }

                }

                yield return new AnimationMetadata(kfm.name, lookup);
            }
        }
    }
}
