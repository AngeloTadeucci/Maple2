using System.Globalization;
using CsvHelper;

namespace Maple2.File.Ingest.Utils;

public static class AiTranslate {
    private static readonly List<(string Kr, string En)> Lookup = new();

    static AiTranslate() {
        using var reader = new StreamReader("Utils/ai_translate.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();

        while (csv.Read()) {
            string kr = csv.GetField("kr");
            string en = csv.GetField("en");
            Lookup.Add((kr, en));
        }
    }

    public static string Translate(string input) {
        // Only translate if there is at least one non-ascii character.
        if (!input.Any(c => c > 0x255)) {
            return input;
        }

        foreach ((string kr, string en) in Lookup) {
            if (input.Contains(kr)) {
                return input.Replace(kr, en);
            }
        }

        Console.WriteLine($"No translation for: {input.Trim()}");
        return input;
    }
}
