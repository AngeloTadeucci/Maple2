using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class BanWordMapper : TypeMapper<BanWordMetadata> {
    private readonly BanWordParser parser;

    public BanWordMapper(M2dReader xmlReader) {
        parser = new BanWordParser(xmlReader);
    }

    protected override IEnumerable<BanWordMetadata> Map() {
        var hashSet = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        foreach ((int Id, string Name) word in parser.ParseBanWords()) {
            if (hashSet.Add(word.Name)) {
                yield return new BanWordMetadata(
                    word.Id, word.Name, false
                );
            }
        }

        foreach ((int Id, string Name) word in parser.ParseUgcBanWords()) {
            if (hashSet.Add(word.Name)) {
                yield return new BanWordMetadata(
                    word.Id, word.Name, true
                );
            }
        }
    }
}
