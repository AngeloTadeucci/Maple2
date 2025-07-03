using System.Xml;
using Maple2.File.IO;

namespace Maple2.File.Ingest.Utils;

public class StringTable {
    public readonly Dictionary<string, string> Table;

    public StringTable(M2dReader xmlReader) {
        Table = new Dictionary<string, string>();

        XmlDocument doc = xmlReader.GetXmlDocument(xmlReader.GetEntry("en/stringcommon.xml"));
        XmlNodeList? nodes = doc.SelectNodes("ms2/key");
        if (nodes == null) {
            throw new InvalidOperationException("No nodes found in stringcommon.xml");
        }
        foreach (XmlNode node in nodes) {
            string? id = node.Attributes?["id"]?.Value;
            string value = node.Attributes?["value"]?.Value ?? "";
            if (id == null) {
                continue;
            }

            Table[id] = value;
        }
    }
}
