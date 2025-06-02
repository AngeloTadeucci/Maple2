using System.Globalization;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Core.Helpers;

public class DebugByteWriter : ByteWriter, IByteWriter {
    private readonly string filePath;

    public DebugByteWriter(SendOp opCode, int size) : base(size) {
        DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(Paths.SOLUTION_DIR, "PacketStructures"));

        string? packetName = Enum.GetName(typeof(SendOp), opCode);
        filePath = Path.Combine(dir.FullName, $"DEBUG - {(ushort) opCode:X4} - {packetName}.txt");
        StreamWriter streamWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        streamWriter.WriteLine("// DEBUG - Packet Structure of " + packetName + " - Timestamp: " + DateTime.Now.ToString("HH:mm:ss.fff"));
        streamWriter.WriteLine($"ByteWriter pWriter = Packet.Of(SendOp.{packetName});");
        streamWriter.Close();
    }

    private void Log(string functionName, string? value, int offset) {
        using StreamWriter streamWriter = File.AppendText(filePath);
        streamWriter.WriteLine($"pWriter.{functionName}({value}); // offset: {offset + 6}"); // Idk why but the offset is always 6 bytes more than it should be
        streamWriter.Close();
    }

    public new void Write<T>(in T value) where T : struct {
        if (typeof(T) == typeof(SendOp)) {
            base.Write<T>(value);
            return;
        }

        if (!typeof(T).IsEnum) {
            Log("Write", value.ToString(), Length);
            base.Write(value);
            return;
        }

        Type underlyingType = Enum.GetUnderlyingType(typeof(T));
        string functionName = underlyingType.Name switch {
            "Byte" => "WriteByte",
            "Int16" or "UInt16" => "WriteShort",
            "Int32" => "WriteInt",
            "Int64" => "WriteLong",
            _ => $"Write<{underlyingType.Name}>",
        };

        object logValue = Convert.ChangeType(value, underlyingType);
        Log(functionName, logValue.ToString(), Length);
        base.Write(value);
    }

    public new void WriteBool(bool value) {
        Log("WriteBool", value.ToString(), Length);
        base.WriteBool(value);
    }

    public new void WriteByte(byte value = 0) {
        Log("WriteByte", value.ToString(), Length);
        base.WriteByte(value);
    }

    public new void WriteShort(short value = 0) {
        Log("WriteShort", value.ToString(), Length);
        base.WriteShort(value);
    }

    public new void WriteInt(int value = 0) {
        Log("WriteInt", value.ToString(), Length);
        base.WriteInt(value);
    }

    public new void WriteFloat(float value = 0.0f) {
        Log("WriteFloat", value.ToString(CultureInfo.CurrentCulture), Length);
        base.WriteFloat(value);
    }

    public new void WriteLong(long value = 0) {
        Log("WriteLong", value.ToString(), Length);
        base.WriteLong(value);
    }

    public new void WriteString(string value = "") {
        Log("WriteString", $"\"{value}\"", Length);
        base.WriteString(value);
    }

    public new void WriteUnicodeString(string value = "") {
        Log("WriteUnicodeString", $"\"{value}\"", Length);
        base.WriteUnicodeString(value);
    }

    public new void WriteRawUnicodeString(string value = "") {
        Log("WriteRawUnicodeString", $"\"{value}\"", Length);
        base.WriteRawUnicodeString(value);
    }
}
