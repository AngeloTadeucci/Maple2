using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using M2dXmlGenerator;
using Maple2.File.Ingest.Utils;
using Maple2.File.IO;
using Maple2.File.IO.Crypto.Common;
using Maple2.Model.Metadata;
using Maple2.Tools;
using static System.Char;


namespace Maple2.File.Ingest.Mapper;

public class TriggerMapper : TypeMapper<TriggerMetadata> {
    private readonly M2dReader reader;

    public TriggerMapper(M2dReader reader) {
        this.reader = reader;
    }

    protected override IEnumerable<TriggerMetadata> Map() {
        IEnumerable<PackFileEntry> triggers = reader.Files.Where(file => file.Name.StartsWith("trigger/"));
        foreach (PackFileEntry file in triggers) {
            // get the folder name from the file path after "trigger/"
            string[] filePath = file.Name["trigger/".Length..].Split('/');
            string folderName = filePath[0];
            string triggerName = filePath[1].Split(".")[0]; // remove the file extension
            string xml = NormalizeTriggerXmlNames(reader.GetXmlDocument(file));

            var trigger = new TriggerMetadata(folderName, triggerName, xml);

            if (Constant.DebugTriggers) { // for debugging purposes
                string filePathName = Path.Combine(Paths.DEBUG_TRIGGERS_DIR, folderName, $"{triggerName}.xml");
                Directory.CreateDirectory(Path.GetDirectoryName(filePathName)!);

                var formattedXml = new XmlDocument();
                formattedXml.LoadXml(xml);
                var settings = new XmlWriterSettings {
                    Indent = true,
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration = true,
                };

                using var writer = XmlWriter.Create(filePathName, settings);
                formattedXml.Save(writer);
            }
            yield return trigger;
        }
    }

    private static readonly Dictionary<string, string> SubStart = new() {
        { "1st", "First" },
        { "2nd", "Second" },
        { "3rd", "Third" },
        { "4th", "Fourth" },
        { "5th", "Fifth" },
        { "6th", "Sixth" },
        { "7th", "Seventh" },
    };

