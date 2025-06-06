﻿/*
* This class is a way to resolve packets sent by the server.
* It will try to find the packet structure using the client error logging system.
* It will save the packet structure in the packet structure file located in the ./PacketStructure folder.
* You can change each packet value in the file and it'll try to continue resolving from the last value.
* If you want to start from the beginning, you can delete the file.
* The resolver will ignore the lines starting with # and 'ByteWriter'.
* More info in the https://github.com/AngeloTadeucci/Maple2/wiki/Packet-Resolver
*/
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Helpers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools;
using Serilog;

namespace Maple2.Server.Game.Util;

public class PacketStructureResolver {
    private const int HeaderLength = 6;

    private readonly string defaultValue;
    private readonly ushort opCode;
    private readonly string? packetName;
    private readonly ByteWriter packet;
    private static readonly ILogger Logger = Log.Logger.ForContext<PacketStructureResolver>();

    private PacketStructureResolver(ushort opCode) {
        defaultValue = "0";
        this.opCode = opCode;
        packet = Packet.Of((SendOp) opCode);
        packetName = Enum.GetName(typeof(SendOp), opCode);
    }

    // resolve opcode
    // Example: resolve 81
    public static PacketStructureResolver? Parse(string input) {
        string[] args = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        // Parse opCode: 81 0081 0x81 0x0081
        ushort opCode;
        string firstArg = args[0];
        if (firstArg.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase)) {
            opCode = Convert.ToUInt16(firstArg, 16);
        } else {
            switch (firstArg.Length) {
                case 2:
                    opCode = firstArg.ToByte();
                    break;
                case 4:
                    // Reverse bytes
                    byte[] bytes = firstArg.ToByteArray();
                    Array.Reverse(bytes);

                    opCode = BitConverter.ToUInt16(bytes);
                    break;
                default:
                    Logger.Information("Invalid opcode.");
                    return null;
            }
        }

        var resolver = new PacketStructureResolver(opCode);
        DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(Paths.SOLUTION_DIR, "PacketStructures"));

        string filePath = $"{dir.FullName}/{resolver.opCode:X4} - {resolver.packetName}.txt";
        if (!File.Exists(filePath)) {
            StreamWriter writer = File.CreateText(filePath);
            writer.WriteLine("# Generated by Maple2 PacketStructureResolver");
            writer.WriteLine($"ByteWriter pWriter = Packet.Of(SendOp.{resolver.packetName});");
            writer.Close();
            return resolver;
        }

        string[] fileLines = File.ReadAllLines(filePath);
        foreach (string line in fileLines) {
            if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith("ByteWriter") || line.StartsWith("//")) {
                continue;
            }

            string[] packetLine = line.Split("(");
            string type = packetLine[0][13..];
            string valueAsString = packetLine[1].Split(")")[0];
            valueAsString = string.IsNullOrEmpty(valueAsString) ? "0" : valueAsString;
            try {
                switch (type) {
                    case "Bool":
                        resolver.packet.WriteBool(bool.Parse(valueAsString));
                        break;
                    case "Byte":
                        resolver.packet.WriteByte(byte.Parse(valueAsString));
                        break;
                    case "Short":
                        resolver.packet.WriteShort(short.Parse(valueAsString));
                        break;
                    case "Int":
                        resolver.packet.WriteInt(int.Parse(valueAsString));
                        break;
                    case "Long":
                        resolver.packet.WriteLong(long.Parse(valueAsString));
                        break;
                    case "Float":
                        resolver.packet.WriteFloat(float.Parse(valueAsString));
                        break;
                    case "UnicodeString":
                        resolver.packet.WriteUnicodeString(valueAsString.Replace("\"", ""));
                        break;
                    case "String":
                        resolver.packet.WriteString(valueAsString.Replace("\"", ""));
                        break;
                    default:
                        Logger.Information("Unknown type: {type}", type);
                        break;
                }
            } catch {
                Logger.Information("Couldn't parse value on function: {line}", line);
                return null;
            }
        }

        return resolver;
    }

    public void Start(GameSession session) {
        session.OnError = AppendAndRetry!;

        // Start off the feedback loop
        session.Send(packet);
    }

    private void AppendAndRetry(object session, string err) {
        SockExceptionInfo info = ErrorParserHelper.Parse(err);
        if (info.SendOp == 0) {
            return;
        }

        if (opCode != (ushort) info.SendOp) {
            Logger.Warning("Error for unexpected op code:{0}", info.SendOp.ToString("X4"));
            return;
        }

        if (packet.Length + HeaderLength != info.Offset) {
            Logger.Warning("Offset:{offset} does not match Packet length:{lenght}", info.Offset, packet.Length + HeaderLength);
            return;
        }

        new SockHintInfo(info.Hint, defaultValue).Update(packet);
        string hint = info.Hint.GetCode() + " - " + info.Offset + "\r\n";

        DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(Paths.SOLUTION_DIR, "PacketStructures"));
        StreamWriter file = File.AppendText(Path.Combine(dir.FullName, $"{opCode:X4} - {packetName}.txt"));
        file.Write(hint);
        file.Close();

        (session as GameSession)?.Send(packet);
    }
}
