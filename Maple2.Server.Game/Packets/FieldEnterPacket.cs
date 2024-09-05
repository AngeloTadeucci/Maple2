using System.Numerics;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class FieldEnterPacket {
    public static ByteWriter Request(IActor<Player> player) {
        var pWriter = Packet.Of(SendOp.RequestFieldEnter);
        pWriter.Write<MigrationError>(MigrationError.ok);
        pWriter.WriteInt(player.Value.Character.MapId);
        pWriter.WriteClass<FieldInstance>(player.Field.FieldInstance);
        pWriter.WriteInt();
        pWriter.Write<Vector3>(player.Position);
        pWriter.Write<Vector3>(player.Rotation);
        pWriter.WriteInt(GameSession.FIELD_KEY);

        return pWriter;
    }

    public static ByteWriter Error(MigrationError error) {
        var pWriter = Packet.Of(SendOp.RequestFieldEnter);
        pWriter.Write<MigrationError>(error);

        return pWriter;
    }
}
