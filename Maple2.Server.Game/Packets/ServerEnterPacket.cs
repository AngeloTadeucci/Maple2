using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class ServerEnterPacket {
    public static ByteWriter Request(IActor<Player> fieldPlayer) {
        Player player = fieldPlayer.Value;

        var pWriter = Packet.Of(SendOp.ServerEnter);
        pWriter.WriteInt(fieldPlayer.ObjectId);      
        pWriter.WriteHexString("D9 00 A4 A7 F8 27 19 24 01 00 CA 1C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 2B 37 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 38 00 70 00 72 00 6F 00 66 00 69 00 6C 00 65 00 2F 00 35 00 33 00 2F 00 36 00 34 00 2F 00 32 00 36 00 30 00 31 00 31 00 35 00 34 00 32 00 30 00 38 00 37 00 30 00 31 00 30 00 38 00 37 00 39 00 36 00 31 00 2F 00 36 00 33 00 38 00 35 00 30 00 34 00 34 00 35 00 33 00 39 00 31 00 32 00 31 00 38 00 30 00 36 00 33 00 36 00 2E 00 70 00 6E 00 67 00 00 15 00 D4 84 1E 00 6F 75 19 03 F7 84 1E 00 81 84 1E 00 70 75 19 03 F9 84 1E 00 71 75 19 03 2A 9C 19 03 41 75 19 03 74 75 19 03 01 B4 C4 04 75 75 19 03 DC 84 1E 00 21 75 19 03 76 75 19 03 77 75 19 03 BE 84 1E 00 E9 4D C1 03 EA 4D C1 03 80 0B B2 03 D3 84 1E 00 03 00 81 84 1E 00 BE 84 1E 00 D3 84 1E 00 00 00 00 00 00 00 00 00 00 00 1C 00 68 00 74 00 74 00 70 00 3A 00 2F 00 2F 00 6D 00 61 00 70 00 6C 00 65 00 73 00 74 00 6F 00 72 00 79 00 32 00 2E 00 6E 00 65 00 78 00 6F 00 6E 00 2E 00 63 00 6F 00 6D 00 18 00 68 00 74 00 74 00 70 00 3A 00 2F 00 2F 00 31 00 32 00 35 00 2E 00 31 00 34 00 31 00 2E 00 32 00 31 00 30 00 2E 00 35 00 31 00 2F 00 6D 00 61 00 0A 00 5E 00 68 00 74 00 74 00 70 00 73 00 3F 00 3A 00 2F 00 2F 00");
        return pWriter;
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
