using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class AnimationMapper(M2dReader xmlReader) : TypeMapper<AnimationMetadata> {
    private readonly AniKeyTextParser parser = new(xmlReader);

    protected override IEnumerable<AnimationMetadata> Map() {
        foreach (AnimationData data in parser.Parse()) {
            foreach (KeyFrameMotion kfm in data.kfm) {
                IEnumerable<(string Name, AnimationSequence Sequence)> sequences = kfm.seq.Select(sequence => {
                    List<AnimationKey> keys = sequence.key.Select(key => new AnimationKey(key.name, (float) key.time)).ToList();
                    return (sequence.name,
                        new AnimationSequence(Id: (short) sequence.id, Time: (float) (sequence.key.FirstOrDefault(key => key.name == "end")?.time ?? default), keys));
                });

                var lookup = new Dictionary<string, AnimationSequence>();
                foreach ((string name, AnimationSequence sequence) in sequences) {
                    if (lookup.ContainsKey(name)) {
                        Console.WriteLine($"Ignore Duplicate: {name} for {kfm.name}");
                        continue;
                    }

                    lookup.Add(name, sequence);
                }

                yield return new AnimationMetadata(kfm.name, lookup);
            }
        }
    }
}
