using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Global.Service;

namespace Maple2.Server.Core.Packets;

public static class LoginResultPacket {
    public static ByteWriter Success(long accountId) {
        var pWriter = Packet.Of(SendOp.LoginResult);
        pWriter.WriteByte(); // Login State
        pWriter.WriteInt(); // Const
        pWriter.WriteUnicodeString(); // Ban reason
        pWriter.WriteLong(accountId);
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); // SyncTime
        pWriter.WriteInt(); // TimeBase (This offsets time)
        pWriter.WriteByte(); // TimeZone
        pWriter.WriteByte(); // BlockType (1 =
        pWriter.WriteInt(); // Const
        pWriter.WriteLong(); // Const
        pWriter.WriteInt(2); // Const
        return pWriter;
    }

    // Set 0 for all values in error login packet
    public static ByteWriter Error(byte code, string message, long accountId) {
        var pWriter = Packet.Of(SendOp.LoginResult);
        pWriter.WriteByte(code); // Login State
        pWriter.WriteInt(); // Const
        pWriter.WriteUnicodeString(message); // Ban reason
        pWriter.WriteLong(accountId);
        pWriter.WriteLong(); // SyncTime
        pWriter.WriteInt(); // SyncTicks
        pWriter.WriteByte(); // TimeZone
        pWriter.WriteByte(); // BlockType
        pWriter.WriteInt(); // Const
        pWriter.WriteLong(); // Const
        pWriter.WriteInt(); // Const
        return pWriter;
    }

    public static ByteWriter Restricted(string reason, long accountId, long startTimeSeconds, long expiryEpochSeconds) {
        var pWriter = Packet.Of(SendOp.LoginResult);
        pWriter.WriteByte((byte) LoginResponse.Types.Code.Restricted);
        pWriter.WriteInt(); // Const
        pWriter.WriteUnicodeString(reason); // Ban reason
        pWriter.WriteLong(accountId);
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); // SyncTime
        pWriter.WriteInt(); // SyncTicks
        pWriter.WriteByte(); // TimeZone
        pWriter.WriteByte(2); // BlockType
        pWriter.WriteInt(); // Const
        pWriter.WriteLong(); // Const
        pWriter.WriteLong(startTimeSeconds); // Start time
        pWriter.WriteLong(expiryEpochSeconds); // End time
        pWriter.WriteInt(); // Const
        return pWriter;
    }
}
