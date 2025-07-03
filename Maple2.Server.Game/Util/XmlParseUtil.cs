using System.Xml;

namespace Maple2.Server.Game.Util;

public static class XmlParseUtil {
    public static Dictionary<string, string> GetParameters(string? xmlString) {
        var parameters = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(xmlString)) {
            return parameters;
        }

        if (xmlString[1] == ' ') {
            xmlString = xmlString.Remove(1, 1);
        }

        XmlDocument xmlParameter = new();
        xmlParameter.LoadXml(xmlString);
        foreach (XmlNode node in xmlParameter) {
            if (node.Attributes == null) {
                continue;
            }

            foreach (XmlAttribute attribute in node.Attributes) {
                parameters.Add(attribute.Name, attribute.Value);
            }
        }
        return parameters;
    }
}