    public static string NormalizeTriggerXmlNames(XmlDocument xml) {
        // check for state nodes with feature attributes and remove disabled ones
        List<XmlNode> nodesToRemove = new List<XmlNode>();
        foreach (XmlNode node in xml.SelectNodes("//state")!) {
            XmlAttribute? featureAttr = node.Attributes?["feature"];
            if (featureAttr != null && !FeatureLocaleFilter.FeatureEnabled(featureAttr.Value)) {
                nodesToRemove.Add(node);
            }
        }

        // Check for action nodes with feature attributes and remove disabled ones
        foreach (XmlNode node in xml.SelectNodes("//action")!) {
            XmlAttribute? featureAttr = node.Attributes?["feature"];
            if (featureAttr != null && !FeatureLocaleFilter.FeatureEnabled(featureAttr.Value)) {
                nodesToRemove.Add(node);
            }
        }

        // Check for condition nodes with feature attributes and remove disabled ones
        foreach (XmlNode node in xml.SelectNodes("//condition")!) {
            XmlAttribute? featureAttr = node.Attributes?["feature"];
            if (featureAttr != null && !FeatureLocaleFilter.FeatureEnabled(featureAttr.Value)) {
                nodesToRemove.Add(node);
            }
        }

        // Remove disabled feature nodes
        foreach (XmlNode node in nodesToRemove) {
            node.ParentNode?.RemoveChild(node);
        }

        // Continue with the existing normalization logic
        foreach (XmlNode node in xml.SelectNodes("//state")!) {
            XmlAttribute? attr = node.Attributes?["name"];
            Debug.Assert(attr?.Value != null, "Unable to find name param");
            attr.Value = FixClassName(attr.Value);
        }

        foreach (XmlNode node in xml.SelectNodes("//transition")!) {
            XmlAttribute? attr = node.Attributes?["state"];
            Debug.Assert(attr?.Value != null, "Unable to find state param");
            attr.Value = FixClassName(attr.Value);
        }

        foreach (XmlNode node in xml.SelectNodes("//action")!) {
            string actionName = string.Empty;
            List<XmlAttribute> nodeParams = [];
            foreach (XmlAttribute? attribute in node.Attributes!) {
                if (attribute is null) continue;
                if (attribute.Name is "name") {
                    attribute.Value = Translate(attribute.Value, TriggerTranslate.TranslateAction);
                    actionName = attribute.Value;
                    continue;
                }

                nodeParams.Add(attribute);
            }

            if (!TriggerDefinitionOverride.ActionOverride.TryGetValue(actionName, out TriggerDefinitionOverride? overrideValue)) continue;

            if (overrideValue.FunctionSplitter is not null) {
                XmlAttribute? attributeSplitter = nodeParams.FirstOrDefault(x => x.Name == overrideValue.FunctionSplitter);
                if (attributeSplitter is not null) {
                    overrideValue.FunctionLookup.TryGetValue(attributeSplitter.Value, out overrideValue);
                    Debug.Assert(overrideValue is not null, $"Unable to find override for {attributeSplitter.Value}");
                } else {
                    string? valueDefault = overrideValue.Types.FirstOrDefault().Value;
                    Debug.Assert(valueDefault is not null, $"Unable to find default value for {overrideValue.Name}");
                    overrideValue.FunctionLookup.TryGetValue(valueDefault, out overrideValue);
                    Debug.Assert(overrideValue is not null, $"Unable to find override for {valueDefault}");
                }
                node.Attributes["name"]!.Value = overrideValue.Name;
            }

            foreach (XmlAttribute xmlAttribute in nodeParams) {
                overrideValue.Names.TryGetValue(TriggerTranslate.ToSnakeCase(xmlAttribute.Name), out string? newName);
                if (newName is null) {
                    if (xmlAttribute.Name != TriggerTranslate.ToSnakeCase(xmlAttribute.Name)) {
                        newName = TriggerTranslate.ToSnakeCase(xmlAttribute.Name);
                    } else {
                        continue;
                    }
                }

                node.Attributes.Remove(xmlAttribute);
                XmlAttribute newAttribute = xml.CreateAttribute(newName);
                newAttribute.Value = xmlAttribute.Value;
                node.Attributes.Append(newAttribute);
            }
        }

        foreach (XmlNode node in xml.SelectNodes("//condition")!) {
            string conditionName = string.Empty;
            List<XmlAttribute> nodeParams = [];
            foreach (XmlAttribute? attribute in node.Attributes!) {
                if (attribute is null) continue;
                if (attribute.Name is "name") {
                    conditionName = attribute.Value;
                    continue;
                }

                nodeParams.Add(attribute);
            }

            if (conditionName.StartsWith('!')) {
                XmlAttribute negateAttribute = xml.CreateAttribute("negate");
                negateAttribute.Value = "true";
                node.Attributes.Append(negateAttribute);
            }

            conditionName = conditionName.TrimStart('!');
            node.Attributes["name"]!.Value = Translate(conditionName, TriggerTranslate.TranslateCondition);

            if (!TriggerDefinitionOverride.ConditionOverride.TryGetValue(node.Attributes["name"]!.Value, out TriggerDefinitionOverride? overrideValue)) continue;
            if (overrideValue.Name != node.Attributes["name"]!.Value) {
                node.Attributes["name"]!.Value = overrideValue.Name;
            }
            foreach (XmlAttribute xmlAttribute in nodeParams) {
                overrideValue.Names.TryGetValue(TriggerTranslate.ToSnakeCase(xmlAttribute.Name), out string? newName);
                if (newName is null) {
                    if (xmlAttribute.Name != TriggerTranslate.ToSnakeCase(xmlAttribute.Name)) {
                        newName = TriggerTranslate.ToSnakeCase(xmlAttribute.Name);
                    } else {
                        continue;
                    }
                }

                node.Attributes.Remove(xmlAttribute);
                XmlAttribute newAttribute = xml.CreateAttribute(newName);
                newAttribute.Value = xmlAttribute.Value;
                node.Attributes.Append(newAttribute);
            }
        }

        return xml.OuterXml;
    }

    [return: NotNullIfNotNull(nameof(name))]
    private static string? FixClassName(string? name) {
        if (name == null) {
            return null;
        }
        if (string.IsNullOrWhiteSpace(name)) {
            return "State";
        }

        // Reserved Keywords
        switch (name) {
            case "None":
                return "StateNone";
            case "True":
                return "StateTrue";
            case "False":
                return "StateFalse";
            case "del":
                return "StateDelete";
        }

        name = name.Replace("-", "To").Replace(" ", "_").Replace(".", "_");
        foreach ((string key, string value) in SubStart) {
            if (name.StartsWith(key)) {
                name = name.Replace(key, value);
                break;
            }
        }

        string prefix = "";
        while (name.Length > 0 && !IsLetter(name[0])) {
            if (name[0] != '_') {
                prefix += name[0];
            }
            name = name[1..];
        }

        // name is already valid
        if (prefix.Length == 0) {
            return name;
        }
        if (name.Length == 0) {
            return $"State{prefix}";
        }

        return !IsLetter(name[^1]) ? $"{name}_{prefix}" : $"{name}{prefix}";
    }

    [return: NotNullIfNotNull(nameof(name))]
    private static string? Translate(string? name, Func<string, string> translator) {
        if (name == null) {
            return null;
        }

        var builder = new StringBuilder();
        foreach (string split in name.Split('_', ' ')) {
            builder.Append(translator(split));
        }

        return TriggerTranslate.ToSnakeCase(builder.ToString());
    }
}
