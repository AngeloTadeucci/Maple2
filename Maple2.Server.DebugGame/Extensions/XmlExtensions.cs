using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Maple2.Server.DebugGame.Extensions;

public static class XmlExtensions {
    public static bool TryGetAttribute(this XmlElement element, string name, [NotNullWhen(returnValue: true)] out string? value) {
        value = null;

        if (!element.HasAttribute(name)) {
            return false;
        }

        value = element.GetAttribute(name);

        return true;
    }
}
