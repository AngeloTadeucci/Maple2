using System.Diagnostics;
using System.Text.RegularExpressions;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;

namespace Maple2.Server.Core.Helpers;

public static partial class ErrorParserHelper {
    private static readonly Regex InfoRegex = ErrorRegex();

    public static SockExceptionInfo Parse(string error) {
        Match match = InfoRegex.Match(error);
        if (match.Groups.Count != 4) {
            throw new ArgumentException($"Failed to parse error: {error}");
        }

        SockExceptionInfo info;
        info.SendOp = (SendOp) ushort.Parse(match.Groups[1].Value);
        info.Offset = uint.Parse(match.Groups[2].Value);
        info.Hint = match.Groups[3].Value.ToSockHint();

        return info;
    }

    [GeneratedRegex(@"\[type=(\d+)\]\[offset=(\d+)\]\[hint=(\w+)\]", RegexOptions.Compiled)]
    private static partial Regex ErrorRegex();
}

public struct SockExceptionInfo {
    public SendOp SendOp;
    public uint Offset;
    public SockHint Hint;
}

public enum SockHint {
    Decode1,
    Decode2,
    Decode4,
    Decodef,
    Decode8,
    DecodeStr,
    DecodeStrA
}
public readonly struct SockHintInfo {
    private readonly SockHint hint;
    // Values
    private readonly byte byteValue;
    private readonly short shortValue;
    private readonly int intValue;
    private readonly long longValue;
    private readonly string stringValue;

    public SockHintInfo(SockHint hint, string value) {
        this.hint = hint;
        stringValue = value == "0" ? "" : value;

        _ = byte.TryParse(value, out byteValue);
        _ = short.TryParse(value, out shortValue);
        _ = int.TryParse(value, out intValue);
        _ = long.TryParse(value, out longValue);
    }

    public void Update(IByteWriter packet) {
        switch (hint) {
            case SockHint.Decode1:
                packet.WriteByte(byteValue);
                break;
            case SockHint.Decode2:
                packet.WriteShort(shortValue);
                break;
            case SockHint.Decode4:
                packet.WriteInt(intValue);
                break;
            case SockHint.Decodef:
                packet.WriteFloat(intValue);
                break;
            case SockHint.Decode8:
                packet.WriteLong(longValue);
                break;
            case SockHint.DecodeStr:
                packet.WriteUnicodeString(stringValue);
                break;
            case SockHint.DecodeStrA:
                packet.WriteString(stringValue);
                break;
            default:
                throw new ArgumentException($"Unexpected hint: {hint}");
        }
    }
}
public static class SockHintExtensions {
    // PacketWriter Code
    public static string GetCode(this SockHint hint) {
        return hint switch {
            SockHint.Decode1 => "pWriter.WriteByte();",
            SockHint.Decode2 => "pWriter.WriteShort();",
            SockHint.Decode4 => "pWriter.WriteInt();",
            SockHint.Decodef => "pWriter.WriteFloat();",
            SockHint.Decode8 => "pWriter.WriteLong();",
            SockHint.DecodeStr => "pWriter.WriteUnicodeString(\"\");",
            SockHint.DecodeStrA => "pWriter.WriteString(\"\");",
            _ => throw new ArgumentException($"Unexpected hint: {hint}")
        };
    }

    public static SockHint ToSockHint(this string sockHint) {
        sockHint = sockHint switch {
            "1" => SockHint.Decode1.ToString(),
            "2" => SockHint.Decode2.ToString(),
            "4" => SockHint.Decode4.ToString(),
            "f" => SockHint.Decodef.ToString(),
            "8" => SockHint.Decode8.ToString(),
            "s" => SockHint.DecodeStr.ToString(),
            "sa" => SockHint.DecodeStrA.ToString(),
            _ => sockHint
        };
        bool result = Enum.TryParse(sockHint, out SockHint hint);
        Debug.Assert(result, $"Failed to parse SockHint:{sockHint}");

        return hint;
    }
}
