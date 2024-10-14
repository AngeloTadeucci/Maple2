using System.Reflection;

namespace Maple2.Server.DebugGame.Graphics.Ui;

public static class UiUtils {
    public static string GetEventName(EventInfo eventInfo) {
        return "Event " + (eventInfo.EventHandlerType?.Name ?? "<null>");
    }

    public static string GetMethodName(MethodInfo methodInfo) {
        return $"Method <{methodInfo.ReturnType}>";
    }

    public static string GetMemberDisplayName(MemberInfo member) {
        string memberType = member.MemberType switch {
            MemberTypes.Event => GetEventName((EventInfo) member),
            MemberTypes.Field => ((FieldInfo) member).FieldType.Name,
            MemberTypes.Method => GetMethodName((MethodInfo) member),
            MemberTypes.Property => ((PropertyInfo) member).PropertyType.Name,
            _ => "<unknown>"
        };

        return $"[{member.Module.ScopeName}]: {memberType} {member.DeclaringType?.Name ?? "null"}.{member.Name}";
    }

    public static void ImGuiError(string message) {
        ImGuiController.Logger.Error(message);

        throw new InvalidDataException(message);
    }
}

