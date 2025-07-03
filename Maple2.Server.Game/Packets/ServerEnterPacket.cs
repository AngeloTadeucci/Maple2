using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class ServerEnterPacket {
    public static ByteWriter Request(IActor<Player> fieldPlayer) {
        Player player = fieldPlayer.Value;

        var pWriter = Packet.Of(SendOp.ServerEnter);
        pWriter.WriteInt(fieldPlayer.ObjectId);
        pWriter.WriteLong(player.Character.Id);
        pWriter.WriteShort(player.Character.Channel);
        pWriter.WriteLong(player.Character.Exp);
        pWriter.WriteLong(player.Character.RestExp);
        pWriter.WriteLong(player.Currency.Meso);
        // These Merets are added up
        pWriter.WriteLong(player.Currency.Meret);
        pWriter.WriteLong(); // Merets2
        pWriter.WriteLong(player.Currency.GameMeret);
        // These Merets are added up. If set, previous are ignored.
        pWriter.WriteLong(); // Game Merets2
        pWriter.WriteLong(); // Event Merets

        pWriter.WriteLong(player.Currency.ValorToken);
        pWriter.WriteLong(player.Currency.Treva);
        pWriter.WriteLong(player.Currency.Rue);
        pWriter.WriteLong(player.Currency.HaviFruit);
        pWriter.WriteLong(player.Currency.ReverseCoin);
        pWriter.WriteLong(player.Currency.MentorToken);
        pWriter.WriteLong(player.Currency.MenteeToken);
        pWriter.WriteLong(player.Currency.StarPoint);
        pWriter.WriteLong(player.Currency.MesoToken);
        pWriter.WriteUnicodeString(player.Character.Picture);
        pWriter.WriteByte();
        pWriter.WriteByte();
        pWriter.WriteShort((short) player.Unlock.Maps.Count);
        foreach (int mapId in player.Unlock.Maps) {
            pWriter.WriteInt(mapId);
        }
        pWriter.WriteShort((short) player.Unlock.Taxis.Count);
        foreach (int mapId in player.Unlock.Taxis) {
            pWriter.WriteInt(mapId);
        }
        pWriter.WriteLong();
        pWriter.WriteUnicodeString();
        pWriter.WriteUnicodeString("http://127.0.0.1:8080");
        pWriter.WriteUnicodeString();
        pWriter.WriteUnicodeString("^https?://127\\.0\\.0\\.1(:\\d+)?");
        pWriter.WriteUnicodeString();

        return pWriter;
    }
}
